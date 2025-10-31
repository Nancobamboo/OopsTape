Shader "Unlit/IceDissolve"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" { }
        _EdgeMaskTex ("Edge Mask Tex", 2D) = "white" { }
        [HDR]_EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
        _EdgeClip ("Edge Clip", float) = 0.0
        _CenterPos ("Center Position", Vector) = (0.5, 0.5, 0.0, 0.0)
        _EdgeScale ("Edge Scale", float) = 0.0
        _EdgeWidth ("Edge Width", float) = 0.2
        _EdgeRange ("Edge Range", Range(0.1, 5.0)) = 0.2
        _IceOpcityIntensity ("Ice opcityIntensity", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags {"RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent+5" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                // float4 worldPos : TEXCOORD1;

            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _EdgeMaskTex_ST;
            float4 _EdgeColor;
            float4 _CenterPos;
            float _EdgeScale;
            float _EdgeClip;
            float _EdgeWidth;
            float _EdgeRange;
            float _IceOpcityIntensity;
            CBUFFER_END

            TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
            TEXTURE2D(_EdgeMaskTex);    SAMPLER(sampler_EdgeMaskTex);

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                // o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));
                
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw);
                float3 baseColor = mainTex.rgb;
                float baseAlpha = mainTex.r;
                float2 maskUV = i.uv.xy * _EdgeMaskTex_ST.xy + _EdgeMaskTex_ST.zw;
                float edgeMask = SAMPLE_TEXTURE2D(_EdgeMaskTex, sampler_EdgeMaskTex, maskUV).r;

                float dis = distance(i.uv.xy, _CenterPos.xy) * _EdgeScale;
                float edgeDis = dis + edgeMask - _EdgeClip;
                float clipMask = 1 - edgeDis;
                clip(clipMask - 0.5);
                // clip(clipValue - 0.5);

                float edgeRange = saturate(saturate(edgeDis - _EdgeWidth) / _EdgeRange);
                float3 edgeColor = edgeRange * _EdgeColor.rgb * _EdgeColor.a;

                float3 finalColor = baseColor + edgeColor;
                float alpha = baseAlpha * _IceOpcityIntensity;

                // UnityApplyDitherCrossFade()

                return float4(finalColor, alpha);
            }
            ENDHLSL

        }
    }
}
