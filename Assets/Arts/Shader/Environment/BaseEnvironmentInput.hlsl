#ifndef THS_ENVIRONMENT_INPUT_INCLUDED
#define THS_ENVIRONMENT_INPUT_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
    float4 _NoiseMask_ST;
    float4 _NoiseTex_ST;
    float4 _NoiseTex2_ST;
    half4 _BaseColor;
    half4 _NoiseColor;
    half _Cutoff;
    half _NoiseBlendMode;
    half _EmissionInt;
CBUFFER_END


TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

#ifdef EMISSION_ON
TEXTURE2D(_EmissionTex);
SAMPLER(sampler_EmissionTex);
#endif

#ifdef NOISE_ON
TEXTURE2D(_NoiseMask);
SAMPLER(sampler_NoiseMask);
TEXTURE2D(_NoiseTex);
SAMPLER(sampler_NoiseTex);
TEXTURE2D(_NoiseTex2);
SAMPLER(sampler_NoiseTex2);
#endif

#endif
