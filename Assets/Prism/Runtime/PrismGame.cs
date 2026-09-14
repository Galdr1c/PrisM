using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Prism {
public class PrismGame : MonoBehaviour {
 Level[] levels;Session session;Result result;BoardRenderer board;Camera cam;
 int levelIndex,selected=-1,completedMask;Kind? armed;bool dragging,rotating,showHint,won,dirty=true,smoke;float settle;
 Vector2 dragOffset;double startAngle;V startDirection;float scale,offsetX,offsetY;Font font;
 readonly Color ink=new Color(.9f,.94f,.94f),muted=new Color(.47f,.59f,.64f),gold=new Color(.91f,.77f,.49f),panel=new Color(.065f,.10f,.13f);
 public static string PieceName(Kind k){switch(k){case Kind.Mirror:return "Ayna";case Kind.Prism:return "Prizma";case Kind.Green:return "Yeşil seçici";case Kind.Red:return "Kırmızı seçici";default:return "Lens";}}
 void Start(){
  Application.targetFrameRate=60;Screen.sleepTimeout=SleepTimeout.NeverSleep;font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
  levels=Levels.Create();cam=Camera.main;if(!cam){cam=new GameObject("Camera").AddComponent<Camera>();cam.tag="MainCamera";}cam.orthographic=true;cam.transform.position=new Vector3(0,0,-10);cam.backgroundColor=new Color(.025f,.042f,.06f);cam.clearFlags=CameraClearFlags.SolidColor;
  smoke=Array.IndexOf(Environment.GetCommandLineArgs(),"-prismSmoke")>=0;
  board=new GameObject("Light laboratory").AddComponent<BoardRenderer>();completedMask=PlayerPrefs.GetInt("prism.completed",0);Load(Mathf.Clamp(PlayerPrefs.GetInt("prism.last",0),0,levels.Length-1));
  if(smoke)StartCoroutine(Smoke());
 }
 void Layout(){scale=Mathf.Min(Screen.width/900f,Screen.height/1340f);offsetX=(Screen.width-900f*scale)*0.5f;offsetY=(Screen.height-1340f*scale)*0.5f;cam.orthographicSize=Screen.height/(scale*160f);cam.transform.position=new Vector3(0,(offsetY+600f*scale-Screen.height*0.5f)/(80f*scale),-10f);}
 void Load(int index){levelIndex=index;session=new Session(levels[index]);selected=-1;armed=null;showHint=false;won=false;settle=0;dirty=false;Solve();if(!smoke){PlayerPrefs.SetInt("prism.last",index);PlayerPrefs.Save();}}
 void Solve(){result=Optics.Solve(session.Level,session.Pieces);board.Draw(session.Level,session.Pieces,result,selected);dirty=false;}
 void Update(){if(session==null)return;Layout();if(!smoke)Pointer();if(dirty)Solve();if(!won&&result!=null&&result.Complete){settle+=Time.deltaTime;if(settle>.65f){won=true;if(!smoke){completedMask|=1<<levelIndex;PlayerPrefs.SetInt("prism.completed",completedMask);PlayerPrefs.Save();}}}else if(result==null||!result.Complete)settle=0;}
 Vector2 Design(Vector2 screen)=>new Vector2((screen.x-offsetX)/scale,(Screen.height-screen.y-offsetY)/scale);
 V World(Vector2 p)=>new V((p.x-450)/80,(600-p.y)/80);
 void Pointer(){
  if(showHint)return;
  Vector2 raw;bool down,held,up;
  if(Touchscreen.current!=null&&(Touchscreen.current.primaryTouch.press.isPressed||Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)){var t=Touchscreen.current.primaryTouch;raw=t.position.ReadValue();down=t.press.wasPressedThisFrame;held=t.press.isPressed;up=t.press.wasReleasedThisFrame;}
  else if(Mouse.current!=null){var m=Mouse.current;raw=m.position.ReadValue();down=m.leftButton.wasPressedThisFrame;held=m.leftButton.isPressed;up=m.leftButton.wasReleasedThisFrame;if(selected>=0&&Math.Abs(m.scroll.ReadValue().y)>0.01f&&!won){session.BeginEdit();session.Pieces[selected].Angle=Normalize(session.Pieces[selected].Angle+Math.Sign(m.scroll.ReadValue().y));session.EndEdit();dirty=true;}}
  else return;
  Vector2 p=Design(raw);V w=World(p);bool onBoard=new Rect(50,200,800,800).Contains(p);
  if(down&&onBoard&&!won){
   if(armed.HasValue){if(ValidPosition(w)&&session.Place(armed.Value,w)){selected=session.Pieces.Count-1;armed=null;dirty=true;}return;}
   if(selected>=0&&(w-session.Pieces[selected].Position).Length>.8&&(w-session.Pieces[selected].Position).Length<1.25){rotating=true;dragging=false;startAngle=session.Pieces[selected].Angle;startDirection=w-session.Pieces[selected].Position;session.BeginEdit();}
   else{selected=-1;double closest=.7;for(int i=0;i<session.Pieces.Count;i++){double dist=(w-session.Pieces[i].Position).Length;if(dist<closest){closest=dist;selected=i;}}
    if(selected>=0){session.BeginEdit();dragging=true;V delta=session.Pieces[selected].Position-w;dragOffset=new Vector2((float)delta.X,(float)delta.Y);}dirty=true;
   }
  }
  if(held&&selected>=0&&!won){var piece=session.Pieces[selected];if(dragging){V proposed=w+new V(dragOffset.x,dragOffset.y);proposed=new V(Math.Max(-4.25,Math.Min(4.25,proposed.X)),Math.Max(-4.25,Math.Min(4.25,proposed.Y)));if(ValidPosition(proposed)){piece.Position=proposed;dirty=true;}}if(rotating){V dir=w-piece.Position;piece.Angle=Normalize(startAngle+(Math.Atan2(dir.Y,dir.X)-Math.Atan2(startDirection.Y,startDirection.X))*180/Math.PI);dirty=true;}}
  if(up&&(dragging||rotating)){session.EndEdit();dragging=rotating=false;}
 }
 bool ValidPosition(V p){if(Math.Abs(p.X)>4.25||Math.Abs(p.Y)>4.25)return false;foreach(var wall in session.Level.Walls){V e=wall.B-wall.A;double t=Math.Max(0,Math.Min(1,V.Dot(p-wall.A,e)/V.Dot(e,e)));if((p-(wall.A+e*t)).Length<.3)return false;}return true;}
 static double Normalize(double angle)=>(angle%360+360)%360;
 GUIStyle TextStyle(int size,Color c,FontStyle weight=FontStyle.Normal,TextAnchor align=TextAnchor.MiddleLeft){return new GUIStyle(GUI.skin.label){font=font,fontSize=size,fontStyle=weight,alignment=align,normal={textColor=c},wordWrap=true};}
 void Text(Rect r,string text,int size,Color c,FontStyle weight=FontStyle.Normal,TextAnchor align=TextAnchor.MiddleLeft){GUI.Label(r,text,TextStyle(size,c,weight,align));}
 void Box(Rect r,Color c){Color old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
 bool Button(Rect r,string label,bool active=false,bool enabled=true){Box(r,active?new Color(.2f,.25f,.24f):panel);Box(new Rect(r.x,r.yMax-1,r.width,1),active?gold:new Color(.15f,.23f,.27f));Text(r,label,18,enabled?(active?gold:ink):muted,FontStyle.Normal,TextAnchor.MiddleCenter);bool old=GUI.enabled;GUI.enabled=enabled;bool hit=GUI.Button(r,GUIContent.none,GUIStyle.none);GUI.enabled=old;return hit;}
 void Rotate(double degrees){if(selected<0)return;session.BeginEdit();session.Pieces[selected].Angle=Normalize(session.Pieces[selected].Angle+degrees);session.EndEdit();dirty=true;}
 void OnGUI(){if(session==null)return;GUI.matrix=Matrix4x4.TRS(new Vector3(offsetX,offsetY,0),Quaternion.identity,new Vector3(scale,scale,1));
  Text(new Rect(50,24,300,32),"P R I S M",25,ink,FontStyle.Bold);Text(new Rect(600,24,250,32),"I Ş I K  A T Ö L Y E S İ",12,muted,FontStyle.Normal,TextAnchor.MiddleRight);
  Box(new Rect(50,74,800,1),new Color(.17f,.23f,.26f));
  Text(new Rect(50,90,570,25),"DENEY  "+(levelIndex+1).ToString("00")+" / 05",13,gold);
  Text(new Rect(50,120,670,44),session.Level.Name,34,ink,FontStyle.Bold);Text(new Rect(50,164,700,27),session.Level.Lesson,17,muted);
  int lit=0;if(result!=null&&result.Energy!=null){int count=Mathf.Min(result.Energy.Length,session.Level.Goals.Length);for(int i=0;i<count;i++)if(result.Energy[i]>=session.Level.Goals[i].Threshold)lit++;}
  Text(new Rect(730,124,120,42),lit+" / "+session.Level.Goals.Length,24,gold,FontStyle.Normal,TextAnchor.MiddleRight);
  Color border=new Color(.17f,.25f,.29f);Box(new Rect(49,199,802,1),border);Box(new Rect(49,1000,802,1),border);Box(new Rect(49,200,1,800),border);Box(new Rect(850,200,1,800),border);
  if(armed.HasValue)Text(new Rect(70,216,650,30),PieceName(armed.Value)+" yerleştirmek için alana dokun",16,gold);
  else if(session.Pieces.Count==0)Text(new Rect(70,216,650,30),"Başlamak için aşağıdan bir parça seç",16,muted);
  if(result!=null&&result.Energy!=null){int count=Mathf.Min(result.Energy.Length,session.Level.Goals.Length);for(int i=0;i<count;i++){var g=session.Level.Goals[i];float gx=450+(float)g.Position.X*80,gy=600-(float)g.Position.Y*80;int percent=Mathf.Min(100,Mathf.RoundToInt((float)(result.Energy[i]/g.Threshold)*100));Text(new Rect(gx-45,gy+(float)g.Radius*80+10,90,22),percent+"%",12,muted,FontStyle.Normal,TextAnchor.MiddleCenter);}}
  Box(new Rect(50,1018,800,72),panel);
  if(selected>=0&&selected<session.Pieces.Count&&!won){var p=session.Pieces[selected];Text(new Rect(70,1026,215,26),PieceName(p.Kind),18,ink);Text(new Rect(70,1053,200,22),"Sürükle · Halkayla döndür",12,muted);if(Button(new Rect(310,1030,68,48),"−15°"))Rotate(-15);if(Button(new Rect(384,1030,56,48),"−1°"))Rotate(-1);Text(new Rect(443,1030,122,48),p.Angle.ToString("0.0")+"°",23,gold,FontStyle.Normal,TextAnchor.MiddleCenter);if(Button(new Rect(568,1030,56,48),"+1°"))Rotate(1);if(Button(new Rect(630,1030,68,48),"+15°"))Rotate(15);if(Button(new Rect(718,1030,114,48),"Kaldır")){session.Remove(selected);selected=-1;dirty=true;}}
  else{Text(new Rect(72,1030,750,46),won?"Bütün hedefler aydınlandı.":"Parçayı seç, yerleştir ve ışığın yolunu değiştir.",17,won?gold:muted);}
  Text(new Rect(50,1101,300,24),"OPTİK PARÇALAR",12,muted);var kinds=new List<Kind>();foreach(var k in session.Level.Stock)if(!kinds.Contains(k))kinds.Add(k);
  for(int i=0;i<kinds.Count;i++){var k=kinds[i];int n=session.Remaining(k);Rect r=new Rect(50+i*192,1134,180,70);if(Button(r,PieceName(k)+"  ·  "+n,armed==k,n>0&&!won)){armed=armed==k?(Kind?)null:k;selected=-1;dirty=true;}}
  if(Button(new Rect(50,1220,140,45),"Geri al",false,!won)){session.Undo();selected=-1;armed=null;dirty=true;}
  if(Button(new Rect(202,1220,140,45),"Sıfırla")){session.Reset();selected=-1;armed=null;won=false;settle=0;dirty=true;}
  if(Button(new Rect(354,1220,140,45),"İpucu",showHint))showHint=!showHint;
  for(int i=0;i<5;i++)if(Button(new Rect(576+i*56,1220,48,45),(completedMask&(1<<i))!=0?"✓":(i+1).ToString(),i==levelIndex))Load(i);
  Text(new Rect(50,1290,800,22),"ACELE YOK.  IŞIĞI TAKİP ET.",12,muted,FontStyle.Normal,TextAnchor.MiddleCenter);
  if(showHint&&!won){Box(new Rect(80,820,740,150),new Color(.08f,.13f,.16f,.98f));Text(new Rect(100,830,700,28),"KÜÇÜK BİR İPUCU",12,gold);Text(new Rect(100,862,700,60),session.Level.Hint,18,ink);if(Button(new Rect(625,927,170,30),"Anladım"))showHint=false;}
  if(won){Box(new Rect(180,390,540,290),new Color(.045f,.085f,.105f,.98f));Text(new Rect(215,415,470,30),"DENEY TAMAMLANDI",13,gold,FontStyle.Normal,TextAnchor.MiddleCenter);Text(new Rect(215,455,470,70),"Işık yolunu buldu.",31,ink,FontStyle.Bold,TextAnchor.MiddleCenter);Text(new Rect(220,535,460,35),"Her doğru açı, yeni bir keşif.",17,muted,FontStyle.Normal,TextAnchor.MiddleCenter);if(Button(new Rect(240,598,420,54),levelIndex<4?"Sonraki deney  →":"Yeniden keşfet  →",true))Load((levelIndex+1)%5);}
 }
 IEnumerator Smoke(){
  string output=Path.Combine(Application.dataPath,"../../TestResults");var args=Environment.GetCommandLineArgs();int arg=Array.IndexOf(args,"-captureDir");if(arg>=0&&arg+1<args.Length)output=args[arg+1];Directory.CreateDirectory(output);yield return null;
  Load(0);yield return new WaitForSeconds(.3f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"01-start.png"));yield return new WaitForSeconds(.3f);
  var checks=new List<string>();for(int i=0;i<levels.Length;i++){Load(i);foreach(var piece in levels[i].Solution){bool placed=session.Place(piece.Kind,piece.Position);if(!placed)throw new Exception("Inventory failure");session.BeginEdit();session.Pieces[session.Pieces.Count-1].Angle=piece.Angle;session.EndEdit();}dirty=true;yield return null;if(!result.Complete)throw new Exception("Unsolved level "+i);checks.Add("PASS runtime level "+(i+1));ScreenCapture.CaptureScreenshot(Path.Combine(output,string.Format("level-{0:00}-solved.png",i+1)));yield return new WaitForSeconds(.2f);session.Undo();dirty=true;yield return null;session.Reset();dirty=true;yield return null;if(result.Complete)throw new Exception("Reset failed");checks.Add("PASS runtime reset "+(i+1));}
  Load(4);session.Reveal();selected=1;dirty=true;yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(output,"02-optics.png"));yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(output,"03-complete.png"));yield return new WaitForSeconds(.5f);File.WriteAllLines(Path.Combine(output,"runtime-smoke.txt"),checks);Application.Quit(0);
 }
}
}
