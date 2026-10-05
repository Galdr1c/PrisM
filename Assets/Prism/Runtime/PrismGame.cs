using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Prism {
public class PrismGame : MonoBehaviour {
 [Serializable] class ProgressData {
  public int version=3;
  public string lastLevelId="";
  public List<string> completedLevelIds=new List<string>();
 }

 Level[] levels;
 Session session;
 Result result;
 BoardRenderer board;
 Camera cam;
 PrismFeedback feedback;
 PrismPresentation presentation;
 Piece trayGhost;
 Kind? trayKind;
 bool transitionBusy,completionPresented;
 float dragStarted;
 int invalidPlacementCount;
 readonly List<UnityEngine.EventSystems.RaycastResult> uiHits=new List<UnityEngine.EventSystems.RaycastResult>();

 public Level[] CampaignLevels=>levels;
 public Level CurrentLevel=>session?.Level;
 public int CurrentIndex=>levelIndex;
 public IList<Piece> CurrentPieces=>session?.Pieces;
 public Result CurrentResult=>result;
 public int SelectedIndex=>selected;
 public PrismFeedback Feedback=>feedback;
 public int CompletedCount=>completedLevelIds.Count;
 public bool IsLevelUnlocked(int index)=>IsUnlocked(index);
 public bool IsLevelComplete(int index)=>index>=0&&index<levels.Length&&completedLevelIds.Contains(levels[index].Id);
 public int Remaining(Kind kind)=>session.Remaining(kind);
 public Kind? ArmedKind=>armed;
 public bool InteractionBusy=>transitionBusy;
 public bool IsAutomatedRun=>smoke||storeCapture;
 public bool HasWon=>won;
 public void RenderSolutionReveal(float fraction){board.SolutionReveal=Mathf.Clamp01(fraction);board.Draw(session.Level,session.Pieces,result,-1);}
 public void CancelInteraction(){
  if(dragging||rotating)session?.EndEdit();
  dragging=rotating=false;trayGhost=null;trayKind=null;armed=null;dirty=true;
 }

 public void ContinueGame(){presentation.ShowGameplay();}
 public void OpenLevel(int index){if(!IsUnlocked(index)){feedback?.Invalid();return;}Load(index);}
 public void AdvanceLevel(){if(levelIndex<levels.Length-1)OpenLevel(levelIndex+1);else presentation.ShowMap();}
 public void ArmPiece(Kind kind){if(session.Remaining(kind)<=0)return;armed=kind;selected=-1;dirty=true;feedback?.Click();}
 public void RotateSelected(double degrees){Rotate(degrees);}
 public void RemoveSelected(){if(selected<0||won||transitionBusy)return;session.Remove(selected);selected=-1;dirty=true;feedback?.Click();}
 public void RequestHint(int stage){board.ShowHint(Mathf.Clamp(stage,1,3),session.Level,session.Pieces);}
 public void SetReducedMotion(bool value){VisualEnvironment.SetReducedMotion(value);dirty=true;}
 public void SetHighContrast(bool value){VisualEnvironment.SetHighContrast(value);dirty=true;}
 public void SetColorSymbols(bool value){VisualEnvironment.SetColorSymbols(value);dirty=true;}
 public void SetPrecisionMode(bool value){VisualEnvironment.SetPrecisionMode(value);}
 public void SetBeamIntensity(float value){VisualEnvironment.SetBeamScale(value);dirty=true;}
 public void SetBloomIntensity(float value){VisualEnvironment.SetBloomScale(value);}

 public void StartTrayDrag(Kind kind,Vector2 screenPosition){
  if(session.Remaining(kind)<=0||won)return;
  trayKind=kind;armed=null;
  double angle=kind==Kind.Lens||kind==Kind.Prism?90:kind==Kind.Sphere?0:45;
  trayGhost=new Piece(kind,ScreenWorld(screenPosition),angle);
  UpdateTrayDrag(screenPosition);
 }
 public void UpdateTrayDrag(Vector2 screenPosition){
  if(trayGhost==null)return;
  trayGhost.Position=ScreenWorld(screenPosition);
  var preview=new List<Piece>(session.Pieces){trayGhost};
  board.Draw(session.Level,preview,Optics.Solve(session.Level,preview),preview.Count-1);
 }
 public void EndTrayDrag(Vector2 screenPosition){
  if(trayGhost==null)return;
  var ghost=trayGhost;ghost.Position=ScreenWorld(screenPosition);
  if(session.Place(ghost.Kind,ghost.Position)){
   selected=session.Pieces.Count-1;session.Pieces[selected].Angle=ghost.Angle;feedback?.Place(ghost.Kind);
  }else feedback?.Invalid();
  trayGhost=null;trayKind=null;dirty=true;
 }
 public void UndoAction(){if(!transitionBusy&&!won)StartCoroutine(AnimateUndo());}
 public void RestartLevel(){if(!transitionBusy)StartCoroutine(AnimateReset());}
 IEnumerator AnimateUndo(){
  transitionBusy=true;
  var before=new List<Piece>();foreach(var p in session.Pieces)before.Add(p.Copy());
  session.Undo();selected=-1;armed=null;settle=0;Solve();
  if(!VisualEnvironment.ReducedMotion&&before.Count==session.Pieces.Count){
   float start=Time.unscaledTime;
   while(Time.unscaledTime-start<.18f){
    float t=Mathf.SmoothStep(0,1,(Time.unscaledTime-start)/.18f);
    var visual=new List<Piece>();
    for(int i=0;i<before.Count;i++){
     var final=session.Pieces[i];
     visual.Add(new Piece(final.Kind,before[i].Position+(final.Position-before[i].Position)*t,
      before[i].Angle+Mathf.DeltaAngle((float)before[i].Angle,(float)final.Angle)*t));
    }
    board.Draw(session.Level,visual,result,-1);yield return null;
   }
  }
  transitionBusy=false;dirty=true;feedback?.Click();
 }
 IEnumerator AnimateReset(){
  transitionBusy=true;selected=-1;armed=null;
  var visual=new List<Piece>(session.Pieces);
  session.Reset();won=false;completionPresented=false;settle=0;Solve();
  if(!VisualEnvironment.ReducedMotion){
   while(visual.Count>0){visual.RemoveAt(visual.Count-1);board.Draw(session.Level,visual,result,-1);yield return new WaitForSecondsRealtime(.04f);}
  }
  dirty=true;transitionBusy=false;
  board.ClearHint();presentation.ShowGameplay();feedback?.Click();
 }
 ProgressData progress=new ProgressData();
 readonly HashSet<string> completedLevelIds=new HashSet<string>();

 int levelIndex,selected=-1,mapChapter=-1,previousLit;
 Kind? armed;
 bool dragging,rotating,showHint,showLevelMap,showSettings,won,showWinPanel,dirty=true,smoke,storeCapture,drawingOverlay;
 float settle,winShownAt=-10f,toastUntil;
 string toast="";
 Color toastColor;
 Vector2 dragOffset;
 double startAngle;
 V startDirection;
 float scale=1,offsetX,offsetY;
 readonly Color danger=new Color(.96f,.40f,.42f);

 string SavePath=>Path.Combine(Application.persistentDataPath,"progress.json");
 public static string PieceName(Kind k)=>PieceInfo.Name(k);

 void Start(){
  Application.targetFrameRate=60;
  Screen.sleepTimeout=SleepTimeout.NeverSleep;
  // Presentation typography is owned by the Canvas/TMP view layer.
  var commandLine=Environment.GetCommandLineArgs();
  smoke=Array.IndexOf(commandLine,"-prismSmoke")>=0;
  storeCapture=Array.IndexOf(commandLine,"-prismStoreCapture")>=0;
  if(smoke||storeCapture)Application.runInBackground=true;

  levels=LevelCatalogLoader.Load();
  if(levels==null||levels.Length==0)throw new Exception("PrisM has no playable levels.");
  LoadProgress();

  cam=Camera.main;
  if(!cam){cam=new GameObject("Camera").AddComponent<Camera>();cam.tag="MainCamera";}
  cam.orthographic=true;
  cam.transform.position=new Vector3(0,0,-10);
  cam.backgroundColor=new Color(.025f,.042f,.06f);
  cam.clearFlags=CameraClearFlags.SolidColor;
  VisualEnvironment.Configure(cam);

  feedback=gameObject.GetComponent<PrismFeedback>();
  if(feedback==null)feedback=gameObject.AddComponent<PrismFeedback>();

  board=new GameObject("Light laboratory").AddComponent<BoardRenderer>();
  Load(FindLevel(progress.lastLevelId),true);
  presentation=gameObject.AddComponent<PrismPresentation>();
  presentation.Initialize(this);
  if(smoke||storeCapture)presentation.ShowGameplay();else presentation.ShowHome();
  if(smoke)StartCoroutine(Smoke());
  else if(storeCapture)StartCoroutine(StoreCapture());
 }

 int FindLevel(string id){
  if(!string.IsNullOrEmpty(id))for(int i=0;i<levels.Length;i++)if(levels[i].Id==id)return i;
  return 0;
 }

 void LoadProgress(){
  progress=new ProgressData();
  completedLevelIds.Clear();
  foreach(var path in new[]{SavePath,SavePath+".tmp",SavePath+".bak"}){
   if(!File.Exists(path))continue;
   try{
    var loaded=JsonUtility.FromJson<ProgressData>(File.ReadAllText(path));
    if(loaded!=null){progress=loaded;break;}
   }catch(Exception e){Debug.LogWarning("Progress copy could not be loaded: "+e.Message);}
  }
  if(progress.completedLevelIds==null)progress.completedLevelIds=new List<string>();
  foreach(var id in progress.completedLevelIds)if(!string.IsNullOrEmpty(id))completedLevelIds.Add(id);

  bool migrated=false;
  if(PlayerPrefs.HasKey("prism.completed")){
   uint mask=unchecked((uint)PlayerPrefs.GetInt("prism.completed",0));
   for(int i=0;i<levels.Length&&i<32;i++)if((mask&(1u<<i))!=0)completedLevelIds.Add(levels[i].Id);
   migrated=true;
  }
  if(string.IsNullOrEmpty(progress.lastLevelId)&&PlayerPrefs.HasKey("prism.last")){
   int old=Mathf.Clamp(PlayerPrefs.GetInt("prism.last",0),0,levels.Length-1);
   progress.lastLevelId=levels[old].Id;
   migrated=true;
  }
  if(migrated&&!smoke&&!storeCapture){
   SaveProgress();
   PlayerPrefs.DeleteKey("prism.completed");
   PlayerPrefs.DeleteKey("prism.last");
   PlayerPrefs.Save();
  }
 }

 void SaveProgress(){
  if(smoke||storeCapture)return;
  progress.version=3;
  var ids=new List<string>(completedLevelIds);
  ids.Sort(StringComparer.Ordinal);
  progress.completedLevelIds=ids;
  try{
   Directory.CreateDirectory(Application.persistentDataPath);
   string temp=SavePath+".tmp";
   File.WriteAllText(temp,JsonUtility.ToJson(progress,true));
   if(File.Exists(SavePath))File.Replace(temp,SavePath,SavePath+".bak");
   else File.Move(temp,SavePath);
  }catch(Exception e){
   Debug.LogWarning("Progress could not be saved: "+e.Message);
  }
 }

 void OnApplicationPause(bool paused){if(paused){CancelInteraction();SaveProgress();}}
 void OnApplicationFocus(bool focused){if(!focused)CancelInteraction();}
 void OnApplicationQuit(){SaveProgress();}

 void Layout(){
  Rect safe=Screen.safeArea;
  if(safe.width<1||safe.height<1)safe=new Rect(0,0,Screen.width,Screen.height);
  float boardPixels=Mathf.Min(safe.width*.96f,safe.height*.76f);
  float pixelsPerUnit=boardPixels/10f;
  Vector2 center=new Vector2(safe.center.x,safe.yMin+safe.height*.53f);
  cam.orthographicSize=Screen.height/(pixelsPerUnit*2f);
  if(won&&!VisualEnvironment.ReducedMotion)cam.orthographicSize*=1f+.035f*Mathf.SmoothStep(0,1,(Time.unscaledTime-winShownAt)/.7f);
  cam.transform.position=new Vector3((Screen.width*.5f-center.x)/pixelsPerUnit,(Screen.height*.5f-center.y)/pixelsPerUnit,-10f);
 }

 int HighestUnlocked(){
  int high=0;
  for(int i=0;i<levels.Length;i++)if(completedLevelIds.Contains(levels[i].Id))high=Mathf.Max(high,i+1);
  return Mathf.Clamp(high,0,levels.Length-1);
 }

 bool IsUnlocked(int index){
  if(smoke||storeCapture)return true;
  if(index<0||index>=levels.Length)return false;
  return index<=HighestUnlocked()||completedLevelIds.Contains(levels[index].Id);
 }

 void Load(int index,bool force=false){
  index=Mathf.Clamp(index,0,levels.Length-1);
  if(!force&&!IsUnlocked(index)){feedback?.Invalid();Notify("Bu deney henüz kilitli.",danger);return;}
  levelIndex=index;
  session=new Session(levels[levelIndex]);
  previousLit=0;
  completionPresented=false;trayGhost=null;trayKind=null;dragging=rotating=false;
  board?.ClearHint();
  if(board)board.SolutionReveal=1f;
  selected=-1;armed=null;showHint=false;showLevelMap=false;showSettings=false;mapChapter=-1;won=false;showWinPanel=false;settle=0;winShownAt=-10f;toast="";toastUntil=0;dirty=false;
  Solve();
  board?.SetCelebration(0f);
  VisualEnvironment.SetCelebration(0f);
  if(!smoke&&!storeCapture){progress.lastLevelId=session.Level.Id;SaveProgress();}
  presentation?.ShowGameplay();
 }

 void Solve(){
  result=Optics.Solve(session.Level,session.Pieces,result);
  int lit=LitGoals();
  if(lit>previousLit&&!smoke&&!storeCapture)feedback?.Goal();
  previousLit=lit;
  board.Draw(session.Level,session.Pieces,result,selected);
  dirty=false;
 }

 void Update(){
  if(session==null)return;
  Layout();
  if(!smoke&&!storeCapture){
   HandleBack();
   Pointer();
  }
  if(dirty)Solve();

  bool complete=session.IsComplete(result);
  if(!transitionBusy&&!won&&complete){
   settle+=Time.deltaTime;
   if(settle>.58f){
    won=true;
    showWinPanel=false;
    winShownAt=Time.unscaledTime;
    bool milestone=(levelIndex+1)%10==0||levelIndex==levels.Length-1;
    if(!smoke&&!storeCapture){
     completedLevelIds.Add(session.Level.Id);
     SaveProgress();
     feedback?.Complete(milestone);
    }
   }
  }else if(!complete||transitionBusy)settle=0;

  float celebration=0f;
  if(won){
   float age=Time.unscaledTime-winShownAt;
   celebration=Mathf.Clamp01(1f-Mathf.Max(0f,age-1.2f)/2.4f);
   if(!completionPresented&&!smoke&&!storeCapture){completionPresented=true;presentation.ShowCompletion(levelIndex==levels.Length-1);}
  }
  VisualEnvironment.SetPaused(presentation!=null&&presentation.BlocksBoardInput&&!won);
  float lift=dragging?Mathf.Clamp01((Time.unscaledTime-dragStarted-.12f)/.12f)*.32f:0f;
  board?.SetInteraction(selected,dragging,rotating,lift);
  board?.SetCelebration(celebration);
  VisualEnvironment.SetCelebration(celebration);
 }

 Vector2 Design(Vector2 screen)=>new Vector2((screen.x-offsetX)/scale,(Screen.height-screen.y-offsetY)/scale);
 V World(Vector2 p)=>new V((p.x-450)/80,(560-p.y)/80);
 V ScreenWorld(Vector2 point){var world=cam.ScreenToWorldPoint(new Vector3(point.x,point.y,10));return new V(world.x,world.y);}

 void HandleBack(){
  if(Keyboard.current==null||!Keyboard.current.escapeKey.wasPressedThisFrame)return;
  if(presentation!=null){presentation.HandleBack();return;}
  if(showHint){showHint=false;return;}
  if(showSettings){showSettings=false;return;}
  if(showLevelMap){if(mapChapter>=0)mapChapter=-1;else showLevelMap=false;return;}
  if(won&&showWinPanel){showWinPanel=false;return;}
  if(won){showLevelMap=true;mapChapter=-1;return;}
  if(armed.HasValue){armed=null;return;}
  if(selected>=0){selected=-1;dirty=true;return;}
#if UNITY_ANDROID
  Application.Quit();
#else
  showLevelMap=true;
#endif
 }

 void Pointer(){
  if(showHint||showLevelMap||showSettings||won||transitionBusy||trayKind.HasValue||(presentation!=null&&presentation.BlocksBoardInput))return;
  Vector2 raw;
  bool down,held,up;

  if(Touchscreen.current!=null&&(Touchscreen.current.primaryTouch.press.isPressed||Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)){
   var t=Touchscreen.current.primaryTouch;
   raw=t.position.ReadValue();down=t.press.wasPressedThisFrame;held=t.press.isPressed;up=t.press.wasReleasedThisFrame;
  }else if(Mouse.current!=null){
   var m=Mouse.current;
   raw=m.position.ReadValue();down=m.leftButton.wasPressedThisFrame;held=m.leftButton.isPressed;up=m.leftButton.wasReleasedThisFrame;
   if(selected>=0&&PieceInfo.CanRotate(session.Pieces[selected].Kind)&&Math.Abs(m.scroll.ReadValue().y)>.01f&&!won){
    session.BeginEdit();
    session.Pieces[selected].Angle=Normalize(session.Pieces[selected].Angle+Math.Sign(m.scroll.ReadValue().y));
    session.EndEdit();dirty=true;feedback?.Rotate();
   }
  }else return;

  V w=ScreenWorld(raw);
  bool onBoard=Math.Abs(w.X)<4.8&&Math.Abs(w.Y)<4.8;
  if(down&&IsOverPresentation(raw))return;

  if(down&&onBoard&&!won){
   if(armed.HasValue){
    if(session.Place(armed.Value,w)){
     selected=session.Pieces.Count-1;armed=null;dirty=true;feedback?.Place();
    }else{
     feedback?.Invalid();if(invalidPlacementCount++<3)Notify("Buraya yerleşemez",danger);
    }
    return;
   }

   if(selected>=0&&PieceInfo.CanRotate(session.Pieces[selected].Kind)&&(w-session.Pieces[selected].Position).Length>.8&&(w-session.Pieces[selected].Position).Length<1.25){
    rotating=true;dragging=false;startAngle=session.Pieces[selected].Angle;startDirection=w-session.Pieces[selected].Position;session.BeginEdit();
   }else{
    selected=-1;
    double closest=.7;
    for(int i=0;i<session.Pieces.Count;i++){
     double dist=(w-session.Pieces[i].Position).Length;
     if(dist<closest){closest=dist;selected=i;}
    }
    if(selected>=0){
     feedback?.Click();
     session.BeginEdit();dragging=true;
     dragStarted=Time.unscaledTime;
     V delta=session.Pieces[selected].Position-w;
     dragOffset=new Vector2((float)delta.X,(float)delta.Y);
    }
    dirty=true;
   }
  }

  if(held&&selected>=0&&!won){
   var piece=session.Pieces[selected];
   if(dragging){
    V proposed=w+new V(dragOffset.x,dragOffset.y);
    proposed=new V(Math.Max(-4.25,Math.Min(4.25,proposed.X)),Math.Max(-4.25,Math.Min(4.25,proposed.Y)));
    if(PlacementRules.IsValid(session.Level,session.Pieces,piece.Kind,proposed,selected)){piece.Position=proposed;dirty=true;}
   }
   if(rotating&&PieceInfo.CanRotate(piece.Kind)){
    V dir=w-piece.Position;
    double before=piece.Angle;
    double rotation=(Math.Atan2(dir.Y,dir.X)-Math.Atan2(startDirection.Y,startDirection.X))*180/Math.PI;
    piece.Angle=Normalize(startAngle+rotation*(VisualEnvironment.PrecisionMode ? .35 : 1));
    if((int)(before/15)!=(int)(piece.Angle/15))feedback?.Rotate();
    dirty=true;
   }
  }

  if(up&&(dragging||rotating)){
   bool changedAngle=rotating&&selected>=0&&Math.Abs(session.Pieces[selected].Angle-startAngle)>.1;
   session.EndEdit();
   if(changedAngle)feedback?.Rotate();
   else feedback?.Click();
   dragging=rotating=false;
  }
 }

 void Notify(string message,Color color){
  presentation?.ShowFeedback(message);
  toast=message??"";
  toastColor=color;
  toastUntil=Time.unscaledTime+2.25f;
 }

 bool IsOverPresentation(Vector2 position){
  var events=UnityEngine.EventSystems.EventSystem.current;
  if(events==null)return false;
  uiHits.Clear();
  events.RaycastAll(new UnityEngine.EventSystems.PointerEventData(events){position=position},uiHits);
  return uiHits.Count>0;
 }

 static double Normalize(double angle)=>(angle%360+360)%360;

 int LitGoals(){
  int lit=0;
  if(result!=null&&result.Energy!=null){
   int count=Mathf.Min(result.Energy.Length,session.Level.Goals.Length);
   for(int i=0;i<count;i++)if(result.Energy[i]>=session.Level.Goals[i].Threshold)lit++;
  }
  return lit;
 }

 void Rotate(double degrees){
  if(selected<0||won||transitionBusy||!PieceInfo.CanRotate(session.Pieces[selected].Kind))return;
  session.BeginEdit();session.Pieces[selected].Angle=Normalize(session.Pieces[selected].Angle+degrees);session.EndEdit();dirty=true;feedback?.Rotate();
 }

 IEnumerator StoreCapture(){
  string output=CaptureOutputDirectory("StoreScreens");
  Directory.CreateDirectory(output);
  Screen.SetResolution(540,960,false);
  yield return new WaitForSeconds(.5f);

  int[] picks={19,99,59,0,79};
  string[] names={"01-spectrum","02-mastery","03-water-glass","04-reflection","05-symphony"};
  for(int n=0;n<picks.Length;n++){
   int index=Mathf.Clamp(picks[n],0,levels.Length-1);
   Load(index,true);
   session.Reveal();
   selected=-1;armed=null;showHint=false;showLevelMap=false;showSettings=false;won=false;settle=0;dirty=true;
   yield return null;
   yield return new WaitForSeconds(.18f);
   yield return StartCoroutine(CaptureStoreFrame(Path.Combine(output,names[n]+".png")));
   yield return new WaitForSeconds(.08f);
  }

  Load(49,true);
  presentation.ShowMap();
  yield return null;
  yield return new WaitForSeconds(.18f);
  yield return StartCoroutine(CaptureStoreFrame(Path.Combine(output,"06-level-map.png")));
  yield return new WaitForSeconds(.08f);

  presentation.ShowHome();
  yield return new WaitForSeconds(1.7f);
  yield return StartCoroutine(CaptureStoreFrame(Path.Combine(output,"07-main-menu.png")));
  presentation.ShowSettings();
  yield return new WaitForSeconds(.3f);
  yield return StartCoroutine(CaptureStoreFrame(Path.Combine(output,"08-settings.png")));

  File.WriteAllText(Path.Combine(output,"capture-complete.txt"),"PrisM store capture complete: "+DateTime.UtcNow.ToString("O"));
  Application.Quit(0);
 }

 IEnumerator CaptureStoreFrame(string path){
  yield return new WaitForEndOfFrame();
  var captured=ScreenCapture.CaptureScreenshotAsTexture(2);
  var texture=new Texture2D(captured.width,captured.height,TextureFormat.RGB24,false);
  texture.SetPixels32(captured.GetPixels32());
  texture.Apply(false,false);
  File.WriteAllBytes(path,texture.EncodeToPNG());
  Destroy(texture);
  Destroy(captured);
 }

 string CaptureOutputDirectory(string fallbackFolder){
  string output=Path.Combine(Application.dataPath,"../../"+fallbackFolder);
  var args=Environment.GetCommandLineArgs();
  int arg=Array.IndexOf(args,"-captureDir");
  if(arg>=0&&arg+1<args.Length)output=args[arg+1];
  return output;
 }

 IEnumerator Smoke(){
  string output=CaptureOutputDirectory("TestResults");
  Directory.CreateDirectory(output);
  yield return null;
  var checks=new List<string>();
  presentation.ShowHome();yield return null;
  if(!presentation.BlocksBoardInput)throw new Exception("Main menu does not block board input");
  ContinueGame();yield return null;
  if(presentation.BlocksBoardInput)throw new Exception("Gameplay input remains blocked");
  presentation.ShowSettings();yield return null;
  if(!presentation.BlocksBoardInput)throw new Exception("Settings does not block board input");
  presentation.HandleBack();yield return null;
  if(presentation.BlocksBoardInput)throw new Exception("Closing settings does not resume gameplay");
  checks.Add("PASS Canvas menu settings back input flow");
  Load(0,true);yield return null;
  Vector3 drop=cam.WorldToScreenPoint(new Vector3(0,-2,0));
  StartTrayDrag(Kind.Mirror,new Vector2(drop.x,drop.y));
  EndTrayDrag(new Vector2(drop.x,drop.y));yield return null;
  if(session.Pieces.Count!=1||session.Remaining(Kind.Mirror)!=0)throw new Exception("Tray drag does not place piece");
  UndoAction();yield return new WaitForSecondsRealtime(.25f);
  if(session.Pieces.Count!=0||session.Remaining(Kind.Mirror)!=1)throw new Exception("Tray undo does not restore inventory");
  checks.Add("PASS Canvas tray placement undo inventory");

  Load(0,true);session.Reveal();session.Pieces[0].Angle+=20;
  session.BeginEdit();session.Pieces[0].Angle=levels[0].Solution[0].Angle;session.EndEdit();dirty=true;
  yield return new WaitForSecondsRealtime(.49f);
  if(won||!session.IsComplete(result))throw new Exception("Undo regression setup is not a pending solution");
  UndoAction();yield return new WaitForSecondsRealtime(.25f);
  if(won||session.IsComplete(result)||settle>0)throw new Exception("Undo animation completed an obsolete solution");
  checks.Add("PASS Canvas undo cancels pending completion");
  Load(0,true);StartTrayDrag(Kind.Mirror,new Vector2(drop.x,drop.y));presentation.ShowHome();ContinueGame();yield return null;
  if(trayGhost!=null||trayKind.HasValue||presentation.BlocksBoardInput)throw new Exception("Screen change did not cancel tray interaction");
  checks.Add("PASS Canvas screen change cancels tray interaction");

  Load(0,true);
  yield return new WaitForSeconds(.15f);
  ScreenCapture.CaptureScreenshot(Path.Combine(output,"01-start.png"));
  yield return new WaitForSeconds(.15f);

  for(int i=0;i<levels.Length;i++){
   Load(i,true);
   foreach(var piece in levels[i].Solution){
    bool placed=session.Place(piece.Kind,piece.Position);
    if(!placed)throw new Exception("Inventory/placement failure in level "+(i+1));
    session.BeginEdit();session.Pieces[session.Pieces.Count-1].Angle=piece.Angle;session.EndEdit();
   }
   dirty=true;
   yield return null;
   if(!session.IsComplete(result))throw new Exception("Unsolved runtime level "+(i+1));
   checks.Add("PASS runtime level "+(i+1));
   if(i==0||i%10==9){
    ScreenCapture.CaptureScreenshot(Path.Combine(output,string.Format("level-{0:000}-solved.png",i+1)));
    yield return new WaitForSeconds(.12f);
   }

   session.Reset();dirty=true;yield return null;
   if(session.IsComplete(result))throw new Exception("Reset failed in level "+(i+1));
   checks.Add("PASS runtime reset "+(i+1));
  }

  Load(Mathf.Min(79,levels.Length-1),true);
  session.Reveal();selected=Mathf.Min(1,session.Pieces.Count-1);dirty=true;yield return null;
  ScreenCapture.CaptureScreenshot(Path.Combine(output,"02-optics.png"));
  yield return new WaitForSeconds(.25f);

  Load(levels.Length-1,true);
  session.Reveal();dirty=true;yield return null;
  ScreenCapture.CaptureScreenshot(Path.Combine(output,"03-final.png"));
  yield return new WaitForSeconds(.25f);

  presentation.ShowCompletion(true);
  yield return new WaitForSecondsRealtime(5.6f);
  ScreenCapture.CaptureScreenshot(Path.Combine(output,"04-campaign-final.png"));
  yield return new WaitForSecondsRealtime(.2f);
  presentation.ShowHome();
  yield return null;
  checks.Add("PASS Canvas campaign finale returns home");

  File.WriteAllLines(Path.Combine(output,"runtime-smoke.txt"),checks);
  Application.Quit(0);
 }
}
}
