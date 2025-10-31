Shader "FX/VFX_Simple" {
    Properties {
        [HDR]_Color ("Color", Color) = (1, 1, 1, 1)
        _FixValue ("x:CC,y:Mask as RGB", vector) = (0, 0, 0, 0)
        _ColorAlphaBrightnessContrast ("_ColorAlphaBrightnessContrast", Vector) = (1, 1, 1, 1)
        
        [Header(Main)]
        _MainTex ("MainTex", 2D) = "white" { }
        _MainRot ("_MainRotX,_MainRotY,_ToggleRot,_MainRotAngle", vector) = (0, 1, 0, 1)
        _Mask ("Mask(R)", 2D) = "white" { }
        _CustomDataUV("_CustomDataUV", float) = 0.0
        
        [Space(32)]
        [Header(Option)]
        [Enum(Off, 0, On, 1)] _MyZWrite ("ZWrite", FLoat) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _MyZTest ("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _MyCullMode ("CullMode", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Src Blend", float) = 5.0
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Dst Blend", float) = 1.0
        
        [HideInInspector]_BlendMode ("_BlendMode", Float) = 0
        [HideInInspector]_NULL_ColorFold ("_NULL_ColorFold", Float) = 1
        [HideInInspector]_NULL_MainFold ("_NULL_MainFold", Float) = 1
        [HideInInspector]_NULL_OptionFold ("_NULL_OptionFold", Float) = 0
        [HideInInspector]_NULL_ShaderID ("_NULL_ShaderID", Float) = 0
        
        [Space(10)]
        [Toggle(_SOFTPARTICLES_ON)] _SoftParticlesEnabled("Soft Particles", Float) = 0
        _SoftParticleFadeParams("Soft Particles Params (Start, End, Scale, Unused)", Vector) = (0,1,1,0)
        _InvFade ("Soft Particles Power", Range(0, 10)) = 1.0
    }

    SubShader {
        Tags {"RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "Queue" = "Transparent" "RenderType" = "Transparent" "PreviewType" = "Plane" }
        Pass {
            Blend [_BlendSrc] [_BlendDst]
            Cull [_MyCullMode]
            ZWrite [_MyZWrite]
            ZTest [_MyZTest]

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "VFXShaderCommon.cginc"
            #pragma multi_compile __ DISABLEMASK
            #pragma multi_compile __ _SOFTPARTICLES_ON
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Mask_ST;
            half4 _FixValue;
            half4 _Color;
            half4 _ColorAlphaBrightnessContrast;
            half4 _MainRot;
            half _CustomDataUV;
            float _InvFade;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
            TEXTURE2D(_Mask);       SAMPLER(sampler_Mask);
            TEXTURE2D(_CameraDepthTexture);       SAMPLER(sampler_CameraDepthTexture);
            
            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                half4 color : COLOR;
            };
            struct Varyings {
                float4 positionCS : SV_POSITION;
                float4 uv : TEXCOORD0;
                #if defined(_SOFTPARTICLES_ON)
                float4 projPos : TEXCOORD4;
                #endif
                half4 color : COLOR;
            };

            Varyings vert(Attributes input) {
                Varyings output = (Varyings)0;
                output.uv.xy = input.uv;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                if (_MainRot.z > 0.5f) {
                    output.uv.xy -= 0.5f;
                    float2 tmp = float2(_MainRot.x, _MainRot.y);
                    output.uv.xy = float2(output.uv.x * tmp.y - output.uv.y * tmp.x, dot(output.uv.xy, tmp)) + 0.5f;
                }
                output.uv.xy = TRANSFORM_TEX(output.uv.xy + input.uv1.xy * _CustomDataUV.xx, _MainTex);
                #ifndef DISABLEMASK
                    output.uv.zw = TRANSFORM_TEX(input.uv, _Mask);
                #endif

                #if defined(_SOFTPARTICLES_ON)
                output.projPos = ComputeScreenPos(output.positionCS);
                output.projPos.z = -TransformWorldToView(TransformObjectToWorld(input.positionOS.xyz)).z;
                #endif
                
                output.color = half4(_ColorAlphaBrightnessContrast.xxx, _ColorAlphaBrightnessContrast.z) * _Color * input.color;
                return output;
            }
            half4 frag(Varyings input) : COLOR {
                half4 _MainTex_var = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,input.uv.xy);
                #ifndef DISABLEMASK
                    _MainTex_var.a *= SAMPLE_TEXTURE2D(_Mask,sampler_Mask ,input.uv.zw).r;
                #endif

                half4 finalColor = half4(_FixValue.x + _MainTex_var.rgb * (1 - _FixValue.x), _MainTex_var.a);
                finalColor = ColorCorrection(finalColor, input.color, _ColorAlphaBrightnessContrast.yyyw);
                finalColor.a = min(finalColor.a, 1);
                
                #ifdef _SOFTPARTICLES_ON
					float linearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, input.positionCS.xy/_ScreenParams.xy).r;
					float sceneZ = LinearEyeDepth(linearDepth,_ZBufferParams);
					float partZ = LinearEyeDepth(input.positionCS.z,_ZBufferParams);
					float fade = saturate (_InvFade * (sceneZ-partZ));
					finalColor.a *= fade;
                #endif
                
                return lerp(finalColor, half4(finalColor.rgb * finalColor.a, 1), _FixValue.y);
            }
            ENDHLSL

        }
    }
    CustomEditor "VFXCommonMaterialEditor"
}
