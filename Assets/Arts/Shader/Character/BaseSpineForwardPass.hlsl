#ifndef DD_BASE_SPINE_FORWARD_PASS_INCLUDED
#define DD_BASE_SPINE_FORWARD_PASS_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../Common/SpriteEffects.hlsl"
#include "BaseSpineInput.hlsl"

struct Attributes
{
    float4 positionCSitionOS : POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
    half3 normalOS : NORMAL;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float4 positionWS : TEXCOORD0;
    half3 normalWS : TEXCOORD1;
    float4 uv0 : TEXCOORD2;
    float4 uv1 : TEXCOORD3;
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings vert(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    output.positionCS = TransformObjectToHClip(input.positionCSitionOS.xyz);
    output.uv0 = float4(input.uv,TRANSFORM_TEX(input.uv, _GlowMask));
    output.color = input.color;
    output.uv1 = float4( TRANSFORM_TEX(input.uv, _DissolveTex),TRANSFORM_TEX(input.uv, _DistortTex));
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    return output;
}

half4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    
    Light mainLight = GetMainLight();
    half shadow = mainLight.shadowAttenuation;
    
    half4 var_MainTex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, input.uv0.xy);
    half alpha = step(0.99,var_MainTex.a);
    half3 normal = SafeNormalize(input.normalWS);

    #ifdef _FAULT_ON
    var_MainTex = Fault(input.uv0.xy);
    alpha = var_MainTex.a; 
    #endif
    
    #ifdef _DISSOLVE_ON
    var_MainTex = dissolve(input.uv0.xy, input.uv1.xy,half4(var_MainTex.rgb,alpha));
    alpha *= var_MainTex.a;
    #endif
    
    #ifdef _ADDTEXTURE_ON
    var_MainTex = AddTexture(input.uv0.xy, input.uv1.zw,var_MainTex);
    #endif
    
    #if defined(_STRAIGHT_ALPHA_INPUT)
    var_MainTex.rgb *= var_MainTex.a;
    #endif

    var_MainTex *= input.color;

    half3 diffuse = var_MainTex.rgb * mainLight.color;
    half3 gi = SampleSH(normal);

    half4 finalColor = half4(diffuse * shadow + gi * var_MainTex.rgb ,alpha);
    
    half3 addLight = half3(0.0, 0.0, 0.0);
    
    #ifdef _ADDITIONAL_LIGHTS
    uint AdditionalLightCount = GetAdditionalLightsCount();
    for(uint LightIndex = 0;LightIndex<AdditionalLightCount;LightIndex++)
    {
        Light light = GetAdditionalLight(LightIndex,input.positionWS);
        half shadow = light.shadowAttenuation * light.distanceAttenuation;
        addLight += shadow * light.color * var_MainTex;
    }
    #endif

    #ifdef _GLOW_ON
    finalColor.rgb += Glow(input.uv0.xy,input.uv0.zw).rgb;
    #endif
    
    finalColor.rgb += addLight;
    
    clip(alpha - 0.01);
    
    return finalColor;
}

#endif
