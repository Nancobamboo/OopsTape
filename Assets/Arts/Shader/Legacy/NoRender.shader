Shader "Unlit/NoRender"
{
  SubShader
    {
        Tags { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent+100"  // ���͸���㼶
            "ForceNoShadowCasting" = "True"  // ������Ӱ
        }

        Pass
        {
            // �ر����д�룬����Ӱ�������Ⱦ
            ZWrite Off
            // �ر���ɫд��
            ColorMask 0
            // ���û�ϼ���
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
                // ֱ�Ӷ�����������
                discard;
                return 0;
            }
            ENDCG
        }
    }
}