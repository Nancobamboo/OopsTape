Shader "Unlit/ToxizZone"
{
    Properties
    {
        _MainTex ("Edge Tex", 2D) = "white" { }
        // _Right ("Right", Float) = 0.8
        // _Left ("Left", Float) = 0.8
        // _Top ("Top", Float) = 0.8
        // _Down ("Down", Float) = 0.8
        // _edgeIndentiy ("edgeIndentiy", Range( 0.1 , 10)) = 10
        // _speed ("speed", Range( 0 , 3)) = 0
        _opcityIntensity ("opcityIntensity", Range(0, 1)) = 0.5
        // _Range ("range", Float) = 1.0
        [HDR]_MainColor ("MainColor", Color) = (1, 1, 1, 0)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent+3" }

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
                float4 worldPos : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor;
                float4 _MainTex_ST;
                half _opcityIntensity;
            CBUFFER_END

            half _Left;
            half _Right;
            half _Down;
            half _Top;
            half _edgeIntensity;
            half _Range;

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);

            v2f vert(appdata v)
            {
                v2f o;
                
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.worldPos = mul(GetObjectToWorldMatrix(), float4(v.vertex.xyz, 1.0));
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half3 worldPos = i.worldPos.xyz;

                half3 worldPosRange = worldPos / max(0.1, (_Range * 10.0));
                half rect = (1.0 - saturate(min(min((_Right + (-worldPosRange.x)), (_Left + worldPosRange.x)), min((worldPosRange.z + _Down), ((-worldPosRange.z) + _Top)))));
                half rectMask = saturate(pow(rect, _edgeIntensity));
                half Alpha = rectMask * _opcityIntensity;
                
                return half4(_MainColor.rgb, Alpha);
            }
            ENDHLSL

        }
    }
}
