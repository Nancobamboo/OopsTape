#ifndef DD_BASE_SPINE_INPUT_INCLUDE
#define DD_BASE_SPINE_INPUT_INCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)

float4 _MainTex_ST;
float4 _GlowColor;
float4 _AddColor;
float4 _GlowMask_ST;
float4 _AddGlowTex_ST;
float4 _DissolveTex_ST;
float4 _DistortTex_ST;
half4 _DissolveGlowColor;
float _GlowThreshold;
float _GlowIntensity;
float _DistortAmount;
float _DistortTexXSpeed;
float _DistortTexYSpeed;
half _DissolveAmount;
half _GlowWidth;
half _FaultAmount;
half _FaultAlpha;
half _UseGlowAlphaWeight; // 0 = use emission.r, 1 = use emission.a
half _AddTextureBlendMode; // 0 = Add, 1 = Alpha
half _Cutoff;

CBUFFER_END

#endif
