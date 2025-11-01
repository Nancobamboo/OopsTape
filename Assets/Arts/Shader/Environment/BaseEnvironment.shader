    Shader "DayDream/Environment/BaseEnvironment"
{
    Properties
    {
        [Enum(Off, 0, Front, 1, Back, 2)] _CullMode("Cull Mode", Float) = 2
        [Header(Main)]
        _MainTex ("Main Texture", 2D) = "black" {}
        _BaseColor ("Tint Color",Color) = (1,1,1,1)
        
        [Header(Emission)]
        [Toggle(EMISSION_ON)]_EmissOn("Emission On", float) = 0
        _EmissionTex("Emission", 2D) = "black" { }
        _EmissionInt("Emission Intensity",Range(0,10)) = 1
        
        [Header(Cut Out)]
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.1
        _StencilRef("Stencil Reference", Float) = 0.0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 4.0
        
        [Header(NoiseBlend)]
        [Toggle(NOISE_ON)]_NoiseOn("Noise On", float) = 0
        _NoiseColor("Noise Color",Color) = (1,1,1,0)
        _NoiseMask("Noise Mask", 2D) = "white" { }
        _NoiseTex("Noise Tex", 2D) = "white" { }
        _NoiseTex2("Noise Tex2", 2D) = "white" { }
        [Enum(multiply, 0, max, 1, min, 2)] _NoiseBlendMode("Noise Blend Mode", Float) = 0
        
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        
        Pass
        {
            Name "ForwardLit"
            Cull [_CullMode]

            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Stencil
            {
                Ref[_StencilRef]
                Comp[_StencilComp]
                Pass Replace
            }
            HLSLPROGRAM
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma shader_feature _ _ALPHATEST_ON
            #pragma shader_feature _ EMISSION_ON
            #pragma shader_feature _ NOISE_ON
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "BaseEnvironmentInput.hlsl"
            #include "BaseEnvironmentForwardPass.hlsl"

            #pragma vertex vert
            #pragma fragment frag
            
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "BaseEnvironmentInput.hlsl"
            #include "../Common/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}