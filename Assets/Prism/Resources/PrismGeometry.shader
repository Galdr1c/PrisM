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
   struct A { float4 positionOS:POSITION; float4 color:COLOR; };
   struct B { float4 positionCS:SV_POSITION; float4 color:COLOR; float2 world:TEXCOORD0; };
   B vert(A v){ B o; float3 w=TransformObjectToWorld(v.positionOS.xyz); o.positionCS=TransformWorldToHClip(w); o.color=v.color; o.world=w.xy; return o; }
   half4 frag(B i):SV_Target {
    clip(4.98-abs(i.world.x)); clip(4.98-abs(i.world.y));
    return half4(SRGBToLinear(i.color.rgb),i.color.a);
   }
   ENDHLSL
  }
 }
}
