Shader "Prism/VertexLight" {
 Properties { }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
  Pass {
   // Every element in this procedural mesh is light laid over a dark board.
   // Additive blending lets overlapping spectral bands reconstruct white.
   Blend SrcAlpha One
   ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
   struct A {float4 positionOS:POSITION;float4 color:COLOR;};
   struct B {float4 positionCS:SV_POSITION;float4 color:COLOR;float2 xy:TEXCOORD0;};
   B vert(A v){B o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.color=v.color;o.xy=v.positionOS.xy;return o;}
   half4 frag(B i):SV_Target {
    clip(4.98-abs(i.xy.x));clip(4.98-abs(i.xy.y));
    // Authored colors are visual sRGB values; vertex colors arrive unconverted.
    return half4(SRGBToLinear(i.color.rgb),i.color.a);
   }
   ENDHLSL
  }
 }
}
