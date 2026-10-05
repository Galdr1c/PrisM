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
 int chapter, hintStage, hintLevel=-1;
 bool draggingTray,completionWaiting,tutorialReplay,interactionSeen,introPlayed,mapLevels;
 float lastInteraction;
 Coroutine reveal;
 Rect lastSafe;int lastWidth,lastHeight;
 TextMeshProUGUI levelTitle,goalStatus,tutorial,feedback;
 RectTransform selectedTools,tutorialFinger;
 RectTransform removeControl;
 readonly List<GameObject> precisionControls=new List<GameObject>();
 readonly Dictionary<Kind,TextMeshProUGUI> counts=new Dictionary<Kind,TextMeshProUGUI>();
 readonly Dictionary<Kind,Button> stockButtons=new Dictionary<Kind,Button>();
 readonly List<Image> goalDots=new List<Image>();
 Coroutine completion;
 float feedbackUntil;
 public bool BlocksBoardInput=>view!=ScreenView.Gameplay||sheet.Length>0||completionWaiting||draggingTray;
 public bool IsGameplay=>view==ScreenView.Gameplay;

 public void Initialize(PrismGame controller){
  game=controller;
  PrismTheme.Font(false);PrismTheme.Font(true);
  var root=new GameObject("Prism · Presentation",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
  root.transform.SetParent(transform,false);canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=20;
  var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(360,800);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=0;
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
  if(levelTitle)levelTitle.text=(game.CurrentIndex+1).ToString("00")+" · "+game.CurrentLevel.Name;
  foreach(var entry in counts){int n=game.Remaining(entry.Key);entry.Value.text="×"+Mathf.Max(0,n);stockButtons[entry.Key].interactable=n>0;}
  var result=game.CurrentResult;int lit=0;
  for(int i=0;i<goalDots.Count;i++){bool on=result!=null&&i<result.Energy.Length&&result.Energy[i]>=game.CurrentLevel.Goals[i].Threshold;goalDots[i].color=on?PrismTheme.Success:Alpha(PrismTheme.Ivory,.22f);if(on)lit++;}
  if(goalStatus)goalStatus.text=lit+" / "+goalDots.Count;
  if(selectedTools)selectedTools.gameObject.SetActive(game.SelectedIndex>=0&&!game.HasWon&&sheet.Length==0&&!completionWaiting);
  foreach(var control in precisionControls)if(control)control.SetActive(VisualEnvironment.PrecisionMode);
  if(removeControl)removeControl.anchoredPosition=new Vector2(VisualEnvironment.PrecisionMode?72:0,0);
  bool learning=game.CurrentIndex<3&&(tutorialReplay||!game.IsLevelComplete(game.CurrentIndex))&&sheet.Length==0&&!completionWaiting;
  if(tutorial){bool visible=learning&&Time.unscaledTime-lastInteraction>=8;tutorial.gameObject.SetActive(visible);if(visible){tutorial.text=game.CurrentIndex==0?game.CurrentPieces.Count==0?"Aynayı ışığın yoluna sürükle":"Aynaya dokun. Açısını değiştir.":game.CurrentIndex==1?"Açıyı değiştir, ışığın yolunu izle.":"Bir hareketi geri almak için ok simgesine dokun.";}}
  if(tutorialFinger){bool visible=learning&&!interactionSeen;tutorialFinger.gameObject.SetActive(visible);if(visible&&!VisualEnvironment.ReducedMotion){float t=Mathf.PingPong(Time.unscaledTime*.38f,1f);tutorialFinger.anchoredPosition=new Vector2(Mathf.Lerp(-90,-20,t),Mathf.Lerp(130,260,t));tutorialFinger.GetComponent<CanvasGroup>().alpha=Mathf.Sin(t*Mathf.PI)*.55f;}}
  if(feedback&&Time.unscaledTime>feedbackUntil)feedback.text="";
 }
 void ApplySafeArea(){
  lastSafe=UnityEngine.Screen.safeArea;lastWidth=UnityEngine.Screen.width;lastHeight=UnityEngine.Screen.height;
  if(lastWidth<=0||lastHeight<=0)return;
  safe.anchorMin=new Vector2(lastSafe.xMin/lastWidth,lastSafe.yMin/lastHeight);safe.anchorMax=new Vector2(lastSafe.xMax/lastWidth,lastSafe.yMax/lastHeight);safe.offsetMin=safe.offsetMax=Vector2.zero;
 }
 void Clear(RectTransform parent){foreach(Transform child in parent)Destroy(child.gameObject);}
 void ClearSheet(){Clear(overlay);sheet="";VisualEnvironment.SetPaused(false);}
 void ChangeScreen(ScreenView next){
  draggingTray=false;game.CancelInteraction();
  if(reveal!=null){StopCoroutine(reveal);reveal=null;}if(screen){var existingGroup=screen.GetComponent<CanvasGroup>();if(existingGroup)existingGroup.alpha=1;}
  if(completion!=null){StopCoroutine(completion);completion=null;}completionWaiting=false;ClearSheet();Clear(screen);view=next;counts.Clear();stockButtons.Clear();goalDots.Clear();selectedTools=null;tutorial=null;tutorialFinger=null;levelTitle=null;goalStatus=null;
 }
 public void ShowHome(){
  tutorialReplay=false;
  ChangeScreen(ScreenView.Home);Backdrop(screen);
  var symbol=Icon(screen,"prism",new Vector2(0,100),144,PrismTheme.Ivory);symbol.rectTransform.anchorMin=symbol.rectTransform.anchorMax=new Vector2(.5f,.65f);
  if(PrismTheme.Glyph){var glyph=Image(screen,"Optical mark",PrismTheme.Ivory);glyph.sprite=PrismTheme.Glyph;glyph.preserveAspect=true;At(glyph.rectTransform,.5f,.65f,0,100,152,152);symbol.gameObject.SetActive(false);}
  Label(screen,"PRISM",32,PrismTheme.Ivory,new Vector2(0,-55),new Vector2(310,48),new Vector2(.5f,.52f),true);
  Label(screen,"Işığın yolunu bul.",16,PrismTheme.Muted,new Vector2(0,-100),new Vector2(310,30),new Vector2(.5f,.52f));
  Primary(screen,game.CompletedCount==0&&game.CurrentIndex==0?"Başla":"Devam et",new Vector2(0,180),()=>game.ContinueGame());
  Label(screen,"Bölüm "+(game.CurrentIndex+1)+" · "+game.CurrentLevel.Name,12,PrismTheme.Muted,new Vector2(0,136),new Vector2(310,34),new Vector2(.5f,0));
  TextButton(screen,"Bölümler",new Vector2(-76,81),new Vector2(128,48),()=>ShowMap());TextButton(screen,"Ayarlar",new Vector2(76,81),new Vector2(128,48),ShowSettings);
  Label(screen,"Hesapsız. Reklamsız. Kendi ritminde.",10,Alpha(PrismTheme.Muted,.65f),new Vector2(0,26),new Vector2(320,20),new Vector2(.5f,0));
  FadeIn(screen);
  if(!introPlayed){introPlayed=true;reveal=StartCoroutine(BrandReveal(false));}
 }
 public void ShowGameplay(){
  ChangeScreen(ScreenView.Gameplay);
  interactionSeen=false;lastInteraction=Time.unscaledTime;
  IconButton(screen,"map",new Vector2(30,-31),new Vector2(0,1),ShowMap,"Bölümler");
  IconButton(screen,"pause",new Vector2(-30,-31),new Vector2(1,1),ShowPause,"Duraklat");
  levelTitle=Label(screen,"",15,PrismTheme.Ivory,new Vector2(0,-30),new Vector2(234,40),new Vector2(.5f,1),true);levelTitle.enableAutoSizing=true;levelTitle.fontSizeMin=11;levelTitle.fontSizeMax=15;
  int goals=game.CurrentLevel.Goals.Length;float total=goals*13;
  for(int i=0;i<goals;i++){var dot=Image(screen,"Hedef "+(i+1),Alpha(PrismTheme.Ivory,.25f));dot.sprite=Circle();At(dot.rectTransform,.5f,1,-total*.5f+i*13+6,-61,5,5);goalDots.Add(dot);}
  goalStatus=Label(screen,"",9,PrismTheme.Muted,new Vector2(0,-78),new Vector2(180,16),new Vector2(.5f,1));
  var tray=Image(screen,"Parça tepsisi",Alpha(PrismTheme.Surface,.92f));tray.sprite=Rounded();tray.type=UnityEngine.UI.Image.Type.Sliced;At(tray.rectTransform,.5f,0,0,73,Mathf.Min(316,CountKinds()*50+20),68);tray.raycastTarget=true;
  var kinds=new List<Kind>();foreach(var k in game.CurrentLevel.Stock)if(!kinds.Contains(k))kinds.Add(k);
  for(int i=0;i<kinds.Count;i++){
   Kind kind=kinds[i];float x=(i-(kinds.Count-1)*.5f)*50;
   var button=IconButton(tray.transform,Glyph(kind),new Vector2(x,0),new Vector2(.5f,.5f),()=>game.ArmPiece(kind),PieceInfo.Name(kind));button.GetComponent<RectTransform>().sizeDelta=new Vector2(48,62);
   var count=Label(button.transform,"",10,PrismTheme.Muted,new Vector2(13,-19),new Vector2(25,16),new Vector2(.5f,.5f));counts[kind]=count;stockButtons[kind]=button;
   var trigger=button.gameObject.AddComponent<EventTrigger>();
   AddEvent(trigger,EventTriggerType.BeginDrag,e=>{if(game.Remaining(kind)<=0)return;draggingTray=true;game.StartTrayDrag(kind,((PointerEventData)e).position);});
   AddEvent(trigger,EventTriggerType.Drag,e=>{if(draggingTray)game.UpdateTrayDrag(((PointerEventData)e).position);});
   AddEvent(trigger,EventTriggerType.EndDrag,e=>{if(!draggingTray)return;game.EndTrayDrag(((PointerEventData)e).position);draggingTray=false;});
  }
  IconButton(screen,"undo",new Vector2(32,25),new Vector2(0,0),()=>game.UndoAction(),"Geri al");
  IconButton(screen,"hint",new Vector2(0,25),new Vector2(.5f,0),ShowHint,"İpucu");
  IconButton(screen,"reset",new Vector2(-32,25),new Vector2(1,0),ShowRestart,"Yeniden başlat");
  selectedTools=Rect("Seçili parça",screen);At(selectedTools,.5f,0,0,151,214,42);
  precisionControls.Clear();
  precisionControls.Add(TextButton(selectedTools,"−1°",new Vector2(-72,0),new Vector2(62,44),()=>game.RotateSelected(-1),new Vector2(.5f,.5f)).gameObject);
  precisionControls.Add(TextButton(selectedTools,"+1°",new Vector2(0,0),new Vector2(62,44),()=>game.RotateSelected(1),new Vector2(.5f,.5f)).gameObject);
  removeControl=IconButton(selectedTools,"trash",new Vector2(72,0),new Vector2(.5f,.5f),()=>game.RemoveSelected(),"Parçayı kaldır").GetComponent<RectTransform>();
  tutorial=Label(screen,"",12,PrismTheme.Muted,new Vector2(0,121),new Vector2(322,30),new Vector2(.5f,0));
  tutorialFinger=Rect("Hayalet dokunuş",screen);At(tutorialFinger,.5f,0,-90,130,25,25);tutorialFinger.gameObject.AddComponent<CanvasGroup>();var finger=Image(tutorialFinger,"Dokunuş",Alpha(PrismTheme.Ivory,.55f));finger.sprite=Circle();Stretch(finger.rectTransform);
  feedback=Label(screen,"",12,PrismTheme.Ivory,new Vector2(0,196),new Vector2(310,42),new Vector2(.5f,0));
  if(!game.IsAutomatedRun)foreach(var kind in kinds){string key="prism.discovered."+kind;if(PlayerPrefs.GetInt(key,0)!=0)continue;PlayerPrefs.SetInt(key,1);PlayerPrefs.Save();StartCoroutine(Discovery(kind));break;}
 }
 public void ShowMap(){
  ChangeScreen(ScreenView.Map);chapter=Mathf.Clamp(game.CurrentIndex/10,0,9);DrawChapterMap();
 }
 void DrawChapterMap(){
  mapLevels=false;
  Clear(screen);Backdrop(screen);IconButton(screen,"back",new Vector2(30,-31),new Vector2(0,1),ShowHome,"Ana menü");Label(screen,"Işık yolları",20,PrismTheme.Ivory,new Vector2(0,-31),new Vector2(250,44),new Vector2(.5f,1),true);
  Label(screen,game.CompletedCount+" / "+game.CampaignLevels.Length+" keşfedildi",11,PrismTheme.Muted,new Vector2(0,-67),new Vector2(260,24),new Vector2(.5f,1));
  var scroll=Scroll(screen,"Chapter takımyıldızı",new Vector2(16,30),new Vector2(-16,-104),false);var content=scroll.content;content.sizeDelta=new Vector2(0,1180);
  string[] roman={"I","II","III","IV","V","VI","VII","VIII","IX","X"};string[] glyphs={"mirror","prism","green","lens","prism","mirror","sphere","prism","mirror","prism"};
  for(int c=0;c<10;c++){
   int selected=c,index=c*10;float x=c%2==0?-65:65,y=-68-c*112;bool open=game.IsLevelUnlocked(index);int done=0;for(int n=0;n<10;n++)if(game.IsLevelComplete(index+n))done++;
   if(c<9){Vector2 delta=new Vector2(-x*2,-112);var link=Image(content,"Chapter ışık yolu",Alpha(done==10?PrismTheme.Accent:PrismTheme.Muted,done==10? .38f: .12f));At(link.rectTransform,.5f,1,x+delta.x*.5f,y+delta.y*.5f,delta.magnitude,1);link.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);}
   var node=CircleButton(content,glyphs[c],new Vector2(x,y),62,()=>{chapter=selected;DrawLevelMap();});node.interactable=open;node.GetComponent<Image>().color=done==10?Alpha(PrismTheme.Success,.2f):c==chapter?Alpha(PrismTheme.Accent,.23f):Alpha(PrismTheme.Surface,.75f);
   if(c==chapter&&open)node.gameObject.AddComponent<PrismNodePulse>();
   var optical=node.GetComponentInChildren<PrismIcon>();if(optical)optical.color=open?PrismTheme.Ivory:Alpha(PrismTheme.Muted,.3f);
   Label(content,roman[c]+" · "+game.CampaignLevels[index].Chapter,13,open?PrismTheme.Ivory:Alpha(PrismTheme.Muted,.3f),new Vector2(x,y-43),new Vector2(178,25),new Vector2(.5f,1),true);
   Label(content,open?done+" / 10":"Henüz keşfedilmedi",10,Alpha(PrismTheme.Muted,open? .85f: .3f),new Vector2(x,y-67),new Vector2(180,20),new Vector2(.5f,1));
  }
  scroll.verticalNormalizedPosition=1;
 }
 void DrawLevelMap(){
  mapLevels=true;
  Clear(screen);Backdrop(screen);IconButton(screen,"back",new Vector2(30,-31),new Vector2(0,1),DrawChapterMap,"Chapter yolu");Label(screen,game.CampaignLevels[chapter*10].Chapter,20,PrismTheme.Ivory,new Vector2(0,-31),new Vector2(250,44),new Vector2(.5f,1),true);
  Label(screen,"10 küçük keşif",11,PrismTheme.Muted,new Vector2(0,-67),new Vector2(260,24),new Vector2(.5f,1));
  var scroll=Scroll(screen,"Takımyıldız",new Vector2(16,58),new Vector2(-16,-104),false);var content=scroll.content;content.sizeDelta=new Vector2(0,940);
  for(int i=0;i<10;i++){
   int index=chapter*10+i;if(index>=game.CampaignLevels.Length)break;float x=(i%2==0?-49:49);float y=-60-i*85;
   if(i<9){var line=Image(content,"Işık bağlantısı",Alpha(game.IsLevelComplete(index)?PrismTheme.Accent:PrismTheme.Ivory,.15f));float nextX=i%2==0?49:-49;Vector2 delta=new Vector2(nextX-x,-85);At(line.rectTransform,.5f,1,x+delta.x*.5f,y+delta.y*.5f,delta.magnitude,1);line.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);}
   bool done=game.IsLevelComplete(index),open=game.IsLevelUnlocked(index);var node=CircleButton(content,done?"check":"",new Vector2(x,y),54,()=>game.OpenLevel(index));node.interactable=open;node.GetComponent<Image>().color=done?Alpha(PrismTheme.Success,.16f):index==game.CurrentIndex?Alpha(PrismTheme.Accent,.23f):Alpha(PrismTheme.Surface,open? .9f: .42f);
   if(index==game.CurrentIndex&&open)node.gameObject.AddComponent<PrismNodePulse>();
   if(!done)Label(node.transform,(index+1).ToString("00"),16,open?PrismTheme.Ivory:Alpha(PrismTheme.Muted,.35f),Vector2.zero,new Vector2(52,36),new Vector2(.5f,.5f),true);
   Label(content,game.CampaignLevels[index].Name,11,open?PrismTheme.Muted:Alpha(PrismTheme.Muted,.35f),new Vector2(x,y-38),new Vector2(178,24),new Vector2(.5f,1));
  }
  TextButton(screen,"Chapter yoluna dön",new Vector2(0,28),new Vector2(210,48),DrawChapterMap);
 }
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
  string[] help={game.CurrentLevel.Hint,"Doğru bölgeyi görmek için ışığın izini takip et.","Hayalet yerleşim yönü, sonraki adımı gösterir."};
  var text=Label(body,help[Mathf.Clamp(hintStage,0,2)],14,PrismTheme.Ivory,new Vector2(0,179),new Vector2(298,66),new Vector2(.5f,0));text.textWrappingMode=TextWrappingModes.Normal;
  Label(body,(hintStage+1)+" / 3 · Kendi ritminde keşfet",11,PrismTheme.Muted,new Vector2(0,126),new Vector2(300,24),new Vector2(.5f,0));
  Primary(body,hintStage==0?"Bölgeyi göster":hintStage==1?"Yönü göster":"Işık izini izle",new Vector2(0,77),()=>{hintStage=Mathf.Min(2,hintStage+1);game.RequestHint(hintStage+1);ClearSheet();});
  TextButton(body,"Oyuna dön",new Vector2(0,24),new Vector2(290,44),ClearSheet);
 }
 public void ShowSettings(){
  var body=Sheet("settings","Ayarlar",Mathf.Min(650,SafeHeight()-45));
  var scroll=Scroll(body,"Ayar satırları",new Vector2(18,15),new Vector2(-18,-76),false);var content=scroll.content;content.sizeDelta=new Vector2(0,898);float y=-15;
  Group(content,"SES",ref y);
  ToggleRow(content,"Müzik",game.Feedback.MusicEnabled,game.Feedback.SetMusic,ref y);
  ToggleRow(content,"Ses efektleri",game.Feedback.AudioEnabled,game.Feedback.SetAudio,ref y);
  ToggleRow(content,"Titreşim",game.Feedback.HapticsEnabled,game.Feedback.SetHaptics,ref y);
  Group(content,"OYUN",ref y);
  ToggleRow(content,"Hassas dönüş",VisualEnvironment.PrecisionMode,game.SetPrecisionMode,ref y);
  Row(content,"Öğreticiyi tekrar izle",()=>{ClearSheet();tutorialReplay=true;game.OpenLevel(0);},ref y,"Göster");
  Group(content,"GÖRÜNTÜ",ref y);
  Row(content,"Kalite",()=>{VisualEnvironment.NextQuality();ShowSettings();},ref y,VisualEnvironment.QualityLabel);
  SliderRow(content,"Işın yoğunluğu",VisualEnvironment.BeamScale,.5f,1.5f,game.SetBeamIntensity,ref y);
  SliderRow(content,"Parlama",VisualEnvironment.BloomScale,0,1.5f,game.SetBloomIntensity,ref y);
  Group(content,"ERİŞİLEBİLİRLİK",ref y);
  ToggleRow(content,"Renk destek sembolleri",VisualEnvironment.ColorSymbols,game.SetColorSymbols,ref y);
  ToggleRow(content,"Azaltılmış hareket",VisualEnvironment.ReducedMotion,game.SetReducedMotion,ref y);
  ToggleRow(content,"Yüksek kontrast",VisualEnvironment.HighContrast,game.SetHighContrast,ref y);
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
  if(PrismTheme.Glyph){var glyph=Image(screen,"Son ışık",PrismTheme.Ivory);glyph.sprite=PrismTheme.Glyph;glyph.preserveAspect=true;At(glyph.rectTransform,.5f,.65f,0,50,144,144);}else Icon(screen,"prism",new Vector2(0,130),144,PrismTheme.Ivory);
  Label(screen,"100 ışık yolu."+Environment.NewLine+"Tek bir başlangıç.",27,PrismTheme.Ivory,new Vector2(0,-65),new Vector2(320,100),new Vector2(.5f,.5f),true);
  Label(screen,"PRISM",18,PrismTheme.Muted,new Vector2(0,-157),new Vector2(290,40),new Vector2(.5f,.5f));
  Primary(screen,"Bölümlere dön",new Vector2(0,136),ShowMap);TextButton(screen,"Emeği geçenler",new Vector2(0,73),new Vector2(280,48),ShowAbout);reveal=StartCoroutine(BrandReveal(true));
 }
 IEnumerator BrandReveal(bool final){
  var cover=Image(overlay,final?"Son ışık sekansı":"İlk ışık",PrismTheme.Background);Stretch(cover.rectTransform);cover.raycastTarget=true;
  var beam=Image(cover.transform,"Gelen ışık",PrismTheme.Ivory);At(beam.rectTransform,.5f,.55f,-150,40,0,2);
  var mark=Image(cover.transform,"Kırılma",PrismTheme.Ivory);mark.sprite=PrismTheme.Glyph;mark.preserveAspect=true;At(mark.rectTransform,.5f,.55f,0,40,132,132);var markGroup=mark.gameObject.AddComponent<CanvasGroup>();markGroup.alpha=0;
  if(!mark.sprite){mark.color=Color.clear;Icon(mark.transform,"prism",Vector2.zero,120,PrismTheme.Ivory);}
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
 public void HandleBack(){if(completionWaiting)return;if(sheet.Length>0){ClearSheet();return;}switch(view){case ScreenView.Gameplay:ShowPause();break;case ScreenView.Map:if(mapLevels)DrawChapterMap();else ShowHome();break;case ScreenView.Final:ShowHome();break;case ScreenView.Home:ShowSettings();break;}}
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
 void Group(Transform parent,string label,ref float y){Label(parent,label,10,PrismTheme.Accent,new Vector2(0,y),new Vector2(290,28),new Vector2(.5f,1),true,TextAlignmentOptions.Left);y-=32;}
 void ToggleRow(Transform parent,string label,bool value,Action<bool> set,ref float y){
  float rowY=y;var row=TextButton(parent,"",new Vector2(0,rowY),new Vector2(296,48),()=>{},new Vector2(.5f,1));Label(row.transform,label,13,PrismTheme.Ivory,new Vector2(-25,0),new Vector2(230,44),new Vector2(.5f,.5f),false,TextAlignmentOptions.Left);
  var track=Image(row.transform,"Anahtar",value?Alpha(PrismTheme.Accent,.65f):Alpha(PrismTheme.Muted,.2f));track.sprite=Rounded();track.type=UnityEngine.UI.Image.Type.Sliced;At(track.rectTransform,1,.5f,-24,0,36,20);
  var dot=Image(track.transform,"Durum",PrismTheme.Ivory);dot.sprite=Circle();At(dot.rectTransform,.5f,.5f,value?8:-8,0,14,14);
  bool current=value;row.onClick.RemoveAllListeners();row.onClick.AddListener(()=>{current=!current;set(current);track.color=current?Alpha(PrismTheme.Accent,.65f):Alpha(PrismTheme.Muted,.2f);dot.rectTransform.anchoredPosition=new Vector2(current?8:-8,0);Click();});y-=49;
 }
 void Row(Transform parent,string label,Action action,ref float y,string value){var row=TextButton(parent,"",new Vector2(0,y),new Vector2(296,48),action,new Vector2(.5f,1));Label(row.transform,label,13,PrismTheme.Ivory,new Vector2(-45,0),new Vector2(190,44),new Vector2(.5f,.5f),false,TextAlignmentOptions.Left);Label(row.transform,value,11,PrismTheme.Muted,new Vector2(90,0),new Vector2(104,44),new Vector2(.5f,.5f),false,TextAlignmentOptions.Right);y-=49;}
 void SliderRow(Transform parent,string label,float value,float min,float max,Action<float> set,ref float y){
  Label(parent,label,13,PrismTheme.Ivory,new Vector2(0,y),new Vector2(286,28),new Vector2(.5f,1),false,TextAlignmentOptions.Left);y-=30;
  var rt=Rect(label,parent);At(rt,.5f,1,0,y,286,32);var slider=rt.gameObject.AddComponent<Slider>();slider.minValue=min;slider.maxValue=max;
  var hit=Image(rt,"Touch",Color.clear);Stretch(hit.rectTransform);hit.raycastTarget=true;
  var track=Image(rt,"Piste",Alpha(PrismTheme.Muted,.25f));At(track.rectTransform,.5f,.5f,0,0,286,3);
  var fillArea=Rect("Progression",rt);Stretch(fillArea);fillArea.offsetMin=new Vector2(7,0);fillArea.offsetMax=new Vector2(-7,0);
  var fill=Image(fillArea,"Intensité",PrismTheme.Accent);Stretch(fill.rectTransform);fill.rectTransform.offsetMin=new Vector2(0,14);fill.rectTransform.offsetMax=new Vector2(0,-14);slider.fillRect=fill.rectTransform;
   var handleArea=Rect("Curseur",rt);handleArea.anchorMin=new Vector2(0,.5f);handleArea.anchorMax=new Vector2(1,.5f);handleArea.offsetMin=new Vector2(7,0);handleArea.offsetMax=new Vector2(-7,0);
  var handle=Image(handleArea,"Point",PrismTheme.Ivory);handle.sprite=Circle();At(handle.rectTransform,.5f,.5f,0,0,16,16);slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.value=value;slider.onValueChanged.AddListener(v=>set(v));y-=36;
 }
 ScrollRect Scroll(Transform parent,string name,Vector2 bottomLeft,Vector2 topRight,bool horizontal){
  var rt=Rect(name,parent);Stretch(rt);rt.offsetMin=bottomLeft;rt.offsetMax=topRight;
  if(horizontal){rt.anchorMin=new Vector2(0,1);rt.anchorMax=new Vector2(1,1);rt.offsetMin=new Vector2(bottomLeft.x,-145);rt.offsetMax=new Vector2(topRight.x,-91);}
  var viewport=Rect("Viewport",rt);Stretch(viewport);viewport.gameObject.AddComponent<RectMask2D>();var hit=Image(viewport,"Touch",Color.clear);Stretch(hit.rectTransform);hit.raycastTarget=true;
  var content=Rect("Content",viewport);content.anchorMin=horizontal?new Vector2(0,0):new Vector2(0,1);content.anchorMax=horizontal?new Vector2(0,1):new Vector2(1,1);content.pivot=horizontal?new Vector2(0,.5f):new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;
  var scroll=rt.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=horizontal;scroll.vertical=!horizontal;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.decelerationRate=.08f;scroll.scrollSensitivity=28;return scroll;
 }
 void Backdrop(Transform parent){var bg=Image(parent,"Gece",PrismTheme.Background);Stretch(bg.rectTransform);bg.raycastTarget=true;for(int i=0;i<18;i++){var star=Image(parent,"Yıldız",Alpha(PrismTheme.Muted,.1f+i%3*.035f));star.sprite=Circle();At(star.rectTransform,(i*37%97)/100f,(i*53%89)/100f,0,0,2,2);}var beam=Image(parent,"Beyaz ışık",Alpha(PrismTheme.Ivory,.16f));At(beam.rectTransform,.5f,.65f,-105,100,140,1);beam.rectTransform.localRotation=Quaternion.Euler(0,0,-10);}
 int CountKinds(){var set=new HashSet<Kind>(game.CurrentLevel.Stock);return set.Count;}
 static string Glyph(Kind kind)=>kind.ToString().ToLowerInvariant();
 void Click(){if(game.Feedback)game.Feedback.Click();}
 Button Primary(Transform parent,string text,Vector2 pos,Action action){var button=TextButton(parent,text,pos,new Vector2(292,56),action);var img=button.GetComponent<Image>();img.color=PrismTheme.Ivory;img.sprite=Rounded();img.type=UnityEngine.UI.Image.Type.Sliced;var label=button.GetComponentInChildren<TextMeshProUGUI>();label.color=PrismTheme.Background;label.font=PrismTheme.Font(true);label.fontSize=17;return button;}
 Button TextButton(Transform parent,string text,Vector2 pos,Vector2 size,Action action,Vector2? anchor=null){var image=Image(parent,text.Length==0?"Control":text,new Color(1,1,1,.001f));At(image.rectTransform,anchor?.x?? .5f,anchor?.y??0,pos.x,pos.y,size.x,size.y);image.raycastTarget=true;var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;var colors=b.colors;colors.highlightedColor=new Color(.84f,.8f,1,1);colors.pressedColor=new Color(.6f,.55f,.8f,1);colors.disabledColor=new Color(1,1,1,.32f);b.colors=colors;b.onClick.AddListener(()=>{Click();action();});Label(b.transform,text,14,PrismTheme.Ivory,Vector2.zero,size,new Vector2(.5f,.5f));b.gameObject.AddComponent<PrismPressMotion>();return b;}
 Button IconButton(Transform parent,string symbol,Vector2 pos,Vector2 anchor,Action action,string accessible){var b=TextButton(parent,"",pos,new Vector2(48,48),action,anchor);b.name=accessible;Icon(b.transform,symbol,Vector2.zero,30,PrismTheme.Muted);return b;}
 Button CircleButton(Transform parent,string symbol,Vector2 pos,float size,Action action){var b=TextButton(parent,"",pos,new Vector2(size,size),action,new Vector2(.5f,1));b.GetComponent<Image>().sprite=Circle();if(symbol.Length>0)Icon(b.transform,symbol,Vector2.zero,32,PrismTheme.Success);return b;}
 PrismIcon Icon(Transform parent,string symbol,Vector2 pos,float size,Color color){var rt=Rect(symbol,parent);At(rt,.5f,.5f,pos.x,pos.y,size,size);var icon=rt.gameObject.AddComponent<PrismIcon>();icon.Symbol=symbol;icon.color=color;icon.raycastTarget=false;return icon;}
 TextMeshProUGUI Label(Transform parent,string value,float size,Color color,Vector2 pos,Vector2 bounds,Vector2 anchor,bool bold=false,TextAlignmentOptions alignment=TextAlignmentOptions.Center){var rt=Rect("Text",parent);At(rt,anchor.x,anchor.y,pos.x,pos.y,bounds.x,bounds.y);var text=rt.gameObject.AddComponent<TextMeshProUGUI>();text.font=PrismTheme.Font(bold);text.text=value;text.fontSize=size;text.color=color;text.alignment=alignment;text.raycastTarget=false;text.overflowMode=TextOverflowModes.Ellipsis;text.textWrappingMode=TextWrappingModes.NoWrap;return text;}
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
 Vector3 target=Vector3.one;
 public void OnPointerDown(PointerEventData data){target=VisualEnvironment.ReducedMotion?Vector3.one:Vector3.one*.96f;}
 public void OnPointerUp(PointerEventData data){target=Vector3.one;}
 public void OnPointerExit(PointerEventData data){target=Vector3.one;}
 void Update(){transform.localScale=Vector3.Lerp(transform.localScale,target,1-Mathf.Exp(-Time.unscaledDeltaTime*24));}
}

public sealed class PrismNodePulse : MonoBehaviour {
 Image image;Color baseColor;
 void Start(){image=GetComponent<Image>();baseColor=image.color;}
 void Update(){if(!image)return;Color next=baseColor;if(!VisualEnvironment.ReducedMotion)next.a=baseColor.a*(.88f+.12f*Mathf.Sin(Time.unscaledTime*1.5f));image.color=next;}
}
}


