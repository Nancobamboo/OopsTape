Shader "UI/GrayScale" {
    Properties {
        [PerRendererData]
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _AlphaTex("Sprite Alpha Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _UseAlphaTex ("Use Separate Alpha Texture", Range(0, 1)) = 1
        _GrayIntensity ("Gray Intensity", Range(0, 1)) = 1.0
        _Brightness ("Brightness", Range(0, 2)) = 0.6
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 255
    }

    SubShader {
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        // 关闭面剔除，让正反两面都可见
        Cull Off
        // Դrgba*Դa + ����rgba*(1-ԴAֵ)
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        ZWrite Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
            };

            uniform sampler2D _MainTex;
            uniform sampler2D _AlphaTex;
            fixed4 _Color;
            fixed _UseAlphaTex;
            fixed _GrayIntensity;
            fixed _Brightness;

            v2f vert(appdata_t IN) {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
#ifdef UNITY_HALF_TEXEL_OFFSET
                OUT.vertex.xy -= (_ScreenParams.zw-1.0);
#endif
                OUT.color = IN.color * _Color;

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target {
                fixed4 original = tex2D(_MainTex, IN.texcoord);
                
                // 处理透明度：根据参数决定是否使用单独的 Alpha 纹理
                fixed alpha;
                if (_UseAlphaTex > 0.5) {
                    // Image 组件：使用单独的 Alpha 纹理（适用于 Sprite）
                    alpha = tex2D(_AlphaTex, IN.texcoord).r * original.a;
                } else {
                    // RawImage 组件：直接使用主纹理的 Alpha 通道（适用于 Texture2D）
                    alpha = original.a;
                }
                
                // 计算灰度值
                fixed grayscale = Luminance(original.rgb);
                
                // 根据灰度强度在原色和灰度之间插值
                fixed4 finalColor;
                finalColor.rgb = lerp(original.rgb, fixed3(grayscale, grayscale, grayscale), _GrayIntensity);
                
                // 应用亮度调整
                finalColor.rgb *= _Brightness;
                
                // 设置最终透明度
                finalColor.a = alpha;
                
                // 应用顶点颜色和材质颜色
                return finalColor * IN.color;
            }
            ENDCG
        }
    }
    FallBack Off
}
