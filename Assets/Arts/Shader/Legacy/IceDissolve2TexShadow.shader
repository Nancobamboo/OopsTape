Shader "Unlit/IceDissolve2TexShadow"
{
    Properties
    {
        [Toggle]_GrowMode ("Grow Mode", float) = 0.0
        _ShadowColor ("ShadowColor", Color) = (0.3,0.3,0.3,1)
        _MainTex ("Main Tex", 2D) = "white" { }
        // _Color ("Tint", Color) = (1, 1, 1, 1)
        _MainTex2 ("Main Tex 2", 2D) = "white" { }
        // _Color2 ("Tint 2", Color) = (1, 1, 1, 1)
        _SpriteRect ("Sprite Rect", vector) = (0, 0, 1, 1)
        _EdgeMaskTex ("Edge Mask Tex", 2D) = "white" { }
        // [HDR]_EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
        _EdgeClip ("Edge Clip", float) = 0.0
        _CenterPos ("Center Position", Vector) = (0.0, 0.0, 0.0, 0.0)
        _EdgeScale ("Edge Scale", float) = 1.0
        _EdgeWidth ("Edge Width", float) = 1.0
        _EdgeIntensity ("Edge Intensity", float) = 2.5

        _Vector ("Vector", Vector) = (-0.35, 0, 0, 0)
        //_ShadowAlpha ("ShadowAlpha", Float) = 0.3
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend DstColor Zero 

        Pass
        {
            Stencil
            {
                Ref 1
                Comp NotEqual
                Pass Replace
            }
                        
            // Blend SrcAlpha OneMinusSrcAlpha
            // ZWrite Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            // #pragma multi_compile _ PIXELSNAP_ON
            // #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 pos : SV_POSITION;
                float4 worldPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _SpriteRect;
            float4 _Vector;
            float4 _MainTex_ST;
            float4 _MainTex2_ST;
            float4 _EdgeMaskTex_ST;
            float4 _CenterPos;
            half4  _ShadowColor;
            half _EdgeScale;
            half _EdgeClip;
            half _EdgeWidth;
            half _EdgeIntensity;
            half _GrowMode;
            CBUFFER_END

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_MainTex2);       SAMPLER(sampler_MainTex2);
            TEXTURE2D(_EdgeMaskTex);    SAMPLER(sampler_EdgeMaskTex);


            v2f vert(appdata v)
            {
                v2f o = (v2f)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.uv2 = (v.uv - _SpriteRect.xy) / _SpriteRect.zw;
                o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));

                // #ifdef PIXELSNAP_ON
                //     o.pos = UnityPixelSnap(v.vertex);
                // #endif
                
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw);
                half4 mainTex2 = SAMPLE_TEXTURE2D(_MainTex2, sampler_MainTex2, i.uv2.xy * _MainTex2_ST.xy + _MainTex2_ST.zw);
                half4 baseColor = mainTex;
                half4 baseColor2 = mainTex2;
                //half baseAlpha = mainTex.r;
                float2 maskUV = i.uv.xy * _EdgeMaskTex_ST.xy + _EdgeMaskTex_ST.zw;
                half edgeMask = SAMPLE_TEXTURE2D(_EdgeMaskTex, sampler_EdgeMaskTex, maskUV).r;

                half dis = distance(i.worldPos.xz, _CenterPos.xy) * _EdgeScale;

                half edgeDis = dis + edgeMask - _EdgeClip;
                edgeDis = saturate(edgeDis);

                if (_GrowMode)
                    clip(1 - edgeDis - 0.5);

                // float edgeRange = saturate(pow(distance(edgeDis, 0.5), _EdgeWidth) * _EdgeIntensity);
                // float4 edgeColor = (1 - edgeRange);
                
                half4 baseColor3 = lerp(baseColor2, baseColor, edgeDis);
                // return edgeRange;

                half4 finalColor;
                // finalColor.rgb = baseColor3.rgb + edgeColor.rgb;

                finalColor.rgb = float3(0, 0, 0);
                finalColor.a = baseColor3.a;

                // float alpha = baseAlpha * _IceOpcityIntensity;
                // finalColor.rgb *= finalColor.a;

                if (finalColor.a > 0.1)
                {
                    finalColor.rgba = _ShadowColor;
                }
                
                clip(finalColor.a - 0.1);
                return finalColor;
            }
            ENDHLSL

        }
    }
}
