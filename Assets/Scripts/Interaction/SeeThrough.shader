Shader "Custom/HoverSeeThrough"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_BaseAlpha ("Base Alpha", Range (0, 1)) = .4
		_SinSpeed ("Sin Speed", Float) = 100
		_SinPower ("Sin Power", Range (0, 1)) = .2
		_RimPower  ("Rim Power", Range (0, 1)) = .2
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry+1" "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
			Cull Off
			ZWrite Off
			ZTest Greater
			Stencil
			{
				Ref 2
				Comp notequal
				Pass replace
				Fail keep
			}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				
				#include "UnityCG.cginc"
				
				struct VertIn
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
				};
				
				struct VertOut
				{
					float4 vertex : SV_POSITION;
					float3 normal : NORMAL;
					float3 viewDirection : POSITION2;
				};
				
				float4 _Color;
				float _BaseAlpha;
				float _SinSpeed;
				float _SinPower;
				float _RimPower;
				
				VertOut vert(VertIn i)
				{
					VertOut o;
					o.vertex = UnityObjectToClipPos(i.vertex);
					o.normal = i.normal;
					o.viewDirection = ObjSpaceViewDir(i.vertex);
					
					return o;
				}
				
				float4 frag(VertOut i) : SV_Target
				{
					float4 color = _Color.rgba;

					float sinEffect = (sin(_Time * _SinSpeed) * 0.5 + 0.5) * _SinPower;
					float rim = saturate(dot(normalize(-i.viewDirection), normalize(i.normal)));
					float rimEffect = (1 - rim) * _RimPower;
					color.a = _BaseAlpha + sinEffect + rimEffect;
				
					return color.rgba;
				}

			ENDCG
		}
	}
}
