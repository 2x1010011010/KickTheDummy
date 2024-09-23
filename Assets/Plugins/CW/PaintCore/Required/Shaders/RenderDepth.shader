Shader "Hidden/PaintCore/CwRenderDepth"
{
	Properties
	{
	}
	SubShader
	{
		Cull Off

		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex   Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

			float4x4 _P3D_DepthMatrix;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float4 position : TEXCOORD0;
			};

			void Vert (in appdata i, out v2f o)
			{
				float4 wpos = mul(unity_ObjectToWorld, i.vertex);

				o.vertex   = UnityObjectToClipPos(i.vertex);
				o.position = mul(_P3D_DepthMatrix, wpos);
			}

			fixed4 Frag (v2f i) : SV_Target
			{
				float d = saturate(1.0f - i.position.z / i.position.w);

				return float4(d, d, d, 1.0f);
			}
			ENDCG
		}
	}
}
