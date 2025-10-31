Shader "Unlit/IceDissolve2TexLine"
{
    Properties
    {
        [Toggle]_GrowMode ("Grow Mode", float) = 0.0
        
        _MainTex ("Main Tex", 2D) = "white" { }
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _MainTex2 ("Main Tex 2", 2D) = "white" { }
        _Color2 ("Tint 2", Color) = (1, 1, 1, 1)
        _LineTex ("Line Tex Mask", 2D) = "white" {}
        _EdgeTransPow ("Edge Translucency Pow", Range(0, 3)) = 1
        // _EdgeTransIntensity ("Edge Translucy Intensity", Range(0, 10)) = 10
        _EdgeMaskTex ("Edge Mask Tex", 2D) = "white" { }
        [HDR]_EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
        _EdgeClip ("Edge Clip", float) = 0.0
        _CenterPos ("Center Position", Vector) = (0.0, 0.0, 0.0, 0.0)
        _EdgeScale ("Edge Scale", float) = 1.0
        _EdgeWidth ("Edge Width", float) = 1.0
        _EdgeIntensity ("Edge Intensity", float) = 2.5
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
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
                float4 pos : SV_POSITION;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _Color2;
            float4 _MainTex_ST;
            float4 _MainTex2_ST;
            float4 _EdgeMaskTex_ST;
            float4 _EdgeColor;
            float4 _CenterPos;
            float _EdgeScale;
            float _EdgeClip;
            float _EdgeWidth;
            float _EdgeIntensity;
            float _EdgeTransPow;
            float _GrowMode;
            CBUFFER_END

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_MainTex2);       SAMPLER(sampler_MainTex2);
            TEXTURE2D(_EdgeMaskTex);    SAMPLER(sampler_EdgeMaskTex);
            TEXTURE2D(_LineTex);        SAMPLER(sampler_LineTex);

            v2f vert(appdata v)
            {
                v2f o = (v2f)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));

                
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 lineTex = SAMPLE_TEXTURE2D(_LineTex, sampler_LineTex, i.uv.xy);

                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw);
                float4 mainTex2 = SAMPLE_TEXTURE2D(_MainTex2, sampler_MainTex2, i.uv.xy * _MainTex2_ST.xy + _MainTex2_ST.zw);
                float4 baseColor = mainTex * _Color;

                float4 baseColor2 = mainTex2 * _Color2;
                baseColor2.rgb = lerp(baseColor2.rgb, baseColor.rgb, lineTex.r);
                baseColor2.rgb *= baseColor.a;

                float edgeTrans =  saturate(min((1 - abs(i.uv.x * 2 - 1)), (1 - abs(i.uv.y * 2 - 1))));
                edgeTrans = pow(edgeTrans, _EdgeTransPow);
                edgeTrans = saturate(edgeTrans * 10);
                // edgeTrans += edgeMask;
                // return float4(baseColor2.rgb, baseColor.a);

                float trans =  baseColor.a * edgeTrans;
                // baseColor.a = saturate(baseColor.a + lineTex.a);
                baseColor.a = lerp(trans, baseColor.a, lineTex.r);
                // return edgeTrans;

                float baseAlpha = mainTex.r;
                float2 maskUV = i.uv.xy * _EdgeMaskTex_ST.xy + _EdgeMaskTex_ST.zw;
                float edgeMask = SAMPLE_TEXTURE2D(_EdgeMaskTex, sampler_EdgeMaskTex, maskUV).r;

                float dis = distance(i.worldPos.xz, _CenterPos.xy) * _EdgeScale;

                float edgeDis = dis + edgeMask - _EdgeClip;
                edgeDis = saturate(edgeDis);

                if (_GrowMode)
                    clip(1 - edgeDis - 0.5);

                float edgeRange = saturate(pow(distance(edgeDis, 0.5), _EdgeWidth) * _EdgeIntensity);
                float4 edgeColor = (1 - edgeRange) * _EdgeColor;
                
                float4 baseColor3 = lerp(baseColor2, baseColor, edgeDis);
                // return edgeRange;

                float4 finalColor;
                finalColor.rgb = baseColor3.rgb + edgeColor.rgb;
                finalColor.a = baseColor.a;

                finalColor.rgb *= finalColor.a;

                return finalColor;
            }
            ENDHLSL

        }
    }
}
