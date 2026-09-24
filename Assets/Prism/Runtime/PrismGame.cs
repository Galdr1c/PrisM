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
 ProgressData progress=new ProgressData();
 readonly HashSet<string> completedLevelIds=new HashSet<string>();

 int levelIndex,selected=-1;
 Kind? armed;
 bool dragging,rotating,showHint,showLevelMap,showSettings,won,dirty=true,smoke,storeCapture;
 float settle;
 Vector2 dragOffset;
 double startAngle;
 V startDirection;
 float scale=1,offsetX,offsetY;
 Font font;

 readonly Color ink=new Color(.9f,.94f,.94f);
 readonly Color muted=new Color(.47f,.59f,.64f);
 readonly Color gold=new Color(.91f,.77f,.49f);
 readonly Color panel=new Color(.065f,.10f,.13f);
 readonly Color border=new Color(.17f,.25f,.29f);

 string SavePath=>Path.Combine(Application.persistentDataPath,"progress.json");
 public static string PieceName(Kind k)=>PieceInfo.Name(k);

 void Start(){
  Application.targetFrameRate=60;
  Screen.sleepTimeout=SleepTimeout.NeverSleep;
  font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
  var commandLine=Environment.GetCommandLineArgs();\n  smoke=Array.IndexOf(commandLine,"-prismSmoke")>=0;\n  storeCapture=Array.IndexOf(commandLine,"-prismStoreCapture")>=0;

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
  if(smoke)StartCoroutine(Smoke());\n  else if(storeCapture)StartCoroutine(StoreCapture());
 }

 int FindLevel(string id){
  if(!string.IsNullOrEmpty(id))for(int i=0;i<levels.Length;i++)if(levels[i].Id==id)return i;
  return 0;
 }

 void LoadProgress(){
  progress=new ProgressData();
  completedLevelIds.Clear();
  try{
   if(File.Exists(SavePath)){
    var loaded=JsonUtility.FromJson<ProgressData>(File.ReadAllText(SavePath));
    if(loaded!=null)progress=loaded;
   }
  }catch(Exception e){
   Debug.LogWarning("Progress could not be loaded: "+e.Message);
   progress=new ProgressData();
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
   if(File.Exists(SavePath))File.Delete(SavePath);
   File.Move(temp,SavePath);
  }catch(Exception e){
   Debug.LogWarning("Progress could not be saved: "+e.Message);
  }
 }

 void OnApplicationPause(bool paused){if(paused)SaveProgress();}
 void OnApplicationQuit(){SaveProgress();}

 void Layout(){
  Rect safe=Screen.safeArea;
  if(safe.width<1||safe.height<1)safe=new Rect(0,0,Screen.width,Screen.height);
  scale=Mathf.Max(.01f,Mathf.Min(safe.width/900f,safe.height/1340f));
  float usedW=900f*scale,usedH=1340f*scale;
  offsetX=safe.x+(safe.width-usedW)*.5f;
  offsetY=(Screen.height-safe.yMax)+(safe.height-usedH)*.5f;
  cam.orthographicSize=Screen.height/(scale*160f);
  cam.transform.position=new Vector3(0,(offsetY+600f*scale-Screen.height*.5f)/(80f*scale),-10f);
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
  if(!force&&!IsUnlocked(index)){feedback?.Invalid();return;}
  levelIndex=index;
  session=new Session(levels[levelIndex]);
  selected=-1;armed=null;showHint=false;showLevelMap=false;showSettings=false;won=false;settle=0;dirty=false;
  Solve();
  if(!smoke&&!storeCapture){progress.lastLevelId=session.Level.Id;SaveProgress();}
 }

 void Solve(){
  result=Optics.Solve(session.Level,session.Pieces);
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
  if(!won&&complete){
   settle+=Time.deltaTime;
   if(settle>.65f){
    won=true;
    if(!smoke&&!storeCapture){
     completedLevelIds.Add(session.Level.Id);
     SaveProgress();
     feedback?.Complete();
    }
   }
  }else if(!complete)settle=0;
 }

 Vector2 Design(Vector2 screen)=>new Vector2((screen.x-offsetX)/scale,(Screen.height-screen.y-offsetY)/scale);
 V World(Vector2 p)=>new V((p.x-450)/80,(600-p.y)/80);

 void HandleBack(){
  if(Keyboard.current==null||!Keyboard.current.escapeKey.wasPressedThisFrame)return;
  if(showHint){showHint=false;return;}
  if(showSettings){showSettings=false;return;}
  if(showLevelMap){showLevelMap=false;return;}
  if(won){won=false;showLevelMap=true;return;}
  if(armed.HasValue){armed=null;return;}
  if(selected>=0){selected=-1;dirty=true;return;}
  showLevelMap=true;
 }

 void Pointer(){
  if(showHint||showLevelMap||showSettings)return;
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
    session.EndEdit();dirty=true;
   }
  }else return;

  Vector2 p=Design(raw);
  V w=World(p);
  bool onBoard=new Rect(50,200,800,800).Contains(p);

  if(down&&onBoard&&!won){
   if(armed.HasValue){
    if(session.Place(armed.Value,w)){selected=session.Pieces.Count-1;armed=null;dirty=true;}
    else feedback?.Invalid();
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
     session.BeginEdit();dragging=true;
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
    piece.Angle=Normalize(startAngle+(Math.Atan2(dir.Y,dir.X)-Math.Atan2(startDirection.Y,startDirection.X))*180/Math.PI);
    dirty=true;
   }
  }

  if(up&&(dragging||rotating)){session.EndEdit();dragging=rotating=false;}
 }

 static double Normalize(double angle)=>(angle%360+360)%360;

 GUIStyle TextStyle(int size,Color c,FontStyle weight=FontStyle.Normal,TextAnchor align=TextAnchor.MiddleLeft){
  return new GUIStyle(GUI.skin.label){font=font,fontSize=size,fontStyle=weight,alignment=align,normal={textColor=c},wordWrap=true};
 }

 void Text(Rect r,string text,int size,Color c,FontStyle weight=FontStyle.Normal,TextAnchor align=TextAnchor.MiddleLeft){
  GUI.Label(r,text,TextStyle(size,c,weight,align));
 }

 void Box(Rect r,Color c){
  Color old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;
 }

 bool Button(Rect r,string label,bool active=false,bool enabled=true,int fontSize=18){
  Box(r,active?new Color(.2f,.25f,.24f):panel);
  Box(new Rect(r.x,r.yMax-1,r.width,1),active?gold:border);
  Text(r,label,fontSize,enabled?(active?gold:ink):muted,FontStyle.Normal,TextAnchor.MiddleCenter);
  bool old=GUI.enabled;GUI.enabled=enabled;
  bool hit=GUI.Button(r,GUIContent.none,GUIStyle.none);
  GUI.enabled=old;
  if(hit)feedback?.Click();
  return hit;
 }

 void Rotate(double degrees){
  if(selected<0||!PieceInfo.CanRotate(session.Pieces[selected].Kind))return;
  session.BeginEdit();session.Pieces[selected].Angle=Normalize(session.Pieces[selected].Angle+degrees);session.EndEdit();dirty=true;
 }

 void OnGUI(){
  if(session==null)return;
  GUI.matrix=Matrix4x4.TRS(new Vector3(offsetX,offsetY,0),Quaternion.identity,new Vector3(scale,scale,1));

  Text(new Rect(50,24,300,32),"P R I S M",25,ink,FontStyle.Bold);
  Text(new Rect(520,24,330,32),"I Ş I K  A T Ö L Y E S İ",12,muted,FontStyle.Normal,TextAnchor.MiddleRight);
  Box(new Rect(50,74,800,1),border);

  string chapter="ÜNİTE "+(levelIndex/10+1).ToString("00")+" · "+session.Level.Chapter.ToUpperInvariant();
  Text(new Rect(50,88,430,25),chapter,13,gold);
  Text(new Rect(490,88,360,25),"DENEY "+(levelIndex+1).ToString("000")+" / "+levels.Length.ToString("000")+"   ·   ZORLUK "+session.Level.Difficulty+"/10",13,muted,FontStyle.Normal,TextAnchor.MiddleRight);
  int titleSize=session.Level.Name.Length>46?18:session.Level.Name.Length>32?22:32;\n  Text(new Rect(50,118,670,40),session.Level.Name,titleSize,ink,FontStyle.Bold);
  Text(new Rect(50,160,690,30),session.Level.Lesson,16,muted);

  int lit=0;
  if(result!=null&&result.Energy!=null){
   int count=Mathf.Min(result.Energy.Length,session.Level.Goals.Length);
   for(int i=0;i<count;i++)if(result.Energy[i]>=session.Level.Goals[i].Threshold)lit++;
  }
  Text(new Rect(730,120,120,34),lit+" / "+session.Level.Goals.Length,22,gold,FontStyle.Normal,TextAnchor.MiddleRight);
  if(session.Level.RequireAllPiecesActive){
   int active=result!=null?result.ActivePieceCount:0;
   Text(new Rect(650,158,200,30),"AKTİF "+active+" / "+session.Pieces.Count,12,active>=session.Pieces.Count?gold:muted,FontStyle.Normal,TextAnchor.MiddleRight);
  }

  Box(new Rect(49,199,802,1),border);Box(new Rect(49,1000,802,1),border);Box(new Rect(49,200,1,800),border);Box(new Rect(850,200,1,800),border);

  if(armed.HasValue)Text(new Rect(70,216,650,30),PieceName(armed.Value)+" yerleştirmek için alana dokun",16,gold);
  else if(session.Pieces.Count==0)Text(new Rect(70,216,650,30),"Başlamak için aşağıdan bir parça seç",16,new Color(.68f,.76f,.78f));

  if(result!=null&&result.Energy!=null){
   int count=Mathf.Min(result.Energy.Length,session.Level.Goals.Length);
   for(int i=0;i<count;i++){
    var g=session.Level.Goals[i];
    float gx=450+(float)g.Position.X*80,gy=600-(float)g.Position.Y*80;
    int percent=Mathf.Min(100,Mathf.RoundToInt((float)(result.Energy[i]/g.Threshold)*100));
    Text(new Rect(gx-45,gy+(float)g.Radius*80+10,90,22),percent+"%",12,muted,FontStyle.Normal,TextAnchor.MiddleCenter);
   }
  }

  Box(new Rect(50,1018,800,72),panel);
  if(selected>=0&&selected<session.Pieces.Count&&!won){
   var p=session.Pieces[selected];
   Text(new Rect(70,1026,215,26),PieceName(p.Kind),18,ink);
   Text(new Rect(70,1053,220,22),PieceInfo.CanRotate(p.Kind)?"Sürükle · Halkayla döndür":"Sürükleyerek konumlandır",12,muted);
   if(PieceInfo.CanRotate(p.Kind)){
    if(Button(new Rect(310,1030,68,48),"−15°"))Rotate(-15);
    if(Button(new Rect(384,1030,56,48),"−1°"))Rotate(-1);
    Text(new Rect(443,1030,122,48),p.Angle.ToString("0.0")+"°",23,gold,FontStyle.Normal,TextAnchor.MiddleCenter);
    if(Button(new Rect(568,1030,56,48),"+1°"))Rotate(1);
    if(Button(new Rect(630,1030,68,48),"+15°"))Rotate(15);
   }else Text(new Rect(320,1030,365,48),"Dönel simetrik · açı gerekmez",16,muted,FontStyle.Normal,TextAnchor.MiddleCenter);
   if(Button(new Rect(718,1030,114,48),"Kaldır")){session.Remove(selected);selected=-1;dirty=true;}
  }else{
   string status=won?"Bütün koşullar tamamlandı.":session.Level.RequireAllPiecesActive?"Tüm parçaları ışık zincirinde aktif kullan.":"Parçayı seç, yerleştir ve ışığın yolunu değiştir.";
   Text(new Rect(72,1030,750,46),status,17,won?gold:muted);
  }

  Text(new Rect(50,1101,300,24),"OPTİK PARÇALAR",12,muted);
  var kinds=new List<Kind>();
  foreach(var k in session.Level.Stock)if(!kinds.Contains(k))kinds.Add(k);
  for(int i=0;i<kinds.Count;i++){
   var k=kinds[i];int n=session.Remaining(k);Rect r=new Rect(50+i*192,1134,180,70);
   if(Button(r,PieceName(k)+"  ·  "+n,armed==k,n>0&&!won)){armed=armed==k?(Kind?)null:k;selected=-1;dirty=true;}
  }

  if(Button(new Rect(50,1220,140,45),"Geri al",false,!won)){session.Undo();selected=-1;armed=null;dirty=true;}
  if(Button(new Rect(202,1220,140,45),"Sıfırla")){session.Reset();selected=-1;armed=null;won=false;settle=0;dirty=true;}
  if(Button(new Rect(354,1220,140,45),"İpucu",showHint,!won))showHint=!showHint;
  if(Button(new Rect(506,1220,140,45),"Bölümler",showLevelMap,!won)){showLevelMap=true;showHint=false;showSettings=false;}
  if(Button(new Rect(658,1220,140,45),"Ayarlar",showSettings,!won)){showSettings=true;showHint=false;showLevelMap=false;}

  Text(new Rect(50,1288,800,24),"ACELE YOK.  IŞIĞI TAKİP ET.",12,muted,FontStyle.Normal,TextAnchor.MiddleCenter);

  if(showHint&&!won)DrawHint();
  if(showLevelMap&&!won)DrawLevelMap();
  if(showSettings&&!won)DrawSettings();
  if(won)DrawWin();
 }

 void DrawHint(){
  Box(new Rect(80,800,740,170),new Color(.08f,.13f,.16f,.985f));
  Text(new Rect(105,815,680,28),"KÜÇÜK BİR İPUCU",12,gold);
  Text(new Rect(105,847,680,72),session.Level.Hint,18,ink);
  if(Button(new Rect(625,927,170,30),"Anladım",false,true,15))showHint=false;
 }

 void DrawLevelMap(){
  Box(new Rect(65,165,770,990),new Color(.04f,.075f,.095f,.99f));
  Text(new Rect(90,184,720,34),"100 DENEY · 10 ÜNİTE",24,ink,FontStyle.Bold);
  Text(new Rect(90,219,720,24),"Tamamlananlar ✓ · sıradaki deney otomatik açılır",13,muted);

  for(int row=0;row<10;row++){
   int first=row*10;
   float y=255+row*78;
   Text(new Rect(88,y,130,48),(row+1).ToString("00")+"  "+levels[first].Chapter,13,row==levelIndex/10?gold:muted,FontStyle.Bold);
   for(int col=0;col<10;col++){
    int index=first+col;
    bool unlocked=IsUnlocked(index);
    bool complete=completedLevelIds.Contains(levels[index].Id);
    string label=complete?"✓":unlocked?(index+1).ToString():"·";
    Rect r=new Rect(225+col*57,y,50,48);
    if(Button(r,label,index==levelIndex,unlocked,14)){Load(index);showLevelMap=false;return;}
   }
  }
  if(Button(new Rect(615,1085,190,45),"Kapat",false,true,16))showLevelMap=false;
 }

 void DrawSettings(){
  Box(new Rect(175,360,550,450),new Color(.04f,.075f,.095f,.99f));
  Text(new Rect(210,385,480,36),"AYARLAR",25,ink,FontStyle.Bold,TextAnchor.MiddleCenter);

  Text(new Rect(215,450,220,46),"Ses",16,muted);
  if(Button(new Rect(455,450,220,46),feedback!=null&&feedback.AudioEnabled?"Açık":"Kapalı",feedback!=null&&feedback.AudioEnabled,true,16))if(feedback!=null)feedback.SetAudio(!feedback.AudioEnabled);

  Text(new Rect(215,515,220,46),"Titreşim",16,muted);
  if(Button(new Rect(455,515,220,46),feedback!=null&&feedback.HapticsEnabled?"Açık":"Kapalı",feedback!=null&&feedback.HapticsEnabled,true,16))if(feedback!=null)feedback.SetHaptics(!feedback.HapticsEnabled);

  Text(new Rect(215,580,220,46),"Görsel kalite",16,muted);
  if(Button(new Rect(455,580,220,46),VisualEnvironment.QualityLabel,false,true,14)){VisualEnvironment.NextQuality();dirty=true;}

  Text(new Rect(215,650,460,26),"Tamamlanan deney: "+completedLevelIds.Count+" / "+levels.Length,14,muted);
  Text(new Rect(215,680,460,42),"Kalite ayarı optik hesaplamayı değiştirmez; yalnız görsel maliyeti ölçekler.",12,muted);

  if(Button(new Rect(455,742,220,42),"Kapat",false,true,16))showSettings=false;
 }

 void DrawWin(){
  Box(new Rect(180,380,540,310),new Color(.045f,.085f,.105f,.99f));
  Text(new Rect(215,405,470,30),"DENEY TAMAMLANDI",13,gold,FontStyle.Normal,TextAnchor.MiddleCenter);
  Text(new Rect(215,448,470,70),"Işık yolunu buldu.",31,ink,FontStyle.Bold,TextAnchor.MiddleCenter);
  Text(new Rect(220,525,460,30),session.Level.Chapter+" · zorluk "+session.Level.Difficulty+"/10",15,muted,FontStyle.Normal,TextAnchor.MiddleCenter);
  Text(new Rect(220,558,460,34),levelIndex<levels.Length-1?"Sıradaki deney açıldı.":"100 deney tamamlandı.",17,gold,FontStyle.Normal,TextAnchor.MiddleCenter);
  string label=levelIndex<levels.Length-1?"Sonraki deney  →":"Bölüm haritası";
  if(Button(new Rect(240,615,420,54),label,true)){
   if(levelIndex<levels.Length-1)Load(levelIndex+1,true);
   else{won=false;showLevelMap=true;}
  }
 }

 IEnumerator StoreCapture(){
  string output=CaptureOutputDirectory("StoreScreens");
  Directory.CreateDirectory(output);
  Screen.SetResolution(1080,1920,false);
  yield return new WaitForSeconds(.5f);

  int[] picks={0,29,49,69,99};
  string[] names={"01-reflection","02-color","03-combination","04-water-glass","05-mastery"};
  for(int n=0;n<picks.Length;n++){
   int index=Mathf.Clamp(picks[n],0,levels.Length-1);
   Load(index,true);
   session.Reveal();
   selected=-1;armed=null;showHint=false;showLevelMap=false;showSettings=false;won=false;settle=0;dirty=true;
   yield return null;
   yield return new WaitForSeconds(.18f);
   ScreenCapture.CaptureScreenshot(Path.Combine(output,names[n]+".png"));
   yield return new WaitForSeconds(.22f);
  }

  Load(49,true);
  showLevelMap=true;
  yield return null;
  yield return new WaitForSeconds(.18f);
  ScreenCapture.CaptureScreenshot(Path.Combine(output,"06-level-map.png"));
  yield return new WaitForSeconds(.22f);

  File.WriteAllText(Path.Combine(output,"capture-complete.txt"),"PrisM store capture complete: "+DateTime.UtcNow.ToString("O"));
  Application.Quit(0);
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

  Load(0,true);
  yield return new WaitForSeconds(.15f);
  ScreenCapture.CaptureScreenshot(Path.Combine(output,"01-start.png"));
  yield return new WaitForSeconds(.15f);

  var checks=new List<string>();
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
   if(i==0||i%10==9)ScreenCapture.CaptureScreenshot(Path.Combine(output,string.Format("level-{0:000}-solved.png",i+1)));

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

  File.WriteAllLines(Path.Combine(output,"runtime-smoke.txt"),checks);
  Application.Quit(0);
 }
}
}
