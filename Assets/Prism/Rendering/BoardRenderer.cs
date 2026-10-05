using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Prism {
public class BoardRenderer : MonoBehaviour {
 sealed class MeshBuffer {
  public readonly List<Vector3> vertices=new List<Vector3>();
  public readonly List<Color> colors=new List<Color>();
  public readonly List<Vector2> uv=new List<Vector2>();
  public readonly List<Vector2> uv2=new List<Vector2>();
  public readonly List<int> indices=new List<int>();
  public void Clear(){vertices.Clear();colors.Clear();uv.Clear();uv2.Clear();indices.Clear();}
  void Vertex(V p,Color c,Vector2 tex,float kind){vertices.Add(new Vector3((float)p.X,(float)p.Y,0));colors.Add(c);uv.Add(tex);uv2.Add(new Vector2(kind,0));}
  public void Tri(V a,V b,V c,Color color){Tri(a,b,c,color,Vector2.zero,Vector2.zero,Vector2.zero,0);}
  public void Tri(V a,V b,V c,Color color,Vector2 ua,Vector2 ub,Vector2 uc,float kind){int n=vertices.Count;Vertex(a,color,ua,kind);Vertex(b,color,ub,kind);Vertex(c,color,uc,kind);indices.Add(n);indices.Add(n+1);indices.Add(n+2);}
  public void Quad(V a,V b,V c,V d,Color color){Tri(a,b,c,color,new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),0);Tri(a,c,d,color,new Vector2(0,0),new Vector2(1,1),new Vector2(0,1),0);}
  public void Line(V a,V b,double width,Color color){V n=(b-a).Unit.Perp*width*.5;Tri(a+n,a-n,b+n,color);Tri(a-n,b-n,b+n,color);}
  public void BeamLine(V a,V b,double width,Color color){V n=(b-a).Unit.Perp*width*.5;int start=vertices.Count;Vertex(a+n,color,new Vector2(0,1),0);Vertex(a-n,color,new Vector2(0,0),0);Vertex(b+n,color,new Vector2(1,1),0);Vertex(b-n,color,new Vector2(1,0),0);indices.Add(start);indices.Add(start+1);indices.Add(start+2);indices.Add(start+1);indices.Add(start+3);indices.Add(start+2);}
  public void BeamCone(V a,V b,double startWidth,double endWidth,Color color){V side=(b-a).Unit.Perp;V na=side*(startWidth*.5),nb=side*(endWidth*.5);int start=vertices.Count;Vertex(a+na,color,new Vector2(0,1),0);Vertex(a-na,color,new Vector2(0,0),0);Vertex(b+nb,color,new Vector2(1,1),0);Vertex(b-nb,color,new Vector2(1,0),0);indices.Add(start);indices.Add(start+1);indices.Add(start+2);indices.Add(start+1);indices.Add(start+3);indices.Add(start+2);}
  public void Disc(V p,double radius,Color color,int segments=0){if(segments<=0)segments=VisualEnvironment.CircleSegments;for(int i=0;i<segments;i++)Tri(p,p+V.Angle(i*360.0/segments)*radius,p+V.Angle((i+1)*360.0/segments)*radius,color);}
  public void RadialDisc(V p,double radius,Color color,int segments=0){if(segments<=0)segments=VisualEnvironment.CircleSegments;for(int i=0;i<segments;i++){double a=i*360.0/segments,b=(i+1)*360.0/segments;Tri(p,p+V.Angle(a)*radius,p+V.Angle(b)*radius,color,Vector2.zero,new Vector2((float)System.Math.Cos(a*System.Math.PI/180),(float)System.Math.Sin(a*System.Math.PI/180)),new Vector2((float)System.Math.Cos(b*System.Math.PI/180),(float)System.Math.Sin(b*System.Math.PI/180)),1);}}
  public void Ring(V p,double radius,double width,Color color,int segments=0){if(segments<=0)segments=VisualEnvironment.CircleSegments;for(int i=0;i<segments;i++)Line(p+V.Angle(i*360.0/segments)*radius,p+V.Angle((i+1)*360.0/segments)*radius,width,color);}
  public void Arc(V p,double radius,double width,double start,double sweep,Color color){int segments=Mathf.Max(1,Mathf.CeilToInt((float)(System.Math.Abs(sweep)/360*VisualEnvironment.CircleSegments)));for(int i=0;i<segments;i++)Line(p+V.Angle(start+sweep*i/segments)*radius,p+V.Angle(start+sweep*(i+1)/segments)*radius,width,color);}
  public void GlassDisc(V p,double radius,Color color){int segments=VisualEnvironment.CircleSegments+8;for(int i=0;i<segments;i++){double a=i*360.0/segments,b=(i+1)*360.0/segments;V va=V.Angle(a),vb=V.Angle(b);Tri(p,p+va*radius,p+vb*radius,color,new Vector2(.5f,.5f),new Vector2(.5f+(float)va.X*.5f,.5f+(float)va.Y*.5f),new Vector2(.5f+(float)vb.X*.5f,.5f+(float)vb.Y*.5f),0);}}
  public void Apply(Mesh mesh){mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetUVs(1,uv2);mesh.SetTriangles(indices,0);mesh.bounds=new Bounds(Vector3.zero,new Vector3(70,70,4));}
 }
 readonly MeshBuffer boardBuffer=new MeshBuffer(),baseBuffer=new MeshBuffer(),beamBuffer=new MeshBuffer(),glassBuffer=new MeshBuffer(),waterBuffer=new MeshBuffer(),foregroundBuffer=new MeshBuffer();
 Mesh boardMesh,baseMesh,beamMesh,glassMesh,waterMesh,foregroundMesh;Material boardMaterial,baseMaterial,beamMaterial,glassMaterial,waterMaterial,foregroundMaterial;
 Texture2D labSurface,waterSurface,masterySurface,currentSurface;
 float celebration;
 public float SolutionReveal=1f;
 int interactionSelected=-1,hintStage;
 bool dragging,rotating;
 float liftWorld;
 Piece hintPiece;
 public void SetInteraction(int selected,bool dragging,bool rotating,float liftWorld){interactionSelected=selected;this.dragging=dragging;this.rotating=rotating;this.liftWorld=Mathf.Clamp(liftWorld,0,.5f);}
 public void ShowHint(int stage,Level level,IList<Piece> pieces){
  hintStage=Mathf.Clamp(stage,0,3);hintPiece=null;
  if(hintStage==0||level==null||level.Solution.Length==0)return;
  // Choose the first solution optic that is missing or differs from its current placement.
  foreach(var solution in level.Solution){bool matched=false;foreach(var current in pieces){if(current.Kind==solution.Kind&&(current.Position-solution.Position).Length<.18&&System.Math.Abs(Mathf.DeltaAngle((float)current.Angle,(float)solution.Angle))<8){matched=true;break;}}if(!matched){hintPiece=solution.Copy();break;}}
  if(hintPiece==null)hintPiece=level.Solution[0].Copy();
 }
 public void ClearHint(){hintStage=0;hintPiece=null;}

 public static readonly Color[] Spectrum={new Color(.55f,.36f,1),new Color(.28f,.48f,1),new Color(.1f,.75f,1),new Color(.35f,1,.65f),new Color(.92f,1,.45f),new Color(1,.66f,.25f),new Color(1,.32f,.33f)};
 void Awake(){
  boardMesh=CreateLayer("Board surface","PrismBoard",out boardMaterial,0);
  baseMesh=CreateLayer("Optical geometry","PrismGeometry",out baseMaterial,1);
  waterMesh=CreateLayer("Water volume","PrismWater",out waterMaterial,2);
  beamMesh=CreateLayer("Light field","PrismBeam",out beamMaterial,3);
  glassMesh=CreateLayer("Optical glass","PrismGlass",out glassMaterial,4);
  foregroundMesh=CreateLayer("Fixtures and obsidian","PrismGeometry",out foregroundMaterial,5);
  foregroundMaterial.renderQueue=3040;
  labSurface=Resources.Load<Texture2D>("Art/LabSurface");
  waterSurface=Resources.Load<Texture2D>("Art/WaterSurface");
  masterySurface=Resources.Load<Texture2D>("Art/MasterySurface");
 }
 Mesh CreateLayer(string name,string shaderResource,out Material material,int order){var go=new GameObject(name);go.transform.SetParent(transform,false);var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.MarkDynamic();go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();var shader=Resources.Load<Shader>(shaderResource);if(shader==null)shader=Resources.Load<Shader>("PrismUnlit");material=new Material(shader){name=name+" material"};renderer.sharedMaterial=material;renderer.sortingOrder=order;return mesh;}

 public void SetCelebration(float amount){
  celebration=Mathf.Clamp01(amount);
  if(beamMaterial!=null)beamMaterial.SetFloat("_Celebration",VisualEnvironment.ReducedMotion?0:celebration);
 }

 public void Draw(Level level,IList<Piece> pieces,Result result,int selected){
  boardBuffer.Clear();baseBuffer.Clear();beamBuffer.Clear();glassBuffer.Clear();waterBuffer.Clear();foregroundBuffer.Clear();
  var tier=VisualEnvironment.Resolved;
  var surface=level.WaterZones.Length>0?waterSurface:level.Difficulty>=9?masterySurface:labSurface;
  if(surface==null)surface=labSurface;
  if(currentSurface!=surface){currentSurface=surface;boardMaterial.SetTexture("_SurfaceTex",surface);}
  boardMaterial.SetFloat("_SurfaceAmount",surface!=null?.025f:0f);
  int chapter=Mathf.Clamp((level.Difficulty-1)/4,0,2);
  boardMaterial.SetColor("_Background",chapter==0?new Color(7f/255,7f/255,17f/255):chapter==1?new Color(9f/255,9f/255,21f/255):new Color(13f/255,11f/255,24f/255));
  boardMaterial.SetColor("_GridColor",VisualEnvironment.HighContrast?new Color(.27f,.24f,.38f):new Color(.10f,.09f,.16f));
  if(waterMaterial!=null)waterMaterial.SetColor("_Tint",new Color(.11f,.31f,.44f,.22f));
  if(beamMaterial!=null){beamMaterial.SetFloat("_Intensity",tier==VisualQualityTier.Low?2.1f:tier==VisualQualityTier.High?2.65f:2.4f);beamMaterial.SetFloat("_Celebration",VisualEnvironment.ReducedMotion?0:celebration);}
  if(glassMaterial!=null)glassMaterial.SetFloat("_EdgeIntensity",tier==VisualQualityTier.Low?.75f:tier==VisualQualityTier.High?1.1f:.95f);
  if(waterMaterial!=null)waterMaterial.SetFloat("_Glow",tier==VisualQualityTier.Low?.72f:tier==VisualQualityTier.High?1.2f:.96f);
  boardBuffer.Quad(new V(-5,-5),new V(5,-5),new V(5,5),new V(-5,5),Color.white);
  Color frame=new Color(.23f,.20f,.34f,.24f);
  double edge=4.72,mark=.34;
  baseBuffer.Line(new V(-edge,-edge),new V(-edge+mark,-edge),.018,frame);baseBuffer.Line(new V(-edge,-edge),new V(-edge,-edge+mark),.018,frame);
  baseBuffer.Line(new V(edge,-edge),new V(edge-mark,-edge),.018,frame);baseBuffer.Line(new V(edge,-edge),new V(edge,-edge+mark),.018,frame);
  baseBuffer.Line(new V(-edge,edge),new V(-edge+mark,edge),.018,frame);baseBuffer.Line(new V(-edge,edge),new V(-edge,edge-mark),.018,frame);
  baseBuffer.Line(new V(edge,edge),new V(edge-mark,edge),.018,frame);baseBuffer.Line(new V(edge,edge),new V(edge,edge-mark),.018,frame);
  foreach(var wz in level.WaterZones){
   waterBuffer.Quad(new V(wz.Min.X,wz.Min.Y),new V(wz.Max.X,wz.Min.Y),new V(wz.Max.X,wz.Max.Y),new V(wz.Min.X,wz.Max.Y),new Color(.72f,.92f,1,1));
   Color border=new Color(.28f,.72f,.9f,.7f);baseBuffer.Line(new V(wz.Min.X,wz.Min.Y),new V(wz.Max.X,wz.Min.Y),.024,border);baseBuffer.Line(new V(wz.Max.X,wz.Min.Y),new V(wz.Max.X,wz.Max.Y),.024,border);baseBuffer.Line(new V(wz.Max.X,wz.Max.Y),new V(wz.Min.X,wz.Max.Y),.024,border);baseBuffer.Line(new V(wz.Min.X,wz.Max.Y),new V(wz.Min.X,wz.Min.Y),.024,border);
  }
  int visibleBeams=Mathf.CeilToInt(result.Beams.Count*Mathf.Clamp01(SolutionReveal)),beamIndex=0;
  foreach(var beam in result.Beams){
   if(beamIndex++>=visibleBeams)break;
   Color c=Spectrum[beam.Band];float strength=Mathf.Clamp01((float)beam.Power*13f);
   double length=(beam.B-beam.A).Length;
   Color halo=c;halo.a=.045f*strength;beamBuffer.BeamCone(beam.A,beam.B,.14*VisualEnvironment.BeamScale,(.18+System.Math.Min(.20,length*.045))*VisualEnvironment.BeamScale,halo);
   Color body=c;body.a=.21f*strength;beamBuffer.BeamCone(beam.A,beam.B,.055*VisualEnvironment.BeamScale,(.07+System.Math.Min(.10,length*.018))*VisualEnvironment.BeamScale,body);
   Color core=c;core.a=.42f*strength;beamBuffer.BeamLine(beam.A,beam.B,.018*VisualEnvironment.BeamScale,core);
   Color cap=c;cap.a=.065f*strength;beamBuffer.RadialDisc(beam.A,.065,cap,12);beamBuffer.RadialDisc(beam.B,.065,cap,12);
  }
  foreach(var wall in level.Walls)DrawObsidianWall(wall.A,wall.B);
  Color white=new Color(.91f,.98f,1);V dir=level.Direction.Unit;
  Color sourceGlow=white;sourceGlow.a=.09f;beamBuffer.RadialDisc(level.Source,.50,sourceGlow);
  Mount(foregroundBuffer,level.Source,.36);foregroundBuffer.Ring(level.Source,.29,.055,new Color(.42f,.59f,.65f));foregroundBuffer.Disc(level.Source,.21,new Color(.055f,.095f,.13f));foregroundBuffer.Disc(level.Source,.125,new Color(.64f,.83f,.91f));foregroundBuffer.Disc(level.Source+new V(-.035,.04),.04,white);
  foregroundBuffer.Line(level.Source+dir*.19,level.Source+dir*.43,.16,new Color(.25f,.38f,.45f));foregroundBuffer.Line(level.Source+dir*.20,level.Source+dir*.44,.055,white);
  for(int i=0;i<level.Goals.Length;i++){var g=level.Goals[i];Color c=g.Band<0?white:Spectrum[g.Band];float energy=Mathf.Clamp01((float)(result.Energy[i]/g.Threshold));Color glow=c;glow.a=.025f+energy*.085f;beamBuffer.RadialDisc(g.Position,g.Radius+.26,glow);if(energy>=.999f){Color achieved=c;achieved.a=.065f;beamBuffer.RadialDisc(g.Position,g.Radius+.50,achieved);}Mount(foregroundBuffer,g.Position,g.Radius+.09);foregroundBuffer.Disc(g.Position,g.Radius,new Color(.04f,.075f,.10f));Color dim=c;dim.a=.52f;
   for(int k=0;k<8;k++)foregroundBuffer.Arc(g.Position,g.Radius,.045,k*45+4,36,dim);
   foregroundBuffer.Ring(g.Position,g.Radius*.72,.024,new Color(.22f,.34f,.4f));
   if(energy>0){c.a=.55f+energy*.4f;foregroundBuffer.Arc(g.Position,g.Radius*.78,.05,90,-360*energy,c);}c.a=.7f;foregroundBuffer.Disc(g.Position,.07+energy*.03,c);
   if(VisualEnvironment.ColorSymbols&&g.Band>=0)DrawBandSymbol(g.Position,g.Band,c);
  }
  for(int i=0;i<pieces.Count;i++){var p=pieces[i];
   if(i==selected){Color halo=new Color(.60f,.43f,1,.09f);beamBuffer.RadialDisc(p.Position,.74,halo);}
   if(i==selected&&i==interactionSelected&&dragging&&liftWorld>0){
    V anchor=p.Position;foregroundBuffer.Ring(anchor,.13,.018,new Color(.77f,.72f,.98f,.65f));
    p=p.Copy();p.Position+=new V(0,liftWorld);foregroundBuffer.Line(anchor,p.Position,.015,new Color(.66f,.59f,.88f,.45f));
   }
   V axis=V.Angle(p.Angle);Color c=white;
   if(p.Kind==Kind.Prism){var vs=Optics.Vertices(p);V shadow=new V(.04,-.07);baseBuffer.Tri(vs[0]+shadow,vs[1]+shadow,vs[2]+shadow,new Color(.015f,.035f,.055f,.9f));glassBuffer.Tri(vs[0],vs[1],vs[2],new Color(.65f,.86f,1,.9f),new Vector2(.5f,1),new Vector2(0,0),new Vector2(1,0),0);for(int k=0;k<3;k++){foregroundBuffer.Line(vs[k],vs[(k+1)%3],.065,new Color(.23f,.42f,.53f,.75f));foregroundBuffer.Line(vs[k],vs[(k+1)%3],.021,new Color(.79f,.94f,1,.85f));foregroundBuffer.Line(p.Position,vs[k],.014,new Color(.42f,.71f,.88f,.35f));}}
   else if(p.Kind==Kind.Sphere){Mount(baseBuffer,p.Position,.62);glassBuffer.GlassDisc(p.Position,.55,new Color(.74f,.91f,1,.9f));foregroundBuffer.Ring(p.Position,.55,.026,white);foregroundBuffer.Arc(p.Position,.43,.026,70,90,new Color(.65f,.86f,1,.6f));foregroundBuffer.Disc(p.Position+new V(-.17,.19),.06,new Color(1,1,1,.65f));}
   else if(p.Kind==Kind.Lens){V side=axis.Perp;for(int k=0;k<24;k++){double t=-1+k/12.0,t2=-1+(k+1)/12.0;double w=.18*(1-t*t),w2=.18*(1-t2*t2);V a=p.Position+axis*(t*.9)+side*w,b=p.Position+axis*(t*.9)-side*w,c1=p.Position+axis*(t2*.9)-side*w2,d=p.Position+axis*(t2*.9)+side*w2;Color lens=new Color(.8f,.94f,1,.75f);Vector2 ua=new Vector2((float)(t+1)*.5f,.5f+(float)w/.36f),ub=new Vector2(ua.x,.5f-(float)w/.36f),uc=new Vector2((float)(t2+1)*.5f,.5f-(float)w2/.36f),ud=new Vector2(uc.x,.5f+(float)w2/.36f);glassBuffer.Tri(a,b,c1,lens,ua,ub,uc,0);glassBuffer.Tri(a,c1,d,lens,ua,uc,ud,0);foregroundBuffer.Line(a,d,.022,white);foregroundBuffer.Line(b,c1,.022,white);}for(int end=-1;end<=1;end+=2){V socket=p.Position+axis*(end*.9);foregroundBuffer.Disc(socket,.095,new Color(.17f,.27f,.34f));foregroundBuffer.Ring(socket,.09,.014,new Color(.46f,.59f,.63f));foregroundBuffer.Disc(socket,.025,new Color(.77f,.65f,.43f));}}
   else{if(p.Kind==Kind.Green)c=Spectrum[3];if(p.Kind==Kind.Red)c=Spectrum[6];V side=axis.Perp;V a=p.Position-axis*.65,b=p.Position+axis*.65;foregroundBuffer.Line(a+side*.025,b+side*.025,.20,new Color(.08f,.14f,.19f));foregroundBuffer.Disc(a,.10,new Color(.18f,.27f,.33f));foregroundBuffer.Disc(b,.10,new Color(.18f,.27f,.33f));foregroundBuffer.Line(a,b,.095,new Color(.29f,.43f,.51f));foregroundBuffer.Line(a+side*.035,b+side*.035,.036,c);foregroundBuffer.Disc(a,.032,new Color(.76f,.65f,.43f));foregroundBuffer.Disc(b,.032,new Color(.76f,.65f,.43f));}
   if(i==selected){double radius=PieceInfo.SelectionRadius(p.Kind);Color selection=new Color(.79f,.68f,1,VisualEnvironment.HighContrast?1:.9f);
    foregroundBuffer.Arc(p.Position,radius,.028,p.Angle-65,130,selection);
    for(int tick=-1;tick<=1;tick++){V tickAxis=V.Angle(p.Angle+tick*45);foregroundBuffer.Line(p.Position+tickAxis*(radius-.04),p.Position+tickAxis*(radius+.05),.018,selection);}
    if(rotating||VisualEnvironment.PrecisionMode)foregroundBuffer.Line(p.Position-axis*.18,p.Position+axis*.18,.015,selection);if(PieceInfo.CanRotate(p.Kind)){V handle=p.Position+axis*radius;foregroundBuffer.Line(p.Position+axis*(radius-.20),handle,.025,new Color(.97f,.77f,.38f,.8f));foregroundBuffer.Disc(handle,.112,new Color(.06f,.12f,.15f),Mathf.Max(14,VisualEnvironment.CircleSegments/2));foregroundBuffer.Ring(handle,.105,.026,new Color(1,.83f,.5f),Mathf.Max(14,VisualEnvironment.CircleSegments/2));foregroundBuffer.Disc(handle,.036,new Color(.97f,.84f,.56f));}}
  }
  DrawHint();
  boardBuffer.Apply(boardMesh);baseBuffer.Apply(baseMesh);waterBuffer.Apply(waterMesh);beamBuffer.Apply(beamMesh);glassBuffer.Apply(glassMesh);foregroundBuffer.Apply(foregroundMesh);
 }
 void DrawObsidianWall(V a,V b){
  V axis=(b-a).Unit,side=axis.Perp;double length=(b-a).Length;
  foregroundBuffer.Line(a+new V(.035,-.055),b+new V(.035,-.055),.42,new Color(.015f,.013f,.025f,.8f));
  int count=System.Math.Max(1,(int)System.Math.Ceiling(length/.52));double step=length/count;
  for(int i=0;i<count;i++){
   double start=i*step+.014,end=(i+1)*step-.014;if(end<=start)continue;
   double chamfer=System.Math.Min(.06,(end-start)*.2),half=.18;
   V[] poly={a+axis*(start+chamfer)+side*half,a+axis*(end-chamfer)+side*half,a+axis*end+side*(half-chamfer),a+axis*end-side*(half-chamfer),a+axis*(end-chamfer)-side*half,a+axis*(start+chamfer)-side*half,a+axis*start-side*(half-chamfer),a+axis*start+side*(half-chamfer)};
   V center=a+axis*((start+end)*.5);Color face=new Color(.085f,.073f,.12f);
   for(int k=0;k<8;k++){foregroundBuffer.Tri(center,poly[k],poly[(k+1)%8],face);foregroundBuffer.Line(poly[k],poly[(k+1)%8],.018,k<3?new Color(.25f,.22f,.34f):new Color(.035f,.029f,.053f));}
   foregroundBuffer.Line(a+axis*(start+chamfer)+side*.12,a+axis*(end-chamfer)+side*.12,.012,new Color(.34f,.29f,.44f,.45f));
  }
 }
 void DrawBandSymbol(V p,int band,Color color){
  // Seven silhouettes remain distinguishable without hue: star, diamond, bars, triangle, square, cross, ring.
  color.a=1;double r=.13;Color backing=new Color(.035f,.025f,.065f,1);foregroundBuffer.Disc(p,.17,backing);
  if(band==6){foregroundBuffer.Ring(p,r,.025,color);return;}
  if(band==2){for(int i=-1;i<=1;i++)foregroundBuffer.Line(p+new V(i*.07,-r),p+new V(i*.07,r),.024,color);return;}
  if(band==5){foregroundBuffer.Line(p-new V(r,0),p+new V(r,0),.03,color);foregroundBuffer.Line(p-new V(0,r),p+new V(0,r),.03,color);return;}
  int corners=band==0?10:band==3?3:4;double rotation=band==4?45:90;
  for(int i=0;i<corners;i++){double r0=band==0&&i%2==1?r*.45:r,r1=band==0&&(i+1)%2==1?r*.45:r;foregroundBuffer.Line(p+V.Angle(rotation+i*360.0/corners)*r0,p+V.Angle(rotation+(i+1)*360.0/corners)*r1,.023,color);}
 }
 void DrawHint(){
  if(hintPiece==null||hintStage==0)return;
  V p=hintPiece.Position;Color c=new Color(.83f,.76f,1,.72f);
  // Stage one locates the area, stage two shows the silhouette, stage three adds its exact orientation.
  foregroundBuffer.Arc(p,hintStage==1?.85:.72,.021,25,130,c);
  foregroundBuffer.Arc(p,hintStage==1?.85:.72,.021,205,130,c);
  if(hintStage<2)return;
  if(hintPiece.Kind==Kind.Prism){var vs=Optics.Vertices(hintPiece);for(int i=0;i<3;i++)foregroundBuffer.Line(vs[i],vs[(i+1)%3],.016,c);}
  else if(hintPiece.Kind==Kind.Sphere)foregroundBuffer.Ring(p,.55,.016,c);
  else {V axis=V.Angle(hintStage>=3?hintPiece.Angle:0);foregroundBuffer.Line(p-axis*.6,p+axis*.6,.028,c);}
  if(hintStage>=3&&PieceInfo.CanRotate(hintPiece.Kind)){V axis=V.Angle(hintPiece.Angle);foregroundBuffer.Line(p,p+axis*.85,.014,c);foregroundBuffer.Disc(p+axis*.85,.055,c);}
 }
 void Mount(MeshBuffer buffer,V p,double radius){buffer.Disc(p+new V(.035,-.055),radius+.03,new Color(.012f,.028f,.043f,.85f));buffer.Disc(p,radius,new Color(.11f,.18f,.24f));buffer.Ring(p,radius,.018,new Color(.28f,.39f,.46f));for(int k=0;k<3;k++)buffer.Disc(p+V.Angle(90+k*120)*(radius*.84),.022,new Color(.57f,.65f,.66f));}
 void OnDestroy(){DestroyMaterial(boardMaterial);DestroyMaterial(baseMaterial);DestroyMaterial(beamMaterial);DestroyMaterial(glassMaterial);DestroyMaterial(waterMaterial);DestroyMaterial(foregroundMaterial);}
 void DestroyMaterial(Material material){if(material==null)return;if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}
}
}
