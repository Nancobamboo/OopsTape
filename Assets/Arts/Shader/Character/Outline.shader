Shader "DayDream/Character/Outline"
{
    Properties
    {
        _Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
        [NoScaleOffset] _MainTex ("Main Texture", 2D) = "white" {}
        _OutlineAutoInOut("Outline Auto In Out", Int) = 0
        _OutlineParams("Outline Params", Vector) = (0,0,0,0)

        //[Toggle()] _OutlineOn("Outline On", Int) = 0
        _OutlineTex("Outline Noise", 2D) = "white"{}
        _OutlineTexIntensity("OutlineTex Intensity", Range(0, 1)) = 0
        [HDR]_OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth("Outline Width", Range(0, 1)) = 0
        _OutlineSoftness("Outline Softness", Range(0,1)) = 0
        
        _OutlineTex1("Outline Noise 2", 2D) = "white"{}
        _OutlineTexIntensity1("OutlineTex Intensity 2", Range(0, 1)) = 0
        [HDR]_OutlineColor1("Outline Color 2", Color) = (1,1,1,1)
        _OutlineWidth1("Outline Width 2", Range(0, 1)) = 0
        _OutlineSoftness1("Outline Softness 2", Range(0,1)) = 0
        
        _StencilRef("Stencil Reference", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"
        }
        
        Pass
        {
            Name "Outline"
            
            ZWrite Off
            
            Tags
            {
                "LightMode" = "OverlayOutline" 
            }
            
            Stencil
            {
                Ref[_StencilRef]
                Comp NotEqual
            }

            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            HLSLPROGRAM
            
            //#pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "BaseSpineOutlineInput.hlsl"
            #include "BaseSpineOutlinePass.hlsl"
            
            #pragma vertex vert
            #pragma fragment frag
            
            ENDHLSL
        }

    }
}