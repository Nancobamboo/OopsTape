Shader "FX/VFX_Advanced" {
    Properties {
        [HDR]_Color ("Color", Color) = (1, 1, 1, 1)
        _FixValue ("x:CC,y:Mask as RGB,z:UVType,w:LoopAnim", vector) = (0, 0, 0, 0)
        _ColorAlphaBrightnessContrast ("_ColorAlphaBrightnessContrast", Vector) = (1, 1, 1, 1)
        
        [Header(Main)]
        _MainTex ("MainTex", 2D) = "white" { }
        _MainRot ("_MainRotX,_MainRotY,_ToggleRot,_MainRotAngle", vector) = (0, 1, 0, 1)
        _DetailTex ("DetailTex", 2D) = "white" { }

        [Header(Distort)]
        _DistortTexRG ("DistortTex(RG)", 2D) = "white" { }
        _DistortMaskRG ("DistortMask(RG)", 2D) = "white" { }

        _DistortXY ("xy:DistortXY", vector) = (0, 0, 0, 0)
        _DistortXY1 ("xy:DistortXY1", vector) = (0, 0, 0, 0)
        _DistortPanner ("DistortPanner", vector) = (0, 0, 0, 0)
        
        [Header(Mask)]
        _Mask ("Mask(R)", 2D) = "white" { }
        _Mask2 ("Mask2(R)", 2D) = "white" { }
        
        [Header(Dissolve)]
        _DissolveTex ("DissolveTex(R)", 2D) = "white" {}
        _DissolvePanner ("DistortPanner", vector) = (0, 0, 0, 0)
        [HDR] _EdgeColor ("Edge Color", Color) = (0, 0, 0, 0)
        _Spread ("Edge Spread", Range(0, 1)) = 1
        _Softness ("Edge Softness", Range(0, 0.5)) = 0.25
        _Width("Edge Width", Range(0, 2)) = 0.1
        
        _Panner1 ("xy:MainPanner,zw:MaskPanner", vector) = (0, 0, 0, 0)
        _Panner2 ("xy:DetailPanner,zw:Mask2Panner", vector) = (0, 0, 0, 0)
        _AnimationSettings ("x:_UsePanner,y:_ParticleEffect,z:AlphaAnim,w:AnimCtl", vector) = (1, 0, 0, 1)
        _ToggleAdvanced ("x:NULL,y:_InvertDistortMask,z:_MaskDistort,w:_Mask2Distort", vector) = (0, 0, 0, 0)
        _CustomDataUV("_CustomDataUV", float) = 0.0
        
        [Space(32)]
        [Header(Option)]
        [Enum(Off, 0, On, 1)] _MyZWrite ("ZWrite", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _MyZTest ("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _MyCullMode ("CullMode", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Src Blend", float) = 5.0
        [Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Dst Blend", float) = 1.0
        
        [HideInInspector] _AlphaLimit ("_AlphaLimit", Float) = 0

        [HideInInspector]_BlendMode ("_BlendMode", Float) = 0
        [HideInInspector]_AnimMode ("_AnimMode", Float) = 0
        [HideInInspector]_NULL_ColorFold ("_NULL_ColorFold", Float) = 1
        [HideInInspector]_NULL_MainFold ("_NULL_MainFold", Float) = 1
        [HideInInspector]_NULL_DistortFold ("_NULL_DistortFold", Float) = 1
        [HideInInspector]_NULL_MaskFold ("_NULL_MaskFold", Float) = 1
        [HideInInspector]_NULL_AnimFold ("_NULL_AnimFold", Float) = 1
        [HideInInspector]_NULL_OptionFold ("_NULL_OptionFold", Float) = 0
        [HideInInspector]_NULL_DissolveFold ("_NULL_DissolveFold", Float) = 0
        [HideInInspector]_NULL_ShaderID ("_NULL_ShaderID", Float) = 1
        
        [Space(10)]
        [Toggle(_SOFTPARTICLES_ON)] _SoftParticlesEnabled("Soft Particles", Float) = 0
        _SoftParticleFadeParams("Soft Particles Params (Start, End, Scale, Unused)", Vector) = (0,1,1,0)
        _InvFade ("Soft Particles Power", Range(0, 10)) = 1.0
        
        [Toggle(_USE_UNSCALED_TIME)] _UseUnscaledTime("Use Unscaled Time", Float) = 0
    }
    
    SubShader {
        Tags {"RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "Queue" = "Transparent" "RenderType" = "Transparent" "PreviewType" = "Plane" "IgnoreProjector" = "True" }
        Pass {
            Blend [_BlendSrc] [_BlendDst]
            Cull [_MyCullMode]
            ZWrite [_MyZWrite]
            ZTest[_MyZTest]

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "VFXShaderCommon.cginc"
            
            #pragma multi_compile __ USE2TEX
            #pragma multi_compile __ USEDISTORTMASK
            #pragma multi_compile __ DISTORT
            #pragma multi_compile __ DISSOLVE
            #pragma multi_compile __ _SOFTPARTICLES_ON
            #pragma multi_compile __ _USE_UNSCALED_TIME
            
            
         CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _DetailTex_ST;
            float4 _DistortTexRG_ST;
            float4 _DistortMaskRG_ST;
            float4 _DissolveTex_ST;
            float4 _Mask_ST;
            float4 _Mask2_ST;
            float4 _FixValue;
            float4 _UnscaledTime;
            float _UseUnscaledTime;
            float _InvFade;
            half4 _Color;
            half4 _EdgeColor;
            half4 _ColorAlphaBrightnessContrast;
            half4 _MainRot;
            half4 _DistortXY;
            half4 _DistortPanner;
            half4 _ToggleAdvanced;
            half4 _Panner1;
            half4 _Panner2;
            half4 _AnimationSettings;
            half2 _DissolvePanner;
            half2 _DistortXY1;
            half _AlphaLimit;
            half _CustomDataUV;
            half _Spread;
            half _Softness;
            half _Width;
            
         CBUFFER_END
            

            TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
            TEXTURE2D(_DetailTex);       SAMPLER(sampler_DetailTex);
            TEXTURE2D(_DistortTexRG);       SAMPLER(sampler_DistortTexRG);
            TEXTURE2D(_DistortMaskRG);       SAMPLER(sampler_DistortMaskRG);
            TEXTURE2D(_DissolveTex);        SAMPLER(sampler_DissolveTex);
            TEXTURE2D(_Mask);       SAMPLER(sampler_Mask);
            TEXTURE2D(_Mask2);       SAMPLER(sampler_Mask2);
            TEXTURE2D(_CameraDepthTexture);       SAMPLER(sampler_CameraDepthTexture);
          
            struct Attributes {
                float4 positionOS : POSITION;
                float4 uv : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                half4 color : COLOR;
            };
            
            struct Varyings {
                float4 positionCS : SV_POSITION;
                float4 distortUV : TEXCOORD0;
                float4 mainUV : TEXCOORD1;
                float4 maskUV : TEXCOORD2;
                #if DISSOLVE
                float3 dissolveUV : TEXCOORD3;
                #endif
                half4 color : COLOR;
                #if defined(_SOFTPARTICLES_ON)
                float4 projPos : TEXCOORD4;
                #endif
            };
            Varyings vert(Attributes input) {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                
                //animation
                //half _AnimVar = lerp(1.0, input.uv.z, _AnimationSettings.y) * lerp(1.0, input.color.a, _AnimationSettings.z) * lerp(_AnimationSettings.w, _AnimationSettings.w * frac(_Time.y), _FixValue.w) + lerp(0.0, _Time.x, _AnimationSettings.x);
                half _AnimVar = lerp(0.0, lerp(_Time.y, _UnscaledTime.y, _UseUnscaledTime), _AnimationSettings.x);
                //Distort related UV
                #ifdef DISTORT
                    output.distortUV = input.uv.xyxy + _AnimVar * _DistortPanner;
                    output.distortUV.xy = TRANSFORM_TEX(output.distortUV.xy, _DistortTexRG);
                    #if USEDISTORTMASK//DistortMask sampling
                        output.distortUV.zw = TRANSFORM_TEX(output.distortUV.zw, _DistortMaskRG);
                    #endif
                #endif

                output.mainUV.xy = lerp(input.uv.xy, input.uv2.zw, _FixValue.z);
                //MainTex rotate
                if (_MainRot.z > 0.5f) {
                    output.mainUV.xy -= 0.5f;
                    float2 tmp = float2(_MainRot.x, _MainRot.y);
                    output.mainUV.xy = float2(output.mainUV.x * tmp.y - output.mainUV.y * tmp.x, dot(output.mainUV.xy, tmp)) + 0.5f;
                }
                //UV output
                output.mainUV.xy = TRANSFORM_TEX((output.mainUV.xy + _AnimVar * _Panner1.xy + input.uv1.xy * _CustomDataUV.x), _MainTex);
                output.maskUV.xy = TRANSFORM_TEX((input.uv.xy + _AnimVar * _Panner1.zw), _Mask);

                #if USE2TEX
                    output.mainUV.zw = TRANSFORM_TEX((input.uv.xy + _AnimVar * _Panner2.xy), _DetailTex);
                    output.maskUV.zw = TRANSFORM_TEX((input.uv.xy + _AnimVar * _Panner2.zw), _Mask2);
                #endif

                #if DISSOLVE
                half _DissolveAmount = lerp(1.0, input.uv.z, _AnimationSettings.y) * lerp(1.0, input.color.a, _AnimationSettings.z) * _AnimationSettings.w;
                output.dissolveUV.xy  = input.uv.xy + _AnimVar * _DissolvePanner.xy;
                output.dissolveUV.xy = TRANSFORM_TEX(output.dissolveUV.xy ,_DissolveTex);
                output.dissolveUV.z = 1-_DissolveAmount;
                #endif
                
                #if defined(_SOFTPARTICLES_ON)
                output.projPos = ComputeScreenPos(output.positionCS);
                output.projPos.z = -TransformWorldToView(TransformObjectToWorld(input.positionOS.xyz)).z;
                #endif

                output.color.rgb = _ColorAlphaBrightnessContrast.x * _Color.rgb * input.color.rgb;
                output.color.a = _ColorAlphaBrightnessContrast.z * _Color.a * lerp(input.color.a, 1.0, _AnimationSettings.z);
                return output;
            }
            half4 frag(Varyings input) : COLOR {
                //Distort sampling

                float4 distortUV = 0;
                #ifdef DISTORT
                half4 distortTexRG_var = SAMPLE_TEXTURE2D(_DistortTexRG, sampler_DistortTexRG,input.distortUV.xy);
                distortUV = (distortTexRG_var.rgrg * 2 - 1) * _DistortXY;
                #if USEDISTORTMASK
                        half4 distortMaskRG_var = SAMPLE_TEXTURE2D(_DistortMaskRG, sampler_DistortMaskRG,input.distortUV.zw);
                        distortMaskRG_var.rg = lerp(distortMaskRG_var.rg, (1 - distortMaskRG_var.rg), _ToggleAdvanced.y);//Is it necessary to reverse DistortMask
                        distortUV.xy *= distortMaskRG_var.rg;
                #endif
                
                #endif
                
                
                half4 mainTex_var = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, input.mainUV.xy + distortUV.xy * _MainTex_ST.xy);
                

                mainTex_var.a = min(1, mainTex_var.a + _AlphaLimit);//GPP solves GrabScene script BUG

                
                half mask_var = SAMPLE_TEXTURE2D(_Mask,sampler_Mask, input.maskUV.xy + distortUV.xy * _Mask_ST.xy * _ToggleAdvanced.z).r;
                

                #if USE2TEX
                half4 detailTex_var = SAMPLE_TEXTURE2D(_DetailTex,sampler_DetailTex, input.mainUV.zw + distortUV.xy * _DetailTex_ST.xy);
                half mask2_var = SAMPLE_TEXTURE2D(_Mask2, sampler_Mask2,input.maskUV.zw + distortUV.xy * _Mask2_ST.xy * _ToggleAdvanced.w).r;
                    
                mainTex_var *= detailTex_var;
                mask_var *= mask2_var;
                #endif
                
                
                
                half4 finalColor;
                finalColor.rgb = (_FixValue.x + mainTex_var.rgb * (1.0h - _FixValue.x)) * input.color.rgb;
                finalColor.a = mainTex_var.a  * input.color.a;

                #if DISSOLVE
                half _DissolveTexR_var = SAMPLE_TEXTURE2D(_DissolveTex,sampler_DissolveTex, input.dissolveUV.xy + distortUV.zw).r;
				half dissolve = ( ( _DissolveTexR_var - (-_Spread + (input.dissolveUV.z - _DissolveTexR_var) * (1.0h + _Spread) / (1.0h - _DissolveTexR_var)) ) / _Spread ) * 2.0h;
                half edge_mask = saturate( ( 1.0h - ( distance( dissolve, _Softness ) / _Width ) ) );
                half edge_alpha = fastSmoothStep( _Softness, 1.0h, dissolve);
                finalColor.rgb = lerp( finalColor.rgb, ( finalColor.rgb * _EdgeColor.rgb ) , edge_mask);
                finalColor.a *= edge_alpha;
                #endif

                #ifdef _SOFTPARTICLES_ON
					float linearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, input.positionCS.xy/_ScreenParams.xy).r;
					float sceneZ = LinearEyeDepth(linearDepth,_ZBufferParams);
					float partZ = LinearEyeDepth(input.positionCS.z,_ZBufferParams);
					float fade = saturate (_InvFade * (sceneZ-partZ));
					finalColor.a *= fade;
                #endif
                
                finalColor = ColorCorrection(finalColor, _ColorAlphaBrightnessContrast.yyyw);
                finalColor.a = min(finalColor.a * mask_var, 1);
                
                return lerp(finalColor, half4(finalColor.rgb * finalColor.a, 1), _FixValue.y);
            }
            ENDHLSL

        }
    }
    CustomEditor "VFXCommonMaterialEditor"
}
