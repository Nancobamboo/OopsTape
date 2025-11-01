Shader "Unlit/IceDissolve2TexWater"
{
    Properties
    {
        [Toggle]_GrowMode ("Grow Mode", float) = 0.0
        _MainTex ("Main Tex", 2D) = "white" { }
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _MainTex2 ("Main Tex2", 2D) = "white" { }
        _MainTex2Intensity ("MainTex Intensity", Range(0,1)) = 0.5
        _MaskTex ("Mask Tex", 2D) = "white" { }
        _Color2 ("Tint 2", Color) = (1, 1, 1, 1)
        _SpriteRect ("Sprite Rect", vector) = (0, 0, 1, 1)
        _WaterEdgeColor("Water Edge Color", Color) = (0, 0, 0, 1)
        _EdgeMaskTex ("Edge Mask Tex", 2D) = "white" { }
        
        [HDR]_EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
        _EdgeClip ("Edge Clip", float) = 0.0
        _CenterPos ("Center Position", Vector) = (0.0, 0.0, 0.0, 0.0)
        _EdgeScale ("Edge Scale", float) = 1.0
        _EdgeWidth ("Edge Width", float) = 1.0
        _EdgeIntensity ("Edge Intensity", float) = 2.5
        
        [Header(IntersectionNoise)]
        _AnimationParams("WaveAnimation XY=Direction, Z=Speed", Vector) = (1,1,1,0)
		[NoScaleOffset][SingleLineTexture]_IntersectionNoise("Intersection noise", 2D) = "white" {}
        _IntersectionColor("Intersection Color", Color) = (1,1,1,1)
		_IntersectionLength("Intersection Distance", Range(0.01 , 5)) = 2
		_IntersectionClipping("Intersection Cutoff", Range(0.01, 1)) = 0.5
		_IntersectionFalloff("Intersection Falloff", Range(0.01 , 1)) = 0.5
		_IntersectionTiling("Intersection Noise Tiling", float) = 0.2
		_IntersectionSpeed("Intersection Speed multiplier", float) = 0.1
		_IntersectionRippleDist("Intersection Ripple distance", float) = 32
		_IntersectionRippleStrength("Intersection Ripple Strength", Range(0 , 1)) = 0.5
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

            half SampleIntersection(float2 uv, float gradient, float2 time,half _IntersectionRippleDist,half _IntersectionRippleStrength,half _IntersectionTiling,Texture2D _IntersectionNoise,sampler sampler_IntersectionNoise,half _IntersectionFalloff,half _IntersectionClipping)
            {
                half inter = 0;

                half sine = sin(- time.y * 100 - (gradient * _IntersectionRippleDist)) * _IntersectionRippleStrength;
                
                float2 nUV = float2(uv.x, uv.y) * _IntersectionTiling;
                half noise = SAMPLE_TEXTURE2D(_IntersectionNoise,sampler_IntersectionNoise, float2(nUV + time.xy)).r;

                
                float dist = saturate(gradient / _IntersectionFalloff);
                
                noise = saturate((noise + sine) * dist + dist);
                
                inter = step(_IntersectionClipping, noise);
                return inter;
                
                return saturate(inter);                
            }
            
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
                float4 _CenterPos;
                float4 _MainTex_ST;
                float4 _MainTex2_ST;
                float4 _MaskTex_ST;
                float4 _EdgeMaskTex_ST;
                float4 _AnimationParams;
                half4 _Color;
                half4 _WaterEdgeColor;
                half4 _Color2;
                half4 _EdgeColor;
                half4 _IntersectionColor;
                float _IntersectionLength;
                float _IntersectionFalloff;
                float _IntersectionTiling;
                float _IntersectionRippleDist;
                float _IntersectionRippleStrength;
                float _IntersectionClipping;
                float _IntersectionSpeed;
                half _MainTex2Intensity;
                half _EdgeScale;
                half _EdgeClip;
                half _EdgeWidth;
                half _EdgeIntensity;
                half _GrowMode;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_MainTex2);        SAMPLER(sampler_MainTex2);
            TEXTURE2D(_MaskTex);       SAMPLER(sampler_MaskTex);
            TEXTURE2D(_EdgeMaskTex);    SAMPLER(sampler_EdgeMaskTex);
            TEXTURE2D(_IntersectionNoise); SAMPLER(sampler_IntersectionNoise);
            
            v2f vert(appdata v)
            {
                v2f o = (v2f)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.uv2 = (v.uv - _SpriteRect.xy) / _SpriteRect.zw;
                o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw);
                half4 mainTe2 = SAMPLE_TEXTURE2D(_MainTex2, sampler_MainTex2, i.worldPos.xz * _MainTex2_ST.xy + _MainTex2_ST.zw * _Time.y);
                half4 maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv2.xy * _MaskTex_ST.xy + _MaskTex_ST.zw);
                
                half4 baseColor = mainTex * _Color;
                half4 baseColor2 = lerp(_WaterEdgeColor,lerp(_Color2,mainTe2,_MainTex2Intensity),maskTex.g);
                
                
                float2 time = (_Time.y * _AnimationParams.z) * _AnimationParams.xy;
                half interSecGradient = 1 - saturate(exp(maskTex.a) / _IntersectionLength);
                half intersection = SampleIntersection(frac(i.uv2.xy), interSecGradient, time * _IntersectionSpeed, _IntersectionRippleDist, _IntersectionRippleStrength, _IntersectionTiling, _IntersectionNoise, sampler_IntersectionNoise,_IntersectionFalloff, _IntersectionClipping);
                intersection *= _IntersectionColor.a;
                baseColor2.rgb += intersection * _IntersectionColor * maskTex.g;
                baseColor2.a = maskTex.r;
                half baseAlpha = mainTex.r;
                float2 maskUV = i.uv2.xy * _EdgeMaskTex_ST.xy + _EdgeMaskTex_ST.zw;
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
