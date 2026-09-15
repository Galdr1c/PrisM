Shader "Prism/Board" {
 Properties {
  _Background("Background", Color) = (0.025,0.045,0.065,1)
  _GridColor("Grid", Color) = (0.16,0.26,0.31,1)
 }
 SubShader {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
  Pass {
   ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   float4 _Background;
   float4 _GridColor;
   struct A { float4 positionOS:POSITION; };
   struct B { float4 positionCS:SV_POSITION; float2 world:TEXCOORD0; };
   B vert(A v){B o;float3 w=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(w);o.world=w.xy;return o;}
   half4 frag(B i):SV_Target {
    float2 cell=frac((i.world+0.25)/0.5)-0.5;
    float dotMask=1.0-smoothstep(0.035,0.065,length(cell));
    float vignette=saturate(1.05-length(i.world)*0.055);
    float3 baseColor=_Background.rgb*(0.84+0.16*vignette);
    return half4(lerp(baseColor,_GridColor.rgb,dotMask*0.72),1);
   }
   ENDHLSL
  }
 }
}
