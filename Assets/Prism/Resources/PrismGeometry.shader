Shader "Prism/Geometry" {
 Properties { }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
  Pass {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
   float _PrismMotionTime;
   struct A { float4 positionOS:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; float2 kind:TEXCOORD1; };
   struct B { float4 positionCS:SV_POSITION; float4 color:COLOR; float2 world:TEXCOORD0; float2 uv:TEXCOORD1; float kind:TEXCOORD2; };
   B vert(A v){ B o; float3 w=TransformObjectToWorld(v.positionOS.xyz); o.positionCS=TransformWorldToHClip(w); o.color=v.color; o.world=w.xy; o.uv=v.uv; o.kind=v.kind.x; return o; }
   half4 frag(B i):SV_Target {
    clip(4.98-abs(i.world.x)); clip(4.98-abs(i.world.y));
    half alpha=i.color.a;
    if(i.kind>.5&&i.kind<1.5)alpha*=pow(saturate(1-length(i.uv)),2);
    half pulse=i.kind>1.5 ? 1+.08*sin(_PrismMotionTime*3.4) : 1;
    return half4(SRGBToLinear(i.color.rgb)*pulse,alpha);
   }
   ENDHLSL
  }
 }
}
