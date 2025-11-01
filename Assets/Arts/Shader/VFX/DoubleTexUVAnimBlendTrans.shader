// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "FX/DoubleTexUVAnimBlendTrans"
{
    Properties
    {
        [Toggle] _IsHit("Is Hit Color?", Float) = 0
        _MainTex1 ("Main Tex1", 2D) = "white" {}
        _MainTex2 ("Main Tex2", 2D) = "white" {}
        _ScrollX ("Main Tex1 UVSpeed X", Float) = 1.0
        _ScrollY ("Main Tex1 UVSpeed Y", Float) = 0.0
        _Scroll2X ("Main Tex2 UVSpeed X", Float) = 1.0
        _Scroll2Y ("Main Tex2 UVSpeed Y", Float) = 0.0
        _Color("Color", Color) = (1,1,1,1)
        _UVXX("Tex2 Addition Offset", vector)=(0.3,1,1,1)
        _MMultiplier ("Brightness", Float) = 2.0
        _HitEffectColor("Hit Effect Color", Color) = (1,0.4,0.3443,1)

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", float)=5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestBlend("DestBlend", float)=10
    }


    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline" "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
        }

        Blend [_SrcBlend] [_DestBlend]
        Cull Off
        Lighting Off
        ZWrite Off
        ColorMask RGB
        Fog
        {
            Mode Off
        }

        LOD 200



        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        TEXTURE2D(_MainTex1); SAMPLER(sampler_MainTex1);
        TEXTURE2D(_MainTex2); SAMPLER(sampler_MainTex2);
CBUFFER_START(UnityPerMaterial)
        float4 _MainTex1_ST;
        float4 _MainTex2_ST;
        float _ScrollX;
        float _ScrollY;
        float _Scroll2X;
        float _Scroll2Y;
        float _MMultiplier;
        float4 _UVXX;
        float4 _Color;
        half4 _HitEffectColor;
  CBUFFER_END
        struct appdata
        {
            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
            half4 color : COLOR;
        };

        struct v2f
        {
            float4 pos : SV_POSITION;
            float4 uv : TEXCOORD0;
            half4 color : TEXCOORD1;
        };


        v2f vert(appdata v)
        {
            v2f o;
            o.pos = TransformObjectToHClip(v.vertex.xyz);
            o.uv.xy = TRANSFORM_TEX(v.texcoord.xy, _MainTex1) + frac(float2(_ScrollX, _ScrollY) * _Time.x);
            o.uv.zw = TRANSFORM_TEX(v.texcoord.xy, _MainTex2) + frac(float2(_Scroll2X, _Scroll2Y) * _Time.x);

            o.color = _MMultiplier * _Color * v.color;
            return o;
        }
        ENDHLSL


        Pass
        {
            HLSLPROGRAM
            #pragma shader_feature _ISHIT_ON
            #pragma vertex vert
            #pragma fragment frag
            half4 frag(v2f i) : COLOR
            {
                half4 o;
                half4 tex = SAMPLE_TEXTURE2D(_MainTex1,sampler_MainTex1, i.uv.xy);
                half2 uv = tex.r * _UVXX.x;

                half4 tex2 = SAMPLE_TEXTURE2D(_MainTex2,sampler_MainTex2, i.uv.zw + uv);

                o = tex * tex2 * i.color;
                #if _ISHIT_ON
			o.rgb *= _HitEffectColor.rgb;
                #endif
                return o;
            }
            ENDHLSL
        }
    }

}