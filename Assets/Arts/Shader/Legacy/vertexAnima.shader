Shader "Unlit/vertexAnima"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        // _Right ("Right", Float) = 0.8
        // _Left ("Left", Float) = 0.8
        // _Top ("Top", Float) = 0.8
        // _Down ("Down", Float) = 0.8
        // _edgeIndentiy ("edgeIndentiy", Range( 0.1 , 10)) = 10
        // _Range ("range", Float) = 1.0
        [HDR]_MainCol ("Main Color", Color) = (1, 1, 1, 1)
        _ScaleRange ("Scale Range", Range(0.0, 1.0)) = 0.5
        _ScaleSpeed ("Scale Speed", Range(0.1, 2.0)) = 1.0
        _Alpha ("Alpha", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent+2" "DisableBatching" = "false" }
        LOD 100

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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainCol;
            float _ScaleRange, _ScaleSpeed;
            float _Alpha;
            CBUFFER_END

            float _Range;
            float _Left;
            float _Right;
            float _Down;
            float _Top;
            float _edgeIntensity;

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);

            v2f vert(appdata v)
            {
                v2f o;
                float time = fmod(_Time.y, 7200);
                // float fracTime = frac(_Time.y * 0.1);

                float scale = _ScaleRange * abs(sin(time * _ScaleSpeed));

                float3 worldNormal = TransformObjectToWorldNormal(v.normal);

                float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
                worldPos -= worldNormal * scale;

                o.pos = TransformWorldToHClip(worldPos);
                //o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = worldPos;
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half3 worldPos = i.worldPos;
                // float rect = ( 1.0 - saturate( min( min( ( _Right + (-worldPos.x) ) , ( _Left + worldPos.x ) ) , min( ( worldPos.z + _Down ) , ( (-worldPos.z) + _Top ) ) ) ) );
                // float rectRange = pow(rect, _edgeIntensity);
                // float lerpResult = lerp( 0.0 , 1.0 , saturate(rectRange));
                // float rectMask = saturate( lerpResult );
                half3 worldPosRange = worldPos / max(0.1, (_Range * 10.0));
                half rect = (1.0 - saturate(min(min((_Right + (-worldPosRange.x)), (_Left + worldPosRange.x)), min((worldPosRange.z + _Down), ((-worldPosRange.z) + _Top)))));
                // float rectRange = pow(rect, _edgeIntensity);
                half rectMask = saturate(pow(rect, _edgeIntensity));
                
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                col.rgb *= _MainCol.rgb;
                clip(col.a - 0.5);
                
                half Alpha = (saturate(rectMask * col.a) * _Alpha);
                
                return float4(col.rgb, Alpha);
            }
            ENDHLSL

        }
    }
}
