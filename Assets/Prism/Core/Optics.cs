using System;
using System.Collections.Generic;
namespace Prism {
public struct V {
 public double X,Y; public V(double x,double y){X=x;Y=y;} public double Length=>Math.Sqrt(X*X+Y*Y); public V Unit=>this/Math.Max(1e-12,Length);
 public static V operator +(V a,V b)=>new V(a.X+b.X,a.Y+b.Y); public static V operator -(V a,V b)=>new V(a.X-b.X,a.Y-b.Y); public static V operator -(V a)=>new V(-a.X,-a.Y); public static V operator *(V a,double s)=>new V(a.X*s,a.Y*s); public static V operator /(V a,double s)=>new V(a.X/s,a.Y/s);
 public static double Dot(V a,V b)=>a.X*b.X+a.Y*b.Y; public static double Cross(V a,V b)=>a.X*b.Y-a.Y*b.X; public V Perp=>new V(-Y,X); public static V Angle(double degrees)=>new V(Math.Cos(degrees*Math.PI/180),Math.Sin(degrees*Math.PI/180));
}
public enum Kind { Mirror,Prism,Green,Red,Lens,Sphere }
public class Piece { public Kind Kind; public V Position; public double Angle; public Piece(Kind k,V p,double a=0){Kind=k;Position=p;Angle=a;} public Piece Copy()=>new Piece(Kind,Position,Angle); }
public class Goal { public V Position; public int Band; public double Radius=0.42; public double Threshold=0.24; public Goal(V p,int b){Position=p;Band=b;} }
public struct Wall { public V A,B; public Wall(V a,V b){A=a;B=b;} }
public struct WaterZone { public V Min,Max; public double Index; public WaterZone(V min,V max,double idx=1.333){Min=min;Max=max;Index=idx;} public bool Contains(V p)=>p.X>=Min.X-1e-5&&p.X<=Max.X+1e-5&&p.Y>=Min.Y-1e-5&&p.Y<=Max.Y+1e-5; }
public class Level { public string Name="",Lesson="",Hint=""; public V Source,Direction; public Piece[] Initial=new Piece[0],Solution=new Piece[0]; public Goal[] Goals=new Goal[0]; public Wall[] Walls=new Wall[0]; public WaterZone[] WaterZones=new WaterZone[0]; public Kind[] Stock=new Kind[0]; public double Width=0.32; }
public struct Beam { public V A,B; public int Band; public double Power; public Beam(V a,V b,int band,double p){A=a;B=b;Band=band;Power=p;} }
public class Result { public List<Beam> Beams=new List<Beam>(); public double[] Energy; public bool Complete,Truncated; }
public static class Optics {
 public static V Reflect(V d,V n)=>(d-n*(2*V.Dot(d,n))).Unit;
 public static bool Refract(V d,V n,double from,double to,out V r){
  if(V.Dot(d,n)>0)n=-n;
  double eta=from/to,c=-V.Dot(d,n),k=1-eta*eta*(1-c*c);
  if(k<0){r=Reflect(d,n);return false;}
  r=(d*eta+n*(eta*c-Math.Sqrt(k))).Unit;return true;
 }
 public static double Index(int band){double wavelength=0.42+band*0.04;return 1.44+0.012/(wavelength*wavelength);}
 public static V[] Vertices(Piece p){var vs=new V[3];for(int i=0;i<3;i++)vs[i]=p.Position+V.Angle(p.Angle+i*120)*0.68;return vs;}
 static bool Intersect(V o,V d,V a,V b,out double t,out V n){
  V edge=b-a; double cross=V.Cross(d,edge);t=0;n=edge.Perp.Unit;
  if(Math.Abs(cross)<1e-10)return false;
  t=V.Cross(a-o,edge)/cross;double u=V.Cross(a-o,d)/cross;
  return t>0.0001&&u>=0&&u<=1;
 }
 static double Circle(V o,V d,Goal g){V q=o-g.Position;double b=V.Dot(q,d),c=V.Dot(q,q)-g.Radius*g.Radius,disc=b*b-c;if(disc<0)return double.PositiveInfinity;double t=-b-Math.Sqrt(disc);return t>0.0001?t:double.PositiveInfinity;}
 public static Result Solve(Level level,IList<Piece> pieces){
  var result=new Result {Energy=new double[level.Goals.Length]}; const int samples=13;
  for(int band=0;band<7;band++)for(int sample=0;sample<samples;sample++){
   V d=level.Direction.Unit,o=level.Source+d.Perp*((sample/(double)(samples-1)-0.5)*level.Width);double power=1.0/samples;
   for(int bounce=0;bounce<24;bounce++){
    double nearest=30; V normal=new V();Piece hit=null;int goal=-1;bool wall=false;WaterZone? hitZone=null;
    foreach(var w in level.Walls){double t;V n;if(Intersect(o,d,w.A,w.B,out t,out n)&&t<nearest){nearest=t;normal=n;wall=true;hitZone=null;hit=null;}}
    foreach(var wz in level.WaterZones){
     V b0=new V(wz.Min.X,wz.Min.Y), b1=new V(wz.Max.X,wz.Min.Y), b2=new V(wz.Max.X,wz.Max.Y), b3=new V(wz.Min.X,wz.Max.Y);
     double t; V n;
     if(Intersect(o,d,b0,b1,out t,out n)&&t<nearest){nearest=t;normal=new V(0,-1);hitZone=wz;hit=null;wall=false;}
     if(Intersect(o,d,b1,b2,out t,out n)&&t<nearest){nearest=t;normal=new V(1,0);hitZone=wz;hit=null;wall=false;}
     if(Intersect(o,d,b2,b3,out t,out n)&&t<nearest){nearest=t;normal=new V(0,1);hitZone=wz;hit=null;wall=false;}
     if(Intersect(o,d,b3,b0,out t,out n)&&t<nearest){nearest=t;normal=new V(-1,0);hitZone=wz;hit=null;wall=false;}
    }
    foreach(var p in pieces){
     if(p.Kind==Kind.Prism){var v=Vertices(p);for(int i=0;i<3;i++){double t;V n;if(Intersect(o,d,v[i],v[(i+1)%3],out t,out n)&&t<nearest){nearest=t;normal=-n;hit=p;hitZone=null;wall=false;}}}
     else if(p.Kind==Kind.Sphere){
      V q=o-p.Position;double b=V.Dot(q,d),c=V.Dot(q,q)-0.55*0.55,disc=b*b-c;
      if(disc>=0){double s=Math.Sqrt(disc),t=c>0?-b-s:-b+s;if(t>0.0001&&t<nearest){nearest=t;normal=(o+d*t-p.Position)/0.55;hit=p;hitZone=null;wall=false;}}
     }
     else {V axis=V.Angle(p.Angle);double t;V n;double size=p.Kind==Kind.Lens?0.9:0.65;if(Intersect(o,d,p.Position-axis*size,p.Position+axis*size,out t,out n)&&t<nearest){nearest=t;normal=n;hit=p;hitZone=null;wall=false;}}
    }
    for(int i=0;i<level.Goals.Length;i++){double t=Circle(o,d,level.Goals[i]);if(t<nearest){nearest=t;goal=i;}}
    V end=o+d*nearest; result.Beams.Add(new Beam(o,end,band,power));
    if(goal>=0){var g=level.Goals[goal];if(g.Band<0)result.Energy[goal]+=power/7;else if(g.Band==band)result.Energy[goal]+=power;break;}
    if(wall||(!hitZone.HasValue&&hit==null))break;
    if(hitZone.HasValue){
     bool entering=V.Dot(d,normal)<0;
     double from=entering?1:hitZone.Value.Index, to=entering?hitZone.Value.Index:1;
     V r;
     Refract(d,normal,from,to,out r);
     d=r;power*=0.99;
    }
    else if(hit!=null){
     if(hit.Kind==Kind.Prism){bool entering=V.Dot(d,normal)<0;V r;Refract(d,normal,entering?1:Index(band),entering?Index(band):1,out r);d=r;power*=0.98;}
     else if(hit.Kind==Kind.Sphere){
      bool entering=V.Dot(d,normal)<0;
      double nSphere=1.50+0.012/Math.Pow(0.42+band*0.04,2);
      double from=entering?1:nSphere, to=entering?nSphere:1;
      V r;
      Refract(d,normal,from,to,out r);
      d=r;power*=0.98;
     }
     else if(hit.Kind==Kind.Lens){V tangent=V.Angle(hit.Angle),axis=tangent.Perp;if(V.Dot(d,axis)<0)axis=-axis;double forward=V.Dot(d,axis);if(forward<0.04)break;double slope=V.Dot(d,tangent)/forward-V.Dot(end-hit.Position,tangent)/2.4;d=(axis+tangent*slope).Unit;power*=0.96;}
     else if(hit.Kind==Kind.Mirror||hit.Kind==Kind.Green&&band==3||hit.Kind==Kind.Red&&band>=5){d=Reflect(d,normal);power*=0.98;}
    }
    o=end+d*0.0002;
    if(bounce==23)result.Truncated=true;
   }
  }
  result.Complete=level.Goals.Length>0;for(int i=0;i<level.Goals.Length;i++)if(result.Energy[i]<level.Goals[i].Threshold)result.Complete=false;
  return result;
 }
}
}
