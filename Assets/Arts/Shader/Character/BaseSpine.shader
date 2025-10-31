Shader "DayDream/Character/BaseSpine"
{
    Properties
    {
        _Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
        [NoScaleOffset] _MainTex ("Main Texture", 2D) = "white" {}
        [Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
        
        // Glow
        [Toggle(_GLOW_ON)] _GlowOn("Glow On", Int) = 0
        [HDR]_GlowColor("Glow Color", Color) = (1, 1, 1, 1)
        _GlowIntensity("GlowIntensity", Range(0, 10)) = 5
        _GlowMask("Glow Mask", 2D) = "white" {}
        _GlowThreshold("Glow Start Alpha Threshold", Range(0, 1)) = 0.01
        
        // Dissolve
        [Toggle(_DISSOLVE_ON)] _DissolveOn("Dissolve On", Int) = 0
        _DissolveTex ("Dissolve Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(-0.1, 1.1)) = 0
        [HDR]_DissolveGlowColor ("Dissolve Glow Color", Color) = (1, 1, 1, 1)
        _GlowWidth ("Glow Width", Range(0, 1)) = 0.05
        
        // Fault
        [Toggle(_FAULT_ON)] _FaultOn("Fault On", Int) = 0
        _FaultAmount ("Fault Amount", Range(0, 1)) = 0.5 // 左右偏移，控制条纹宽度
        _FaultAlpha ("Fault Alpha", Range(0, 1)) = 0.25 // 重影alpha值
        
        // Add texture
        [Toggle(_ADDTEXTURE_ON)] _AddTextureOn("Add Texture On", Int) = 0
        _AddGlowTex("GlowTexture", 2D) = "white" {}
        [HDR]_AddColor("Add Texture Color", Color) = (1,1,1,1)
        _DistortTex("DistortionTexture", 2D) = "white" {}
        _DistortAmount("DistortionAmount", Range(0, 2)) = 2
        _DistortTexXSpeed("DistortionTexXSpeed", Range(-5, 5)) = 0
        _DistortTexYSpeed("DistortionTexYSpeed", Range(-5, 5)) = -5
        [Enum(Add,0, Alpha,1)] _AddTextureBlendMode("Add/AlphaBlend", Float) = 0
        [Enum(R,0, A,1)] _UseGlowAlphaWeight("Use Glow Channel", Float) = 0
        
        // //Outline
        // [Toggle()] _OutlineOn("Outline On", Int) = 0
        // _OutlineTex("Outline Noise", 2D) = "white"{}
        // _OutlineTexIntensity("OutlineTex Intensity", Range(0, 1)) = 0
        // [HDR]_OutlineColor("Outline Color", Color) = (1,1,1,1)
        // _OutlineWidth("Outline Width", Range(0, 1)) = 0
        // _OutlineSoftness("Outline Softness", Range(0,1)) = 0
        
        // _OutlineTex1("Outline Noise 2", 2D) = "white"{}
        // _OutlineTexIntensity1("OutlineTex Intensity 2", Range(0, 1)) = 0
        // [HDR]_OutlineColor1("Outline Color 2", Color) = (1,1,1,1)
        // _OutlineWidth1("Outline Width 2", Range(0, 1)) = 0
        // _OutlineSoftness1("Outline Softness 2", Range(0,1)) = 0
        
//        //Outline
//        [Toggle(_OUTLINE_LAYER1_ON)] _OutlineLayer1On("OutlineLayer1 On", Int) = 0
//        [HDR]_OutlineColor1("Outline L1 Color",Color) = (1,1,1,1)
//        [Toggle(_OUTLINE_LAYER2_ON)] _OutlineLayer2On("OutlineLayer2 On", Int) = 0
//        _OutlineTex("Outline Noise", 2D) = "white"{}
//        [HDR]_OutlineColor2("Outline L2 Color",Color) = (1,1,1,1)
//        _OutlineSoftness("Outline Hardness zx:L1 zw:L2", Vector) = (0,0,0,0)
        
        _StencilRef("Stencil Reference", Float) = 1.0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"
        }
        
        Pass
        {
            Name "ForwardLit"

            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Cull Off
            //ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Lighting Off
            
            Stencil
            {
                Ref[_StencilRef]
                Comp[_StencilComp]
                Pass Replace
            }
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "BaseSpineInput.hlsl"
            #include "../Common/SpriteEffects.hlsl"
            #include "BaseSpineForwardPass.hlsl"
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
            #pragma shader_feature _ _GLOW_ON
            #pragma shader_feature _ _DISSOLVE_ON
            #pragma shader_feature _ _FAULT_ON
            #pragma shader_feature _ _ADDTEXTURE_ON
            #pragma vertex  vert
            #pragma fragment frag
            ENDHLSL
        }

        // Pass
        // {
        //     Name "Outline"
            
        //     ZWrite Off
            
        //     Tags
        //     {
        //         "LightMode" = "OverlayOutline" 
        //     }
            
        //     Stencil
        //     {
        //         Ref[_StencilRef]
        //         Comp NotEqual
        //     }

        //     Cull Off
        //     Blend SrcAlpha OneMinusSrcAlpha
            
        //     HLSLPROGRAM
            
        //     //#pragma multi_compile_fog
        //     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        //     #include "BaseSpineInput.hlsl"
        //     #include "BaseSpineOutllinePass.hlsl"
            
        //     #pragma vertex vert
        //     #pragma fragment frag
            
        //     ENDHLSL
        // }

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
            Cull[_Cull]

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
            #include "BaseSpineInput.hlsl"
            #include "../Common//ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    CustomEditor "DayDream.BaseSpineShaderGUI"
}