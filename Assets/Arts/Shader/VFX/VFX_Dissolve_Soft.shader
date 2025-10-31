Shader "FX/VFX_Dissolve_Soft" {
    Properties {
        [MainColor][HDR] _Color("Base Color", Color) = (1,1,1,1)
        _FixValue ("x:CC,y:Mask as RGB", vector) = (0, 0, 0, 0)
        _ColorAlphaBrightnessContrast ("_ColorAlphaBrightnessContrast", vector) = (1, 1, 1, 1)

        [Header(Main)]
        [MainTexture] _MainTex("Base Map", 2D) = "white" {}
        _DissolveTex ("DissolveTex(R)", 2D) = "white" {}
        _DirMap ("DirMap(R)",2D) = "white" {}
        _Mask ("Mask(R)", 2D) = "white" { }
        
        [HDR] _EdgeColor ("Edge Color", Color) = (0, 0, 0, 0)
        _Spread ("Edge Spread", Range(0, 1)) = 1
        _Softness ("Edge Softness", Range(0, 0.5)) = 0.25
        _Width("Edge Width", Range(0, 2)) = 0.1
        _AnimationSettings ("x:NULL,y:_ParticleEffect,z:AlphaAnim,w:AnimCtl", vector) = (1, 0, 0, 1)
        _CustomDataUV("_CustomDataUV", float) = 0.0
        
        [Space(32)]
        [Header(Option)]
        [Enum(Off, 0, On, 1)] _MyZWrite ("ZWrite", float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _MyZTest ("ZTest", float) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _MyCullMode ("CullMode", float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Src Blend", float) = 5.0
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Dst Blend", float) = 1.0

        [HideInInspector] _AnimMode ("_AnimMode", Float) = 0
        [HideInInspector] _BlendMode ("_BlendMode", float) = 0
        [HideInInspector]_NULL_ColorFold ("_NULL_ColorFold", float) = 1
        [HideInInspector]_NULL_MainFold ("_NULL_MainFold", float) = 1
        [HideInInspector]_NULL_DissolveFold ("_NULL_DissolveFold", float) = 1
        [HideInInspector]_NULL_OptionFold ("_NULL_OptionFold", float) = 0
        [HideInInspector]_NULL_ShaderID ("_NULL_ShaderID", float) = 3
        [HideInInspector]_NULL_AnimFold ("_NULL_AnimFold", Float) = 1
        
        [Space(32)]
        [Toggle] _NULL_DebugMode ("DebugMode(CloseToEnableGUI)", float) = 0
        
        [Space(10)]
        [Toggle(_SOFTPARTICLES_ON)] _SoftParticlesEnabled("Soft Particles", Float) = 0
        _SoftParticleFadeParams("Soft Particles Params (Start, End, Scale, Unused)", Vector) = (0,1,1,0)
        _InvFade ("Soft Particles Power", Range(0, 10)) = 1.0
    }
    
    SubShader {
        Tags { "IgnoreProjector" = "True" "Queue" = "Transparent" "RenderType" = "Transparent" "PreviewType" = "Plane" }
        Pass {
            Blend [_BlendSrc] [_BlendDst]
            Cull [_MyCullMode]
            ZWrite [_MyZWrite]
            ZTest [_MyZTest]

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ _SOFTPARTICLES_ON

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "VFXShaderCommon.cginc"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_DissolveTex); SAMPLER(sampler_DissolveTex);
            TEXTURE2D(_DirMap); SAMPLER(sampler_DirMap);
            TEXTURE2D(_Mask); SAMPLER(sampler_Mask);
            TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST, _DissolveTex_ST, _DirMap_ST,_Mask_ST;
            half4 _Color, _FixValue, _ColorAlphaBrightnessContrast, _EdgeColor,_AnimationSettings;
            half _Spread, _Softness, _Width, _CustomDataUV;;
            half _InvFade;
            CBUFFER_END

            struct Attributes {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float4 uv : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
            };
            struct Varyings {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float4 uv  : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
                float  uv3 : TEXCOORD2;
                #if defined(_SOFTPARTICLES_ON)
                float4 projPos : TEXCOORD4;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID

            };
            Varyings vert(Attributes input) {
                
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                output.uv = float4(TRANSFORM_TEX(input.uv.xy + input.uv1.xy * _CustomDataUV.xx, _MainTex),TRANSFORM_TEX(input.uv.xy, _DissolveTex));
                output.uv2 = float4(TRANSFORM_TEX(input.uv.xy, _DirMap),TRANSFORM_TEX(input.uv.xy,_Mask));
                half _AnimVar = lerp(1.0, input.uv.z, _AnimationSettings.y) * lerp(1.0, input.color.a, _AnimationSettings.z) * _AnimationSettings.w;
                output.uv3.x = 1-_AnimVar;
                output.color.rgb = _ColorAlphaBrightnessContrast.x * _Color.rgb * input.color.rgb;

                #if defined(_SOFTPARTICLES_ON)
                output.projPos = ComputeScreenPos(output.positionCS);
                output.projPos.z = -TransformWorldToView(TransformObjectToWorld(input.positionOS.xyz)).z;
                #endif
                
                output.color.a = _ColorAlphaBrightnessContrast.z * _Color.a * lerp(input.color.a, 1.0, _AnimationSettings.z);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target {

                UNITY_SETUP_INSTANCE_ID(input);
                half4 _BaseMap_var = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, input.uv.xy);
                half _DissolveTexR_var = SAMPLE_TEXTURE2D(_DissolveTex,sampler_DissolveTex, input.uv.zw).r;
                half _DirMapR_var = SAMPLE_TEXTURE2D(_DirMap,sampler_DirMap, input.uv2.xy).r*_DissolveTexR_var;
                half4 _Mask_var = SAMPLE_TEXTURE2D(_Mask,sampler_Mask, input.uv2.zw);
                
                // soft dissolve
				half dissolve = ( ( _DirMapR_var - (-_Spread + (input.uv3.x - _DirMapR_var) * (1.0h + _Spread) / (1.0h - _DirMapR_var)) ) / _Spread ) * 2.0h;
                //dissolve *= _DissolveTexR_var;
                half edge_mask = saturate( ( 1.0h - ( distance( dissolve, _Softness ) / _Width ) ) );
                half edge_alpha = fastSmoothStep( _Softness, 1.0h, dissolve);

                half4 finalColor;
                finalColor.rgb = (_FixValue.x + _BaseMap_var.rgb * (1.0h - _FixValue.x)) * input.color.rgb;
                finalColor.rgb = lerp( finalColor.rgb, ( finalColor.rgb * _EdgeColor.rgb ) , edge_mask);
                finalColor.a = _BaseMap_var.a * edge_alpha * input.color.a * _Mask_var.r;
                finalColor = ColorCorrection(finalColor, _ColorAlphaBrightnessContrast.yyyw);
                finalColor = lerp(finalColor, half4(finalColor.rgb * finalColor.a, 1.0h), _FixValue.y);
                finalColor.a = min(finalColor.a, 1.0h);
                #ifdef _SOFTPARTICLES_ON
					float linearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, input.positionCS.xy/_ScreenParams.xy).r;
					float sceneZ = LinearEyeDepth(linearDepth,_ZBufferParams);
					float partZ = LinearEyeDepth(input.positionCS.z,_ZBufferParams);
					float fade = saturate (_InvFade * (sceneZ-partZ));
					finalColor.a *= fade;
                #endif
               return finalColor;
            }
            ENDHLSL
        }
    }
    CustomEditor "VFXCommonMaterialEditor"
}
