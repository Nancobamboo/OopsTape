#ifndef DD_BASE_SPINE_OUTLINE_INPUT_INCLUDE
#define DD_BASE_SPINE_OUTLINE_INPUT_INCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

TEXTURE2D(_OutlineTex);
SAMPLER(sampler_OutlineTex);

TEXTURE2D(_OutlineTex1);
SAMPLER(sampler_OutlineTex1);

CBUFFER_START(UnityPerMaterial)

float4 _MainTex_ST;
float4 _OutlineTex_ST;
float4 _OutlineTex1_ST;
half4 _OutlineColor;
half4 _OutlineColor1;
half4 _OutlineParams;
half _Cutoff;
half _OutlineWidth;
half _OutlineSoftness;
half _OutlineTexIntensity;
half _OutlineWidth1;
half _OutlineSoftness1;
half _OutlineTexIntensity1;
half _OutlineAutoInOut;

CBUFFER_END

#endif
