Shader "Unlit/IceDissolve2Tex"
{
    Properties
    {
        [Toggle]_GrowMode ("Grow Mode", float) = 0.0
        _MainTex ("Main Tex", 2D) = "white" { }
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _MainTex2 ("Main Tex 2", 2D) = "white" { }
        _Color2 ("Tint 2", Color) = (1, 1, 1, 1)
        _SpriteRect ("Sprite Rect", vector) = (0, 0, 1, 1)
        _EdgeMaskTex ("Edge Mask Tex", 2D) = "white" { }
        [HDR]_EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
        _EdgeClip ("Edge Clip", float) = 0.0
        _CenterPos ("Center Position", Vector) = (0.0, 0.0, 0.0, 0.0)
        _EdgeScale ("Edge Scale", float) = 1.0
        _EdgeWidth ("Edge Width", float) = 1.0
        _EdgeIntensity ("Edge Intensity", float) = 2.5
        [Toggle(DISSOLVE_ON)]_DISSOLVE_ON("DISSOLVE ON",Float) = 1.0
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
            #pragma multi_compile _ DISSOLVE_ON
            // #pragma multi_compile _ PIXELSNAP_ON
            // #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR; 
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 pos : SV_POSITION;
                float4 worldPos : TEXCOORD2;
                half4 color : COLOR; 
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _SpriteRect;
                float4 _CenterPos;
                float4 _MainTex_ST;
                float4 _MainTex2_ST;
                float4 _EdgeMaskTex_ST;
                half4 _Color;
                half4 _Color2;
                half4 _EdgeColor;
                half _EdgeScale;
                half _EdgeClip;
                half _EdgeWidth;
                half _EdgeIntensity;
                half _GrowMode;
            CBUFFER_END
            
            
            TEXTURE2D(_MainTex2);       SAMPLER(sampler_MainTex2);
            
            #ifdef DISSOLVE_ON
            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_EdgeMaskTex);    SAMPLER(sampler_EdgeMaskTex);
            #endif
            
            v2f vert(appdata input)
            {
                v2f output = (v2f)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.pos = TransformObjectToHClip(input.vertex.xyz);
                output.uv = input.uv;
                output.uv2 = (input.uv - _SpriteRect.xy) / _SpriteRect.zw;
                output.worldPos = mul(unity_ObjectToWorld, float4(input.vertex.xyz, 1.0));
                output.color = input.color;
                return output;
            }

            half4 frag(v2f input) : SV_Target
            {
                half4 mainTex2 = SAMPLE_TEXTURE2D(_MainTex2, sampler_MainTex2, input.uv2.xy * _MainTex2_ST.xy + _MainTex2_ST.zw) * input.color;
                half4 finalColor = mainTex2;
                #ifdef DISSOLVE_ON
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw);
                half4 baseColor = mainTex * _Color;
                half4 baseColor2 = mainTex2 * _Color2;
                //half baseAlpha = mainTex.r;
                float2 maskUV = input.uv.xy * _EdgeMaskTex_ST.xy + _EdgeMaskTex_ST.zw;
                half edgeMask = SAMPLE_TEXTURE2D(_EdgeMaskTex, sampler_EdgeMaskTex, maskUV).r;

                half dis = distance(input.worldPos.xz, _CenterPos.xy) * _EdgeScale;

                half edgeDis = dis + edgeMask - _EdgeClip;
                edgeDis = saturate(edgeDis);
                
                if (_GrowMode)
                    clip(1 - edgeDis - 0.5);

                half edgeRange = saturate(pow(distance(edgeDis, 0.5), _EdgeWidth) * _EdgeIntensity);
                half4 edgeColor = (1 - edgeRange) * _EdgeColor;
                
                half4 baseColor3 = lerp(baseColor2, baseColor, edgeDis);
                // return edgeRange;
                finalColor.rgb = baseColor3.rgb + edgeColor.rgb;
                finalColor.a = baseColor3.a;
                #endif
                
                // float alpha = baseAlpha * _IceOpcityIntensity;
                finalColor.rgb *= finalColor.a;

                return finalColor;
            }
            ENDHLSL

        }
    }
}
