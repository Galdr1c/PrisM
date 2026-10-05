Shader "Prism/Water" {
 Properties { _Tint("Tint", Color) = (0.12,0.48,0.65,0.28) _Glow("Glow", Range(0,3)) = 1.1 }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+10" }
  Pass {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
   float4 _Tint; float _Glow;
   float _PrismMotionTime;
   struct A {float4 positionOS:POSITION;float4 color:COLOR;};
   struct B {float4 positionCS:SV_POSITION;float4 color:COLOR;float2 world:TEXCOORD0;};
   B vert(A v){B o;float3 w=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(w);o.color=v.color;o.world=w.xy;return o;}
   half4 frag(B i):SV_Target {
    clip(4.98-abs(i.world.x));clip(4.98-abs(i.world.y));
    float t=_PrismMotionTime;
    float a=sin(i.world.x*7.3+i.world.y*4.9+t*0.9);
    float b=sin(i.world.x*3.8-i.world.y*8.1-t*0.7);
    float glow=pow(saturate(0.5+0.25*a+0.25*b),5.0);
    float3 rgb=_Tint.rgb*SRGBToLinear(i.color.rgb)+float3(0.15,0.48,0.7)*glow*_Glow;
    return half4(rgb,saturate(_Tint.a*i.color.a+glow*0.08));
   }
   ENDHLSL
  }
 }
}
