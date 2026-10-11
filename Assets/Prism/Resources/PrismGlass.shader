Shader "Prism/Glass" {
 Properties {
  _Tint("Tint", Color) = (0.77,0.79,1,0.50)
  _EdgeIntensity("Edge Intensity", Range(0,4)) = 0.55
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
   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
   float4 _Tint;
   float _EdgeIntensity;
   float _PrismMotionTime;
   struct A {float4 positionOS:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
   struct B {float4 positionCS:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float2 world:TEXCOORD1;};
   B vert(A v){B o;float3 w=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(w);o.color=v.color;o.uv=v.uv;o.world=w.xy;return o;}
   half4 frag(B i):SV_Target {
    clip(4.98-abs(i.world.x));clip(4.98-abs(i.world.y));
    float2 p=i.uv*2.0-1.0;
    float edge=smoothstep(0.42,1.0,length(p));
    float shimmer=0.5+0.5*sin(i.world.x*5.4+i.world.y*3.7+_PrismMotionTime*0.2);
    float spectral=0.5+0.5*sin((i.world.x-i.world.y)*8.0+_PrismMotionTime*0.35);
    float3 rainbow=lerp(float3(0.43,0.53,0.82),float3(0.60,0.40,0.78),spectral);
    float highlight=pow(saturate(1.0-length(p-float2(-0.35,0.4))*0.9),5.0);
    float3 rgb=lerp(_Tint.rgb*SRGBToLinear(i.color.rgb),rainbow,edge*0.08);
    rgb*=0.85+shimmer*0.04+edge*_EdgeIntensity;
    rgb+=float3(.6,.72,1)*highlight*.12;
    float alpha=saturate(_Tint.a*i.color.a+edge*0.08+highlight*.04);
    return half4(rgb,alpha);
   }
   ENDHLSL
  }
 }
}
