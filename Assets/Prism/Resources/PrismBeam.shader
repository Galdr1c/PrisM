Shader "Prism/BeamGlow" {
 Properties {
  _Intensity("HDR Intensity", Range(0.5,8)) = 2.4
  _CoreWhite("Core White", Range(0,1)) = 0.16
  _Celebration("Completion Celebration", Range(0,1)) = 0
 }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+20" }
  Pass {
   Blend One One
   ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
   float _Intensity;
   float _CoreWhite;
   float _Celebration;
   struct A {float4 positionOS:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float2 uv2:TEXCOORD1;};
   struct B {float4 positionCS:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float kind:TEXCOORD1;float2 world:TEXCOORD2;};
   B vert(A v){B o;float3 w=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(w);o.color=v.color;o.uv=v.uv;o.kind=v.uv2.x;o.world=w.xy;return o;}
   half4 frag(B i):SV_Target {
    clip(4.98-abs(i.world.x));clip(4.98-abs(i.world.y));
    float profile=i.kind>0.5?pow(saturate(1.0-length(i.uv)),2.0):pow(saturate(1.0-abs(i.uv.y*2.0-1.0)),1.8);
    float core=pow(profile,5.0);
    float3 baseRgb=SRGBToLinear(i.color.rgb);
    float3 rgb=lerp(baseRgb,1.0.xxx,core*_CoreWhite*(i.kind>0.5?0.35:1.0));
    float pulse=0.975+0.025*sin(_Time.y*2.1+i.uv.x*9.0);
    float victoryWave=0.5+0.5*sin(_Time.y*9.5+i.world.x*3.4-i.world.y*2.7);
    float victoryGain=1.0+_Celebration*(0.35+0.45*victoryWave);
    float sparkle=pow(victoryWave,14.0)*_Celebration*profile;
    rgb=rgb*victoryGain+sparkle*0.42;
    return half4(rgb*(i.color.a*profile*_Intensity*pulse),1);
   }
   ENDHLSL
  }
 }
}
