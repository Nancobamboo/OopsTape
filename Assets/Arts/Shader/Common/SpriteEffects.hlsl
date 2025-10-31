#ifndef SPRITE_EFFECTS_INCLUDED
#define SPRITE_EFFECTS_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "../Character/BaseSpineInput.hlsl"


TEXTURE2D(_GlowMask);
SAMPLER(sampler_GlowMask);

TEXTURE2D(_GlowTex);
SAMPLER(sampler_GlowTex);

TEXTURE2D(_DissolveTex);
SAMPLER(sampler_DissolveTex);

TEXTURE2D(_AddGlowTex);
SAMPLER(sampler_AddGlowTex);
// ST and params moved to CBUFFER in BaseSpineInput.hlsl
TEXTURE2D(_DistortTex);
SAMPLER(sampler_DistortTex);


half4 Glow(float2 uv, float2 glowMaskUV)
{
    half4 emission = SAMPLE_TEXTURE2D(_GlowTex, sampler_GlowTex, uv);
    half mask = SAMPLE_TEXTURE2D(_GlowMask, sampler_GlowMask, glowMaskUV).r;

    float3 glow = mask * _GlowColor * _GlowIntensity;

    emission.rgb = emission.a * mask * _GlowIntensity * _GlowColor * mask + glow;
    
    return emission;
}

half4 dissolve(float2 uv, float2 dissolveUV,half4 color)
{
    // 采样 Dissolve 纹理的 r 通道值
    float dissolveTemp = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, dissolveUV).r;

    // 溶解判定
    half visible = step(_DissolveAmount, dissolveTemp);

    // alpha 控制
    color.a *= visible;
                
    // 溶解边缘区域权重：以阈值为中心，宽度由 _GlowWidth 控制
    float width = max(_GlowWidth, 1e-5);
    float edge = smoothstep(_DissolveAmount - width, _DissolveAmount, dissolveTemp)
               - smoothstep(_DissolveAmount, _DissolveAmount + width, dissolveTemp);
    edge = saturate(edge); // 保证范围 [0,1]

    // 边缘发光叠加
    color.rgb += _DissolveGlowColor.rgb * edge * 10;
    
    return color;
}

half4 Fault(float2 uv)
{
    half off = _FaultAmount * 0.1;

    half4 r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + half2( off, 0));
    half4 g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    half4 b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - half2( off, 0));

    r *= step(0.99, r.aaaa);
    g *= step(0.99, g.aaaa);
    b *= step(0.99, b.aaaa);

    r.a *= 0.5;
    b.a *= 0.5;

    half3 rgb = half3(r.r, g.g, b.b);

    half alpha = max(max(r.a, b.a) * _FaultAlpha, max(max(g.a, r.a), b.a));

    return half4(rgb, alpha);
}

half4 AddTexture(float2 uv, float2 DistortUV ,half4 color)
{
    // 扰动贴图的UV滚动
    float2 scrollUV = DistortUV + frac(float2(_DistortTexXSpeed, _DistortTexYSpeed) * _Time.y);

    // 动态扰动强度
    float4 Insten = float4(1, 1, 1, 1);
    float t = lerp(1.0 - frac(Insten.y * frac(_Time.x)), frac(Insten.x * frac(_Time.x)), sign(sin(Insten.w * frac(_Time.x) * 3.1415926)));

    half distortSample = SAMPLE_TEXTURE2D(_DistortTex, sampler_DistortTex, scrollUV).r;
    float dynamicDistort = _DistortAmount * (0.5 + 0.5 * t);
    float outDistortAmount = (distortSample - 0.5) * 0.2 * dynamicDistort;

    float2 destUV = uv + float2(outDistortAmount, outDistortAmount);

    // Glow 使用独立滚动的UV
    float2 glowFlowUV = uv*_AddGlowTex_ST.xy+_AddGlowTex_ST.zw + frac(float2(_DistortTexXSpeed, _DistortTexYSpeed) * _Time.y);

    half4 emission = SAMPLE_TEXTURE2D(_AddGlowTex, sampler_AddGlowTex, glowFlowUV);
    
    emission.rgb = emission * color.a * _AddColor.rgb;

    // 当没有扰动时，直接叠加发光；有扰动时用扰动贴图做调制
    float distortMask = (_DistortAmount > 0.0001) ? SAMPLE_TEXTURE2D(_DistortTex, sampler_DistortTex, destUV).r : 1.0;

    // Blend mode: 0(Add) / 1(Alpha)
    if (_AddTextureBlendMode < 0.5)
    {
        color.rgb += emission.rgb * distortMask;
        return color;
    }
    else
    {
        float useAlpha = round(saturate(_UseGlowAlphaWeight)); // force 0 or 1
        float channelWeight = lerp(emission.r, emission.a, useAlpha);
        float alphaBlend = channelWeight * _AddColor.a * distortMask;
        color.rgb = lerp(color.rgb, emission.rgb, alphaBlend);
    }
    color.a = max(color.a, 0.0);

    return color;
}
#endif
