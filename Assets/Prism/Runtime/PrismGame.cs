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

 int levelIndex,selected=-1,mapChapter=-1,previousLit;
 Kind? armed;
 bool dragging,rotating,angleTween,showHint,showLevelMap,showSettings,won,showWinPanel,dirty=true,smoke,storeCapture,drawingOverlay;
 int angleTweenIndex=-1;
 double angleTweenFrom,angleTweenTo;
 float angleTweenStart,settle,winShownAt=-10f,toastUntil;
 string toast="";
 Color toastColor;
 Vector2 dragOffset;
 double startAngle;
 V startDirection;
 float scale=1,offsetX,offsetY;
 Font font;
 Texture2D circleTex;

 readonly Color ink=new Color(.92f,.965f,.975f);
 readonly Color muted=new Color(.66f,.75f,.80f);
 readonly Color gold=new Color(.96f,.78f,.40f);
 readonly Color cyan=new Color(.34f,.82f,.96f);
 readonly Color success=new Color(.34f,.92f,.68f);
 readonly Color danger=new Color(.96f,.40f,.42f);
 readonly Color surface=new Color(.028f,.052f,.068f);
 readonly Color panel=new Color(.052f,.086f,.108f);
 readonly Color raised=new Color(.072f,.116f,.142f);
 readonly Color border=new Color(.14f,.225f,.265f);

 string SavePath=>Path.Combine(Application.persistentDataPath,"progress.json");
 public static string PieceName(Kind k)=>PieceInfo.Name(k);

 void Start(){
  Application.targetFrameRate=60;
  Screen.sleepTimeout=SleepTimeout.NeverSleep;
  font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
  circleTex=CreateCircleTexture(64);
  var commandLine=Environment.GetCommandLineArgs();
  smoke=Array.IndexOf(commandLine,"-prismSmoke")>=0;
  storeCapture=Array.IndexOf(commandLine,"-prismStoreCapture")>=0;

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
 void OnDestroy(){if(circleTex!=null)Destroy(circleTex);}

 void Layout(){
  Rect safe=Screen.safeArea;
  if(safe.width<1||safe.height<1)safe=new Rect(0,0,Screen.width,Screen.height);
  scale=Mathf.Max(.01f,Mathf.Min(safe.width/900f,safe.height/1540f));
  float usedW=900f*scale,usedH=1540f*scale;
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
  if(!force&&!IsUnlocked(index)){feedback?.Invalid();Notify("Bu bölüm henüz kilitli.",danger);return;}
  levelIndex=index;
  session=new Session(levels[levelIndex]);
  previousLit=0;
  selected=-1;armed=null;showHint=false;showLevelMap=false;showSettings=false;mapChapter=-1;won=false;showWinPanel=false;angleTween=false;angleTweenIndex=-1;settle=0;winShownAt=-10f;toast="";toastUntil=0;dirty=false;
  Solve();
  board?.SetCelebration(0f);
  VisualEnvironment.SetCelebration(0f);
  if(!smoke&&!storeCapture){progress.lastLevelId=session.Level.Id;SaveProgress();}
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
   AnimateRotation();
   Pointer();
  }
  if(dirty)Solve();

  bool complete=session.IsComplete(result);
  if(!won&&complete){
   settle+=Time.deltaTime;
   if(settle>.58f){
    won=true;
    showWinPanel=true;
    winShownAt=Time.unscaledTime;
    bool milestone=(levelIndex+1)%10==0||levelIndex==levels.Length-1;
    if(!smoke&&!storeCapture){
     completedLevelIds.Add(session.Level.Id);
     SaveProgress();
     feedback?.Complete(milestone);
    }
   }
  }else if(!complete)settle=0;

  float celebration=0f;
  if(won){
   float age=Time.unscaledTime-winShownAt;
   celebration=Mathf.Clamp01(1f-Mathf.Max(0f,age-1.2f)/2.4f);
  }
  board?.SetCelebration(celebration);
  VisualEnvironment.SetCelebration(celebration);
 }

 Vector2 Design(Vector2 screen)=>new Vector2((screen.x-offsetX)/scale,(Screen.height-screen.y-offsetY)/scale);
 V World(Vector2 p)=>new V((p.x-450)/80,(600-p.y)/80);

 void HandleBack(){
  if(Keyboard.current==null||!Keyboard.current.escapeKey.wasPressedThisFrame)return;
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
  if(showHint||showLevelMap||showSettings||won||angleTween)return;
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
    if(session.Place(armed.Value,w)){
     selected=session.Pieces.Count-1;armed=null;dirty=true;feedback?.Place();
    }else{
     feedback?.Invalid();Notify("Buraya yerleştirilemez · kaynak, hedef, duvar veya başka bir parçayla çakışıyor.",danger);
    }
    return;
   }

   if(selected>=0&&PieceInfo.CanRotate(session.Pieces[selected].Kind)&&(w-session.Pieces[selected].Position).Length>.8&&(w-session.Pieces[selected].Position).Length<1.42){
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
    double target=Normalize(startAngle+(Math.Atan2(dir.Y,dir.X)-Math.Atan2(startDirection.Y,startDirection.X))*180/Math.PI);
    if(Math.Abs(target-piece.Angle)>.08){piece.Angle=target;dirty=true;feedback?.Rotate();}
   }
  }

  if(up&&(dragging||rotating)){session.EndEdit();feedback?.Click();dragging=rotating=false;}
 }

 void Notify(string message,Color color){
  toast=message??"";
  toastColor=color;
  toastUntil=Time.unscaledTime+2.25f;
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

 GUIStyle TextStyle(int size,Color c,FontStyle weight=FontStyle.Normal,TextAnchor align=TextAnchor.MiddleLeft){
  return new GUIStyle(GUI.skin.label){
   font=font,fontSize=size,fontStyle=weight,alignment=align,
   normal={textColor=c},wordWrap=true,clipping=TextClipping.Clip
  };
 }

 void Text(Rect r,string text,int size,Color c,FontStyle weight=FontStyle.Normal,TextAnchor align=TextAnchor.MiddleLeft){
  GUI.Label(r,text,TextStyle(size,c,weight,align));
 }

 void Box(Rect r,Color c){
  Color old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;
 }

 Texture2D CreateCircleTexture(int size){
  var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name="PrisM UI circle",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
  var pixels=new Color32[size*size];float center=(size-1)*.5f,radius=center-.5f;
  for(int y=0;y<size;y++)for(int x=0;x<size;x++){
   float dx=x-center,dy=y-center,d=Mathf.Sqrt(dx*dx+dy*dy);
   byte a=(byte)Mathf.RoundToInt(Mathf.Clamp01(radius-d+1f)*255f);
   pixels[y*size+x]=new Color32(255,255,255,a);
  }
  texture.SetPixels32(pixels);texture.Apply(false,true);return texture;
 }

 void Tint(Rect r,Texture texture,Color color){
  Color old=GUI.color;GUI.color=color;GUI.DrawTexture(r,texture,ScaleMode.StretchToFill,true);GUI.color=old;
 }

 void RoundBox(Rect r,Color color,float radius=18f){
  radius=Mathf.Min(radius,Mathf.Min(r.width,r.height)*.5f);
  if(radius<2f||circleTex==null){Box(r,color);return;}
  Box(new Rect(r.x+radius,r.y,r.width-radius*2,r.height),color);
  Box(new Rect(r.x,r.y+radius,r.width,r.height-radius*2),color);
  float d=radius*2;
  Tint(new Rect(r.x,r.y,d,d),circleTex,color);
  Tint(new Rect(r.xMax-d,r.y,d,d),circleTex,color);
  Tint(new Rect(r.x,r.yMax-d,d,d),circleTex,color);
  Tint(new Rect(r.xMax-d,r.yMax-d,d,d),circleTex,color);
 }

 void Line(Vector2 a,Vector2 b,Color color,float width=3){
  Matrix4x4 old=GUI.matrix;
  GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg,a);
  Box(new Rect(a.x,a.y-width*.5f,Vector2.Distance(a,b),width),color);
  GUI.matrix=old;
 }

 void PieceIcon(Rect r,Kind kind,Color color){
  Vector2 c=r.center;float s=Mathf.Min(r.width,r.height)*.38f;
  if(kind==Kind.Prism){
   Vector2 a=c+new Vector2(0,-s),b=c+new Vector2(-s,s),d=c+new Vector2(s,s);
   Line(a,b,color);Line(b,d,color);Line(d,a,color);
   Line(c+new Vector2(-s*1.4f,0),c,color,2);Line(c,c+new Vector2(s*1.4f,-s*.4f),gold,2);
  }else if(kind==Kind.Lens||kind==Kind.Sphere){
   Vector2 previous=c+new Vector2(s,0);
   for(int i=1;i<=24;i++){
    float angle=i*Mathf.PI/12;Vector2 next=c+new Vector2(Mathf.Cos(angle)*s*(kind==Kind.Lens? .45f:1),Mathf.Sin(angle)*s);
    if(i==1)previous=c+new Vector2(s*(kind==Kind.Lens? .45f:1),0);
    Line(previous,next,color,2);previous=next;
   }
   Line(c+new Vector2(-s*1.35f,0),c+new Vector2(s*1.35f,0),gold,2);
  }else{
   Color optic=kind==Kind.Red?new Color(.95f,.40f,.35f):kind==Kind.Green?new Color(.40f,.85f,.63f):color;
   Line(c+new Vector2(-s,s),c+new Vector2(s,-s),optic,6);
   Line(c+new Vector2(-s*1.3f,-s*.35f),c+new Vector2(0,-s*.35f),gold,2);
   Line(c+new Vector2(0,-s*.35f),c+new Vector2(0,-s*1.25f),gold,2);
  }
 }

 void Card(Rect r,Color fill,Color edge){
  RoundBox(new Rect(r.x+4,r.y+7,r.width,r.height),new Color(0,0,0,.24f),22);
  RoundBox(r,fill,22);
  RoundBox(new Rect(r.x+1,r.y+1,r.width-2,3),new Color(edge.r,edge.g,edge.b,.62f),2);
  RoundBox(new Rect(r.x+14,r.y+9,r.width-28,2),new Color(1,1,1,.035f),1);
 }

 void ProgressBar(Rect r,float value,Color fill){
  value=Mathf.Clamp01(value);
  RoundBox(r,new Color(.075f,.12f,.14f,.92f),r.height*.5f);
  if(value>0f){
   float width=Mathf.Max(r.height,r.width*value);
   RoundBox(new Rect(r.x,r.y,Mathf.Min(width,r.width),r.height),fill,r.height*.5f);
  }
 }

 void Pill(Rect r,string label,Color accent,int fontSize=13){
  RoundBox(new Rect(r.x+1,r.y+2,r.width,r.height),new Color(0,0,0,.18f),r.height*.5f);
  RoundBox(r,new Color(accent.r*.14f,accent.g*.14f,accent.b*.14f,.96f),r.height*.5f);
  RoundBox(new Rect(r.x+8,r.y+2,r.width-16,2),new Color(accent.r,accent.g,accent.b,.52f),1);
  Text(r,label,fontSize,accent,FontStyle.Bold,TextAnchor.MiddleCenter);
 }

 bool Button(Rect r,string label,bool active=false,bool enabled=true,int fontSize=24,bool primary=false,bool destructive=false){
  Color accent=destructive?danger:(primary?gold:(active?cyan:new Color(.34f,.48f,.55f)));
  Color fill=destructive?new Color(.19f,.075f,.085f):primary?new Color(.22f,.165f,.075f):(active?new Color(.07f,.18f,.21f):raised);
  if(!enabled)fill=new Color(panel.r,panel.g,panel.b,.68f);
  float radius=Mathf.Clamp(r.height*.20f,12f,22f);
  RoundBox(new Rect(r.x+3,r.y+6,r.width,r.height),new Color(0,0,0,.25f),radius);
  RoundBox(r,fill,radius);
  RoundBox(new Rect(r.x+10,r.y+5,r.width-20,3),new Color(accent.r,accent.g,accent.b,enabled? .56f: .16f),2);
  RoundBox(new Rect(r.x+14,r.y+r.height*.20f,r.width-28,2),new Color(1,1,1,enabled? .035f: .012f),1);
  Text(r,label,fontSize,enabled?(primary?new Color(1,.94f,.75f):(active?cyan:ink)):muted,primary||active?FontStyle.Bold:FontStyle.Normal,TextAnchor.MiddleCenter);
  bool old=GUI.enabled;
  GUI.enabled=enabled&&(!(showHint||showLevelMap||showSettings||(won&&showWinPanel))||drawingOverlay);
  bool hit=GUI.Button(r,GUIContent.none,GUIStyle.none);
  GUI.enabled=old;
  if(hit)feedback?.Click();
  return hit;
 }

 int GoalsLit(){
  if(result==null||result.Energy==null)return 0;
  int lit=0,count=Mathf.Min(result.Energy.Length,session.Level.Goals.Length);
  for(int i=0;i<count;i++)if(result.Energy[i]>=session.Level.Goals[i].Threshold)lit++;
  return lit;
 }

 int ChapterCompleted(int chapterIndex){
  if(chapterIndex<0||chapterIndex*10>=levels.Length)return 0;
  int first=chapterIndex*10,end=Mathf.Min(first+10,levels.Length),count=0;
  for(int i=first;i<end;i++)if(completedLevelIds.Contains(levels[i].Id))count++;
  return count;
 }

 void DrawHeader(){
  Text(new Rect(50,18,250,48),"P R I S M",35,ink,FontStyle.Bold);
  Text(new Rect(300,25,550,30),"IŞIĞI BÜK  ·  RENKLERİ AYIR  ·  YOLU BUL",18,muted,FontStyle.Normal,TextAnchor.MiddleRight);
  Box(new Rect(50,72,800,1),border);

  int chapterIndex=levelIndex/10;
  string chapter="DÜNYA "+(chapterIndex+1).ToString("00")+"  ·  "+session.Level.Chapter.ToUpperInvariant();
  Text(new Rect(50,86,500,28),chapter,19,gold,FontStyle.Bold);
  Text(new Rect(560,86,290,28),"BÖLÜM "+(levelIndex+1).ToString("000")+" / "+levels.Length.ToString("000"),19,muted,FontStyle.Bold,TextAnchor.MiddleRight);

  int titleSize=session.Level.Name.Length>46?25:session.Level.Name.Length>32?28:34;
  Text(new Rect(50,114,620,42),session.Level.Name,titleSize,ink,FontStyle.Bold);
  Pill(new Rect(700,118,150,34),"ZORLUK  "+session.Level.Difficulty+"/10",session.Level.Difficulty>=8?gold:cyan,12);
  Text(new Rect(50,156,650,27),session.Level.Lesson,16,muted);
  int completed=ChapterCompleted(chapterIndex);
  ProgressBar(new Rect(50,188,800,4),(completed+(levelIndex%10)/10f)/10f,new Color(gold.r,gold.g,gold.b,.8f));
 }

 void DrawBoardHud(){
  Box(new Rect(49,199,802,1),border);
  Box(new Rect(49,1000,802,1),border);
  Box(new Rect(49,200,1,800),border);
  Box(new Rect(850,200,1,800),border);

  int lit=GoalsLit();
  Pill(new Rect(690,216,140,34),lit+" / "+session.Level.Goals.Length+" HEDEF",lit==session.Level.Goals.Length?success:cyan,12);

  if(armed.HasValue){
   Card(new Rect(70,216,500,46),new Color(.045f,.09f,.105f,.94f),new Color(cyan.r,cyan.g,cyan.b,.45f));
   Text(new Rect(86,222,468,34),PieceName(armed.Value)+" hazır · yerleştirmek için tahtaya dokun",15,cyan,FontStyle.Bold);
  }else if(session.Pieces.Count==0&&!won){
   Text(new Rect(70,220,500,28),"Aşağıdan bir optik parça seçerek başla.",14,new Color(.65f,.76f,.79f));
  }

  if(session.Level.RequireAllPiecesActive){
   int active=result!=null?result.ActivePieceCount:0;
   Pill(new Rect(690,258,140,32),"AKTİF "+active+" / "+session.Pieces.Count,active>=session.Pieces.Count&&session.Pieces.Count>0?success:gold,11);
  }

  if(result!=null&&result.Energy!=null){
   int count=Mathf.Min(result.Energy.Length,session.Level.Goals.Length);
   for(int i=0;i<count;i++){
    var g=session.Level.Goals[i];
    float gx=450+(float)g.Position.X*80,gy=600-(float)g.Position.Y*80;
    int percent=Mathf.Min(100,Mathf.RoundToInt((float)(result.Energy[i]/g.Threshold)*100));
    Color targetColor=g.Band<0?ink:BoardRenderer.Spectrum[g.Band];
    Rect tag=new Rect(gx-58,gy+(float)g.Radius*80+9,116,24);
    Box(tag,new Color(.02f,.04f,.05f,.82f));
    Text(tag,PieceInfo.BandName(g.Band).ToUpperInvariant()+" "+percent+"%",10,percent>=100?targetColor:muted,percent>=100?FontStyle.Bold:FontStyle.Normal,TextAnchor.MiddleCenter);
   }
  }

  if(!won&&Time.unscaledTime<toastUntil&&!string.IsNullOrEmpty(toast)){
   float fade=Mathf.Clamp01((toastUntil-Time.unscaledTime)/.35f);
   Card(new Rect(120,918,660,58),new Color(.045f,.065f,.07f,.94f*fade),new Color(toastColor.r,toastColor.g,toastColor.b,.72f*fade));
   Text(new Rect(142,928,616,38),toast,13,new Color(toastColor.r,toastColor.g,toastColor.b,fade),FontStyle.Bold,TextAnchor.MiddleCenter);
  }

  if(won&&!showWinPanel){
   Card(new Rect(175,914,550,64),new Color(.04f,.11f,.095f,.96f),new Color(success.r,success.g,success.b,.7f));
   Text(new Rect(195,924,510,24),"✓  BÖLÜM TAMAMLANDI",15,success,FontStyle.Bold,TextAnchor.MiddleCenter);
   Text(new Rect(195,948,510,20),"Çözüm görünümü · ışık yolunu inceleyebilirsin",12,muted,FontStyle.Normal,TextAnchor.MiddleCenter);
  }
 }

 float InspectorHeight=>selected>=0&&selected<session.Pieces.Count&&!won?(PieceInfo.CanRotate(session.Pieces[selected].Kind)?205f:120f):184f;
 float PaletteY=>1018f+InspectorHeight+43f;
 float NavigationY=>PaletteY+116f;

 void DrawInspector(){
  Card(new Rect(50,1018,800,InspectorHeight),panel,border);
  if(selected>=0&&selected<session.Pieces.Count&&!won){
   var p=session.Pieces[selected];
   Text(new Rect(72,1027,340,24),"SEÇİLİ PARÇA",16,muted,FontStyle.Bold);
   Text(new Rect(72,1051,360,34),PieceName(p.Kind),27,ink,FontStyle.Bold);
   Text(new Rect(72,1085,410,28),PieceInfo.CanRotate(p.Kind)?"Sürükle veya hassas açı kontrolünü kullan.":"Konumu sürükleyerek ayarla.",18,muted);
   if(PieceInfo.CanRotate(p.Kind)){
    Pill(new Rect(485,1038,176,52),p.Angle.ToString("0.0")+"°",gold,25);
    if(Button(new Rect(680,1028,145,105),"KALDIR",false,true,22,false,true)){session.Remove(selected);selected=-1;dirty=true;}
    if(Button(new Rect(72,1110,140,105),"−15°",false,true,22))Rotate(-15);
    if(Button(new Rect(222,1110,140,105),"−1°",false,true,22))Rotate(-1);
    if(Button(new Rect(372,1110,140,105),"+1°",false,true,22))Rotate(1);
    if(Button(new Rect(522,1110,140,105),"+15°",false,true,22))Rotate(15);
   }else{
    Pill(new Rect(485,1042,176,48),"DÖNEL SİMETRİ",cyan,11);
    if(Button(new Rect(680,1028,145,105),"KALDIR",false,true,22,false,true)){session.Remove(selected);selected=-1;dirty=true;}
   }
   return;
  }

  int lit=GoalsLit();
  int active=result!=null?result.ActivePieceCount:0;
  Text(new Rect(72,1038,280,26),won?"IŞIK YOLU TAMAM":"IŞIK YOLU",11,won?success:muted,FontStyle.Bold);
  Text(new Rect(72,1067,610,34),won?"Işık yolu kararlı ve bütün hedef koşulları sağlandı.":session.Level.RequireAllPiecesActive?"Tüm parçaları ışık zincirinde aktif kullan.":"Işığı hedef eşiklerine ulaştır; doğru düzenek anında tepki verir.",18,won?ink:muted,FontStyle.Normal);

  MiniMetric(new Rect(72,1120,210,58),"HEDEFLER",lit+" / "+session.Level.Goals.Length,lit==session.Level.Goals.Length?success:cyan);
  MiniMetric(new Rect(294,1120,210,58),"IŞIKTA",active+" / "+session.Pieces.Count,session.Level.RequireAllPiecesActive&&active>=session.Pieces.Count&&session.Pieces.Count>0?success:gold);
  MiniMetric(new Rect(516,1120,310,58),"PARÇA SAYISI",session.Level.Par+" parça",muted);
 }

 void MiniMetric(Rect r,string title,string value,Color accent){
  Box(r,new Color(surface.r,surface.g,surface.b,.92f));
  Box(new Rect(r.x,r.y,3,r.height),new Color(accent.r,accent.g,accent.b,.8f));
  Text(new Rect(r.x+14,r.y+6,r.width-22,18),title,10,muted,FontStyle.Bold);
  Text(new Rect(r.x+14,r.y+23,r.width-22,29),value,18,accent,FontStyle.Bold);
 }

 void DrawPalette(){
  Text(new Rect(50,PaletteY-30,360,26),"IŞIK OYUNCAKLARI",18,gold,FontStyle.Bold);
  Text(new Rect(500,PaletteY-30,350,26),"Seç · bırak · parlamasını izle",17,muted,FontStyle.Normal,TextAnchor.MiddleRight);
  var kinds=new List<Kind>();
  foreach(var k in session.Level.Stock)if(!kinds.Contains(k))kinds.Add(k);
  int count=Mathf.Max(1,kinds.Count);
  float gap=10f,width=(800f-gap*(count-1))/count;
  for(int i=0;i<kinds.Count;i++){
   var k=kinds[i];
   int n=session.Remaining(k);
   Rect r=new Rect(50+i*(width+gap),PaletteY,width,105);
   if(Button(r,"",armed==k,n>0&&!won)){
    armed=armed==k?(Kind?)null:k;
    selected=-1;dirty=true;
   }
   PieceIcon(new Rect(r.center.x-29,r.y+4,58,50),k,n>0?ink:muted);
   Text(new Rect(r.x+9,r.y+55,r.width-18,43),PieceName(k),23,n>0?ink:muted,FontStyle.Normal,TextAnchor.MiddleCenter);
   Text(new Rect(r.xMax-40,r.y+9,30,32),n.ToString(),22,n>0?gold:muted,FontStyle.Bold,TextAnchor.MiddleCenter);
  }
 }

 void DrawNavigation(){
  if(won&&!showWinPanel){
   if(Button(new Rect(50,NavigationY,190,92),"SONUÇ KARTI",false,true,15)){showWinPanel=true;winShownAt=Time.unscaledTime-.55f;}
   if(Button(new Rect(252,NavigationY,190,92),"BÖLÜMLER",false,true,15)){showLevelMap=true;mapChapter=-1;}
   string next=levelIndex<levels.Length-1?"SONRAKİ BÖLÜM  →":"BÖLÜM HARİTASI";
   if(Button(new Rect(454,NavigationY,396,92),next,false,true,17,true)){
    if(levelIndex<levels.Length-1)Load(levelIndex+1,true);
    else{showLevelMap=true;mapChapter=-1;}
   }
   Text(new Rect(50,NavigationY+116,800,24),"Çözüm kaydedildi.",11,success,FontStyle.Bold,TextAnchor.MiddleCenter);
   return;
  }

  float gap=10f,width=152f;
  if(Button(new Rect(50,NavigationY,width,92),"GERİ AL",false,!won,15)){session.Undo();selected=-1;armed=null;dirty=true;}
  if(Button(new Rect(50+(width+gap),NavigationY,width,92),"SIFIRLA",false,!won,15)){session.Reset();selected=-1;armed=null;settle=0;dirty=true;}
  if(Button(new Rect(50+2*(width+gap),NavigationY,width,92),"İPUCU",showHint,!won,15)){showHint=true;showLevelMap=false;showSettings=false;}
  if(Button(new Rect(50+3*(width+gap),NavigationY,width,92),"BÖLÜMLER",showLevelMap,!won,15)){showLevelMap=true;mapChapter=-1;showHint=false;showSettings=false;}
  if(Button(new Rect(50+4*(width+gap),NavigationY,width,92),"AYARLAR",showSettings,!won,15)){showSettings=true;showHint=false;showLevelMap=false;}
  Text(new Rect(50,NavigationY+116,800,24),"ACELE YOK  ·  IŞIĞI OKU  ·  GEOMETRİYİ KUR",11,muted,FontStyle.Bold,TextAnchor.MiddleCenter);
 }

 void Rotate(double degrees){
  if(selected<0||selected>=session.Pieces.Count||!PieceInfo.CanRotate(session.Pieces[selected].Kind))return;
  if(angleTween)FinishRotationTween();
  angleTween=true;angleTweenIndex=selected;angleTweenFrom=session.Pieces[selected].Angle;
  double raw=Normalize(angleTweenFrom+degrees),delta=raw-angleTweenFrom;
  if(delta>180)delta-=360;else if(delta<-180)delta+=360;
  angleTweenTo=angleTweenFrom+delta;angleTweenStart=Time.unscaledTime;
  session.BeginEdit();feedback?.Rotate();
 }

 void AnimateRotation(){
  if(!angleTween)return;
  if(angleTweenIndex<0||angleTweenIndex>=session.Pieces.Count){angleTween=false;angleTweenIndex=-1;return;}
  float t=Mathf.Clamp01((Time.unscaledTime-angleTweenStart)/.14f);
  float eased=1f-Mathf.Pow(1f-t,3f);
  session.Pieces[angleTweenIndex].Angle=Normalize(angleTweenFrom+(angleTweenTo-angleTweenFrom)*eased);
  dirty=true;
  if(t>=1f)FinishRotationTween();
 }

 void FinishRotationTween(){
  if(!angleTween)return;
  if(angleTweenIndex>=0&&angleTweenIndex<session.Pieces.Count)session.Pieces[angleTweenIndex].Angle=Normalize(angleTweenTo);
  session.EndEdit();angleTween=false;angleTweenIndex=-1;dirty=true;
 }

 void OnGUI(){
  if(session==null)return;
  GUI.matrix=Matrix4x4.TRS(new Vector3(offsetX,offsetY,0),Quaternion.identity,new Vector3(scale,scale,1));

  DrawHeader();
  DrawBoardHud();
  DrawInspector();
  DrawPalette();
  DrawNavigation();

  bool modal=showHint||showLevelMap||showSettings||(won&&showWinPanel);
  if(modal)Box(new Rect(0,0,900,1540),new Color(0,0,0,.70f));
  drawingOverlay=true;
  if(showHint)DrawHint();
  else if(showLevelMap)DrawLevelMap();
  else if(showSettings)DrawSettings();
  else if(won&&showWinPanel)DrawWin();
  drawingOverlay=false;
 }

 void DrawHint(){
  Rect r=new Rect(78,740,744,420);
  Card(r,new Color(.035f,.068f,.086f,.995f),new Color(gold.r,gold.g,gold.b,.55f));
  Pill(new Rect(110,776,136,34),"İPUCU",gold,12);
  Text(new Rect(110,830,680,38),"Bir sonraki düşünce adımı",25,ink,FontStyle.Bold);
  Text(new Rect(110,884,680,135),session.Level.Hint,19,new Color(.82f,.88f,.89f));
  Box(new Rect(110,1035,680,1),border);
  Text(new Rect(110,1052,390,30),"Çözümü vermeden yön gösterir.",13,muted);
  if(Button(new Rect(540,1046,250,100),"TAHTAYA DÖN",false,true,24,true))showHint=false;
 }

 void DrawLevelMap(){
  Rect modal=new Rect(48,112,804,1195);
  Card(modal,new Color(.025f,.052f,.068f,.995f),border);
  Text(new Rect(82,142,590,42),mapChapter<0?"BÖLÜM HARİTASI":(mapChapter+1).ToString("00")+"  ·  "+levels[mapChapter*10].Chapter.ToUpperInvariant(),27,ink,FontStyle.Bold);
  Text(new Rect(82,184,590,28),mapChapter<0?"10 dünya · 100 bölüm · ilerleme cihazında saklanır":"Bir bölüm tamamlandığında sıradaki otomatik açılır.",14,muted);
  if(Button(new Rect(704,140,112,100),"KAPAT",false,true,22))showLevelMap=false;

  int chapterCount=(levels.Length+9)/10;
  if(mapChapter<0){
   for(int i=0;i<chapterCount;i++){
    int row=i/2,col=i%2;
    Rect r=new Rect(82+col*365,240+row*180,350,154);
    int completed=ChapterCompleted(i);
    bool current=i==levelIndex/10;
    if(Button(r,"",current,true,18,current)){
     mapChapter=i;return;
    }
    Text(new Rect(r.x+20,r.y+18,r.width-40,32),(i+1).ToString("00")+"   "+levels[i*10].Chapter.ToUpperInvariant(),21,current?gold:ink,FontStyle.Bold);
    Text(new Rect(r.x+20,r.y+60,r.width-40,26),completed+" / 10 tamamlandı",17,completed==10?success:muted,completed==10?FontStyle.Bold:FontStyle.Normal);
    ProgressBar(new Rect(r.x+20,r.y+100,r.width-40,6),completed/10f,completed==10?success:(current?gold:cyan));
    string state=completed==10?"DÜNYA TAMAM":current?"DEVAM EDİYOR":IsUnlocked(i*10)?"AÇIK":"KİLİTLİ";
    Text(new Rect(r.x+20,r.y+115,r.width-40,24),state,14,completed==10?success:(current?gold:muted),FontStyle.Bold,TextAnchor.MiddleRight);
   }
   return;
  }

  int first=mapChapter*10;
  int count=Mathf.Min(10,levels.Length-first);
  for(int i=0;i<count;i++){
   int row=i/2,col=i%2,index=first+i;
   bool unlocked=IsUnlocked(index);
   bool complete=completedLevelIds.Contains(levels[index].Id);
   Rect r=new Rect(82+col*365,240+row*154,350,130);
   string status=complete?"✓  TAMAM":unlocked?(index==levelIndex?"ŞİMDİ":"AÇIK"):"KİLİTLİ";
   Color state=complete?success:(index==levelIndex?gold:muted);
   if(Button(r,"BÖLÜM "+(index+1).ToString("000")+"   "+status+Environment.NewLine+levels[index].Name,index==levelIndex,unlocked,16,index==levelIndex)){
    Load(index);return;
   }
   Text(new Rect(r.x+18,r.y+98,r.width-36,20),"Zorluk "+levels[index].Difficulty+"/10",10,state,FontStyle.Bold,TextAnchor.MiddleRight);
  }
  if(Button(new Rect(82,1128,250,100),"←  DÜNYALAR",false,true,22)){mapChapter=-1;return;}
  int chapterDone=ChapterCompleted(mapChapter);
  Text(new Rect(355,1143,270,30),chapterDone+" / 10 tamamlandı",14,chapterDone==10?success:muted,FontStyle.Bold,TextAnchor.MiddleCenter);
  ProgressBar(new Rect(372,1184,236,5),chapterDone/10f,chapterDone==10?success:gold);
 }

 void DrawSettings(){
  Rect modal=new Rect(95,218,710,1120);
  Card(modal,new Color(.025f,.052f,.068f,.995f),border);
  Text(new Rect(130,250,520,44),"AYARLAR",28,ink,FontStyle.Bold);
  Text(new Rect(130,294,520,28),"Sessiz, çevrimdışı ve dikkat dağıtmayan oyun deneyimi",14,muted);
  if(Button(new Rect(660,244,110,100),"KAPAT",false,true,22))showSettings=false;

  SettingRow(new Rect(130,350,640,128),"SES EFEKTLERİ","Dokunma, hata ve tamamlanma sesleri.",feedback!=null&&feedback.AudioEnabled?"AÇIK":"KAPALI",feedback!=null&&feedback.AudioEnabled,
   ()=>{if(feedback!=null)feedback.SetAudio(!feedback.AudioEnabled);});
  SettingRow(new Rect(130,496,640,128),"MÜZİK","Optik laboratuvar için sakin arka plan müziği.",feedback!=null&&feedback.MusicEnabled?"AÇIK":"KAPALI",feedback!=null&&feedback.MusicEnabled,
   ()=>{if(feedback!=null)feedback.SetMusic(!feedback.MusicEnabled);});
  SettingRow(new Rect(130,642,640,128),"TİTREŞİM","Bölüm tamamlandığında cihaz geri bildirimi.",feedback!=null&&feedback.HapticsEnabled?"AÇIK":"KAPALI",feedback!=null&&feedback.HapticsEnabled,
   ()=>{if(feedback!=null)feedback.SetHaptics(!feedback.HapticsEnabled);});

  Text(new Rect(130,800,640,24),"GÖRÜNÜM",18,muted,FontStyle.Bold);
  Text(new Rect(130,832,640,28),"Optik çözüm değişmez; yalnız render maliyeti ölçeklenir.",18,muted);
  VisualQualityTier[] tiers={VisualQualityTier.Auto,VisualQualityTier.Low,VisualQualityTier.Medium,VisualQualityTier.High};
  string[] labels={"OTOMATİK","DÜŞÜK","ORTA","YÜKSEK"};
  for(int i=0;i<4;i++){
   if(Button(new Rect(130+i*158,880,148,100),labels[i],VisualEnvironment.Requested==tiers[i],true,20,VisualEnvironment.Requested==tiers[i])){
    VisualEnvironment.SetQuality(tiers[i]);dirty=true;
   }
  }
  Text(new Rect(130,997,640,24),"Etkin profil: "+VisualEnvironment.QualityLabel,18,cyan,FontStyle.Bold);

  Box(new Rect(130,1040,640,1),border);
  int complete=completedLevelIds.Count;
  Text(new Rect(130,1070,330,28),"KAMPANYA İLERLEMESİ",18,muted,FontStyle.Bold);
  Text(new Rect(610,1066,160,34),complete+" / "+levels.Length,24,complete==levels.Length?success:gold,FontStyle.Bold,TextAnchor.MiddleRight);
  ProgressBar(new Rect(130,1110,640,8),complete/(float)levels.Length,complete==levels.Length?success:gold);
  Text(new Rect(130,1142,640,72),"İlerleme yerel olarak saklanır. Reklam, hesap, analytics veya zorunlu internet bağlantısı yoktur.",20,muted);
 }

 void SettingRow(Rect r,string title,string description,string state,bool on,Action toggle){
  Card(r,panel,on?new Color(cyan.r,cyan.g,cyan.b,.45f):border);
  Text(new Rect(r.x+22,r.y+18,330,26),title,20,ink,FontStyle.Bold);
  Text(new Rect(r.x+22,r.y+49,380,60),description,19,muted);
  if(Button(new Rect(r.xMax-190,r.y+14,160,100),state,on,true,24,on))toggle();
 }

 void DrawWin(){
  float t=Mathf.Clamp01((Time.unscaledTime-winShownAt)/.58f);
  float ease=1f-Mathf.Pow(1f-t,3f);
  float width=Mathf.Lerp(630f,690f,ease);
  float height=840f;
  Rect r=new Rect(450-width*.5f,770-height*.5f,width,height);
  Card(r,new Color(.027f,.061f,.075f,.997f),new Color(gold.r,gold.g,gold.b,.68f));

  float spectrumWidth=(r.width-80)/7f;
  for(int i=0;i<7;i++)Box(new Rect(r.x+40+i*spectrumWidth,r.y+34,spectrumWidth+1,5),new Color(BoardRenderer.Spectrum[i].r,BoardRenderer.Spectrum[i].g,BoardRenderer.Spectrum[i].b,.9f));

  bool final=levelIndex==levels.Length-1;
  bool chapterEnd=(levelIndex+1)%10==0;
  string kicker=final?"KAMPANYA TAMAMLANDI":chapterEnd?"DÜNYA TAMAMLANDI":"BÖLÜM TAMAMLANDI";
  Color kickerColor=final?success:gold;
  Text(new Rect(r.x+40,r.y+62,r.width-80,34),kicker,14,kickerColor,FontStyle.Bold,TextAnchor.MiddleCenter);
  Text(new Rect(r.x+42,r.y+106,r.width-84,58),final?"100 ışık problemi çözüldü.":"Harika! Işık yolu parladı.",31,ink,FontStyle.Bold,TextAnchor.MiddleCenter);
  Text(new Rect(r.x+55,r.y+166,r.width-110,54),session.Level.Name,17,muted,FontStyle.Normal,TextAnchor.MiddleCenter);

  int lit=GoalsLit();
  int active=result!=null?result.ActivePieceCount:0;
  float metricY=r.y+244;
  float metricW=(r.width-130)/3f;
  WinMetric(new Rect(r.x+45,metricY,metricW,100),"HEDEF",lit+" / "+session.Level.Goals.Length,success);
  WinMetric(new Rect(r.x+55+metricW,metricY,metricW,100),"AKTİF",active+" / "+session.Pieces.Count,cyan);
  WinMetric(new Rect(r.x+65+metricW*2,metricY,metricW,100),"ZORLUK",session.Level.Difficulty+" / 10",gold);

  string message=final
   ?"Bütün deneyler tamamlandı. Artık bölüm haritasından istediğin düzeneğe geri dönebilirsin."
   :chapterEnd
    ?"Bu dünyanın bütün bölümleri tamamlandı. Sıradaki dünya daha kıvrımlı ve daha kurnaz."
    :"Işık hedefini buldu. Çözümün kaydedildi; istersen parlayan yolu biraz daha izle.";
  Text(new Rect(r.x+55,r.y+372,r.width-110,76),message,15,new Color(.76f,.84f,.86f),FontStyle.Normal,TextAnchor.MiddleCenter);

  float campaign=(levelIndex+1)/(float)levels.Length;
  Text(new Rect(r.x+55,r.y+466,r.width-110,22),"KAMPANYA  "+(levelIndex+1)+" / "+levels.Length,11,muted,FontStyle.Bold);
  ProgressBar(new Rect(r.x+55,r.y+496,r.width-110,7),campaign,final?success:gold);

  string primary=final?"BÖLÜM HARİTASI":chapterEnd?"SONRAKİ DÜNYA  →":"SONRAKİ BÖLÜM  →";
  if(Button(new Rect(r.x+55,r.yMax-220,r.width-110,105),primary,false,true,25,true)){
   if(levelIndex<levels.Length-1)Load(levelIndex+1,true);
   else{showWinPanel=false;showLevelMap=true;mapChapter=-1;}
  }
  if(Button(new Rect(r.x+55,r.yMax-100,r.width-110,90),"ÇÖZÜMÜ İNCELE",false,true,23))showWinPanel=false;
 }

 void WinMetric(Rect r,string title,string value,Color accent){
  Box(r,new Color(surface.r,surface.g,surface.b,.95f));
  Box(new Rect(r.x,r.y,r.width,2),new Color(accent.r,accent.g,accent.b,.75f));
  Text(new Rect(r.x+8,r.y+14,r.width-16,20),title,10,muted,FontStyle.Bold,TextAnchor.MiddleCenter);
  Text(new Rect(r.x+8,r.y+40,r.width-16,40),value,21,accent,FontStyle.Bold,TextAnchor.MiddleCenter);
 }

 IEnumerator StoreCapture(){
  string output=CaptureOutputDirectory("StoreScreens");
  Directory.CreateDirectory(output);
  Screen.SetResolution(540,960,false);
  yield return new WaitForSeconds(.5f);

  int[] picks={0,29,69,59,99};
  string[] names={"01-reflection","02-color","03-combination","04-water-glass","05-mastery"};
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
  showLevelMap=true;
  yield return null;
  yield return new WaitForSeconds(.18f);
  yield return StartCoroutine(CaptureStoreFrame(Path.Combine(output,"06-level-map.png")));
  yield return new WaitForSeconds(.08f);

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

  File.WriteAllLines(Path.Combine(output,"runtime-smoke.txt"),checks);
  Application.Quit(0);
 }
}
}
