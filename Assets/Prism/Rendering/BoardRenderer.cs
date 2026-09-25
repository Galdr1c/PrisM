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
  public void Disc(V p,double radius,Color color,int segments=0){if(segments<=0)segments=VisualEnvironment.CircleSegments;for(int i=0;i<segments;i++)Tri(p,p+V.Angle(i*360.0/segments)*radius,p+V.Angle((i+1)*360.0/segments)*radius,color);}
  public void RadialDisc(V p,double radius,Color color,int segments=0){if(segments<=0)segments=VisualEnvironment.CircleSegments;for(int i=0;i<segments;i++){double a=i*360.0/segments,b=(i+1)*360.0/segments;Tri(p,p+V.Angle(a)*radius,p+V.Angle(b)*radius,color,Vector2.zero,new Vector2((float)System.Math.Cos(a*System.Math.PI/180),(float)System.Math.Sin(a*System.Math.PI/180)),new Vector2((float)System.Math.Cos(b*System.Math.PI/180),(float)System.Math.Sin(b*System.Math.PI/180)),1);}}
  public void Ring(V p,double radius,double width,Color color,int segments=0){if(segments<=0)segments=VisualEnvironment.CircleSegments;for(int i=0;i<segments;i++)Line(p+V.Angle(i*360.0/segments)*radius,p+V.Angle((i+1)*360.0/segments)*radius,width,color);}
  public void Apply(Mesh mesh){mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetUVs(1,uv2);mesh.SetTriangles(indices,0);mesh.bounds=new Bounds(Vector3.zero,new Vector3(70,70,4));}
 }
 readonly MeshBuffer boardBuffer=new MeshBuffer(),baseBuffer=new MeshBuffer(),beamBuffer=new MeshBuffer(),glassBuffer=new MeshBuffer(),waterBuffer=new MeshBuffer();
 Mesh boardMesh,baseMesh,beamMesh,glassMesh,waterMesh;Material boardMaterial,baseMaterial,beamMaterial,glassMaterial,waterMaterial;
 public static readonly Color[] Spectrum={new Color(.55f,.36f,1),new Color(.28f,.48f,1),new Color(.1f,.75f,1),new Color(.35f,1,.65f),new Color(.92f,1,.45f),new Color(1,.66f,.25f),new Color(1,.32f,.33f)};
 void Awake(){
  boardMesh=CreateLayer("Board surface","PrismBoard",out boardMaterial,0);
  baseMesh=CreateLayer("Optical geometry","PrismGeometry",out baseMaterial,1);
  waterMesh=CreateLayer("Water volume","PrismWater",out waterMaterial,2);
  beamMesh=CreateLayer("Light field","PrismBeam",out beamMaterial,3);
  glassMesh=CreateLayer("Optical glass","PrismGlass",out glassMaterial,4);
 }
 Mesh CreateLayer(string name,string shaderResource,out Material material,int order){var go=new GameObject(name);go.transform.SetParent(transform,false);var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.MarkDynamic();go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();var shader=Resources.Load<Shader>(shaderResource);if(shader==null)shader=Resources.Load<Shader>("PrismUnlit");material=new Material(shader){name=name+" material"};renderer.sharedMaterial=material;renderer.sortingOrder=order;return mesh;}
 public void Draw(Level level,IList<Piece> pieces,Result result,int selected){
  boardBuffer.Clear();baseBuffer.Clear();beamBuffer.Clear();glassBuffer.Clear();waterBuffer.Clear();
  var tier=VisualEnvironment.Resolved;
  if(beamMaterial!=null)beamMaterial.SetFloat("_Intensity",tier==VisualQualityTier.Low?2.55f:tier==VisualQualityTier.High?3.85f:3.3f);
  if(glassMaterial!=null)glassMaterial.SetFloat("_EdgeIntensity",tier==VisualQualityTier.Low?1.15f:tier==VisualQualityTier.High?1.75f:1.5f);
  if(waterMaterial!=null)waterMaterial.SetFloat("_Glow",tier==VisualQualityTier.Low?.72f:tier==VisualQualityTier.High?1.2f:.96f);
  boardBuffer.Quad(new V(-5,-5),new V(5,-5),new V(5,5),new V(-5,5),Color.white);
  foreach(var wz in level.WaterZones){
   waterBuffer.Quad(new V(wz.Min.X,wz.Min.Y),new V(wz.Max.X,wz.Min.Y),new V(wz.Max.X,wz.Max.Y),new V(wz.Min.X,wz.Max.Y),new Color(.72f,.92f,1,1));
   Color border=new Color(.28f,.72f,.9f,.7f);baseBuffer.Line(new V(wz.Min.X,wz.Min.Y),new V(wz.Max.X,wz.Min.Y),.024,border);baseBuffer.Line(new V(wz.Max.X,wz.Min.Y),new V(wz.Max.X,wz.Max.Y),.024,border);baseBuffer.Line(new V(wz.Max.X,wz.Max.Y),new V(wz.Min.X,wz.Max.Y),.024,border);baseBuffer.Line(new V(wz.Min.X,wz.Max.Y),new V(wz.Min.X,wz.Min.Y),.024,border);
  }
  foreach(var beam in result.Beams){
   Color c=Spectrum[beam.Band];float strength=Mathf.Clamp01((float)beam.Power*13f);
   Color halo=c;halo.a=.055f*strength;beamBuffer.BeamLine(beam.A,beam.B,.21,halo);
   Color glow=c;glow.a=.18f*strength;beamBuffer.BeamLine(beam.A,beam.B,.065,glow);
   Color core=c;core.a=.52f*strength;beamBuffer.BeamLine(beam.A,beam.B,.018,core);
  }
  foreach(var wall in level.Walls){baseBuffer.Line(wall.A,wall.B,.2,new Color(.12f,.18f,.22f));baseBuffer.Line(wall.A,wall.B,.026,new Color(.34f,.45f,.49f));}
  Color white=new Color(.91f,.98f,1);V dir=level.Direction.Unit;
  Color sourceGlow=white;sourceGlow.a=.18f;beamBuffer.RadialDisc(level.Source,.52,sourceGlow);baseBuffer.Disc(level.Source,.3,new Color(.14f,.21f,.25f));baseBuffer.Ring(level.Source,.3,.025,new Color(.48f,.67f,.71f));baseBuffer.Disc(level.Source,.16,white);baseBuffer.Line(level.Source,level.Source+dir*.42,.07,white);
  for(int i=0;i<level.Goals.Length;i++){var g=level.Goals[i];Color c=g.Band<0?white:Spectrum[g.Band];float energy=Mathf.Clamp01((float)(result.Energy[i]/g.Threshold));Color glow=c;glow.a=.06f+energy*.18f;beamBuffer.RadialDisc(g.Position,g.Radius+.32+energy*.08,glow);baseBuffer.Disc(g.Position,g.Radius,new Color(.06f,.1f,.13f));Color dim=c;dim.a=.5f;baseBuffer.Ring(g.Position,g.Radius,.027,dim);if(energy>0){c.a=.45f+energy*.45f;baseBuffer.Ring(g.Position,g.Radius*.8,.045,c);}c.a=.55f+energy*.45f;baseBuffer.Disc(g.Position,.065+energy*.045,c);
   if(g.Band==3){baseBuffer.Line(g.Position+new V(-.07,0),g.Position+new V(0,.1),.018,c);baseBuffer.Line(g.Position+new V(0,.1),g.Position+new V(.07,0),.018,c);}else if(g.Band==6)baseBuffer.Ring(g.Position,.12,.018,c);else if(g.Band==1)baseBuffer.Ring(g.Position,.10,.016,c);
  }
  for(int i=0;i<pieces.Count;i++){var p=pieces[i];V axis=V.Angle(p.Angle);Color c=white;
   if(p.Kind==Kind.Prism){var vs=Optics.Vertices(p);glassBuffer.Tri(vs[0],vs[1],vs[2],new Color(.72f,.9f,1,.9f),new Vector2(.5f,1),new Vector2(0,0),new Vector2(1,0),0);for(int k=0;k<3;k++)baseBuffer.Line(vs[k],vs[(k+1)%3],.03,new Color(.82f,.96f,1,.9f));baseBuffer.Line(p.Position,vs[0],.01,new Color(.5f,.8f,1,.35f));}
   else if(p.Kind==Kind.Sphere){glassBuffer.RadialDisc(p.Position,.55,new Color(.82f,.95f,1,.9f),VisualEnvironment.CircleSegments+8);baseBuffer.Ring(p.Position,.55,.03,white);baseBuffer.Ring(p.Position,.38,.014,new Color(.6f,.85f,1,.35f));baseBuffer.Disc(p.Position+new V(-.14,.14),.055,new Color(1,1,1,.75f),Mathf.Max(14,VisualEnvironment.CircleSegments/2));}
   else if(p.Kind==Kind.Lens){V side=axis.Perp;for(int k=0;k<24;k++){double t=-1+k/12.0,t2=-1+(k+1)/12.0;double w=.18*(1-t*t),w2=.18*(1-t2*t2);V a=p.Position+axis*(t*.9)+side*w,b=p.Position+axis*(t*.9)-side*w,c1=p.Position+axis*(t2*.9)-side*w2,d=p.Position+axis*(t2*.9)+side*w2;glassBuffer.Quad(a,b,c1,d,new Color(.8f,.94f,1,.75f));baseBuffer.Line(a,d,.02,white);baseBuffer.Line(b,c1,.02,white);}}
   else{if(p.Kind==Kind.Green)c=Spectrum[3];if(p.Kind==Kind.Red)c=Spectrum[6];baseBuffer.Line(p.Position-axis*.65,p.Position+axis*.65,.13,new Color(c.r,c.g,c.b,.15f));baseBuffer.Line(p.Position-axis*.65,p.Position+axis*.65,.045,c);}
   if(i==selected){double radius=PieceInfo.SelectionRadius(p.Kind);baseBuffer.Ring(p.Position,radius,.014,new Color(.9f,.77f,.46f,.8f));if(PieceInfo.CanRotate(p.Kind))baseBuffer.Disc(p.Position+axis*radius,.065,new Color(1,.83f,.5f),Mathf.Max(14,VisualEnvironment.CircleSegments/2));}
  }
  boardBuffer.Apply(boardMesh);baseBuffer.Apply(baseMesh);waterBuffer.Apply(waterMesh);beamBuffer.Apply(beamMesh);glassBuffer.Apply(glassMesh);
 }
 void OnDestroy(){DestroyMaterial(boardMaterial);DestroyMaterial(baseMaterial);DestroyMaterial(beamMaterial);DestroyMaterial(glassMaterial);DestroyMaterial(waterMaterial);}
 void DestroyMaterial(Material material){if(material==null)return;if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}
}
}
