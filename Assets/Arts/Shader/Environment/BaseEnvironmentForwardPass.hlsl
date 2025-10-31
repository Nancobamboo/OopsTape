#ifndef THS_ENVIRONMENT_FORWARD_PASS_INCLUDED
#define THS_ENVIRONMENT_FORWARD_PASS_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "BaseEnvironmentInput.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    float2 staticLightmapUV : TEXCOORD1;
    half3 normalOS : NORMAL;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    #ifdef NOISE_ON
    float4 uv0 : TEXCOORD0;
    #else
    float2 uv0 : TEXCOORD0;
    #endif
    half3 normalWS : TEXCOORD1;
    float4 shadowCoord : TEXCOORD2;
    float3 positionWS : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


Varyings vert(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.uv0.xy = TRANSFORM_TEX(input.uv, _MainTex);
    #ifdef NOISE_ON
    output.uv0.zw = TRANSFORM_TEX(input.uv, _NoiseMask);
    #endif
    output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    return output;
}


half4 frag(Varyings input, half facing : VFACE) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    
    float4 shadowCooord = input.shadowCoord;
    Light mainLight = GetMainLight(shadowCooord);
    half shadow = mainLight.shadowAttenuation;
    half3 normal = SafeNormalize(input.normalWS);

    if(facing < 0)
    {
        normal = -normal;
    }
    
    half4 var_MainTex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv0.xy) * _BaseColor;

    
    #ifdef NOISE_ON
    half var_NoiseMask = SAMPLE_TEXTURE2D(_NoiseMask,sampler_NoiseMask,input.uv0.zw);
    float2 worldUV = input.positionWS.xz;
    half var_NoiseTex = SAMPLE_TEXTURE2D(_NoiseTex,sampler_NoiseTex,worldUV * _NoiseTex_ST.xy + frac(_NoiseTex_ST.zw * _Time.y));
    half var_NoiseTex1 = SAMPLE_TEXTURE2D(_NoiseTex2,sampler_NoiseTex2,frac(worldUV * _NoiseTex2_ST.xy + frac(_NoiseTex2_ST.zw * _Time.y)));
    half noise = 0;
    if(_NoiseBlendMode<0.5)
    {
        noise = var_NoiseTex * var_NoiseTex1;
    }
    else if(_NoiseBlendMode<1.5)
    {
        noise = max(var_NoiseTex,var_NoiseTex1);
    }
    else
    {
        noise = min(var_NoiseTex,var_NoiseTex1);
    }
    noise *= var_NoiseMask;
    var_MainTex.rgb = lerp(var_MainTex.rgb * _NoiseColor.rgb, var_MainTex.rgb, noise);
    #endif
    
    half NoL = max(0, dot(normal, mainLight.direction));
    half3 diffuse = var_MainTex.rgb * NoL * mainLight.color;
    half3 gi = SampleSH(normal);
    
    half4 finalColor = half4(diffuse * shadow + gi * var_MainTex.rgb ,var_MainTex.a);

    half3 addLight = half3(0.0, 0.0, 0.0);
    #ifdef _ADDITIONAL_LIGHTS
    uint AdditionalLightCount = GetAdditionalLightsCount();
    for(uint LightIndex = 0;LightIndex<AdditionalLightCount;LightIndex++)
    {
        Light light = GetAdditionalLight(LightIndex,input.positionWS);
        half shadow = light.shadowAttenuation * light.distanceAttenuation ;
        addLight += shadow * light.color * var_MainTex;
    }
    #endif
    finalColor.rgb += addLight;
    
    #ifdef EMISSION_ON
    half3 emission = SAMPLE_TEXTURE2D(_EmissionTex, sampler_EmissionTex, input.uv0).rgb * _EmissionInt;
    finalColor.rgb += emission;
    #endif

    clip(finalColor.a - _Cutoff);
    return finalColor;
}


#endif
