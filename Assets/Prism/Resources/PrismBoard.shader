Shader "Prism/Board" {
 Properties {
  _Background("Background", Color) = (0.025,0.045,0.065,1)
  _GridColor("Grid", Color) = (0.16,0.26,0.31,1)
  _SurfaceTex("Laboratory Surface", 2D) = "black" {}
  _SurfaceAmount("Surface Amount", Range(0,0.35)) = 0
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
   TEXTURE2D(_SurfaceTex); SAMPLER(sampler_SurfaceTex);
   float _SurfaceAmount;
   struct A { float4 positionOS:POSITION; };
   struct B { float4 positionCS:SV_POSITION; float2 world:TEXCOORD0; };
   B vert(A v){B o;float3 w=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(w);o.world=w.xy;return o;}
   half4 frag(B i):SV_Target {
    float2 cell=frac((i.world+0.25)/0.5)-0.5;
    float aa=max(fwidth(length(cell)),0.008);
    float dotMask=1.0-smoothstep(0.045-aa,0.045+aa,length(cell));
    float radius=length(i.world);
    float vignette=saturate(1.08-radius*0.06);
    float centerGlow=saturate(1.0-radius/6.5);
    float edgeFade=1.0-smoothstep(3.6,5.2,radius);
    float3 baseColor=_Background.rgb*(0.80+0.17*vignette)+float3(0.012,0.025,0.029)*centerGlow;
    float3 surface=SAMPLE_TEXTURE2D(_SurfaceTex,sampler_SurfaceTex,saturate(i.world/10.0+0.5)).rgb;
    baseColor=lerp(baseColor,surface,_SurfaceAmount);
    float gridStrength=dotMask*(0.40+0.16*centerGlow)*edgeFade;
    return half4(lerp(baseColor,_GridColor.rgb,gridStrength),1);
   }
   ENDHLSL
  }
 }
}
