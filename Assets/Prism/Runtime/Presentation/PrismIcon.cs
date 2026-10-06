using UnityEngine;
using UnityEngine.UI;

namespace Prism {
/// <summary>Resolution independent optical and navigation glyphs.</summary>
public sealed class PrismIcon : MaskableGraphic {
 public string Symbol="prism";
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();var r=rectTransform.rect;float s=Mathf.Min(r.width,r.height)*.34f;Vector2 c=r.center;
  System.Action<Vector2,Vector2,float> line=(a,b,w)=>{Vector2 n=(b-a).normalized; n=new Vector2(-n.y,n.x)*w;int i=vh.currentVertCount;vh.AddVert(c+a-n,color,Vector2.zero);vh.AddVert(c+a+n,color,Vector2.zero);vh.AddVert(c+b+n,color,Vector2.zero);vh.AddVert(c+b-n,color,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);};
  float stroke=1.2f;
  System.Action<float,float> ring=(radius,phase)=>{for(int i=0;i<32;i++){float a=(i/32f*Mathf.PI*2)+phase,b=((i+1)/32f*Mathf.PI*2)+phase;line(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,stroke);}};
  switch(Symbol){
   case "back":line(new Vector2(s*.35f,s*.7f),new Vector2(-s*.35f,0),stroke);line(new Vector2(-s*.35f,0),new Vector2(s*.35f,-s*.7f),stroke);break;
   case "close":line(new Vector2(-s*.55f,-s*.55f),new Vector2(s*.55f,s*.55f),stroke);line(new Vector2(-s*.55f,s*.55f),new Vector2(s*.55f,-s*.55f),stroke);break;
   case "pause":line(new Vector2(-s*.28f,-s*.65f),new Vector2(-s*.28f,s*.65f),stroke);line(new Vector2(s*.28f,-s*.65f),new Vector2(s*.28f,s*.65f),stroke);break;
   case "reset":case "undo":for(int i=0;i<24;i++){float a=i/24f*4.5f-.8f,b=(i+1)/24f*4.5f-.8f;line(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*s*.65f,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*s*.65f,stroke);}line(new Vector2(-s*.55f,s*.45f),new Vector2(-s*.75f,s*.1f),stroke);line(new Vector2(-s*.75f,s*.1f),new Vector2(-s*.3f,s*.13f),stroke);break;
   case "hint":ring(s*.45f,0);line(new Vector2(-s*.2f,-s*.65f),new Vector2(s*.2f,-s*.65f),stroke);for(int i=0;i<5;i++){float a=i*Mathf.PI/4;line(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*s*.75f,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*s,stroke);}break;
   case "settings":ring(s*.55f,0);ring(s*.17f,0);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;line(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*s*.55f,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*s*.8f,stroke);}break;
   case "map":ring(s*.18f,0);line(new Vector2(-s*.7f,-s*.6f),Vector2.zero,stroke);line(Vector2.zero,new Vector2(s*.6f,s*.65f),stroke);break;
   case "check":line(new Vector2(-s*.65f,0),new Vector2(-s*.1f,-s*.4f),stroke);line(new Vector2(-s*.1f,-s*.4f),new Vector2(s*.7f,s*.5f),stroke);break;
   case "mirror":line(new Vector2(-s*.65f,-s*.65f),new Vector2(s*.65f,s*.65f),stroke*1.8f);break;
   case "lens":for(int i=0;i<16;i++){float a=-Mathf.PI*.5f+i/16f*Mathf.PI,b=-Mathf.PI*.5f+(i+1)/16f*Mathf.PI;line(new Vector2(Mathf.Cos(a)*s*.35f,Mathf.Sin(a)*s),new Vector2(Mathf.Cos(b)*s*.35f,Mathf.Sin(b)*s),stroke);line(new Vector2(-Mathf.Cos(a)*s*.35f,Mathf.Sin(a)*s),new Vector2(-Mathf.Cos(b)*s*.35f,Mathf.Sin(b)*s),stroke);}break;
   case "sphere":ring(s*.75f,0);break;
   case "red":case "green":line(new Vector2(-s*.55f,-s*.8f),new Vector2(s*.55f,s*.8f),stroke*2);ring(s*.16f,0);break;
   case "spectrum":for(int i=-2;i<=2;i++)line(new Vector2(-s*.7f,0),new Vector2(s*.75f,i*s*.3f),stroke*.7f);break;
   case "split":line(new Vector2(-s*.8f,0),Vector2.zero,stroke);line(Vector2.zero,new Vector2(s*.7f,s*.6f),stroke);line(Vector2.zero,new Vector2(s*.7f,-s*.6f),stroke);break;
   case "gate":line(new Vector2(-s*.5f,-s),new Vector2(-s*.5f,-s*.25f),stroke*1.6f);line(new Vector2(-s*.5f,s*.25f),new Vector2(-s*.5f,s),stroke*1.6f);line(new Vector2(-s*.8f,0),new Vector2(s*.85f,0),stroke);break;
   case "wave":for(int i=0;i<24;i++){float x=i/24f*2-1,next=(i+1)/24f*2-1;line(new Vector2(x*s,Mathf.Sin(x*Mathf.PI*2)*s*.35f),new Vector2(next*s,Mathf.Sin(next*Mathf.PI*2)*s*.35f),stroke);}break;
   case "interference":line(new Vector2(-s*.8f,-s*.55f),new Vector2(s*.8f,s*.55f),stroke);line(new Vector2(-s*.8f,s*.55f),new Vector2(s*.8f,-s*.55f),stroke);ring(s*.3f,0);break;
   case "orbit":ring(s*.7f,0);line(new Vector2(-s*.8f,-s*.4f),new Vector2(s*.8f,s*.4f),stroke);ring(s*.12f,0);break;
   case "trash":line(new Vector2(-s*.4f,s*.4f),new Vector2(-s*.3f,-s*.65f),stroke);line(new Vector2(s*.4f,s*.4f),new Vector2(s*.3f,-s*.65f),stroke);line(new Vector2(-s*.3f,-s*.65f),new Vector2(s*.3f,-s*.65f),stroke);line(new Vector2(-s*.6f,s*.55f),new Vector2(s*.6f,s*.55f),stroke);break;
   default:line(new Vector2(0,s*.85f),new Vector2(-s*.8f,-s*.55f),stroke);line(new Vector2(-s*.8f,-s*.55f),new Vector2(s*.8f,-s*.55f),stroke);line(new Vector2(s*.8f,-s*.55f),new Vector2(0,s*.85f),stroke);break;
  }
 }
}
}

namespace Prism {
/// <summary>UI-native optical motif; independent of the app icon and any raster logo.</summary>
public sealed class PrismOpticalMark : MaskableGraphic {
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();var r=rectTransform.rect;float size=Mathf.Min(r.width,r.height);Vector2 c=r.center;
  float thickness=Mathf.Max(1.2f,size*.017f);
  PrismUiOptics.Line(vh,c+new Vector2(-.46f,0)*size,c+new Vector2(-.17f,0)*size,thickness,PrismTheme.Ivory);
  Vector2 a=c+new Vector2(-.17f,-.23f)*size,b=c+new Vector2(-.17f,.23f)*size,d=c+new Vector2(.14f,0)*size;
  // Closed refractive body makes the event legible as a solid silhouette at 32 px.
  PrismUiOptics.Triangle(vh,a,b,d,new Color(.90f,.87f,1,.18f));
  PrismUiOptics.Line(vh,a,b,thickness,PrismTheme.Ivory);PrismUiOptics.Line(vh,b,d,thickness,PrismTheme.Ivory);PrismUiOptics.Line(vh,d,a,thickness,PrismTheme.Ivory);
  for(int i=0;i<7;i++){
   var tint=PrismUiOptics.Spectrum[i];
   Vector2 lower=c+new Vector2(.46f,(i-3.5f)*.067f)*size,upper=c+new Vector2(.46f,(i-2.5f)*.067f)*size;
   PrismUiOptics.Triangle(vh,d,lower,upper,tint);
  }
 }
}
public sealed class PrismBeamPreview : MaskableGraphic {
 float beam=-1,bloom=-1;
 void Update(){if(beam!=VisualEnvironment.BeamScale||bloom!=VisualEnvironment.BloomScale){beam=VisualEnvironment.BeamScale;bloom=VisualEnvironment.BloomScale;SetVerticesDirty();}}
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();var r=rectTransform.rect;Vector2 from=new Vector2(r.xMin+16,r.center.y),split=new Vector2(r.center.x-8,r.center.y);
  PrismUiOptics.Line(vh,from,split,1.1f*VisualEnvironment.BeamScale,PrismTheme.Ivory);
  for(int i=0;i<7;i++){
   Vector2 end=new Vector2(r.xMax-16,r.center.y+(i-3)*5);Color tint=PrismUiOptics.Spectrum[i];
   for(int layer=3;layer>=1;layer--){Color halo=tint;halo.a=VisualEnvironment.BloomScale*.04f;PrismUiOptics.Line(vh,split,end,layer*3*VisualEnvironment.BeamScale,halo);}
   PrismUiOptics.Line(vh,split,end,.8f*VisualEnvironment.BeamScale,tint);
  }
 }
}
public sealed class PrismOpticalAmbience : MaskableGraphic {
 float last;
 void Update(){if(!VisualEnvironment.ReducedMotion&&Time.unscaledTime-last>.08f){last=Time.unscaledTime;SetVerticesDirty();}}
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();var r=rectTransform.rect;float phase=VisualEnvironment.ReducedMotion?0:Time.unscaledTime*.08f;
  for(int band=0;band<5;band++){
   Color tint=band%2==0?new Color(.25f,.19f,.38f,.016f):new Color(.15f,.25f,.33f,.016f);
   for(int i=0;i<32;i++){
    float x=i/32f,next=(i+1)/32f;
    Vector2 a=new Vector2(Mathf.Lerp(r.xMin,r.xMax,x),r.yMin+r.height*(.4f+band*.045f+Mathf.Sin(x*4+phase+band*.55f)*.09f));
    Vector2 b=new Vector2(Mathf.Lerp(r.xMin,r.xMax,next),r.yMin+r.height*(.4f+band*.045f+Mathf.Sin(next*4+phase+band*.55f)*.09f));
    PrismUiOptics.Line(vh,a,b,3+band,tint);
   }
  }
 }
}
static class PrismUiOptics {
 public static readonly Color[] Spectrum={new Color(.59f,.43f,1),new Color(.38f,.58f,1),new Color(.43f,.84f,.9f),new Color(.47f,.87f,.64f),new Color(.95f,.88f,.55f),new Color(1,.65f,.45f),new Color(.94f,.43f,.53f)};
 public static void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color tint){Vector2 d=(b-a).normalized,n=new Vector2(-d.y,d.x)*width*.5f;int i=vh.currentVertCount;vh.AddVert(a-n,tint,Vector2.zero);vh.AddVert(a+n,tint,Vector2.zero);vh.AddVert(b+n,tint,Vector2.zero);vh.AddVert(b-n,tint,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
 public static void Triangle(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Color tint){int i=vh.currentVertCount;vh.AddVert(a,tint,Vector2.zero);vh.AddVert(b,tint,Vector2.zero);vh.AddVert(c,tint,Vector2.zero);vh.AddTriangle(i,i+1,i+2);}
}
}
