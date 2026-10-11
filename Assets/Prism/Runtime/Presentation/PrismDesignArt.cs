using UnityEngine;
using UnityEngine.UI;

namespace Prism {
/// <summary>Canvas-native home optical scene, designed in a 328 by 310 frame.</summary>
public sealed class PrismHomeScene : MaskableGraphic {
 float lastRefresh, lastBeam = -1, lastBloom = -1;
 bool lastReduced;
 protected override void Awake(){base.Awake();raycastTarget=false;}
 void Update(){
  bool changed=lastReduced!=VisualEnvironment.ReducedMotion||lastBeam!=VisualEnvironment.BeamScale||lastBloom!=VisualEnvironment.BloomScale;
  if(changed||(!VisualEnvironment.ReducedMotion&&Time.unscaledTime-lastRefresh>.08f)){
   lastRefresh=Time.unscaledTime;lastReduced=VisualEnvironment.ReducedMotion;lastBeam=VisualEnvironment.BeamScale;lastBloom=VisualEnvironment.BloomScale;SetVerticesDirty();
  }
 }
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();Rect r=rectTransform.rect;float scale=Mathf.Min(r.width/328f,r.height/310f);
  Vector2 origin=r.center+new Vector2(-164,155)*scale;
  float pulse=VisualEnvironment.ReducedMotion?1f:.96f+.04f*Mathf.Sin(Time.unscaledTime*.7f);
  float bloom=VisualEnvironment.BloomScale*pulse;
  // All points use a top-left design origin; keep the optical event inside safe bounds.
  System.Func<float,float,Vector2> p=(x,y)=>origin+new Vector2(x,-y)*scale;
  System.Action<float,float,float,float> slab=(x,y,w,h)=>{
   var bounds=new Rect(origin.x+x*scale,origin.y-(y+h)*scale,w*scale,h*scale);
   PrismDesignMesh.RoundRect(vh,new Rect(bounds.x+3*scale,bounds.y-5*scale,bounds.width,bounds.height),6*scale,new Color32(3,2,14,160),new Color32(3,2,14,160));
   PrismDesignMesh.RoundRect(vh,bounds,6*scale,new Color32(75,69,113,255),new Color32(38,34,62,255));
  };
  slab(0,28,72,27);slab(296,7,29,126);slab(0,174,62,29);slab(293,232,32,72);slab(137,231,31,72);
  PrismDesignMesh.Disc(vh,p(35,12),1.4f*scale,PrismTheme.Ivory);
  PrismDesignMesh.Disc(vh,p(172,31),1.2f*scale,PrismTheme.Accent);
  PrismDesignMesh.Disc(vh,p(55,88),1.6f*scale,PrismTheme.Ivory);
  PrismDesignMesh.Disc(vh,p(273,63),1.5f*scale,PrismTheme.Ivory);
  PrismDesignMesh.Disc(vh,p(87,148),1.2f*scale,new Color32(201,192,255,255));

  Vector2 source=p(13,120),entry=p(119,120),exit=p(196,136);
  // A slit source gives the white beam a physical origin.
  PrismDesignMesh.RoundRect(vh,new Rect(p(0,134).x,p(0,134).y,14*scale,28*scale),5*scale,new Color32(75,69,113,255),new Color32(38,34,62,255));
  PrismUiOptics.Line(vh,p(13,114),p(13,126),2*scale,PrismTheme.Ivory);
  for(int layer=3;layer>=1;layer--)PrismUiOptics.Line(vh,source,entry,(3+layer*3)*scale,new Color(1,1,1,.025f*bloom));
  PrismUiOptics.Line(vh,source,entry,3*scale*VisualEnvironment.BeamScale,PrismTheme.Ivory);

  Vector2 tip=p(153,61),left=p(98,155),right=p(208,155),facet=p(153,123);
  PrismDesignMesh.GradientTriangle(vh,tip,left,right,new Color32(166,204,245,240),new Color32(91,106,192,225),new Color32(159,137,223,235));
  PrismUiOptics.Triangle(vh,left,right,facet,new Color(.15f,.10f,.42f,.4f));
  PrismUiOptics.Triangle(vh,tip,p(125,108),facet,new Color(1,1,1,.2f));
  Color edge=new Color32(221,243,255,255),seam=new Color(1,1,1,.34f);
  PrismUiOptics.Line(vh,tip,left,2.1f*scale,edge);PrismUiOptics.Line(vh,left,right,2.1f*scale,edge);PrismUiOptics.Line(vh,right,tip,2.1f*scale,edge);
  PrismUiOptics.Line(vh,tip,facet,1.2f*scale,seam);PrismUiOptics.Line(vh,left,facet,1.2f*scale,seam);PrismUiOptics.Line(vh,right,facet,1.2f*scale,seam);
  PrismUiOptics.Line(vh,entry,exit,2.7f*scale*VisualEnvironment.BeamScale,PrismTheme.Ivory);
  PrismDesignMesh.Disc(vh,entry,3.8f*scale,PrismTheme.Ivory);PrismDesignMesh.Disc(vh,exit,4.2f*scale,PrismTheme.Ivory);

  Vector2 receiver=p(239,274);
  // Spectrum reaches the receiving aperture as a broad fan, not seven thin laser lines.
  for(int band=0;band<7;band++){
   Color tint=PrismUiOptics.Spectrum[band];
   Vector2 a=p(215+band*6.8f,267-band*2.2f),b=p(215+(band+1)*6.8f,267-(band+1)*2.2f);
   Color halo=tint;halo.a=.035f*bloom;
   PrismUiOptics.Line(vh,exit,(a+b)*.5f,12*scale,halo);
   tint.a=.88f*pulse;PrismUiOptics.Triangle(vh,exit,a,b,tint);
  }
  PrismDesignMesh.Glow(vh,receiver,38*scale,new Color(1,.92f,.8f,.085f*bloom));
  PrismDesignMesh.Disc(vh,receiver+new Vector2(3,-5)*scale,29*scale,new Color32(3,2,14,230));
  PrismDesignMesh.Disc(vh,receiver,29*scale,new Color32(75,69,113,255));
  PrismDesignMesh.Disc(vh,receiver,27*scale,new Color32(28,24,52,255));
  for(int band=0;band<7;band++){
   float a=90+band*360f/7f+3,b=90+(band+1)*360f/7f-3;
   PrismDesignMesh.Arc(vh,receiver,23*scale,17*scale,a,b,PrismUiOptics.Spectrum[band]);
  }
  PrismDesignMesh.Disc(vh,receiver,14*scale,new Color32(14,11,38,255));
  PrismDesignMesh.Glow(vh,receiver,15*scale,new Color(1,1,1,.16f*bloom));
  PrismDesignMesh.Disc(vh,receiver,7.8f*scale,PrismTheme.Ivory);
 }
}

/// <summary>Curved map links in the same local coordinates as the node centers.</summary>
public sealed class PrismMapPath : MaskableGraphic {
 Vector2[] centers=new Vector2[0];float[] radii=new float[0];int completed;
 protected override void Awake(){base.Awake();raycastTarget=false;}
 /// <param name="nodeCenters">Local RectTransform positions; for a top pivot, y decreases downwards.</param>
 /// <param name="nodeRadii">Outer node radii, including their stone lip.</param>
 /// <param name="completedLinkCount">Number of continuous lit links, starting with the first link.</param>
 public void Configure(Vector2[] nodeCenters,float[] nodeRadii,int completedLinkCount){
  centers=nodeCenters==null?new Vector2[0]:(Vector2[])nodeCenters.Clone();
  radii=new float[centers.Length];
  for(int i=0;i<radii.Length;i++)radii[i]=nodeRadii!=null&&i<nodeRadii.Length?Mathf.Max(0,nodeRadii[i]):24;
  completed=Mathf.Clamp(completedLinkCount,0,Mathf.Max(0,centers.Length-1));SetVerticesDirty();
 }
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();
  for(int link=0;link<centers.Length-1;link++){
   Vector2 a=centers[link],b=centers[link+1];float distance=Vector2.Distance(a,b);
   float startRadius=radii[link]+6,endRadius=radii[link+1]+6;
   if(distance<=startRadius+endRadius)continue;
   float vertical=(b.y-a.y)*.47f;
   Vector2 c=a+new Vector2(0,vertical),d=b-new Vector2(0,vertical);
   // Both coordinates are monotonic, so distance from either endpoint is monotonic.
   // Search the whole curve to also handle unequal node radii.
   float start=Boundary(a,a,c,d,b,startRadius,true),end=Boundary(b,a,c,d,b,endRadius,false);
   if(start>=end)continue;
   Vector2 previous=Bezier(a,c,d,b,start);float travelled=0,nextDot=0;
   for(int step=1;step<=48;step++){
    Vector2 point=Bezier(a,c,d,b,Mathf.Lerp(start,end,step/48f));
    float length=Vector2.Distance(previous,point);
    if(link<completed){
     Color halo=PrismTheme.Accent;halo.a=.08f;PrismUiOptics.Line(vh,previous,point,7,halo);
     Color tint=Color.Lerp(PrismTheme.Accent,PrismTheme.Ivory,.3f);tint.a=.9f;PrismUiOptics.Line(vh,previous,point,2.3f,tint);
    }else{
     while(nextDot<=travelled+length){
      if(length>.001f){Color tint=PrismTheme.Muted;tint.a=.27f;PrismDesignMesh.Disc(vh,Vector2.Lerp(previous,point,(nextDot-travelled)/length),1.25f,tint,8);}
      nextDot+=10;
     }
    }
    travelled+=length;previous=point;
   }
  }
 }
 static Vector2 Bezier(Vector2 a,Vector2 c,Vector2 d,Vector2 b,float t){float u=1-t;return u*u*u*a+3*u*u*t*c+3*u*t*t*d+t*t*t*b;}
 static float Boundary(Vector2 center,Vector2 a,Vector2 c,Vector2 d,Vector2 b,float radius,bool fromStart){
  float low=0,high=1;
  for(int i=0;i<18;i++){
   float middle=(low+high)*.5f;bool inside=(Bezier(a,c,d,b,middle)-center).sqrMagnitude<radius*radius;
   if(inside==fromStart)low=middle;else high=middle;
  }
  return (low+high)*.5f;
 }
}

static class PrismDesignMesh {
 public static void GradientTriangle(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Color ca,Color cb,Color cc){
  int i=vh.currentVertCount;vh.AddVert(a,ca,Vector2.zero);vh.AddVert(b,cb,Vector2.zero);vh.AddVert(c,cc,Vector2.zero);vh.AddTriangle(i,i+1,i+2);
 }
 public static void Disc(VertexHelper vh,Vector2 center,float radius,Color tint,int segments=32){
  for(int i=0;i<segments;i++){float a=i*Mathf.PI*2/segments,b=(i+1)*Mathf.PI*2/segments;PrismUiOptics.Triangle(vh,center,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,tint);}
 }
 public static void Glow(VertexHelper vh,Vector2 center,float radius,Color tint){
  Color clear=tint;clear.a=0;
  for(int i=0;i<32;i++){float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;GradientTriangle(vh,center,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,tint,clear,clear);}
 }
 public static void Arc(VertexHelper vh,Vector2 center,float outer,float inner,float from,float to,Color tint){
  for(int step=0;step<8;step++){
   float a=Mathf.Lerp(from,to,step/8f)*Mathf.Deg2Rad,b=Mathf.Lerp(from,to,(step+1)/8f)*Mathf.Deg2Rad;
   Vector2 u=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),v=new Vector2(Mathf.Cos(b),Mathf.Sin(b));
   PrismUiOptics.Triangle(vh,center+u*inner,center+u*outer,center+v*outer,tint);PrismUiOptics.Triangle(vh,center+u*inner,center+v*outer,center+v*inner,tint);
  }
 }
 public static void RoundRect(VertexHelper vh,Rect r,float radius,Color top,Color bottom){
  radius=Mathf.Min(radius,Mathf.Min(r.width,r.height)*.5f);
  Vector2 previous=Vector2.zero,first=Vector2.zero;bool hasPrevious=false;
  for(int corner=0;corner<4;corner++){
   Vector2 center=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius);
   for(int step=0;step<=6;step++){
    float angle=(corner*90+step*15)*Mathf.Deg2Rad;Vector2 point=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
    if(!hasPrevious){first=point;hasPrevious=true;}else AddRectTriangle(vh,r,previous,point,top,bottom);
    previous=point;
   }
  }
  AddRectTriangle(vh,r,previous,first,top,bottom);
 }
 static void AddRectTriangle(VertexHelper vh,Rect r,Vector2 a,Vector2 b,Color top,Color bottom){
  GradientTriangle(vh,r.center,a,b,Color.Lerp(bottom,top,.5f),Color.Lerp(bottom,top,Mathf.InverseLerp(r.yMin,r.yMax,a.y)),Color.Lerp(bottom,top,Mathf.InverseLerp(r.yMin,r.yMax,b.y)));
 }
}
}
