Shader "Prism/Glass" {
 Properties {
  _Tint("Tint", Color) = (0.5,0.85,1,0.28)
  _EdgeIntensity("Edge Intensity", Range(0,4)) = 1.6
 }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+30" }
  Pass {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   float4 _Tint;
   float _EdgeIntensity;
   struct A {float4 positionOS:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
   struct B {float4 positionCS:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;};
   B vert(A v){B o;float3 w=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(w);o.color=v.color;o.uv=v.uv;o.world=w.xy;return o;}
   half4 frag(B i):SV_Target {
    float2 p=i.uv*2.0-1.0;
    float edge=smoothstep(0.42,1.0,length(p));
    float shimmer=0.5+0.5*sin(i.world.x*5.4+i.world.y*3.7+_Time.y*0.55);
    float spectral=0.5+0.5*sin((i.world.x-i.world.y)*8.0+_Time.y*0.35);
    float3 rainbow=lerp(float3(0.45,0.8,1.0),float3(1.0,0.5,0.85),spectral);
    float3 rgb=lerp(_Tint.rgb*i.color.rgb,rainbow,edge*0.32);
    rgb*=0.72+shimmer*0.18+edge*_EdgeIntensity;
    float alpha=saturate(_Tint.a*i.color.a + edge*0.2);
    return half4(rgb,alpha);
   }
   ENDHLSL
  }
 }
}
