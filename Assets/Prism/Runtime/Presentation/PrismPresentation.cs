using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Prism {
/// <summary>Canvas presentation. Optical state and persistence remain in PrismGame.</summary>
public sealed class PrismPresentation : MonoBehaviour {
 enum ScreenView { Home,Gameplay,Map,Final }
 PrismGame game;
 Canvas canvas;
 RectTransform safe,screen,overlay;
 ScreenView view;
 string sheet="";
 int chapter, mapSelection, hintStage, hintLevel=-1;
 bool draggingTray,completionWaiting,tutorialReplay,interactionSeen,introPlayed,mapLevels;
 float lastInteraction;
 Coroutine reveal;
 Rect lastSafe;int lastWidth,lastHeight;
 TextMeshProUGUI levelTitle,goalStatus,tutorial,feedback,angleStatus;
 RectTransform selectedTools,tutorialFinger;
 RectTransform removeControl,trayRoot;
 readonly Dictionary<Kind,Image> trayHalos=new Dictionary<Kind,Image>();
 readonly List<GameObject> precisionControls=new List<GameObject>();
 readonly Dictionary<Kind,TextMeshProUGUI> counts=new Dictionary<Kind,TextMeshProUGUI>();
 readonly Dictionary<Kind,Button> stockButtons=new Dictionary<Kind,Button>();
 readonly List<Image> goalDots=new List<Image>();
 Coroutine completion;
 float feedbackUntil;
 public bool BlocksBoardInput=>view!=ScreenView.Gameplay||sheet.Length>0||completionWaiting||draggingTray;
 public bool IsGameplay=>view==ScreenView.Gameplay;
 public bool HasModal=>sheet.Length>0;
 public void CancelTrayGesture(){draggingTray=false;}

 public void Initialize(PrismGame controller){
  game=controller;
  PrismTheme.Font(false);PrismTheme.Font(true);
  var root=new GameObject("Prism · Presentation",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
  root.transform.SetParent(transform,false);canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=20;
  var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(360,800);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
  safe=Rect("Safe area",root.transform);Stretch(safe);
  screen=Rect("Screen",safe);Stretch(screen);overlay=Rect("Sheets",safe);Stretch(overlay);
  if(EventSystem.current==null){var events=new GameObject("Prism · UI input",typeof(EventSystem),typeof(InputSystemUIInputModule));events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();}
  ApplySafeArea();
 }
 void Update(){
  if(game==null)return;
  if(lastSafe!=UnityEngine.Screen.safeArea||lastWidth!=UnityEngine.Screen.width||lastHeight!=UnityEngine.Screen.height)ApplySafeArea();
  if(view!=ScreenView.Gameplay)return;
  if(Pointer.current!=null&&Pointer.current.press.wasPressedThisFrame){interactionSeen=true;lastInteraction=Time.unscaledTime;}
  if(levelTitle)levelTitle.text=game.CurrentLevel.Name;
  foreach(var entry in counts){
   int n=game.Remaining(entry.Key);bool active=game.ArmedKind==entry.Key;
   entry.Value.text=n<=0?"Tahtada":"×"+n;entry.Value.color=Alpha(PrismTheme.Muted,n<=0?.62f:active?.65f:.9f);
   var button=stockButtons[entry.Key];button.interactable=n>0&&!game.HasWon&&!game.InteractionBusy;button.GetComponent<Image>().color=active?PrismTheme.StoneLight:PrismTheme.Stone;
   var icon=button.GetComponentInChildren<PrismIcon>();if(icon){Color tint=entry.Key==Kind.Green?PrismTheme.Success:entry.Key==Kind.Red?new Color32(240,110,135,255):PrismTheme.Ivory;icon.color=Alpha(tint,n<=0?.4f:1);icon.rectTransform.anchoredPosition=new Vector2(0,active?13:9);}
   if(trayHalos.TryGetValue(entry.Key,out var halo))halo.color=Alpha(PrismTheme.Accent,active?.15f:0);
  }
  if(trayRoot){var target=new Vector2(0,draggingTray?144:156);trayRoot.anchoredPosition=VisualEnvironment.ReducedMotion?target:Vector2.Lerp(trayRoot.anchoredPosition,target,1-Mathf.Exp(-Time.unscaledDeltaTime*20));}
  var result=game.CurrentResult;int lit=0;
  for(int i=0;i<goalDots.Count;i++){bool on=result!=null&&i<result.Energy.Length&&result.Energy[i]>=game.CurrentLevel.Goals[i].Threshold;goalDots[i].color=on?PrismTheme.Success:Alpha(PrismTheme.Ivory,.22f);if(on)lit++;}
  if(goalStatus)goalStatus.text=lit+" / "+goalDots.Count;
  if(selectedTools)selectedTools.gameObject.SetActive(game.SelectedIndex>=0&&!game.HasWon&&sheet.Length==0&&!completionWaiting);
  bool rotatable=game.SelectedIndex>=0&&PieceInfo.CanRotate(game.CurrentPieces[game.SelectedIndex].Kind);
  foreach(var control in precisionControls)if(control)control.SetActive(rotatable);
  if(angleStatus&&game.SelectedIndex>=0)angleStatus.text=rotatable?Mathf.RoundToInt((float)game.CurrentPieces[game.SelectedIndex].Angle)+"°":"Cam küre";
  bool learning=game.CurrentIndex<3&&(tutorialReplay||!game.IsLevelComplete(game.CurrentIndex))&&sheet.Length==0&&!completionWaiting;
  if(tutorial){bool visible=learning&&Time.unscaledTime-lastInteraction>=8;tutorial.gameObject.SetActive(visible);if(visible){tutorial.text=game.CurrentIndex==0?game.CurrentPieces.Count==0?"Aynayı ışığın yoluna sürükle":"Aynaya dokun. Açısını değiştir.":game.CurrentIndex==1?"Açıyı değiştir, ışığın yolunu izle.":"Bir hareketi geri almak için ok simgesine dokun.";}}
  if(tutorialFinger){bool visible=learning&&!interactionSeen;tutorialFinger.gameObject.SetActive(visible);if(visible&&!VisualEnvironment.ReducedMotion){float t=Mathf.PingPong(Time.unscaledTime*.38f,1f);tutorialFinger.anchoredPosition=new Vector2(Mathf.Lerp(-90,-20,t),Mathf.Lerp(130,260,t));tutorialFinger.GetComponent<CanvasGroup>().alpha=Mathf.Sin(t*Mathf.PI)*.55f;}}
  if(feedback&&Time.unscaledTime>feedbackUntil)feedback.text="";
 }
 void ApplySafeArea(){
  lastSafe=UnityEngine.Screen.safeArea;lastWidth=UnityEngine.Screen.width;lastHeight=UnityEngine.Screen.height;
  if(lastWidth<=0||lastHeight<=0)return;
  safe.anchorMin=new Vector2(lastSafe.xMin/lastWidth,lastSafe.yMin/lastHeight);safe.anchorMax=new Vector2(lastSafe.xMax/lastWidth,lastSafe.yMax/lastHeight);safe.offsetMin=safe.offsetMax=Vector2.zero;
  if(view==ScreenView.Home){Canvas.ForceUpdateCanvases();LayoutHome();}
 }
 void Clear(RectTransform parent){foreach(Transform child in parent)Destroy(child.gameObject);}
 void ClearSheet(){Clear(overlay);sheet="";VisualEnvironment.SetPaused(false);}
 void ChangeScreen(ScreenView next){
  draggingTray=false;game.CancelInteraction();
  if(reveal!=null){StopCoroutine(reveal);reveal=null;}if(screen){var existingGroup=screen.GetComponent<CanvasGroup>();if(existingGroup)existingGroup.alpha=1;}
  if(completion!=null){StopCoroutine(completion);completion=null;}completionWaiting=false;ClearSheet();Clear(screen);view=next;counts.Clear();stockButtons.Clear();trayHalos.Clear();trayRoot=null;goalDots.Clear();selectedTools=null;tutorial=null;tutorialFinger=null;levelTitle=null;goalStatus=null;angleStatus=null;
 }
 public void ShowHome(){
  tutorialReplay=false;
  ChangeScreen(ScreenView.Home);Backdrop(screen);
  var scene=Rect("Ana menü optik sahnesi",screen);At(scene,.5f,1,0,-255,328,310);scene.gameObject.AddComponent<PrismHomeScene>();
  var wordmark=Label(screen,"PRISM",46,PrismTheme.Ivory,new Vector2(0,0),new Vector2(320,68),new Vector2(.5f,.44f),true);wordmark.characterSpacing=8;
  Label(screen,"Işığın yolunu bul.",16,PrismTheme.Muted,new Vector2(0,-49),new Vector2(310,30),new Vector2(.5f,.44f));
  Primary(screen,game.CompletedCount==0&&game.CurrentIndex==0?"Başla":"Devam et",new Vector2(0,208),()=>game.ContinueGame());
  Label(screen,"Bölüm "+(game.CurrentIndex+1)+" · "+game.CurrentLevel.Name,12,PrismTheme.Muted,new Vector2(0,145),new Vector2(310,30),new Vector2(.5f,0));
  Label(screen,game.CompletedCount+" / 100 ışık yolu keşfedildi",12,PrismTheme.Muted,new Vector2(0,115),new Vector2(310,24),new Vector2(.5f,0));
  TextButton(screen,"Bölümler",new Vector2(-76,68),new Vector2(140,52),()=>ShowMap());TextButton(screen,"Ayarlar",new Vector2(76,68),new Vector2(140,52),ShowSettings);
  Label(screen,"Hesapsız. Reklamsız. Kendi ritminde.",10,Alpha(PrismTheme.Muted,.65f),new Vector2(0,26),new Vector2(320,20),new Vector2(.5f,0));
  Canvas.ForceUpdateCanvases();LayoutHome();
  FadeIn(screen);
  if(!introPlayed){introPlayed=true;reveal=StartCoroutine(BrandReveal(false));}
 }
 public void ShowGameplay(){
  ChangeScreen(ScreenView.Gameplay);
  interactionSeen=false;lastInteraction=Time.unscaledTime;
  IconButton(screen,"map",new Vector2(30,-31),new Vector2(0,1),ShowMap,"Bölümler");
  IconButton(screen,"pause",new Vector2(-30,-31),new Vector2(1,1),ShowPause,"Duraklat");
  Label(screen,"BÖLÜM "+(game.CurrentIndex+1).ToString("00")+" · "+game.CurrentLevel.Chapter,12,PrismTheme.Muted,new Vector2(0,-24),new Vector2(232,24),new Vector2(.5f,1));
  levelTitle=Label(screen,"",17,PrismTheme.Ivory,new Vector2(0,-48),new Vector2(232,28),new Vector2(.5f,1),true);levelTitle.enableAutoSizing=true;levelTitle.fontSizeMin=12;levelTitle.fontSizeMax=17;
  var targetPlate=Image(screen,"Hedef ilerlemesi",PrismTheme.Surface);targetPlate.sprite=Rounded();targetPlate.type=UnityEngine.UI.Image.Type.Sliced;At(targetPlate.rectTransform,.5f,1,0,-82,Mathf.Max(124,game.CurrentLevel.Goals.Length*17+76),30);
  int goals=game.CurrentLevel.Goals.Length;float total=goals*17;
  for(int i=0;i<goals;i++){var dot=Image(screen,"Hedef "+(i+1),Alpha(PrismTheme.Ivory,.25f));dot.sprite=Circle();At(dot.rectTransform,.5f,1,-total*.5f+i*17-20,-82,9,9);goalDots.Add(dot);}
  goalStatus=Label(screen,"",12,PrismTheme.Ivory,new Vector2(total*.5f+12,-82),new Vector2(48,24),new Vector2(.5f,1));
  var tray=Image(screen,"Parça tepsisi",PrismTheme.Surface);tray.sprite=Rounded();tray.type=UnityEngine.UI.Image.Type.Sliced;float slotWidth=Mathf.Min(86,320f/Mathf.Max(1,CountKinds()));At(tray.rectTransform,.5f,0,0,156,CountKinds()*slotWidth+14,84);tray.raycastTarget=true;trayRoot=tray.rectTransform;
  var kinds=new List<Kind>();foreach(var k in game.CurrentLevel.Stock)if(!kinds.Contains(k))kinds.Add(k);
  for(int i=0;i<kinds.Count;i++){
   Kind kind=kinds[i];float x=(i-(kinds.Count-1)*.5f)*slotWidth;
   var button=IconButton(tray.transform,Glyph(kind),new Vector2(x,0),new Vector2(.5f,.5f),()=>game.ArmPiece(kind),PieceInfo.Name(kind));button.GetComponent<RectTransform>().sizeDelta=new Vector2(slotWidth-4,76);button.GetComponent<Image>().sprite=Rounded();button.GetComponent<Image>().type=UnityEngine.UI.Image.Type.Sliced;
   var cardShadow=button.GetComponent<PrismPressMotion>().Shadow.GetComponent<Image>();cardShadow.sprite=Rounded();cardShadow.type=UnityEngine.UI.Image.Type.Sliced;
   var pieceIcon=button.GetComponentInChildren<PrismIcon>();pieceIcon.rectTransform.sizeDelta=new Vector2(30,30);
   button.transition=Selectable.Transition.None;
   var caption=Label(button.transform,PieceInfo.Name(kind).Replace(" seçici",Environment.NewLine+"seçici"),12,PrismTheme.Ivory,new Vector2(0,-21),new Vector2(slotWidth-8,34),new Vector2(.5f,.5f));caption.textWrappingMode=TextWrappingModes.Normal;
   var halo=Image(button.transform,"Seçili optik ışık",Color.clear);halo.sprite=Circle();At(halo.rectTransform,.5f,.5f,0,6,43,43);halo.transform.SetAsFirstSibling();trayHalos[kind]=halo;
   var count=Label(button.transform,"",12,PrismTheme.Muted,new Vector2(0,28),new Vector2(slotWidth-8,18),new Vector2(.5f,.5f));counts[kind]=count;stockButtons[kind]=button;
   var trigger=button.gameObject.AddComponent<EventTrigger>();
   AddEvent(trigger,EventTriggerType.BeginDrag,e=>{if(game.HasWon||game.InteractionBusy||game.Remaining(kind)<=0)return;draggingTray=true;game.StartTrayDrag(kind,((PointerEventData)e).position);});
   AddEvent(trigger,EventTriggerType.Drag,e=>{if(draggingTray)game.UpdateTrayDrag(((PointerEventData)e).position);});
   AddEvent(trigger,EventTriggerType.EndDrag,e=>{if(!draggingTray)return;game.EndTrayDrag(((PointerEventData)e).position);draggingTray=false;});
  }
  var undo=IconButton(screen,"undo",new Vector2(68,48),new Vector2(0,0),()=>game.UndoAction(),"Geri al");Label(undo.transform,"Geri al",12,PrismTheme.Muted,new Vector2(0,-32),new Vector2(76,20),new Vector2(.5f,.5f));
  var hint=IconButton(screen,"hint",new Vector2(0,48),new Vector2(.5f,0),ShowHint,"İpucu");hint.GetComponentInChildren<PrismIcon>().color=PrismTheme.Accent;Label(hint.transform,"İpucu",12,PrismTheme.Accent,new Vector2(0,-32),new Vector2(76,20),new Vector2(.5f,.5f));
  var reset=IconButton(screen,"reset",new Vector2(-68,48),new Vector2(1,0),ShowRestart,"Yeniden başlat");Label(reset.transform,"Yeniden",12,PrismTheme.Muted,new Vector2(0,-32),new Vector2(76,20),new Vector2(.5f,.5f));
  selectedTools=Rect("Seçili parça",screen);At(selectedTools,.5f,0,0,244,300,48);
  var toolSurface=Image(selectedTools,"Açı yüzeyi",PrismTheme.Surface);toolSurface.sprite=Rounded();toolSurface.type=UnityEngine.UI.Image.Type.Sliced;Stretch(toolSurface.rectTransform);
  precisionControls.Clear();
  precisionControls.Add(TextButton(selectedTools,"−1°",new Vector2(-94,0),new Vector2(58,48),()=>game.RotateSelected(-1),new Vector2(.5f,.5f)).gameObject);
  angleStatus=Label(selectedTools,"",16,PrismTheme.Accent,new Vector2(-14,0),new Vector2(88,32),new Vector2(.5f,.5f),true);
  precisionControls.Add(TextButton(selectedTools,"+1°",new Vector2(64,0),new Vector2(48,48),()=>game.RotateSelected(1),new Vector2(.5f,.5f)).gameObject);
  removeControl=IconButton(selectedTools,"trash",new Vector2(124,0),new Vector2(.5f,.5f),()=>game.RemoveSelected(),"Parçayı kaldır").GetComponent<RectTransform>();
  tutorial=Label(screen,"",12,PrismTheme.Muted,new Vector2(0,121),new Vector2(322,30),new Vector2(.5f,0));
  tutorialFinger=Rect("Hayalet dokunuş",screen);At(tutorialFinger,.5f,0,-90,130,25,25);tutorialFinger.gameObject.AddComponent<CanvasGroup>();var finger=Image(tutorialFinger,"Dokunuş",Alpha(PrismTheme.Ivory,.55f));finger.sprite=Circle();Stretch(finger.rectTransform);
  feedback=Label(screen,"",12,PrismTheme.Ivory,new Vector2(0,304),new Vector2(310,32),new Vector2(.5f,0));
  if(!game.IsAutomatedRun)foreach(var kind in kinds){string key="prism.discovered."+kind;if(PlayerPrefs.GetInt(key,0)!=0)continue;PlayerPrefs.SetInt(key,1);PlayerPrefs.Save();StartCoroutine(Discovery(kind));break;}
 }
 public void ShowMap(){
  ChangeScreen(ScreenView.Map);chapter=Mathf.Clamp(game.CurrentIndex/10,0,9);DrawChapterMap();
 }
 void DrawChapterMap(){
  mapLevels=false;
  Clear(screen);Backdrop(screen);IconButton(screen,"back",new Vector2(30,-31),new Vector2(0,1),ShowHome,"Ana menü");Label(screen,"Işık yolları",20,PrismTheme.Ivory,new Vector2(0,-31),new Vector2(250,44),new Vector2(.5f,1),true);
  Label(screen,game.CompletedCount+" / "+game.CampaignLevels.Length+" keşfedildi",11,PrismTheme.Muted,new Vector2(0,-67),new Vector2(260,24),new Vector2(.5f,1));
  var scroll=Scroll(screen,"Chapter ışık yolu",new Vector2(16,92),new Vector2(-16,-104),false);var content=scroll.content;content.sizeDelta=new Vector2(0,1360);
  string[] roman={"I","II","III","IV","V","VI","VII","VIII","IX","X"};string[] glyphs={"mirror","spectrum","green","lens","split","gate","sphere","wave","interference","orbit"};
  var centers=new Vector2[10];var radii=new float[10];int completedLinks=0;
  for(int c=0;c<10;c++){centers[c]=new Vector2(c%2==0?-84:84,-72-c*128);radii[c]=c==chapter?52:42;if(game.IsLevelComplete(c*10+9))completedLinks=c+1;}
  MapPath(content,centers,radii,completedLinks);
  for(int c=0;c<10;c++){
   int selected=c,index=c*10;float x=centers[c].x,y=centers[c].y,labelX=c%2==0?62:-62;bool open=game.IsLevelUnlocked(index);int done=0;for(int n=0;n<10;n++)if(game.IsLevelComplete(index+n))done++;
   var node=CircleButton(content,open?glyphs[c]:"lock",new Vector2(x,y),radii[c]*2,()=>{chapter=selected;mapSelection=Mathf.Clamp(game.CurrentIndex,selected*10,selected*10+9);if(!game.IsLevelUnlocked(mapSelection))mapSelection=selected*10;DrawLevelMap();});node.interactable=open;node.GetComponent<Image>().color=c==chapter?PrismTheme.StoneLight:PrismTheme.Surface;
   if(c==chapter&&open)node.gameObject.AddComponent<PrismNodePulse>();
   var optical=node.GetComponentInChildren<PrismIcon>();if(optical)optical.color=open?PrismTheme.Ivory:Alpha(PrismTheme.Muted,.3f);
   Label(content,roman[c]+(c==chapter?" · ŞİMDİ":""),12,open?PrismTheme.Accent:Alpha(PrismTheme.Muted,.4f),new Vector2(labelX,y+30),new Vector2(178,22),new Vector2(.5f,1),true);
   Label(content,game.CampaignLevels[index].Chapter,17,open?PrismTheme.Ivory:Alpha(PrismTheme.Muted,.45f),new Vector2(labelX,y+2),new Vector2(178,30),new Vector2(.5f,1),true);
   Label(content,open?done+" / 10 tamamlandı":"Önceki üniteyi keşfet",12,Alpha(PrismTheme.Muted,open?.9f:.4f),new Vector2(labelX,y-29),new Vector2(178,24),new Vector2(.5f,1));
   if(done==10){var badge=Image(node.transform,"Keşfedildi",PrismTheme.Success);badge.sprite=Circle();At(badge.rectTransform,.5f,.5f,29,-29,24,24);Icon(badge.transform,"check",Vector2.zero,16,PrismTheme.Background);}
  }
  Primary(screen,"Bölüm "+(game.CurrentIndex+1)+"'ya devam et",new Vector2(0,44),()=>game.ContinueGame());
  scroll.verticalNormalizedPosition=Mathf.Clamp01(1-Mathf.Max(0,chapter*128-256)/(1360-Mathf.Max(200,SafeHeight()-196)));
 }
 void DrawLevelMap(){
  mapLevels=true;
  Clear(screen);Backdrop(screen);IconButton(screen,"back",new Vector2(30,-31),new Vector2(0,1),DrawChapterMap,"Chapter yolu");Label(screen,game.CampaignLevels[chapter*10].Chapter,20,PrismTheme.Ivory,new Vector2(0,-31),new Vector2(250,44),new Vector2(.5f,1),true);
  Label(screen,"On küçük keşif",12,PrismTheme.Muted,new Vector2(0,-67),new Vector2(260,24),new Vector2(.5f,1));
  var scroll=Scroll(screen,"Bölüm yolu",new Vector2(16,166),new Vector2(-16,-104),false);var content=scroll.content;content.sizeDelta=new Vector2(0,530);
  var centers=new[]{new Vector2(-100,-52),new Vector2(0,-52),new Vector2(100,-52),new Vector2(100,-164),new Vector2(0,-164),new Vector2(-100,-164),new Vector2(-100,-276),new Vector2(0,-276),new Vector2(100,-276),new Vector2(0,-398)};
  var radii=new float[10];int completedLinks=0;for(int i=0;i<10;i++){radii[i]=chapter*10+i==mapSelection?36:30;if(game.IsLevelComplete(chapter*10+i))completedLinks=i+1;}
  MapPath(content,centers,radii,completedLinks);
  for(int i=0;i<10;i++){
   int index=chapter*10+i;if(index>=game.CampaignLevels.Length)break;bool done=game.IsLevelComplete(index),open=game.IsLevelUnlocked(index);var node=CircleButton(content,open?"":"lock",centers[i],radii[i]*2,()=>{mapSelection=index;DrawLevelMap();});node.interactable=open;node.GetComponent<Image>().color=index==mapSelection?PrismTheme.StoneLight:PrismTheme.Surface;
   if(index==mapSelection&&open)node.gameObject.AddComponent<PrismNodePulse>();
   if(open)Label(node.transform,(index+1).ToString("00"),18,PrismTheme.Ivory,Vector2.zero,new Vector2(56,36),new Vector2(.5f,.5f),true);
   if(done){var badge=Image(node.transform,"Tamamlandı",PrismTheme.Success);badge.sprite=Circle();At(badge.rectTransform,.5f,.5f,22,-22,18,18);Icon(badge.transform,"check",Vector2.zero,12,PrismTheme.Background);}
  }
  var selectedLevel=game.CampaignLevels[mapSelection];var card=Image(screen,"Seçilen ışık yolu",PrismTheme.Stone);card.sprite=Rounded();card.type=UnityEngine.UI.Image.Type.Sliced;At(card.rectTransform,.5f,0,0,81,328,148);
  Label(card.transform,"BÖLÜM "+(mapSelection+1),12,PrismTheme.Accent,new Vector2(0,52),new Vector2(300,22),new Vector2(.5f,.5f),true);
  Label(card.transform,selectedLevel.Name,16,PrismTheme.Ivory,new Vector2(0,23),new Vector2(302,28),new Vector2(.5f,.5f),true);
  Label(card.transform,selectedLevel.Stock.Length+" optik parça · "+selectedLevel.Goals.Length+" hedef",12,PrismTheme.Muted,new Vector2(0,-3),new Vector2(302,22),new Vector2(.5f,.5f));
  Primary(card.transform,"Oyna",new Vector2(0,34),()=>game.OpenLevel(mapSelection));
 }
 void MapPath(RectTransform parent,Vector2[] points,float[] radii,int complete){var rt=Rect("Optik yol",parent);rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.pivot=new Vector2(.5f,1);rt.anchoredPosition=Vector2.zero;rt.sizeDelta=new Vector2(328,parent.sizeDelta.y);rt.gameObject.AddComponent<PrismMapPath>().Configure(points,radii,complete);}
 public void ShowPause(){
  var body=Sheet("pause","Bir nefes.",310);
  Primary(body,"Devam et",new Vector2(0,218),ClearSheet);
  TextButton(body,"Yeniden başlat",new Vector2(0,162),new Vector2(290,48),ShowRestart);
  TextButton(body,"Bölümler",new Vector2(-74,99),new Vector2(140,48),ShowMap);
  TextButton(body,"Ayarlar",new Vector2(74,99),new Vector2(140,48),ShowSettings);
  TextButton(body,"Ana menü",new Vector2(0,41),new Vector2(290,48),ShowHome);
 }
 void ShowRestart(){
  var body=Sheet("restart","Baştan deneyelim mi?",240);Label(body,"Bu bölümdeki yerleşim temizlenecek.",12,PrismTheme.Muted,new Vector2(0,157),new Vector2(300,34),new Vector2(.5f,0));
  Primary(body,"Yeniden başlat",new Vector2(0,98),()=>{ClearSheet();game.RestartLevel();});TextButton(body,"Vazgeç",new Vector2(0,38),new Vector2(280,48),ClearSheet);
 }
 public void ShowHint(){
  if(hintLevel!=game.CurrentIndex){hintLevel=game.CurrentIndex;hintStage=0;}DrawHint();
 }
 void DrawHint(){
  var body=Sheet("hint","Bir ışık izi",290);
  string[] help={game.CurrentLevel.Hint,"Bölge işaretlendi. Şimdi parçayı keşfet.","Hayalet parça göründü. Son adımda tam açısını inceleyebilirsin.","Hayalet parçanın yönünü incele. Yerleşimini kendin deneyebilirsin."};
  var text=Label(body,help[Mathf.Clamp(hintStage,0,3)],14,PrismTheme.Ivory,new Vector2(0,179),new Vector2(298,66),new Vector2(.5f,0));text.textWrappingMode=TextWrappingModes.Normal;
  Label(body,hintStage+" / 3 · Kendi ritminde keşfet",11,PrismTheme.Muted,new Vector2(0,126),new Vector2(300,24),new Vector2(.5f,0));
  Primary(body,hintStage==0?"Bölgeyi göster":hintStage==1?"Parçayı göster":hintStage==2?"Tam açıyı göster":"Açıyı yeniden göster",new Vector2(0,77),RevealNextHint);
  TextButton(body,"Oyuna dön",new Vector2(0,24),new Vector2(290,44),ClearSheet);
 }
 public void RevealNextHint(){hintStage=Mathf.Min(3,hintStage+1);game.RequestHint(hintStage);ClearSheet();}
 public void ShowSettings(){
  var body=Sheet("settings","Ayarlar",Mathf.Min(650,SafeHeight()-45));
  var scroll=Scroll(body,"Ayar satırları",new Vector2(18,15),new Vector2(-18,-76),false);var content=scroll.content;content.sizeDelta=new Vector2(0,898);float y=-15;
  Group(content,"SES",ref y);
  ToggleRow(content,"Müzik",game.Feedback.MusicEnabled,game.Feedback.SetMusic,ref y);
  ToggleRow(content,"Ses efektleri",game.Feedback.AudioEnabled,game.Feedback.SetAudio,ref y);
  ToggleRow(content,"Titreşim",game.Feedback.HapticsEnabled,game.Feedback.SetHaptics,ref y);
  Group(content,"GÖRÜNTÜ",ref y);
  Row(content,"Kalite",ShowQuality,ref y,new[]{"Otomatik","Düşük","Orta","Yüksek"}[(int)VisualEnvironment.Requested]);
  Group(content,"ERİŞİLEBİLİRLİK",ref y);
  ToggleRow(content,"Hareketi azalt",VisualEnvironment.ReducedMotion,game.SetReducedMotion,ref y);
  ToggleRow(content,"Renk desteği",VisualEnvironment.ColorSymbols,game.SetColorSymbols,ref y);
  ToggleRow(content,"Yüksek kontrast",VisualEnvironment.HighContrast,game.SetHighContrast,ref y);
  Row(content,"Gelişmiş görüntü",ShowAdvanced,ref y,"Aç");
  Group(content,"OYUN",ref y);
  ToggleRow(content,"Hassas dönüş",VisualEnvironment.PrecisionMode,game.SetPrecisionMode,ref y);
  Row(content,"Öğreticiyi tekrar izle",()=>{ClearSheet();tutorialReplay=true;game.OpenLevel(0);},ref y,"Göster");
  Group(content,"HAKKINDA",ref y);Row(content,"PRISM · "+Application.version,ShowAbout,ref y,"Bilgi");
  content.sizeDelta=new Vector2(0,-y+25);
 }
 void ShowAbout(){
  var body=Sheet("about","PRISM",335);var text=Label(body,string.Join(Environment.NewLine,new[]{"Işığın yolunu bul.","","İlerlemen yalnızca bu cihazda saklanır.","Hesap, reklam ve analiz takibi yok.","","Optik tasarım ve geliştirme · PRISM","","Unity · TextMeshPro · URP","Sora · Open Font License"}),13,PrismTheme.Muted,new Vector2(0,154),new Vector2(298,194),new Vector2(.5f,0));text.textWrappingMode=TextWrappingModes.Normal;TextButton(body,view==ScreenView.Final?"Finale dön":"Ayarlara dön",new Vector2(0,40),new Vector2(280,48),view==ScreenView.Final?(Action)ClearSheet:ShowSettings);
 }
 public void ShowCompletion(bool final){
   if(completionWaiting||sheet=="completion"||view==ScreenView.Final)return;game.CancelInteraction();draggingTray=false;ClearSheet();completionWaiting=true;completion=StartCoroutine(CompletionSequence(final));
 }
 IEnumerator CompletionSequence(bool final){
  // Leave the optical solution alone long enough to be the reward.
    var hud=screen.GetComponent<CanvasGroup>();if(!hud)hud=screen.gameObject.AddComponent<CanvasGroup>();hud.alpha=0;
  yield return new WaitForSecondsRealtime(VisualEnvironment.ReducedMotion ? .8f : 1.15f);
    if(final&&!VisualEnvironment.ReducedMotion){
     float start=Time.unscaledTime;
     while(Time.unscaledTime-start<1.1f){game.RenderSolutionReveal((Time.unscaledTime-start)/1.1f);yield return null;}
     game.RenderSolutionReveal(1f);
    }
    hud.alpha=1;
  completionWaiting=false;completion=null;
  if(final){ShowFinal();yield break;}
  var body=Sheet("completion",game.CurrentIndex%10==9?"Yeni bir ufuk.":"Işık yolu tamamlandı",256);
  Label(body,game.CurrentLevel.Name,13,PrismTheme.Muted,new Vector2(0,173),new Vector2(300,26),new Vector2(.5f,0));
  Label(body,game.CompletedCount+" ışık yolu keşfedildi",11,PrismTheme.Muted,new Vector2(0,139),new Vector2(300,24),new Vector2(.5f,0));
  Primary(body,game.CurrentIndex%10==9?"Yeni bölüme geç":"Devam et",new Vector2(0,87),()=>{ClearSheet();game.AdvanceLevel();});
  TextButton(body,"Çözümü incele",new Vector2(0,32),new Vector2(290,48),ClearSheet);
 }
 void ShowFinal(){
  ChangeScreen(ScreenView.Final);Backdrop(screen);
  Brand(screen,new Vector2(0,50),164,new Vector2(.5f,.65f));
  Label(screen,"100 ışık yolu."+Environment.NewLine+"Tek bir başlangıç.",27,PrismTheme.Ivory,new Vector2(0,-65),new Vector2(320,100),new Vector2(.5f,.5f),true);
  Label(screen,"PRISM",18,PrismTheme.Muted,new Vector2(0,-157),new Vector2(290,40),new Vector2(.5f,.5f));
  Primary(screen,"Bölümlere dön",new Vector2(0,136),ShowMap);TextButton(screen,"Emeği geçenler",new Vector2(0,73),new Vector2(280,48),ShowAbout);reveal=StartCoroutine(BrandReveal(true));
 }
 IEnumerator BrandReveal(bool final){
  var cover=Image(overlay,final?"Son ışık sekansı":"İlk ışık",PrismTheme.Background);Stretch(cover.rectTransform);cover.raycastTarget=true;
  var beam=Image(cover.transform,"Gelen ışık",PrismTheme.Ivory);At(beam.rectTransform,.5f,.55f,-150,40,0,2);
  var mark=Brand(cover.transform,new Vector2(0,40),152,new Vector2(.5f,.55f));var markGroup=mark.gameObject.AddComponent<CanvasGroup>();markGroup.alpha=0;
  var words=Label(cover.transform,"PRISM",29,PrismTheme.Ivory,new Vector2(0,-70),new Vector2(300,48),new Vector2(.5f,.55f),true);var wordGroup=words.gameObject.AddComponent<CanvasGroup>();wordGroup.alpha=0;
  var spectral=Rect("Spektrum çıkışı",cover.transform);At(spectral,.5f,.55f,107,51,160,30);var spectralGroup=spectral.gameObject.AddComponent<CanvasGroup>();spectralGroup.alpha=0;
  Color[] bands={new Color(.59f,.43f,1),new Color(.38f,.58f,1),new Color(.43f,.84f,.9f),new Color(.47f,.87f,.64f),new Color(.95f,.88f,.55f),new Color(1,.65f,.45f),new Color(.94f,.43f,.53f)};
  for(int i=0;i<7;i++){var band=Image(spectral,"Renk "+i,Alpha(bands[i],.62f));At(band.rectTransform,0,.5f,80,(i-3)*3,160,3);band.rectTransform.localRotation=Quaternion.Euler(0,0,(i-3)*2);}
  float duration=VisualEnvironment.ReducedMotion ? .85f : final ? 2.8f : 1.65f,start=Time.unscaledTime;
  while(cover&&Time.unscaledTime-start<duration){float t=(Time.unscaledTime-start)/duration;
   if(VisualEnvironment.ReducedMotion){markGroup.alpha=1;wordGroup.alpha=1;spectralGroup.alpha=.5f;beam.rectTransform.sizeDelta=new Vector2(130,2);}
   else {float enter=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.38f));beam.rectTransform.sizeDelta=new Vector2(150*enter,2);beam.rectTransform.anchoredPosition=new Vector2(-150+75*enter,40);markGroup.alpha=Mathf.Clamp01((t-.2f)/.22f);spectralGroup.alpha=Mathf.Clamp01((t-.38f)/.24f);wordGroup.alpha=Mathf.Clamp01((t-.57f)/.19f);}
   var coverGroup=cover.GetComponent<CanvasGroup>()??cover.gameObject.AddComponent<CanvasGroup>();coverGroup.alpha=t>.85f?1-(t-.85f)/.15f:1;yield return null;
  }
  if(cover)Destroy(cover.gameObject);reveal=null;
 }
 IEnumerator Discovery(Kind kind){
  yield return new WaitForSecondsRealtime(1.7f);if(view!=ScreenView.Gameplay)yield break;
  string note;switch(kind){case Kind.Prism:note="Prizma · Beyaz ışık, yedi renk.";break;case Kind.Lens:note="Lens · Dağılan ışığı bir araya getir.";break;case Kind.Sphere:note="Cam küre · Işığa yeni bir yol aç.";break;case Kind.Red:case Kind.Green:note=PieceInfo.Name(kind)+" · Bir rengi geçir.";break;default:note="Ayna · Işığın yönünü değiştir.";break;}
  var discovery=Label(screen,note,12,PrismTheme.Ivory,new Vector2(0,-113),new Vector2(316,44),new Vector2(.5f,1));FadeIn(discovery.rectTransform);yield return new WaitForSecondsRealtime(1.7f);if(discovery)Destroy(discovery.gameObject);
 }
 public void HandleBack(){if(completionWaiting)return;if(sheet=="advanced"||sheet=="quality"||sheet=="about"){ShowSettings();return;}if(sheet.Length>0){ClearSheet();return;}switch(view){case ScreenView.Gameplay:ShowPause();break;case ScreenView.Map:if(mapLevels)DrawChapterMap();else ShowHome();break;case ScreenView.Final:ShowHome();break;case ScreenView.Home:ShowSettings();break;}}
 public void ShowFeedback(string message){if(feedback){feedback.text=message;feedbackUntil=Time.unscaledTime+2.4f;}}
 RectTransform Sheet(string id,string title,float height){
   game.CancelInteraction();draggingTray=false;ClearSheet();sheet=id;VisualEnvironment.SetPaused(true);
  var dim=Image(overlay,"Dünya perdesi",new Color(.015f,.012f,.03f,.64f));Stretch(dim.rectTransform);dim.raycastTarget=true;var close=dim.gameObject.AddComponent<Button>();close.onClick.AddListener(ClearSheet);
  var body=Image(overlay,"Cam yüzey",Alpha(PrismTheme.Surface,.98f));body.sprite=Rounded();body.type=UnityEngine.UI.Image.Type.Sliced;body.raycastTarget=true;
  At(body.rectTransform,.5f,0,0,height*.5f+8,344,height);body.rectTransform.anchorMin=new Vector2(0,0);body.rectTransform.anchorMax=new Vector2(1,0);body.rectTransform.sizeDelta=new Vector2(-16,height);
  Label(body.transform,title,20,PrismTheme.Ivory,new Vector2(0,-39),new Vector2(278,42),new Vector2(.5f,1),true);
  IconButton(body.transform,"close",new Vector2(-27,-35),new Vector2(1,1),ClearSheet,"Kapat");
  var grab=Image(body.transform,"Tutamak",Alpha(PrismTheme.Ivory,.2f));At(grab.rectTransform,.5f,1,0,-9,28,3);
  var dragHit=Image(body.transform,"Sheet tutamağı",Color.clear);At(dragHit.rectTransform,.5f,1,0,-12,100,24);dragHit.raycastTarget=true;
  var trigger=dragHit.gameObject.AddComponent<EventTrigger>();AddEvent(trigger,EventTriggerType.EndDrag,e=>{var pointer=(PointerEventData)e;if(pointer.position.y-pointer.pressPosition.y<-55)ClearSheet();});
  FadeIn(body.rectTransform);return body.rectTransform;
 }
 float SafeHeight()=>safe.rect.height>100?safe.rect.height:800;
 void LayoutHome(){
  if(!screen)return;float height=SafeHeight(),titleY=Mathf.Max(330,height*.44f);
  var scene=screen.Find("Ana menü optik sahnesi") as RectTransform;
  float artHeight=Mathf.Min(310,Mathf.Max(120,height-titleY-98));
  if(scene)At(scene,.5f,0,0,titleY+58+artHeight*.5f,328*artHeight/310,artHeight);
  foreach(var label in screen.GetComponentsInChildren<TextMeshProUGUI>()){
   if(label.text=="PRISM")At(label.rectTransform,.5f,0,0,titleY,320,68);
   else if(label.text=="Işığın yolunu bul.")At(label.rectTransform,.5f,0,0,titleY-49,310,30);
  }
 }
 void Group(Transform parent,string label,ref float y){Label(parent,label,10,PrismTheme.Accent,new Vector2(0,y),new Vector2(290,28),new Vector2(.5f,1),true,TextAlignmentOptions.Left);y-=32;}
 void ToggleRow(Transform parent,string label,bool value,Action<bool> set,ref float y){
  float rowY=y;var row=TextButton(parent,"",new Vector2(0,rowY),new Vector2(296,48),()=>{},new Vector2(.5f,1));Label(row.transform,label,13,PrismTheme.Ivory,new Vector2(-25,0),new Vector2(230,44),new Vector2(.5f,.5f),false,TextAlignmentOptions.Left);
  var track=Image(row.transform,"Anahtar",value?Alpha(PrismTheme.Accent,.65f):Alpha(PrismTheme.Muted,.2f));track.sprite=Rounded();track.type=UnityEngine.UI.Image.Type.Sliced;At(track.rectTransform,1,.5f,-24,0,36,20);
  var dot=Image(track.transform,"Durum",PrismTheme.Ivory);dot.sprite=Circle();At(dot.rectTransform,.5f,.5f,value?8:-8,0,14,14);
  bool current=value;row.onClick.RemoveAllListeners();row.onClick.AddListener(()=>{current=!current;set(current);track.color=current?Alpha(PrismTheme.Accent,.65f):Alpha(PrismTheme.Muted,.2f);StartCoroutine(ToggleMotion(dot.rectTransform,current?8:-8));Click();});y-=49;
 }
 void Row(Transform parent,string label,Action action,ref float y,string value){var row=TextButton(parent,"",new Vector2(0,y),new Vector2(296,48),action,new Vector2(.5f,1));Label(row.transform,label,13,PrismTheme.Ivory,new Vector2(-45,0),new Vector2(190,44),new Vector2(.5f,.5f),false,TextAlignmentOptions.Left);Label(row.transform,value,11,PrismTheme.Muted,new Vector2(90,0),new Vector2(104,44),new Vector2(.5f,.5f),false,TextAlignmentOptions.Right);y-=49;}
 public void ShowQuality(){
  var body=Sheet("quality","Görüntü kalitesi",370);float y=-94;
  string[] labels={"Otomatik","Düşük","Orta","Yüksek"};
  for(int i=0;i<labels.Length;i++){
   var tier=(VisualQualityTier)i;bool chosen=VisualEnvironment.Requested==tier;
   var row=TextButton(body,labels[i],new Vector2(0,y),new Vector2(296,48),()=>{game.SetVisualQuality(tier);ShowQuality();},new Vector2(.5f,1));
   if(chosen)Icon(row.transform,"check",new Vector2(119,0),24,PrismTheme.Accent);y-=52;
  }
  TextButton(body,"Ayarlara dön",new Vector2(0,34),new Vector2(280,48),ShowSettings);
 }
 public void ShowAdvanced(){
  var body=Sheet("advanced","Gelişmiş görüntü",420);
  // Sheet child is above the world dimmer: controls never darken this live preview.
  var previewRect=Rect("Canlı ışık önizlemesi",body);At(previewRect,.5f,1,0,-117,286,62);
  previewRect.gameObject.AddComponent<PrismBeamPreview>().raycastTarget=false;
  Label(body,"Temsili ışık önizlemesi",12,PrismTheme.Muted,new Vector2(0,-161),new Vector2(286,26),new Vector2(.5f,1));
  DiscreteRow(body,"Işın",new[]{"Yumuşak","Dengeli","Parlak"},new[]{.65f,1f,1.4f},VisualEnvironment.BeamScale,game.SetBeamIntensity,-203);
  DiscreteRow(body,"Parlama",new[]{"Kapalı","Dengeli","Güçlü"},new[]{0f,1f,1.5f},VisualEnvironment.BloomScale,game.SetBloomIntensity,-297);
  TextButton(body,"Ayarlara dön",new Vector2(0,34),new Vector2(280,48),ShowSettings);
 }
 void DiscreteRow(Transform parent,string label,string[] labels,float[] values,float current,Action<float> set,float y){
  Label(parent,label,14,PrismTheme.Ivory,new Vector2(0,y),new Vector2(286,26),new Vector2(.5f,1),false,TextAlignmentOptions.Left);
  var buttons=new List<Button>();int nearest=0;for(int i=1;i<values.Length;i++)if(Mathf.Abs(current-values[i])<Mathf.Abs(current-values[nearest]))nearest=i;
  for(int i=0;i<labels.Length;i++){
   int choice=i;var b=TextButton(parent,labels[i],new Vector2((i-1)*98,y-38),new Vector2(94,48),()=>{set(values[choice]);for(int n=0;n<buttons.Count;n++)buttons[n].GetComponent<Image>().color=Alpha(PrismTheme.Accent,n==choice?.25f:.05f);},new Vector2(.5f,1));
   b.GetComponent<Image>().sprite=Rounded();b.GetComponent<Image>().type=UnityEngine.UI.Image.Type.Sliced;b.GetComponent<Image>().color=Alpha(PrismTheme.Accent,i==nearest?.25f:.05f);buttons.Add(b);
  }
 }
 ScrollRect Scroll(Transform parent,string name,Vector2 bottomLeft,Vector2 topRight,bool horizontal){
  var rt=Rect(name,parent);Stretch(rt);rt.offsetMin=bottomLeft;rt.offsetMax=topRight;
  if(horizontal){rt.anchorMin=new Vector2(0,1);rt.anchorMax=new Vector2(1,1);rt.offsetMin=new Vector2(bottomLeft.x,-145);rt.offsetMax=new Vector2(topRight.x,-91);}
  var viewport=Rect("Viewport",rt);Stretch(viewport);viewport.gameObject.AddComponent<RectMask2D>();var hit=Image(viewport,"Touch",Color.clear);Stretch(hit.rectTransform);hit.raycastTarget=true;
  var content=Rect("Content",viewport);content.anchorMin=horizontal?new Vector2(0,0):new Vector2(0,1);content.anchorMax=horizontal?new Vector2(0,1):new Vector2(1,1);content.pivot=horizontal?new Vector2(0,.5f):new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;
  var scroll=rt.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=horizontal;scroll.vertical=!horizontal;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.decelerationRate=.08f;scroll.scrollSensitivity=28;return scroll;
 }
 IEnumerator ToggleMotion(RectTransform dot,float destination){
  float origin=dot.anchoredPosition.x,start=Time.unscaledTime;
  while(dot&&!VisualEnvironment.ReducedMotion&&Time.unscaledTime-start<.12f){dot.anchoredPosition=new Vector2(Mathf.Lerp(origin,destination,Mathf.SmoothStep(0,1,(Time.unscaledTime-start)/.12f)),0);yield return null;}
  if(dot)dot.anchoredPosition=new Vector2(destination,0);
 }
 PrismOpticalMark Brand(Transform parent,Vector2 position,float size,Vector2 anchor){var rt=Rect("Optik prizma motifi",parent);At(rt,anchor.x,anchor.y,position.x,position.y,size,size);var graphic=rt.gameObject.AddComponent<PrismOpticalMark>();graphic.raycastTarget=false;return graphic;}
 void Backdrop(Transform parent){var bg=Image(parent,"Gece",PrismTheme.Background);Stretch(bg.rectTransform);bg.raycastTarget=true;var ambient=Rect("Sessiz optik alan",parent);Stretch(ambient);ambient.gameObject.AddComponent<PrismOpticalAmbience>().raycastTarget=false;for(int i=0;i<18;i++){var star=Image(parent,"Yıldız",Alpha(PrismTheme.Muted,.1f+i%3*.035f));star.sprite=Circle();At(star.rectTransform,(i*37%97)/100f,(i*53%89)/100f,0,0,2,2);}}
 int CountKinds(){var set=new HashSet<Kind>(game.CurrentLevel.Stock);return set.Count;}
 static string Glyph(Kind kind)=>kind.ToString().ToLowerInvariant();
 void Click(){if(game.Feedback)game.Feedback.Click();}
 Button Primary(Transform parent,string text,Vector2 pos,Action action){var button=TextButton(parent,text,pos,new Vector2(292,60),action);var img=button.GetComponent<Image>();img.color=PrismTheme.Primary;var label=button.GetComponentInChildren<TextMeshProUGUI>();label.color=PrismTheme.PrimaryInk;label.font=PrismTheme.Font(true);label.fontSize=18;button.GetComponent<PrismPressMotion>().Shadow.GetComponent<Image>().color=new Color32(140,128,198,255);return button;}
 Button TextButton(Transform parent,string text,Vector2 pos,Vector2 size,Action action,Vector2? anchor=null){var image=Image(parent,text.Length==0?"Control":text,PrismTheme.Surface);At(image.rectTransform,anchor?.x?? .5f,anchor?.y??0,pos.x,pos.y,Mathf.Max(48,size.x),Mathf.Max(48,size.y));image.sprite=Rounded();image.type=UnityEngine.UI.Image.Type.Sliced;image.raycastTarget=true;var edge=image.gameObject.AddComponent<Outline>();edge.effectColor=new Color(1,1,1,.12f);edge.effectDistance=new Vector2(1,-1);var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;var colors=b.colors;colors.highlightedColor=Color.white;colors.pressedColor=new Color(.9f,.9f,.96f,1);colors.disabledColor=new Color(1,1,1,.46f);b.colors=colors;b.onClick.AddListener(()=>{Click();action();});Label(b.transform,text,14,PrismTheme.Ivory,Vector2.zero,size,new Vector2(.5f,.5f));var motion=b.gameObject.AddComponent<PrismPressMotion>();var shadow=Image(parent,"Taş alt dudak",new Color32(20,17,38,230));shadow.sprite=Rounded();shadow.type=UnityEngine.UI.Image.Type.Sliced;shadow.transform.SetSiblingIndex(b.transform.GetSiblingIndex());motion.Shadow=shadow.rectTransform;return b;}
 Button IconButton(Transform parent,string symbol,Vector2 pos,Vector2 anchor,Action action,string accessible){var b=TextButton(parent,"",pos,new Vector2(48,48),action,anchor);b.name=accessible;b.GetComponent<Image>().sprite=Circle();b.GetComponent<Image>().type=UnityEngine.UI.Image.Type.Simple;b.GetComponent<PrismPressMotion>().Shadow.GetComponent<Image>().sprite=Circle();b.GetComponent<PrismPressMotion>().Shadow.GetComponent<Image>().type=UnityEngine.UI.Image.Type.Simple;Icon(b.transform,symbol,Vector2.zero,26,PrismTheme.Ivory);return b;}
 Button CircleButton(Transform parent,string symbol,Vector2 pos,float size,Action action){var b=TextButton(parent,"",pos,new Vector2(size,size),action,new Vector2(.5f,1));b.GetComponent<Image>().sprite=Circle();b.GetComponent<Image>().type=UnityEngine.UI.Image.Type.Simple;b.GetComponent<PrismPressMotion>().Shadow.GetComponent<Image>().sprite=Circle();b.GetComponent<PrismPressMotion>().Shadow.GetComponent<Image>().type=UnityEngine.UI.Image.Type.Simple;if(symbol.Length>0)Icon(b.transform,symbol,Vector2.zero,32,PrismTheme.Accent);return b;}
 PrismIcon Icon(Transform parent,string symbol,Vector2 pos,float size,Color color){var rt=Rect(symbol,parent);At(rt,.5f,.5f,pos.x,pos.y,size,size);var icon=rt.gameObject.AddComponent<PrismIcon>();icon.Symbol=symbol;icon.color=color;icon.raycastTarget=false;return icon;}
 TextMeshProUGUI Label(Transform parent,string value,float size,Color color,Vector2 pos,Vector2 bounds,Vector2 anchor,bool bold=false,TextAlignmentOptions alignment=TextAlignmentOptions.Center){var rt=Rect("Text",parent);At(rt,anchor.x,anchor.y,pos.x,pos.y,bounds.x,bounds.y);var text=rt.gameObject.AddComponent<TextMeshProUGUI>();text.font=PrismTheme.Font(bold);text.text=value;text.fontSize=Mathf.Max(12,size);text.color=color;text.alignment=alignment;text.raycastTarget=false;text.overflowMode=TextOverflowModes.Ellipsis;text.textWrappingMode=TextWrappingModes.NoWrap;return text;}
 static RectTransform Rect(string name,Transform parent){var obj=new GameObject(name,typeof(RectTransform));obj.transform.SetParent(parent,false);return obj.GetComponent<RectTransform>();}
 static Image Image(Transform parent,string name,Color color){var rt=Rect(name,parent);var img=rt.gameObject.AddComponent<Image>();img.color=color;img.raycastTarget=false;return img;}
 static void At(RectTransform rt,float ax,float ay,float x,float y,float w,float h){rt.anchorMin=rt.anchorMax=new Vector2(ax,ay);rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=new Vector2(x,y);rt.sizeDelta=new Vector2(w,h);}
 static void Stretch(RectTransform rt){rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;}
 static Color Alpha(Color color,float alpha){color.a=alpha;return color;}
 static void AddEvent(EventTrigger target,EventTriggerType type,Action<BaseEventData> action){var entry=new EventTrigger.Entry{eventID=type};entry.callback.AddListener(data=>action(data));target.triggers.Add(entry);}
 static Sprite circle,rounded;
 static Sprite Circle(){if(circle)return circle;circle=Shape(false);return circle;}
 static Sprite Rounded(){if(rounded)return rounded;rounded=Shape(true);return rounded;}
 static Sprite Shape(bool roundRect){const int n=64;var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);tex.filterMode=FilterMode.Bilinear;tex.wrapMode=TextureWrapMode.Clamp;var pixels=new Color[n*n];for(int y=0;y<n;y++)for(int x=0;x<n;x++){float dx=Mathf.Abs(x-31.5f),dy=Mathf.Abs(y-31.5f);float d=roundRect?new Vector2(Mathf.Max(0,dx-18),Mathf.Max(0,dy-18)).magnitude-13:Mathf.Sqrt(dx*dx+dy*dy)-30;pixels[y*n+x]=new Color(1,1,1,Mathf.Clamp01(.5f-d));}tex.SetPixels(pixels);tex.Apply(false,true);return Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,roundRect?new Vector4(20,20,20,20):Vector4.zero);}
 void FadeIn(RectTransform target){if(VisualEnvironment.ReducedMotion)return;StartCoroutine(Fade(target));}
 IEnumerator Fade(RectTransform target){var group=target.GetComponent<CanvasGroup>()??target.gameObject.AddComponent<CanvasGroup>();float t=0;while(target&&t<.24f){t+=Time.unscaledDeltaTime;group.alpha=Mathf.Clamp01(t/.24f);yield return null;}if(target)group.alpha=1;}
}

public sealed class PrismPressMotion : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
 public RectTransform Shadow;
 RectTransform face;Button button;Vector2 rest;bool pressed;float depth;
 void Awake(){face=GetComponent<RectTransform>();button=GetComponent<Button>();rest=face.anchoredPosition;}
 public void OnPointerDown(PointerEventData data){if(button&&button.interactable)pressed=true;}
 public void OnPointerUp(PointerEventData data){pressed=false;}
 public void OnPointerExit(PointerEventData data){pressed=false;}
 void Update(){if(!button||!button.interactable)pressed=false;float target=pressed?3:0;depth=VisualEnvironment.ReducedMotion?target:Mathf.Lerp(depth,target,1-Mathf.Exp(-Time.unscaledDeltaTime*(pressed?32:20)));face.anchoredPosition=rest-Vector2.up*depth;if(Shadow){Shadow.anchorMin=face.anchorMin;Shadow.anchorMax=face.anchorMax;Shadow.pivot=face.pivot;Shadow.sizeDelta=face.sizeDelta;Shadow.anchoredPosition=rest-Vector2.up*5;}}
 void OnEnable(){if(Shadow)Shadow.gameObject.SetActive(true);}
 void OnDisable(){pressed=false;depth=0;if(face)face.anchoredPosition=rest;if(Shadow)Shadow.gameObject.SetActive(false);}
 void OnDestroy(){if(Shadow)Destroy(Shadow.gameObject);}
}

public sealed class PrismNodePulse : MonoBehaviour {
 Image image;Color baseColor;
 void Start(){image=GetComponent<Image>();baseColor=image.color;}
 void Update(){if(!image)return;Color next=baseColor;if(!VisualEnvironment.ReducedMotion)next.a=baseColor.a*(.88f+.12f*Mathf.Sin(Time.unscaledTime*1.5f));image.color=next;}
}
}


