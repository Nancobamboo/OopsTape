Shader "Unlit/NoRender"
{
  SubShader
    {
        Tags { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent+100"  // 最高透明层级
            "ForceNoShadowCasting" = "True"  // 禁用阴影
        }

        Pass
        {
            // 关闭深度写入，避免影响后续渲染
            ZWrite Off
            // 关闭颜色写入
            ColorMask 0
            // 禁用混合计算
            Blend Zero One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 直接丢弃所有像素
                discard;
                return 0;
            }
            ENDCG
        }
    }
}