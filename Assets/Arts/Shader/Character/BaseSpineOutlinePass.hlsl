#ifndef DD_BASE_SPINE_FORWARD_PASS_INCLUDED
#define DD_BASE_SPINE_FORWARD_PASS_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "BaseSpineOutlineInput.hlsl"

float EaseInOut(float t, float duration, float inTime, float outTime)
{
      
    float deltaTime = _Time.y - t;
    deltaTime = clamp(deltaTime, 0.0, duration);

    float a = smoothstep(0.0, inTime, deltaTime);                 
    float b = smoothstep(duration - outTime, duration, deltaTime);
    return min(a, 1.0 - b);
}

struct Attributes
{
    float4 positionCSitionOS : POSITION;
    float2 uv : TEXCOORD0;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float4 uv : TEXCOORD0;
    float2 uv1 : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings vert(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    output.positionCS = TransformObjectToHClip(input.positionCSitionOS.xyz);
    output.uv.xy = input.uv;
    output.uv.zw = input.uv * _OutlineTex_ST.xy + frac(_OutlineTex_ST.zw * _Time.y);
    output.uv1.xy = input.uv * _OutlineTex1_ST.xy + frac(_OutlineTex1_ST.zw * _Time.y);
    return output;
}

half4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);

    half4 texColor = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, input.uv.xy);
    
    half4 var_OutlineTex = SAMPLE_TEXTURE2D(_OutlineTex,sampler_OutlineTex, input.uv.zw);
    half alpha =texColor.a - _Cutoff + _OutlineWidth;
    alpha = lerp(alpha,alpha * var_OutlineTex,_OutlineTexIntensity);
    alpha = _OutlineColor.a * smoothstep(0,_OutlineSoftness,alpha * rcp(_OutlineWidth));
    

    half4 var_OutlineTex1= SAMPLE_TEXTURE2D(_OutlineTex1,sampler_OutlineTex1, input.uv1.xy);
    half alpha1 =texColor.a - _Cutoff + _OutlineWidth1;
    alpha1 = lerp(alpha1,alpha1 * var_OutlineTex1,_OutlineTexIntensity1);
    alpha1 = _OutlineColor1.a * smoothstep(0,_OutlineSoftness1,alpha1 * rcp(_OutlineWidth1));

    half a0Masked = alpha * (1 - alpha1);

    half finalAlpha = saturate(a0Masked + alpha1);
    half3 finalRGB= saturate(_OutlineColor.rgb * a0Masked + _OutlineColor1.rgb * alpha1);
    
    if(_OutlineAutoInOut > 0.5)
    {
        finalAlpha *= EaseInOut(_OutlineParams.x, _OutlineParams.y, _OutlineParams.z, _OutlineParams.w);
    }

    clip(finalAlpha);
    
    return half4(finalRGB,finalAlpha);
}

#endif
