using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.UI;

namespace Prism {
/// <summary>Semantic text for controls whose visual glyph is not a text label.</summary>
public sealed class PrismAccessibilityLabel : MonoBehaviour {
 public string Label;
 public Func<string> ValueGetter;
 public bool IsToggle;
}

/// <summary>
/// Native screen-reader hierarchy for uGUI menus and sheets. This does not make
/// the spatial optical board playable without vision; board interaction needs
/// a separate nonvisual design and physical-device accessibility validation.
/// </summary>
public sealed class PrismScreenReader : MonoBehaviour {
 sealed class Entry {
  public RectTransform Rect;
  public Button Button;
  public TMP_Text Text;
  public ScrollRect Scroll;
  public PrismAccessibilityLabel Semantic;
  public AccessibilityNode Node;
 }
 readonly List<Entry> entries=new List<Entry>();
 readonly List<Component> candidates=new List<Component>();
 readonly List<Component> previous=new List<Component>();
 readonly Vector3[] corners=new Vector3[4];
 PrismPresentation presentation;
 RectTransform currentRoot;
 AccessibilityHierarchy hierarchy;
 float nextRefresh;
 bool subscribed;
 public int NodeCount {get;private set;}
 public int NativeButtonCount {get;private set;}
 public AccessibilityHierarchy NativeHierarchy=>hierarchy;

 public void Initialize(PrismPresentation owner){presentation=owner;RefreshNow();}
 void OnEnable(){
  if(!subscribed){AssistiveSupport.screenReaderStatusChanged+=ReaderChanged;subscribed=true;}
  if(presentation!=null)RefreshNow();
 }
 void OnDisable(){
  if(subscribed){AssistiveSupport.screenReaderStatusChanged-=ReaderChanged;subscribed=false;}
  if(AssistiveSupport.activeHierarchy==hierarchy)AssistiveSupport.activeHierarchy=null;
 }
 void ReaderChanged(bool enabled){
  RefreshNow();
  if(enabled&&hierarchy!=null){AssistiveSupport.activeHierarchy=hierarchy;AssistiveSupport.notificationDispatcher.SendScreenChanged();}
  else if(AssistiveSupport.activeHierarchy==hierarchy)AssistiveSupport.activeHierarchy=null;
 }
 void LateUpdate(){if(presentation!=null&&Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.15f;RefreshNow();}}

 public void RefreshNow(){
  var root=presentation!=null?presentation.AccessibleRoot:null;
  candidates.Clear();
  if(root!=null)Collect(root);
  bool changed=root!=currentRoot||candidates.Count!=previous.Count;
  if(!changed)for(int i=0;i<candidates.Count;i++)if(candidates[i]!=previous[i]){changed=true;break;}
  if(changed){currentRoot=root;Rebuild();}
  UpdateNodes();
  if(AssistiveSupport.isScreenReaderEnabled&&hierarchy!=null&&AssistiveSupport.activeHierarchy!=hierarchy)AssistiveSupport.activeHierarchy=hierarchy;
 }

 void Collect(RectTransform root){
  // Include masked/offscreen candidates in topology, but expose only visible
  // entries. Scrolling then updates isActive without replacing node IDs/focus.
  foreach(var rect in root.GetComponentsInChildren<RectTransform>(true)){
   var button=rect.GetComponent<Button>();
   if(button!=null){candidates.Add(button);continue;}
   if(rect.GetComponentInParent<Button>()!=null)continue;
   var scroll=rect.GetComponent<ScrollRect>();
   if(scroll!=null){candidates.Add(scroll);continue;}
   var text=rect.GetComponent<TMP_Text>();
   if(text!=null)candidates.Add(text);
  }
 }
 void Rebuild(){
  hierarchy=new AccessibilityHierarchy();entries.Clear();previous.Clear();previous.AddRange(candidates);
  var scrollNodes=new Dictionary<ScrollRect,AccessibilityNode>();
  foreach(var candidate in candidates){
   if(candidate==null)continue;
   var entry=new Entry{Rect=candidate.transform as RectTransform,Button=candidate as Button,Text=candidate as TMP_Text,Scroll=candidate as ScrollRect,Semantic=candidate.GetComponent<PrismAccessibilityLabel>()};
   AccessibilityNode parent=null;var ancestorScroll=candidate.transform.parent!=null?candidate.transform.parent.GetComponentInParent<ScrollRect>():null;
   if(ancestorScroll!=null)scrollNodes.TryGetValue(ancestorScroll,out parent);
   var node=hierarchy.AddNode(Label(entry),parent);entry.Node=node;entries.Add(entry);
   if(entry.Scroll!=null)scrollNodes[entry.Scroll]=node;
   node.role=entry.Button!=null?(entry.Semantic!=null&&entry.Semantic.IsToggle?AccessibilityRole.Toggle:AccessibilityRole.Button):entry.Scroll!=null?AccessibilityRole.ScrollView:AccessibilityRole.StaticText;
   node.frameGetter=()=>Frame(entry.Rect);
   if(entry.Button!=null)node.invoked+=()=>Activate(entry);
   if(entry.Scroll!=null)node.scrolled+=direction=>Scroll(entry,direction);
  }
  if(AssistiveSupport.isScreenReaderEnabled){AssistiveSupport.activeHierarchy=hierarchy;AssistiveSupport.notificationDispatcher.SendScreenChanged();}
 }
 void UpdateNodes(){
  NodeCount=NativeButtonCount=0;bool visibilityChanged=false;
  foreach(var entry in entries){
   bool active=Visible(entry.Rect);
   if(entry.Text!=null)active&=entry.Text.enabled&&entry.Text.color.a>.01f&&!string.IsNullOrWhiteSpace(entry.Text.text);
   // Transparent hit surfaces still represent real actions (angle/target info).
   // CanvasGroup, active state and viewport clipping govern their exposure.
   if(entry.Button!=null)active&=entry.Button.enabled;
   string label=Label(entry);active&=!string.IsNullOrWhiteSpace(label);
   if(entry.Node.isActive!=active){entry.Node.isActive=active;visibilityChanged=true;}
   if(entry.Node.label!=label)entry.Node.label=label;
   string value=active?(entry.Semantic?.ValueGetter?.Invoke()??""):"";
   var state=entry.Button!=null&&!entry.Button.IsInteractable()?AccessibilityState.Disabled:AccessibilityState.None;
   if(entry.Semantic!=null&&entry.Semantic.IsToggle&&value=="Açık")state|=AccessibilityState.Selected;
   if(entry.Node.state!=state)entry.Node.state=state;
   if(entry.Button!=null&&!entry.Button.IsInteractable())value=string.IsNullOrEmpty(value)?"Kullanılamıyor":value+", kullanılamıyor";
   if(entry.Node.value!=value)entry.Node.value=value;
   if(active){NodeCount++;if(entry.Button!=null&&entry.Button.IsInteractable())NativeButtonCount++;}
  }
  if(visibilityChanged&&AssistiveSupport.isScreenReaderEnabled)AssistiveSupport.notificationDispatcher.SendLayoutChanged();
 }
 string Label(Entry entry){
  if(entry.Semantic!=null&&!string.IsNullOrWhiteSpace(entry.Semantic.Label))return entry.Semantic.Label;
  if(entry.Text!=null)return Clean(entry.Text.text);
  if(entry.Scroll!=null)return "Kaydırılabilir liste";
  if(entry.Button==null)return "";
  var result=new StringBuilder();
  foreach(var text in entry.Button.GetComponentsInChildren<TMP_Text>(false)){
   if(!text.enabled||text.color.a<=.01f)continue;string label=Clean(text.text);if(string.IsNullOrWhiteSpace(label))continue;
   if(result.Length>0)result.Append(", ");result.Append(label);
  }
  return result.Length>0?result.ToString():entry.Button.name=="Control"?"":entry.Button.name;
 }
 static string Clean(string value){
  if(string.IsNullOrEmpty(value))return "";
  var text=new StringBuilder();bool tag=false;
  foreach(char c in value){if(c=='<'){tag=true;continue;}if(c=='>'){tag=false;continue;}if(!tag)text.Append(c);}
  return text.ToString().Replace(Environment.NewLine," ").Trim();
 }
 bool Activate(Entry entry){
  if(entry.Button==null||!entry.Button.isActiveAndEnabled||!Visible(entry.Rect)||!entry.Node.isActive||!entry.Button.IsInteractable())return false;
  entry.Button.onClick.Invoke();RefreshNow();return true;
 }
 bool Scroll(Entry entry,AccessibilityScrollDirection direction){
  var scroll=entry.Scroll;if(scroll==null||!Visible(entry.Rect)||!scroll.enabled)return false;
  bool forward=direction==AccessibilityScrollDirection.Down||direction==AccessibilityScrollDirection.Forward||direction==AccessibilityScrollDirection.Right;
  bool backward=direction==AccessibilityScrollDirection.Up||direction==AccessibilityScrollDirection.Backward||direction==AccessibilityScrollDirection.Left;
  if(!forward&&!backward)return false;
  float sign=forward?-1:1;
  if(scroll.vertical){float before=scroll.verticalNormalizedPosition;scroll.verticalNormalizedPosition=Mathf.Clamp01(before+sign*.65f);if(Mathf.Abs(before-scroll.verticalNormalizedPosition)<.001f)return false;}
  else if(scroll.horizontal){float before=scroll.horizontalNormalizedPosition;scroll.horizontalNormalizedPosition=Mathf.Clamp01(before-sign*.65f);if(Mathf.Abs(before-scroll.horizontalNormalizedPosition)<.001f)return false;}
  else return false;
  Canvas.ForceUpdateCanvases();RefreshNow();
  if(AssistiveSupport.isScreenReaderEnabled)AssistiveSupport.notificationDispatcher.SendPageScrolledAnnouncement("Liste kaydırıldı",entry.Node);
  return true;
 }
 bool Visible(RectTransform rect){
  var activeRoot=presentation!=null?presentation.AccessibleRoot:null;
  if(rect==null||!rect.gameObject.activeInHierarchy||activeRoot==null||!rect.IsChildOf(activeRoot)&&rect!=activeRoot)return false;
  float alpha=1;
  for(Transform ancestor=rect;ancestor!=null;ancestor=ancestor.parent){
   bool ignore=false;
   foreach(var group in ancestor.GetComponents<CanvasGroup>()){
    if(!group.enabled)continue;alpha*=group.alpha;
    // A visible but deliberately blocked HUD must not accept native actions.
    if(!group.interactable||!group.blocksRaycasts)return false;
    ignore|=group.ignoreParentGroups;
   }
   if(alpha<=.01f)return false;
   if(ignore)break;
  }
  return Frame(rect).width>1&&Frame(rect).height>1;
 }
 Rect Frame(RectTransform rect){
  if(rect==null)return Rect.zero;
  var canvas=rect.GetComponentInParent<Canvas>();Camera camera=canvas!=null&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;
  Rect frame=ScreenRect(rect,camera);frame=Intersect(frame,new Rect(0,0,Screen.width,Screen.height));
  for(Transform ancestor=rect.parent;ancestor!=null;ancestor=ancestor.parent){
   var mask=ancestor.GetComponent<RectMask2D>();var stencil=ancestor.GetComponent<Mask>();
   if(mask!=null&&mask.enabled||stencil!=null&&stencil.enabled)if(ancestor is RectTransform clip)frame=Intersect(frame,ScreenRect(clip,camera));
  }
  return frame;
 }
 Rect ScreenRect(RectTransform rect,Camera camera){
  rect.GetWorldCorners(corners);Vector2 min=new Vector2(float.MaxValue,float.MaxValue),max=new Vector2(float.MinValue,float.MinValue);
  for(int i=0;i<4;i++){Vector2 point=RectTransformUtility.WorldToScreenPoint(camera,corners[i]);min=Vector2.Min(min,point);max=Vector2.Max(max,point);}
  return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
 }
 static Rect Intersect(Rect a,Rect b){float left=Mathf.Max(a.xMin,b.xMin),bottom=Mathf.Max(a.yMin,b.yMin),right=Mathf.Min(a.xMax,b.xMax),top=Mathf.Min(a.yMax,b.yMax);return right>left&&top>bottom?Rect.MinMaxRect(left,bottom,right,top):Rect.zero;}
}
}
