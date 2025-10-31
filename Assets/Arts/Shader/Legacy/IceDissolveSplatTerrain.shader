Shader "Unlit/IceDissolveSplatTerrain"
{
    Properties
    {
        [Toggle]_GrowMode ("Grow Mode", float) = 0.0
        _MainTex ("Main Tex", 2D) = "white" { }
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _MainTex2 ("Main Tex 2", 2D) = "white" { }
        _Color2 ("Tint 2", Color) = (1, 1, 1, 1)
        _MainTex3 ("Main Tex 3", 2D) = "white" { }
        _Color3 ("Tint 3", Color) = (1, 1, 1, 1)
        _MainTex4 ("Main Tex 4", 2D) = "white" { }
        _Color4 ("Tint 4", Color) = (1, 1, 1, 1)
        _MainTex5 ("Main Tex 5", 2D) = "white" { }
        _Color5 ("Tint 5", Color) = (1, 1, 1, 1)
        _SplatMaskTex ("Splat Mask Tex", 2D) = "black" { }
        _Weight("Blend Weight" , Range(0.001,1)) = 0.2
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
                float4 objectPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _CenterPos;
                float4 _MainTex_ST;
                float4 _MainTex2_ST;
                float4 _MainTex3_ST;
                float4 _MainTex4_ST;
                float4 _MainTex5_ST;
                float4 _SplatMaskTex_ST;
                float4 _EdgeMaskTex_ST;
                half4 _Color;
                half4 _Color2;
                half4 _Color3;
                half4 _Color4;
                half4 _Color5;
                half4 _Color6;
                half4 _EdgeColor;
                half _EdgeScale;
                half _EdgeClip;
                half _EdgeWidth;
                half _EdgeIntensity;
                half _GrowMode;
                half _Weight;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_MainTex2);       SAMPLER(sampler_MainTex2);
            TEXTURE2D(_MainTex3);       SAMPLER(sampler_MainTex3);
            TEXTURE2D(_MainTex4);       SAMPLER(sampler_MainTex4);
            TEXTURE2D(_MainTex5);       SAMPLER(sampler_MainTex5);
            TEXTURE2D(_SplatMaskTex);    SAMPLER(sampler_SplatMaskTex);
            TEXTURE2D(_EdgeMaskTex);    SAMPLER(sampler_EdgeMaskTex);

            half4 Blend(half highgt1 ,half highgt2,half highgt3,half highgt4 , half3 control) 
            {
                half4 blend;
                half4 blendFactor = half4(saturate(1 - control.r - control.g - control.b),control.r,control.g,control.b);
                
                blend.r = highgt1 * blendFactor.r;
                blend.g = highgt2 * blendFactor.g;;
                blend.b = highgt3 * blendFactor.b;
                blend.a = highgt4 * blendFactor.a;

                half ma = max(blend.r, max(blend.g, max(blend.b, blend.a)));
                blend = max(blend - ma +_Weight , 0) * blendFactor;
                return blend * rcp(blend.r + blend.g + blend.b + blend.a);
            }
            
            v2f vert(appdata v)
            {
                v2f o = (v2f)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));
                o.objectPos = v.vertex;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw);
                half4 mainTex2 = SAMPLE_TEXTURE2D(_MainTex2, sampler_MainTex2, i.uv.xy * _MainTex2_ST.xy + _MainTex2_ST.zw) * _Color2;
                half4 mainTex3 = SAMPLE_TEXTURE2D(_MainTex3, sampler_MainTex3, i.uv.xy * _MainTex3_ST.xy + _MainTex3_ST.zw) * _Color3;
                half4 mainTex4 = SAMPLE_TEXTURE2D(_MainTex4, sampler_MainTex4, i.uv.xy * _MainTex4_ST.xy + _MainTex4_ST.zw) * _Color4;
                half4 mainTex5 = SAMPLE_TEXTURE2D(_MainTex5, sampler_MainTex5, i.uv.xy * _MainTex5_ST.xy + _MainTex5_ST.zw) * _Color5;
                half3 SplatMaskTex = SAMPLE_TEXTURE2D(_SplatMaskTex, sampler_SplatMaskTex, (i.objectPos.xy * float2(1,-1) * _ProjectionParams.x * _SplatMaskTex_ST.xy)-0.5 + _SplatMaskTex_ST.zw);
                
                half4 baseColor = mainTex * _Color;
                // half maskR = max(max(SplatMaskTex.r,SplatMaskTex.g),max(SplatMaskTex.b,SplatMaskTex.a));
                // half maskG = max(SplatMaskTex.g,max(SplatMaskTex.b,SplatMaskTex.a));
                // half maskB = max(SplatMaskTex.b,SplatMaskTex.a);

                half4 blend = Blend(mainTex2.a,mainTex3.a,mainTex4.a,mainTex5.a,SplatMaskTex);
                half4 baseColor2 = 1;
                baseColor2.rgb = blend.r * mainTex2 + blend.g * mainTex3 + blend.b * mainTex4 + blend.a * mainTex5;

                //half baseAlpha = mainTex.r;
                
                float2 maskUV = i.uv.xy * _EdgeMaskTex_ST.xy + _EdgeMaskTex_ST.zw;
                half edgeMask = SAMPLE_TEXTURE2D(_EdgeMaskTex, sampler_EdgeMaskTex, maskUV).r;

                half dis = distance(i.worldPos.xz, _CenterPos.xy) * _EdgeScale;

                half edgeDis = dis + edgeMask - _EdgeClip;
                edgeDis = saturate(edgeDis);
                
                if (_GrowMode)
                    clip(1 - edgeDis - 0.5);

                half edgeRange = saturate(pow(distance(edgeDis, 0.5), _EdgeWidth) * _EdgeIntensity);
                half4 edgeColor = (1 - edgeRange) * _EdgeColor;
                
                half4 baseColor3 = lerp(baseColor2, baseColor, edgeDis);
                // return edgeRange;

                half4 finalColor;
                finalColor.rgb = baseColor3.rgb + edgeColor.rgb;
                finalColor.a = baseColor3.a;

                // float alpha = baseAlpha * _IceOpcityIntensity;
                finalColor.rgb *= finalColor.a;
                
                return finalColor;
            }
            ENDHLSL

        }
    }
}
