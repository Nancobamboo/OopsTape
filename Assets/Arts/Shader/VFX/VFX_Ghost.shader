Shader "FX/VFX_Ghost" {
    Properties {
        [HDR]_Color ("Color", Color) = (1, 1, 1, 1)
        
        [Header(Main)]
        _MainTex ("MainTex", 2D) = "white" { }
    
        _MainTexIntensity("MainTex Intensity", Range(0, 1)) = 1
        [Toggle()]_MainTexFade("MainTex Fade", float) = 1
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
            
            #pragma multi_compile __ _SOFTPARTICLES_ON
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _Color;
            half _InvFade;
            half _MainTexIntensity;
            half _MainTexFade;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
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
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 offsetDirection = SafeNormalize(positionWS.xyz - _WorldSpaceCameraPos) * -0.1;
                positionWS = positionWS.xyz - offsetDirection;
                output.positionCS = TransformWorldToHClip(positionWS);
                
                #if defined(_SOFTPARTICLES_ON)
                output.projPos = ComputeScreenPos(output.positionCS);
                output.projPos.z = -TransformWorldToView(TransformObjectToWorld(input.positionOS.xyz)).z;
                #endif
                
                output.color = _Color * input.color;
                return output;
            }
            half4 frag(Varyings input) : COLOR {
                half4 _MainTex_var = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,input.uv.xy);
                clip(_MainTex_var.a - 0.9);

                half4 finalColor = half4(lerp(_Color.rgb, _MainTex_var.rgb, _MainTexIntensity), _Color.a);
                if(_MainTexFade > 0.5){
                    finalColor.rgb = lerp(_Color.rgb, finalColor.rgb, _Color.a);
                }
                
                finalColor.a = min(finalColor.a, 1);
                
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

}
