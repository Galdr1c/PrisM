Shader "Prism/BeamGlow" {
 Properties {
  _Intensity("HDR Intensity", Range(0.5,8)) = 3.4
  _CoreWhite("Core White", Range(0,1)) = 0.55
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
   float _Intensity;
   float _CoreWhite;
   struct A {float4 positionOS:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float2 uv2:TEXCOORD1;};
   struct B {float4 positionCS:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float kind:TEXCOORD1;};
   B vert(A v){B o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.color=v.color;o.uv=v.uv;o.kind=v.uv2.x;return o;}
   half4 frag(B i):SV_Target {
    float profile;
    if(i.kind>0.5) profile=pow(saturate(1.0-length(i.uv)),2.0);
    else {float across=abs(i.uv.y*2.0-1.0);profile=pow(saturate(1.0-across),1.8);}
    float core=pow(profile,5.0);
    float3 rgb=lerp(i.color.rgb,1.0.xxx,core*_CoreWhite);
    float pulse=0.975+0.025*sin(_Time.y*2.1+i.uv.x*9.0);
    return half4(rgb*(i.color.a*profile*_Intensity*pulse),1);
   }
   ENDHLSL
  }
 }
}
