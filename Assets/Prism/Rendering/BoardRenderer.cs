using System.Collections.Generic;
using UnityEngine;
namespace Prism {
public class BoardRenderer : MonoBehaviour {
 readonly List<Vector3> vertices=new List<Vector3>();readonly List<Color> colors=new List<Color>();readonly List<int> indices=new List<int>();Mesh mesh;
 public static readonly Color[] Spectrum={new Color(.55f,.36f,1),new Color(.28f,.48f,1),new Color(.1f,.75f,1),new Color(.35f,1,.65f),new Color(.92f,1,.45f),new Color(1,.66f,.25f),new Color(1,.32f,.33f)};
 void Awake(){mesh=new Mesh {name="Optical board",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;gameObject.AddComponent<MeshRenderer>().sharedMaterial=new Material(Resources.Load<Shader>("PrismUnlit"));}
 void Tri(V a,V b,V c,Color color){int n=vertices.Count;vertices.Add(new Vector3((float)a.X,(float)a.Y,0));vertices.Add(new Vector3((float)b.X,(float)b.Y,0));vertices.Add(new Vector3((float)c.X,(float)c.Y,0));colors.Add(color);colors.Add(color);colors.Add(color);indices.Add(n);indices.Add(n+1);indices.Add(n+2);}
 void Line(V a,V b,double width,Color color){V n=(b-a).Unit.Perp*width*.5;Tri(a+n,a-n,b+n,color);Tri(a-n,b-n,b+n,color);}
 void Disc(V p,double radius,Color color){for(int i=0;i<48;i++)Tri(p,p+V.Angle(i*7.5)*radius,p+V.Angle((i+1)*7.5)*radius,color);}
 void Ring(V p,double radius,double width,Color color){for(int i=0;i<64;i++)Line(p+V.Angle(i*5.625)*radius,p+V.Angle((i+1)*5.625)*radius,width,color);}
 public void Draw(Level level,IList<Piece> pieces,Result result,int selected){
  vertices.Clear();colors.Clear();indices.Clear();
  Color bg=new Color(.043f,.07f,.095f);Tri(new V(-5,-5),new V(5,-5),new V(5,5),bg);Tri(new V(-5,-5),new V(5,5),new V(-5,5),bg);
  for(double x=-4.5;x<5;x+=.5)for(double y=-4.5;y<5;y+=.5)Disc(new V(x,y),.012,new Color(.16f,.23f,.27f,.65f));
  foreach(var wz in level.WaterZones){
   Color wbg=new Color(.08f,.24f,.32f,.35f);Tri(new V(wz.Min.X,wz.Min.Y),new V(wz.Max.X,wz.Min.Y),new V(wz.Max.X,wz.Max.Y),wbg);Tri(new V(wz.Min.X,wz.Min.Y),new V(wz.Max.X,wz.Max.Y),new V(wz.Min.X,wz.Max.Y),wbg);
   Color wBorder=new Color(.22f,.62f,.78f,.45f);Line(new V(wz.Min.X,wz.Min.Y),new V(wz.Max.X,wz.Min.Y),.03,wBorder);Line(new V(wz.Max.X,wz.Min.Y),new V(wz.Max.X,wz.Max.Y),.03,wBorder);Line(new V(wz.Max.X,wz.Max.Y),new V(wz.Min.X,wz.Max.Y),.03,wBorder);Line(new V(wz.Min.X,wz.Max.Y),new V(wz.Min.X,wz.Min.Y),.03,wBorder);
  }
  foreach(var beam in result.Beams){
   Color c=Spectrum[beam.Band];
   c.a=.035f;Line(beam.A,beam.B,.16,c);
   c.a=.22f;Line(beam.A,beam.B,.045,c);
   c.a=.65f;Line(beam.A,beam.B,.014,c);
   Color core=Color.Lerp(c,Color.white,.65f);core.a=.85f;Line(beam.A,beam.B,.005,core);
  }
  foreach(var wall in level.Walls){Line(wall.A,wall.B,.2,new Color(.12f,.18f,.22f));Line(wall.A,wall.B,.026,new Color(.3f,.4f,.44f));}
  Color white=new Color(.91f,.98f,1);V dir=level.Direction.Unit;
  Disc(level.Source,.3,new Color(.14f,.21f,.25f));Ring(level.Source,.3,.025,new Color(.48f,.67f,.71f));Disc(level.Source,.16,white);Line(level.Source,level.Source+dir*.42,.07,white);
  for(int i=0;i<level.Goals.Length;i++){var g=level.Goals[i];Color c=g.Band<0?white:Spectrum[g.Band];float energy=Mathf.Clamp01((float)(result.Energy[i]/g.Threshold));Color glow=c;glow.a=.03f+energy*.09f;for(int j=0;j<4;j++)Disc(g.Position,g.Radius+.11+j*.075,glow);Disc(g.Position,g.Radius,new Color(.06f,.1f,.13f));Color dim=c;dim.a=.45f;Ring(g.Position,g.Radius,.027,dim);if(energy>0){c.a=energy;Ring(g.Position,g.Radius*.8,.045,c);}c.a=.45f+energy*.55f;Disc(g.Position,.065+energy*.045,c);
   if(g.Band==3){Line(g.Position+new V(-.07,0),g.Position+new V(0,.1),.018,c);Line(g.Position+new V(0,.1),g.Position+new V(.07,0),.018,c);}
   else if(g.Band==6)Ring(g.Position,.12,.018,c);
   else if(g.Band==1)Ring(g.Position,.10,.016,c);
  }
  for(int i=0;i<pieces.Count;i++){var p=pieces[i];V axis=V.Angle(p.Angle);Color c=white;
   if(p.Kind==Kind.Prism){var vs=Optics.Vertices(p);Tri(vs[0],vs[1],vs[2],new Color(.35f,.65f,.75f,.15f));for(int k=0;k<3;k++)Line(vs[k],vs[(k+1)%3],.035,white);Line(p.Position,vs[0],.012,new Color(.5f,.8f,1,.35f));}
   else if(p.Kind==Kind.Sphere){
    Disc(p.Position,.55,new Color(.4f,.75f,.9f,.18f));Ring(p.Position,.55,.032,white);Ring(p.Position,.38,.016,new Color(.6f,.85f,1,.35f));Disc(p.Position+new V(-.14,.14),.065,new Color(1,1,1,.65f));
   }
   else if(p.Kind==Kind.Lens){V side=axis.Perp;for(int k=0;k<24;k++){double t=-1+k/12.0,t2=-1+(k+1)/12.0;double w=.18*(1-t*t),w2=.18*(1-t2*t2);Line(p.Position+axis*(t*.9)+side*w,p.Position+axis*(t2*.9)+side*w2,.03,white);Line(p.Position+axis*(t*.9)-side*w,p.Position+axis*(t2*.9)-side*w2,.03,white);}}
   else{if(p.Kind==Kind.Green)c=Spectrum[3];if(p.Kind==Kind.Red)c=Spectrum[6];Line(p.Position-axis*.65,p.Position+axis*.65,.13,new Color(c.r,c.g,c.b,.15f));Line(p.Position-axis*.65,p.Position+axis*.65,.045,c);}
   if(i==selected){Ring(p.Position,p.Kind==Kind.Sphere?.85:1.02,.014,new Color(.9f,.77f,.46f,.7f));Disc(p.Position+axis*(p.Kind==Kind.Sphere?.85:1.02),.065,new Color(1,.83f,.5f));}
  }
  mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();
 }
}
}
