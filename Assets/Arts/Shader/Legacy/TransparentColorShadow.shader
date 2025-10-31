Shader "Unlit/TransparentWithColorShadow" {
    Properties {
        [hideininspector]_Color ("Color", Color) = (1,1,1,1)
        _ShadowColor ("ShadowColor", Color) = (0.3,0.3,0.3,1)
        _MainTex ("Texture", 2D) = "white" {}
        _ShadowAngle ("ShadowAngle",Range(0,360)) = 45
        _ShadowHeight ("Shadowheight",Range(0,1)) = 0.5
        _Offset("Offset",Vector) = (0,0,0,0)
        
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        
        Pass {
            
            Stencil
            {
                Ref 1
                Comp NotEqual
                Pass Replace
            }
            
            Blend DstColor Zero 
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float2 TransformCoordinates2D(float2 original, float theta,float2 offset)
            {
                float thetaR = radians(theta);
                
                float sinTheta = sin(thetaR);
                float cosTheta = cos(thetaR);

                float2x2 transform = float2x2(
                    1.0, -sinTheta,
                    0.0,  cosTheta
                );

                return mul(transform, original) + offset;
            }
            
            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _ShadowColor;
            half2 _Offset;
            half _ShadowAngle;
            half _ShadowHeight;
            CBUFFER_END

            Varyings vert (Attributes v) {
                Varyings o;
                float xScaleSign= sign(UNITY_MATRIX_M[2][0]);
                v.vertex.y *= _ShadowHeight;
                v.vertex.xy = TransformCoordinates2D(v.vertex.xy,_ShadowAngle * xScaleSign,_Offset);
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            half4 frag (Varyings i) : SV_Target {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                clip(col.a - 0.1);
                return half4(_ShadowColor);
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
