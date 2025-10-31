Shader "UI/GuideMask"
{
	Properties{
//	    [PerRendererData] _MainTex("Sprite Texture", 2D)="white"{}
//      _StencilComp("Stencil Comparison", Float) = 8
//		_Stencil("Stencil ID", Float) = 0
//		_StencilOp("Stencil Operation", Float) = 0
//		_StencilWriteMask("Stencil Write Mask", Float) = 255
//		_StencilReadMask("Stencil Read Mask", Float) = 255
//		_ColorMask("Color Mask", Float) = 15
		
		_Color("Tint",Color)=(1,1,1,1)
		_Origin("Rect",Vector) = (0,0,0,0)
		_TopOri("TopCircle",Vector) = (0,0,0,0)
		_Raid("RectRaid",Range(0,100)) = 0
		_MaskType("Type",Float) = 0
 
    }
	SubShader{
		Tags{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
			"RenderPipeline" = "UniversalPipeline"
        }
		
//		Stencil{
//			Ref[_Stencil]
//			Comp[_StencilComp]
//			Pass[_StencilOp]
//			ReadMask[_StencilReadMask]
//			WriteMask[_StencilWriteMask]
//		}
//		ColorMask[_ColorMask]
		
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		
 
		Pass{
			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
 
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			//sampler2D _MainTex;
			CBUFFER_START(UnityPerMaterial)
			half4 _Color;
			half4 _TextureSampleAdd;
			float4 _ClipRect;
			float4 _Origin;
			float4 _TopOri;
			float _Raid;
			float _MaskType;
			//0 Circl 1 Rect 2 CircleRect 
			CBUFFER_END
			
			//CircleRectVectory 
			half checkInCircleRectVectory (float4 worldPosition) {
				float4 rec1Pos=float4(_Origin.x-_Origin.z/2,_Origin.y-_Origin.w/2-_Raid,_Origin.x+_Origin.z/2,_Origin.y+_Origin.w/2+_Raid);
				float4 rec2Pos=float4(_Origin.x-_Origin.z/2+_Raid,_Origin.y-_Origin.w/2-2*_Raid,_Origin.x+_Origin.z/2-_Raid,_Origin.y+_Origin.w/2+2*_Raid);
				half2 step1=step(rec1Pos.xy, worldPosition.xy) * step(worldPosition.xy, rec1Pos.zw);
				half2 step2=step(rec2Pos.xy, worldPosition.xy) * step(worldPosition.xy, rec2Pos.zw);
				half rec1=step1.x*step1.y<1?0:1;
				half rec2=step2.x*step2.y<1?0:1;
				half dis1=distance(float2(_Origin.x+_Origin.z/2-_Raid,_Origin.y+_Origin.w/2+_Raid),worldPosition.xy)<_Raid?1:0;
				half dis2=distance(float2(_Origin.x-_Origin.z/2+_Raid,_Origin.y-_Origin.w/2-_Raid),worldPosition.xy)<_Raid?1:0;
				half dis3=distance(float2(_Origin.x+_Origin.z/2-_Raid,_Origin.y-_Origin.w/2-_Raid),worldPosition.xy)<_Raid?1:0;
				half dis4=distance(float2(_Origin.x-_Origin.z/2+_Raid,_Origin.y+_Origin.w/2+_Raid),worldPosition.xy)<_Raid?1:0;
				return (dis1+dis2+dis3+dis4+rec1+rec2)>0?0:1;
			}
 
			//CircleRectHorizontal
			half checkInCircleRectHorizontal (float4 worldPosition) {
 
				float4 rec1Pos=float4(_Origin.x-_Origin.z/2-_Raid,_Origin.y-_Origin.w/2,_Origin.x+_Origin.z/2+_Raid,_Origin.y+_Origin.w/2);
				float4 rec2Pos=float4(_Origin.x-_Origin.z/2-2*_Raid,_Origin.y-_Origin.w/2+_Raid,_Origin.x+_Origin.z/2+2*_Raid,_Origin.y+_Origin.w/2-_Raid);
				half2 step1=step(rec1Pos.xy, worldPosition.xy) * step(worldPosition.xy, rec1Pos.zw);
				half2 step2=step(rec2Pos.xy, worldPosition.xy) * step(worldPosition.xy, rec2Pos.zw);
				half rec1=step1.x*step1.y<1?0:1;
				half rec2=step2.x*step2.y<1?0:1;
				half dis1=distance(float2(_Origin.x-_Origin.z/2-_Raid,_Origin.y+_Origin.w/2-_Raid),worldPosition.xy)<_Raid?1:0;
				half dis2=distance(float2(_Origin.x-_Origin.z/2-_Raid,_Origin.y-_Origin.w/2+_Raid),worldPosition.xy)<_Raid?1:0;
				half dis3=distance(float2(_Origin.x+_Origin.z/2+_Raid,_Origin.y+_Origin.w/2-_Raid),worldPosition.xy)<_Raid?1:0;
				half dis4=distance(float2(_Origin.x+_Origin.z/2+_Raid,_Origin.y-_Origin.w/2+_Raid),worldPosition.xy)<_Raid?1:0;
				return (dis1+dis2+dis3+dis4+rec1+rec2)>0?0:1;
			}
			
			//Circl
			half checkInCircle (float4 worldPosition) {
				half dis1=distance(worldPosition.xy, _Origin.xy)< _Origin.z?1:0;
				half dis2=distance(worldPosition.xy, _TopOri.xy)< _TopOri.z?1:0;
				return (dis1+dis2)>0?0:1;
			}

			half getAlphaInCircle(float4 worldPosition,half maxA) {
				half dis = distance(worldPosition.xy, _Origin.xy);
				half disFactor = dis / _Origin.z;
				if(disFactor < 1)
				{
					return 0;
				}
				if(disFactor > 1.5)
				{
					return maxA;
				}
				return (disFactor - 1) / 0.5 * maxA;
			}
			
			//Rect
			half checkInRect (float4 worldPosition) {
				float4 temp=float4(_Origin.x-_Origin.z/2,_Origin.y-_Origin.w/2,_Origin.x+_Origin.z/2,_Origin.y+_Origin.w/2);
				float2 inside = step(temp.xy, worldPosition.xy) * step(worldPosition.xy, temp.zw);
				return inside.x*inside.y>0?0:1;
 
			}
			
			struct Attributes
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
            };
 
			struct Varyings{
				float4 vertex:SV_POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
            };
			
			Varyings vert(Attributes v){
				Varyings o;
				o.worldPosition = v.vertex;
				o.vertex = TransformObjectToHClip(v.vertex.xyz);
				o.texcoord = v.texcoord;
				o.color = v.color * _Color;
				return o;
			}
					
			half4 frag(Varyings i) : SV_Target{
				//half4 color = (tex2D(_MainTex, i.texcoord) + _TextureSampleAdd) * i.color;
				half4 color = i.color;
				if(_MaskType==0){
					color.a=checkInCircle(i.worldPosition)==0?0:color.a;
                }else if(_MaskType==1){
					color.a=checkInRect(i.worldPosition)==0?0:color.a;
                }else if(_MaskType==3){
					color.a=checkInCircleRectVectory(i.worldPosition)==0?0:color.a;
                }
				else if(_MaskType==2){
					color.a=checkInCircleRectHorizontal(i.worldPosition)==0?0:color.a;
                }
				else if(_MaskType==4){
					color.a=getAlphaInCircle(i.worldPosition,color.a);
				}
				return color;
			}
			ENDHLSL
        }
    }
}