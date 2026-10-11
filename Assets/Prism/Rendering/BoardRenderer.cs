using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Prism {
public class BoardRenderer : MonoBehaviour {
 static readonly Color StoneFace=new Color(56f/255,51f/255,79f/255);
 static readonly Color StoneLight=new Color(75f/255,69f/255,113f/255);
 static readonly Color StoneDark=new Color(38f/255,34f/255,62f/255);
 static readonly Color LavenderEdge=new Color(111f/255,102f/255,150f/255);
 static readonly Color NightInterior=new Color(13f/255,11f/255,38f/255);
 static readonly Vector2[] PrismFacetUv={new Vector2(.5f,1),new Vector2(0,0),new Vector2(1,0)};
 static readonly Color[] PrismFacetColors={new Color(.71f,.80f,1,.94f),new Color(.55f,.52f,.91f,.94f),new Color(.76f,.64f,.97f,.94f)};
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
  public void Line(V a,V b,double width,Color color,float kind=0){V n=(b-a).Unit.Perp*width*.5;Tri(a+n,a-n,b+n,color,Vector2.zero,Vector2.zero,Vector2.zero,kind);Tri(a-n,b-n,b+n,color,Vector2.zero,Vector2.zero,Vector2.zero,kind);}
  public void BeamLine(V a,V b,double width,Color color){V n=(b-a).Unit.Perp*width*.5;int start=vertices.Count;Vertex(a+n,color,new Vector2(0,1),2);Vertex(a-n,color,new Vector2(0,0),2);Vertex(b+n,color,new Vector2(1,1),2);Vertex(b-n,color,new Vector2(1,0),2);indices.Add(start);indices.Add(start+1);indices.Add(start+2);indices.Add(start+1);indices.Add(start+3);indices.Add(start+2);}
  public void BeamCone(V a,V b,double startWidth,double endWidth,Color color){V side=(b-a).Unit.Perp;V na=side*(startWidth*.5),nb=side*(endWidth*.5);int start=vertices.Count;Vertex(a+na,color,new Vector2(0,1),0);Vertex(a-na,color,new Vector2(0,0),0);Vertex(b+nb,color,new Vector2(1,1),0);Vertex(b-nb,color,new Vector2(1,0),0);indices.Add(start);indices.Add(start+1);indices.Add(start+2);indices.Add(start+1);indices.Add(start+3);indices.Add(start+2);}
  public void Disc(V p,double radius,Color color,int segments=0){if(segments<=0)segments=VisualEnvironment.CircleSegments;for(int i=0;i<segments;i++)Tri(p,p+V.Angle(i*360.0/segments)*radius,p+V.Angle((i+1)*360.0/segments)*radius,color);}
  public void RadialDisc(V p,double radius,Color color,int segments=0){if(segments<=0)segments=VisualEnvironment.CircleSegments;for(int i=0;i<segments;i++){double a=i*360.0/segments,b=(i+1)*360.0/segments;Tri(p,p+V.Angle(a)*radius,p+V.Angle(b)*radius,color,Vector2.zero,new Vector2((float)System.Math.Cos(a*System.Math.PI/180),(float)System.Math.Sin(a*System.Math.PI/180)),new Vector2((float)System.Math.Cos(b*System.Math.PI/180),(float)System.Math.Sin(b*System.Math.PI/180)),1);}}
  public void Ring(V p,double radius,double width,Color color,int segments=0){if(segments<=0)segments=VisualEnvironment.CircleSegments;for(int i=0;i<segments;i++)Line(p+V.Angle(i*360.0/segments)*radius,p+V.Angle((i+1)*360.0/segments)*radius,width,color);}
  public void Arc(V p,double radius,double width,double start,double sweep,Color color,float kind=0){int segments=Mathf.Max(1,Mathf.CeilToInt((float)(System.Math.Abs(sweep)/360*VisualEnvironment.CircleSegments)));for(int i=0;i<segments;i++)Line(p+V.Angle(start+sweep*i/segments)*radius,p+V.Angle(start+sweep*(i+1)/segments)*radius,width,color,kind);}
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
 Piece invalidPlacementGhost;
 struct WallImpact {public V Position;public Color Color;public float Power;}
 readonly List<WallImpact> wallImpacts=new List<WallImpact>(8);
 V? pointerAnchor;
 int previewIndex=-1;
 bool previewAllowed=true;
 public void SetPlacementPreview(int index,bool allowed){previewIndex=index;previewAllowed=allowed;}
 public void SetInvalidPlacementGhost(Piece ghost){invalidPlacementGhost=ghost?.Copy();}
 public void SetPointerAnchor(V? anchor){pointerAnchor=anchor;}
 public void SetInteraction(int selected,bool dragging,bool rotating,float liftWorld){interactionSelected=selected;this.dragging=dragging;this.rotating=rotating;this.liftWorld=Mathf.Clamp(liftWorld,0,.8f);}
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
  boardMaterial.SetFloat("_SurfaceAmount",surface!=null?.012f:0f);
  boardMaterial.SetColor("_Background",NightInterior);
  boardMaterial.SetColor("_EdgeBackground",new Color(7f/255,6f/255,26f/255));
  boardMaterial.SetColor("_GridColor",VisualEnvironment.HighContrast?new Color(.82f,.77f,1):new Color(180f/255,169f/255,1));
  // Dot alpha is blended in linear light; keep its perceived contrast close to the SVG reference.
  boardMaterial.SetFloat("_GridStrength",VisualEnvironment.HighContrast?.04f:.009f);
  if(waterMaterial!=null)waterMaterial.SetColor("_Tint",new Color(.11f,.31f,.44f,.22f));
  if(beamMaterial!=null){
   float brightness=Mathf.Lerp(.8f,1.2f,Mathf.InverseLerp(.5f,1.5f,VisualEnvironment.BeamScale));
   beamMaterial.SetFloat("_Intensity",(tier==VisualQualityTier.Low?1.25f:tier==VisualQualityTier.High?1.65f:1.5f)*brightness);
   beamMaterial.SetFloat("_Celebration",VisualEnvironment.ReducedMotion?0:celebration);
   var center=selected>=0&&selected<pieces.Count?pieces[selected].Position:new V();
   beamMaterial.SetVector("_SelectedPosition",new Vector4((float)center.X,(float)center.Y,0,selected>=0&&selected<pieces.Count?1:0));
  }
  if(glassMaterial!=null){glassMaterial.SetColor("_Tint",new Color(.77f,.79f,1,.50f));glassMaterial.SetFloat("_EdgeIntensity",tier==VisualQualityTier.Low?.45f:tier==VisualQualityTier.High?.65f:.55f);}
  if(waterMaterial!=null)waterMaterial.SetFloat("_Glow",tier==VisualQualityTier.Low?.72f:tier==VisualQualityTier.High?1.2f:.96f);
  boardBuffer.Quad(new V(-5,-5),new V(5,-5),new V(5,5),new V(-5,5),Color.white);
  foreach(var wz in level.WaterZones){
   waterBuffer.Quad(new V(wz.Min.X,wz.Min.Y),new V(wz.Max.X,wz.Min.Y),new V(wz.Max.X,wz.Max.Y),new V(wz.Min.X,wz.Max.Y),new Color(.72f,.92f,1,1));
   Color border=new Color(.28f,.72f,.9f,.7f);baseBuffer.Line(new V(wz.Min.X,wz.Min.Y),new V(wz.Max.X,wz.Min.Y),.024,border);baseBuffer.Line(new V(wz.Max.X,wz.Min.Y),new V(wz.Max.X,wz.Max.Y),.024,border);baseBuffer.Line(new V(wz.Max.X,wz.Max.Y),new V(wz.Min.X,wz.Max.Y),.024,border);baseBuffer.Line(new V(wz.Min.X,wz.Max.Y),new V(wz.Min.X,wz.Min.Y),.024,border);
  }
  int visibleBeams=Mathf.CeilToInt(result.Beams.Count*Mathf.Clamp01(SolutionReveal)),beamIndex=0;
  foreach(var beam in result.Beams){
   if(beamIndex++>=visibleBeams)break;
   // Seven spectral channels and thirteen integration lanes share ONE emitter's energy.
   // Do not normalize every lane back to full brightness: additive overlap washes out colors.
   Color c=Spectrum[beam.Band];
   float strength=Mathf.Clamp01((float)beam.Power*Optics.SourceSamples/Optics.SpectralBands);
   if((beam.B-beam.A).Length<.0005)continue;
   float width=VisualEnvironment.BeamScale;
   Color halo=c;halo.a=.07f*strength;beamBuffer.BeamCone(beam.A,beam.B,.15*width,.15*width,halo);
   Color body=c;body.a=.28f*strength;beamBuffer.BeamCone(beam.A,beam.B,.06*width,.06*width,body);
   Color core=c;core.a=.50f*strength;beamBuffer.BeamLine(beam.A,beam.B,.016*width,core);
  }
  foreach(var wall in level.Walls)DrawObsidianWall(wall,result);
  if(dragging)DrawPlacementBoundary();
  if(pointerAnchor.HasValue&&selected>=0&&selected<pieces.Count){
   foregroundBuffer.Ring(pointerAnchor.Value,.12,.018,new Color(.78f,.71f,1,VisualEnvironment.HighContrast?1:.7f));
   foregroundBuffer.Line(pointerAnchor.Value,pieces[selected].Position,.014,new Color(.78f,.71f,1,VisualEnvironment.HighContrast?.8f:.45f));
  }
  Color white=new Color(.96f,.95f,.91f);V dir=level.Direction.Unit,sourceSide=dir.Perp;
  // level.Source is the leading optical aperture, not the center of a circular lamp.
  // Wide lens lessons require an equally wide slit emitter, never a large beam from a tiny disc.
  double aperture=System.Math.Max(.08,level.Width);
  V bodyCenter=level.Source-dir*.37;
  if(aperture>.54){
   double half=aperture*.5+.10;
   V back=bodyCenter-dir*.22;
   foregroundBuffer.Quad(back-sourceSide*half,back+sourceSide*half,level.Source+sourceSide*half,level.Source-sourceSide*half,StoneFace);
   foregroundBuffer.Line(back-sourceSide*half,back+sourceSide*half,.05,LavenderEdge);
   foregroundBuffer.Line(level.Source-sourceSide*(aperture*.5),level.Source+sourceSide*(aperture*.5),.075,white);
   foregroundBuffer.Line(level.Source-sourceSide*half,level.Source-sourceSide*half-dir*.42,.038,new Color(.48f,.58f,.65f));
   foregroundBuffer.Line(level.Source+sourceSide*half,level.Source+sourceSide*half-dir*.42,.038,new Color(.48f,.58f,.65f));
  }else{
   foregroundBuffer.Disc(bodyCenter+new V(.022,-.028),.34,StoneDark);
   foregroundBuffer.Disc(bodyCenter,.31,StoneFace);
   foregroundBuffer.Ring(bodyCenter,.30,.025,VisualEnvironment.HighContrast?new Color(.82f,.77f,1):LavenderEdge);
   foregroundBuffer.Disc(bodyCenter,.235,NightInterior);
   foregroundBuffer.Ring(bodyCenter,.22,.025,new Color(.71f,.65f,.91f));
   foregroundBuffer.Disc(bodyCenter,.15,white);
   foregroundBuffer.Line(bodyCenter+dir*.18,level.Source,.12,new Color(.33f,.40f,.47f));
   foregroundBuffer.Line(level.Source-sourceSide*(aperture*.5),level.Source+sourceSide*(aperture*.5),.065,white);
  }
  Color sourceGlow=white;sourceGlow.a=.026f;beamBuffer.RadialDisc(level.Source,.19,sourceGlow);
  for(int i=0;i<level.Goals.Length;i++){var g=level.Goals[i];Color c=g.Band<0?white:Spectrum[g.Band];float energy=Mathf.Clamp01((float)(result.Energy[i]/g.Threshold));Color glow=c;glow.a=.025f+energy*.085f;beamBuffer.RadialDisc(g.Position,g.Radius+.26,glow);if(energy>=.999f){Color achieved=c;achieved.a=.065f;beamBuffer.RadialDisc(g.Position,g.Radius+.50,achieved);}ReceiverBase(g.Position,g.Radius+.09);foregroundBuffer.Disc(g.Position,g.Radius,NightInterior);Color dim=c;dim.a=.52f;
   float pulse=energy>=.7f&&energy<.999f&&!VisualEnvironment.ReducedMotion?2f:0;
   for(int k=0;k<8;k++){Color segment=dim;segment.a=energy>=1||energy>=.30f&&k<Mathf.CeilToInt(energy*8)? .55f+energy*.35f:.22f;foregroundBuffer.Arc(g.Position,g.Radius,.045,k*45+4,36,segment,pulse);}
   foregroundBuffer.Ring(g.Position,g.Radius*.72,.024,LavenderEdge);
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
   if(p.Kind==Kind.Prism){
    var vs=Optics.Vertices(p);V shadow=new V(.04,-.07);
    baseBuffer.Tri(vs[0]+shadow,vs[1]+shadow,vs[2]+shadow,new Color(.04f,.03f,.09f,.65f));
    // Three visual facets share the original optical triangle and its centroid.
    for(int k=0;k<3;k++){
     glassBuffer.Tri(p.Position,vs[k],vs[(k+1)%3],PrismFacetColors[k],new Vector2(.5f,1f/3),PrismFacetUv[k],PrismFacetUv[(k+1)%3],0);
     foregroundBuffer.Line(vs[k],vs[(k+1)%3],.065,new Color(.32f,.30f,.50f,.75f));
     foregroundBuffer.Line(vs[k],vs[(k+1)%3],.021,new Color(.79f,.90f,1,VisualEnvironment.HighContrast?1:.9f));
     foregroundBuffer.Line(p.Position,vs[k],.014,new Color(.77f,.76f,1,.45f));
    }
   }
   else if(p.Kind==Kind.Sphere){Mount(baseBuffer,p.Position,.62);glassBuffer.GlassDisc(p.Position,.55,new Color(.74f,.91f,1,.9f));foregroundBuffer.Ring(p.Position,.55,.026,white);foregroundBuffer.Arc(p.Position,.43,.026,70,90,new Color(.65f,.86f,1,.6f));foregroundBuffer.Disc(p.Position+new V(-.17,.19),.06,new Color(1,1,1,.65f));}
   else if(p.Kind==Kind.Lens){V side=axis.Perp;for(int k=0;k<24;k++){double t=-1+k/12.0,t2=-1+(k+1)/12.0;double w=.18*(1-t*t),w2=.18*(1-t2*t2);V a=p.Position+axis*(t*.9)+side*w,b=p.Position+axis*(t*.9)-side*w,c1=p.Position+axis*(t2*.9)-side*w2,d=p.Position+axis*(t2*.9)+side*w2;Color lens=new Color(.8f,.94f,1,.75f);Vector2 ua=new Vector2((float)(t+1)*.5f,.5f+(float)w/.36f),ub=new Vector2(ua.x,.5f-(float)w/.36f),uc=new Vector2((float)(t2+1)*.5f,.5f-(float)w2/.36f),ud=new Vector2(uc.x,.5f+(float)w2/.36f);glassBuffer.Tri(a,b,c1,lens,ua,ub,uc,0);glassBuffer.Tri(a,c1,d,lens,ua,uc,ud,0);foregroundBuffer.Line(a,d,.022,white);foregroundBuffer.Line(b,c1,.022,white);}for(int end=-1;end<=1;end+=2){V socket=p.Position+axis*(end*.9);foregroundBuffer.Disc(socket,.095,StoneDark);foregroundBuffer.Ring(socket,.09,.014,LavenderEdge);foregroundBuffer.Disc(socket,.025,new Color(1,.80f,.42f));}}
   else{if(p.Kind==Kind.Green)c=Spectrum[3];if(p.Kind==Kind.Red)c=Spectrum[6];V side=axis.Perp;V a=p.Position-axis*.65,b=p.Position+axis*.65;foregroundBuffer.Line(a+side*.025,b+side*.025,.20,StoneDark);foregroundBuffer.Disc(a,.10,StoneFace);foregroundBuffer.Disc(b,.10,StoneFace);foregroundBuffer.Line(a,b,.095,new Color(.44f,.49f,.62f));foregroundBuffer.Line(a+side*.035,b+side*.035,.036,c);foregroundBuffer.Disc(a,.032,new Color(1,.80f,.42f));foregroundBuffer.Disc(b,.032,new Color(1,.80f,.42f));if(p.Kind==Kind.Red||p.Kind==Kind.Green)DrawSelectorIdentity(p.Position,axis,p.Kind,c);}
   if(i==previewIndex&&!previewAllowed&&invalidPlacementGhost==null){
    foregroundBuffer.Ring(p.Position,PieceInfo.SelectionRadius(p.Kind),.035,new Color(1f,.37f,.43f,.94f));
    foregroundBuffer.Line(p.Position+new V(-.14,-.14),p.Position+new V(.14,.14),.027,new Color(1f,.37f,.43f));
   }
   if(i==selected){double radius=PieceInfo.SelectionRadius(p.Kind);Color selection=new Color(.79f,.68f,1,VisualEnvironment.HighContrast?1:.9f);
    foregroundBuffer.Arc(p.Position,radius,.028,p.Angle-65,130,selection);
    for(int tick=-1;tick<=1;tick++){V tickAxis=V.Angle(p.Angle+tick*45);foregroundBuffer.Line(p.Position+tickAxis*(radius-.04),p.Position+tickAxis*(radius+.05),.018,selection);}
    if(rotating||VisualEnvironment.PrecisionMode)foregroundBuffer.Line(p.Position-axis*.18,p.Position+axis*.18,.015,selection);if(PieceInfo.CanRotate(p.Kind)){V handle=p.Position+axis*radius;foregroundBuffer.Line(p.Position+axis*(radius-.20),handle,.025,new Color(.97f,.77f,.38f,.8f));foregroundBuffer.Disc(handle,.112,new Color(.06f,.12f,.15f),Mathf.Max(14,VisualEnvironment.CircleSegments/2));foregroundBuffer.Ring(handle,.105,.026,new Color(1,.83f,.5f),Mathf.Max(14,VisualEnvironment.CircleSegments/2));foregroundBuffer.Disc(handle,.036,new Color(.97f,.84f,.56f));}}
  }
  if(invalidPlacementGhost!=null)DrawInvalidPlacementGhost();
  DrawHint();
  boardBuffer.Apply(boardMesh);baseBuffer.Apply(baseMesh);waterBuffer.Apply(waterMesh);beamBuffer.Apply(beamMesh);glassBuffer.Apply(glassMesh);foregroundBuffer.Apply(foregroundMesh);
 }
 void DrawPlacementBoundary(){
  // PlacementRules constrains the optic's centre, not its entire rendered footprint.
  // Show that exact region only while moving a piece so fixtures remain uncluttered.
  const double extent=PlacementRules.BoardHalfExtent,step=.20,dash=.10;
  Color border=new Color(.79f,.73f,1,VisualEnvironment.HighContrast?.85f:.55f);
  for(double t=-extent;t<extent;t+=step){double end=System.Math.Min(t+dash,extent);foregroundBuffer.Line(new V(t,-extent),new V(end,-extent),.018,border);foregroundBuffer.Line(new V(t,extent),new V(end,extent),.018,border);foregroundBuffer.Line(new V(-extent,t),new V(-extent,end),.018,border);foregroundBuffer.Line(new V(extent,t),new V(extent,end),.018,border);}
 }
 void DrawSelectorIdentity(V center,V axis,Kind kind,Color tint){
  // Body badge echoes the tray glyph: red diamond/two bars; green hexagon/one bar.
  // It is decorative and deliberately leaves the optical segment unchanged.
  V side=axis.Perp;const double radius=.22;
  foregroundBuffer.Disc(center,.26,StoneDark);
  int corners=kind==Kind.Red?4:6;double rotation=kind==Kind.Red?90:0;
  tint.a=1;
  for(int k=0;k<corners;k++){V a=V.Angle(rotation+k*360.0/corners)*radius,b=V.Angle(rotation+(k+1)*360.0/corners)*radius;foregroundBuffer.Line(center+axis*a.X+side*a.Y,center+axis*b.X+side*b.Y,.030,tint);}
  if(kind==Kind.Red){for(int k=-1;k<=1;k+=2)foregroundBuffer.Line(center-axis*.09+side*(k*.052),center+axis*.09+side*(k*.052),.023,tint);}
  else foregroundBuffer.Line(center-axis*.12,center+axis*.12,.028,tint);
 }
 void DrawInvalidPlacementGhost(){
  var p=invalidPlacementGhost;Color fill=new Color(1,.37f,.43f,.16f),edge=new Color(1,.37f,.43f,.85f);V axis=V.Angle(p.Angle);
  if(p.Kind==Kind.Prism){var vertices=Optics.Vertices(p);foregroundBuffer.Tri(vertices[0],vertices[1],vertices[2],fill);for(int k=0;k<3;k++)foregroundBuffer.Line(vertices[k],vertices[(k+1)%3],.035,edge);}
  else if(p.Kind==Kind.Sphere){foregroundBuffer.Disc(p.Position,.55,fill);foregroundBuffer.Ring(p.Position,.55,.035,edge);}
  else {double size=p.Kind==Kind.Lens?.9:.65;foregroundBuffer.Line(p.Position-axis*size,p.Position+axis*size,p.Kind==Kind.Lens?.26:.18,fill);foregroundBuffer.Line(p.Position-axis*size,p.Position+axis*size,.038,edge);if(p.Kind==Kind.Red||p.Kind==Kind.Green)DrawSelectorIdentity(p.Position,axis,p.Kind,edge);}
  foregroundBuffer.Line(p.Position+new V(-.10,-.10),p.Position+new V(.10,.10),.03,edge);foregroundBuffer.Line(p.Position+new V(-.10,.10),p.Position+new V(.10,-.10),.03,edge);
 }
 void DrawObsidianWall(Wall wall,Result result){
  V a=wall.A,b=wall.B;
  V axis=(b-a).Unit,side=axis.Perp;double length=(b-a).Length;
  if(length<.001)return;
  double half=WallGeometry.Thickness(wall)*.5;
  var outline=WallGeometry.Vertices(wall);
  V depth=new V(.026,-.038);
  for(int k=0;k<outline.Length;k++)foregroundBuffer.Quad(outline[k],outline[(k+1)%outline.Length],outline[(k+1)%outline.Length]+depth,outline[k]+depth,StoneDark);
  // A continuous physical face under the seams: no fake gaps or oversized shadow.
  foregroundBuffer.Quad(outline[0],outline[1],outline[2],outline[3],StoneDark);
  int count=System.Math.Max(1,(int)System.Math.Ceiling(length/.52));double step=length/count;
  for(int i=0;i<count;i++){
   double start=i*step+.014,end=(i+1)*step-.014;if(end<=start)continue;
   double chamfer=System.Math.Min(System.Math.Min(.045,half*.35),(end-start)*.2);
   V[] poly={a+axis*(start+chamfer)+side*half,a+axis*(end-chamfer)+side*half,a+axis*end+side*(half-chamfer),a+axis*end-side*(half-chamfer),a+axis*(end-chamfer)-side*half,a+axis*(start+chamfer)-side*half,a+axis*start-side*(half-chamfer),a+axis*start+side*(half-chamfer)};
   V center=a+axis*((start+end)*.5);float variation=.96f+.04f*Mathf.Sin(i*1.7f);Color face=new Color(StoneFace.r*variation,StoneFace.g*variation,StoneFace.b*variation);
   for(int k=0;k<8;k++){foregroundBuffer.Tri(center,poly[k],poly[(k+1)%8],face);foregroundBuffer.Line(poly[k],poly[(k+1)%8],.017,k<3?(VisualEnvironment.HighContrast?LavenderEdge:StoneLight):StoneDark);}
   foregroundBuffer.Line(a+axis*(start+chamfer)+side*(half-.035),a+axis*(end-chamfer)+side*(half-.035),.011,new Color(StoneLight.r,StoneLight.g,StoneLight.b,.85f));
  }
  wallImpacts.Clear();
  foreach(var beam in result.Beams){
   if(WallGeometry.Distance(beam.B,wall)>.025)continue;
   double along=V.Dot(beam.B-a,axis);if(along<-.03||along>length+.03)continue;
   float power=Mathf.Max(0,(float)beam.Power);int group=-1;
   for(int i=0;i<wallImpacts.Count;i++)if((wallImpacts[i].Position-beam.B).Length<.22){group=i;break;}
   if(group<0)wallImpacts.Add(new WallImpact{Position=beam.B,Color=Spectrum[beam.Band]*power,Power=power});
   else {var impact=wallImpacts[group];float total=impact.Power+power;impact.Position=(impact.Position*impact.Power+beam.B*power)/System.Math.Max(.0001,total);impact.Color+=Spectrum[beam.Band]*power;impact.Power=total;wallImpacts[group]=impact;}
  }
  foreach(var impact in wallImpacts){
   Color tint=impact.Color/Mathf.Max(.0001f,impact.Power);float strength=Mathf.Clamp01(impact.Power*2);
   tint.a=.08f*strength;foregroundBuffer.RadialDisc(impact.Position,.17,tint,12);
   double along=V.Dot(impact.Position-a,axis),sideSign=V.Dot(impact.Position-a,side)>=0?1:-1;
   V hit=a+axis*System.Math.Max(0,System.Math.Min(length,along))+side*(half*sideSign);
   tint.a=.10f*strength;foregroundBuffer.Line(hit-axis*.10,hit+axis*.10,.022,tint);
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
  if(hintPiece.Kind==Kind.Prism){var silhouette=hintPiece.Copy();if(hintStage<3)silhouette.Angle=90;var vs=Optics.Vertices(silhouette);for(int i=0;i<3;i++)foregroundBuffer.Line(vs[i],vs[(i+1)%3],.016,c);}
  else if(hintPiece.Kind==Kind.Sphere)foregroundBuffer.Ring(p,.55,.016,c);
  else {V axis=V.Angle(hintStage>=3?hintPiece.Angle:0);foregroundBuffer.Line(p-axis*.6,p+axis*.6,.028,c);}
  if(hintStage>=3&&PieceInfo.CanRotate(hintPiece.Kind)){V axis=V.Angle(hintPiece.Angle);foregroundBuffer.Line(p,p+axis*.85,.014,c);foregroundBuffer.Disc(p+axis*.85,.055,c);}
 }
 void Mount(MeshBuffer buffer,V p,double radius){buffer.Disc(p+new V(.035,-.055),radius+.03,new Color(.012f,.028f,.043f,.85f));buffer.Disc(p,radius,new Color(.11f,.18f,.24f));buffer.Ring(p,radius,.018,new Color(.28f,.39f,.46f));for(int k=0;k<3;k++)buffer.Disc(p+V.Angle(90+k*120)*(radius*.84),.022,new Color(.57f,.65f,.66f));}
 void ReceiverBase(V p,double radius){foregroundBuffer.Disc(p+new V(.02,-.035),radius,StoneDark);foregroundBuffer.Disc(p,radius,StoneFace);foregroundBuffer.Ring(p,radius,.018,VisualEnvironment.HighContrast?new Color(.82f,.77f,1):LavenderEdge);}
 void OnDestroy(){DestroyMaterial(boardMaterial);DestroyMaterial(baseMaterial);DestroyMaterial(beamMaterial);DestroyMaterial(glassMaterial);DestroyMaterial(waterMaterial);DestroyMaterial(foregroundMaterial);}
 void DestroyMaterial(Material material){if(material==null)return;if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}
}
}
