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
   case "trash":line(new Vector2(-s*.4f,s*.4f),new Vector2(-s*.3f,-s*.65f),stroke);line(new Vector2(s*.4f,s*.4f),new Vector2(s*.3f,-s*.65f),stroke);line(new Vector2(-s*.3f,-s*.65f),new Vector2(s*.3f,-s*.65f),stroke);line(new Vector2(-s*.6f,s*.55f),new Vector2(s*.6f,s*.55f),stroke);break;
   default:line(new Vector2(0,s*.85f),new Vector2(-s*.8f,-s*.55f),stroke);line(new Vector2(-s*.8f,-s*.55f),new Vector2(s*.8f,-s*.55f),stroke);line(new Vector2(s*.8f,-s*.55f),new Vector2(0,s*.85f),stroke);break;
  }
 }
}
}
