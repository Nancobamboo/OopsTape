Shader "FX/VFX_Dissolve" {
    Properties {
        [HDR]_Color ("Color", Color) = (1, 1, 1, 1)
        _FixValue ("x:CC,y:Mask as RGB", vector) = (0, 0, 0, 0)
        _ColorAlphaBrightnessContrast ("_ColorAlphaBrightnessContrast", Vector) = (1, 1, 1, 1)

        [Header(Main)]
        _MainTex ("MainTex", 2D) = "white" { }
        _MainRot ("_MainRotX,_MainRotY,_ToggleRot,_MainRotAngle", vector) = (0, 1, 0, 1)
        _DissolveTex ("DissolveTex(R)", 2D) = "white" { }
        _DissolveTex2 ("DissolveTex2", 2D) = "white" { }
        _SeamTex ("SeamTex", 2D) = "white" { }
        _Mask ("Mask(R)", 2D) = "white" { }
        
        _ToggleDissolve ("x:InvertDissolveTex,y:SeamBlendMode,z:SeamClampLeft,w:SeamClampRight", vector) = (0, 0, 1, 0)
        _Value ("x:_LerpDissolve,y:_SeamSize,z:SeamBrightness,w:SeamContrast", vector) = (0, 1, 1, 1)
        _AnimationSettings ("x:NULL,y:_ParticleEffect,z:AlphaAnim,w:AnimCtl", vector) = (1, 0, 0, 0.2)
        _CustomDataUV("_CustomDataUV", float) = 0.0

        [Space(32)]
        [Header(Option)]
        [Enum(Off, 0, On, 1)] _MyZWrite ("ZWrite", FLoat) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _MyZTest ("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _MyCullMode ("CullMode", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Src Blend", float) = 5.0
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Dst Blend", float) = 10.0

        [HideInInspector] _BlendMode ("_BlendMode", Float) = 1
        [HideInInspector] _AnimMode ("_AnimMode", Float) = 0
        [HideInInspector]_NULL_ColorFold ("_NULL_ColorFold", Float) = 1
        [HideInInspector]_NULL_MainFold ("_NULL_MainFold", Float) = 1
        [HideInInspector]_NULL_SeamFold ("_NULL_SeamFold", Float) = 1
        [HideInInspector]_NULL_AnimFold ("_NULL_AnimFold", Float) = 1
        [HideInInspector]_NULL_OptionFold ("_NULL_OptionFold", Float) = 0
        [HideInInspector]_NULL_ShaderID ("_NULL_ShaderID", Float) = 2
        
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
            #pragma multi_compile __ USEDISSOLVETEX2
            #pragma multi_compile __ USESEAMTEX

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_SeamTex); SAMPLER(sampler_SeamTex);
            TEXTURE2D(_DissolveTex); SAMPLER(sampler_DissolveTex);
            TEXTURE2D(_DissolveTex2); SAMPLER(sampler_DissolveTex2);
            TEXTURE2D(_Mask); SAMPLER(sampler_Mask);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _SeamTex_ST;
            float4 _DissolveTex_ST;
            float4 _DissolveTex2_ST;
            float4 _Mask_ST;
            half4 _Color;
            half4 _FixValue;
            half4 _ColorAlphaBrightnessContrast;
            half4 _MainRot;
            half4 _ToggleDissolve;
            half4 _Value;
            half4 _AnimationSettings;
            half _CustomDataUV;
            CBUFFER_END

            struct Attributes {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float4 uv : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
            };
            struct Varyings {
                float4 positionCS : SV_POSITION;
                float4 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
                float2 uv3 : TEXCOORD2;
                float4 color : COLOR;
                #if defined(_SOFTPARTICLES_ON)
                float4 projPos : TEXCOORD4;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            Varyings vert(Attributes input) {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                half _AnimVar = lerp(1.0, input.uv.z, _AnimationSettings.y) * lerp(1.0, input.color.a, _AnimationSettings.z) * _AnimationSettings.w;
                #if USESEAMTEX
                    _AnimVar = _AnimVar / abs(_SeamTex_ST.x * _Value.y) + _AnimVar - 1;
                #else
                    _AnimVar = _AnimVar / _Value.y + _AnimVar - 1;
                #endif

                output.uv = float4(TRANSFORM_TEX(input.uv.xy + input.uv1.xy * _CustomDataUV.xx, _MainTex), input.uv.z, _AnimVar);

                if (_MainRot.z > 0.5f) {
                    output.uv.xy -= 0.5f;
                    float2 tmp = float2(_MainRot.x, _MainRot.y);
                    output.uv.xy = float2(output.uv.x * tmp.y - output.uv.y * tmp.x, dot(output.uv.xy, tmp)) + 0.5f;
                }

                output.uv2 = float4(TRANSFORM_TEX(input.uv.xy, _DissolveTex), TRANSFORM_TEX(input.uv.xy, _Mask));
                #if USEDISSOLVETEX2
                    output.uv3.xy = TRANSFORM_TEX(input.uv.xy, _DissolveTex2);
                #endif

                #if defined(_SOFTPARTICLES_ON)
                output.projPos = ComputeScreenPos(output.positionCS);
                output.projPos.z = -TransformWorldToView(TransformObjectToWorld(input.positionOS.xyz)).z;
                #endif

                output.color.rgb = _ColorAlphaBrightnessContrast.x * _Color.rgb * input.color.rgb;
                output.color.a = _ColorAlphaBrightnessContrast.z * _Color.a * lerp(input.color.a, 1.0, _AnimationSettings.z);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            half4 frag(Varyings input) : COLOR {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 _MainTex_var = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, input.uv.xy);
                half _DissolveTexR_var = SAMPLE_TEXTURE2D(_DissolveTex,sampler_DissolveTex, input.uv2.xy).r;
                half Dissolve_var = lerp(_DissolveTexR_var, 1 - _DissolveTexR_var, _ToggleDissolve.x);
                half4 _Mask_var = SAMPLE_TEXTURE2D(_Mask,sampler_Mask, input.uv2.zw);
                #if USEDISSOLVETEX2
                    half4 _DissolveTex2_var = SAMPLE_TEXTURE2D(_DissolveTex2,sampler_DissolveTex2, input.uv3.xy);
                    Dissolve_var = lerp(Dissolve_var, dot(1, _DissolveTex2_var.rgb) * 0.33333, _Value.x);
                #endif

                float _SeamU = Dissolve_var + input.uv.w;
                _SeamU *= _Value.y;
                _MainTex_var.a *= lerp(1.0, _SeamU > 0, _ToggleDissolve.z) * lerp(1.0, (1.0 - _SeamU) > 0, _ToggleDissolve.w);
                #if USESEAMTEX
                    half4 _SeamTex_var = SAMPLE_TEXTURE2D(_SeamTex,sampler_SeamTex,TRANSFORM_TEX(float2(_SeamU, 0.5),_SeamTex));
                    _SeamTex_var.rgb = ColorCorrection(_SeamTex_var.rgb, _Value.z, _Value.w);
                    _MainTex_var.a *= _SeamTex_var.a;
                    _MainTex_var.rgb = lerp(_MainTex_var.rgb * _SeamTex_var.rgb, _MainTex_var.rgb + _SeamTex_var.rgb, _ToggleDissolve.y);
                #endif

                #ifdef _SOFTPARTICLES_ON
					float linearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, input.positionCS.xy/_ScreenParams.xy).r;
					float sceneZ = LinearEyeDepth(linearDepth,_ZBufferParams);
					float partZ = LinearEyeDepth(input.positionCS.z,_ZBufferParams);
					float fade = saturate (_InvFade * (sceneZ-partZ));
					finalColor.a *= fade;
                #endif
                
                half4 finalColor;
                finalColor.rgb = (_FixValue.x + _MainTex_var.rgb * (1.0 - _FixValue.x)) * input.color.rgb;
                finalColor.a = _MainTex_var.a * _Mask_var.r * input.color.a;
                finalColor = ColorCorrection(finalColor, _ColorAlphaBrightnessContrast.yyyw);
                finalColor.a = min(finalColor.a, 1);
                return lerp(finalColor, half4(finalColor.rgb * finalColor.a, 1), _FixValue.y);
            }
            ENDHLSL

        }
    }
    CustomEditor "VFXCommonMaterialEditor"
}
