Shader "Hidden/FluidFlow/Extensions/QueryPaint"
{
	SubShader
	{
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local __ FF_UV1

			#include "../FluidFlow.cginc"

			struct appdata
			{
				#ifdef FF_UV1
					float2 uv : TEXCOORD1;
				#else
					float2 uv : TEXCOORD0;
				#endif
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.uv = AtlasTransformUV(v.uv);
				o.vertex = ProjectUVToClipSpace(o.uv);
				return o;
			}

			sampler2D _FF_MainTex;
			float4 _FF_Min;
			float4 _FF_Max;

			float4 frag(v2f i) : SV_Target
			{
				float4 col = tex2D(_FF_MainTex, i.uv);
				bool4 match = col >= _FF_Min && col <= _FF_Max;
				return match.x && match.y && match.z && match.w ? 1 : 0;
			}
			ENDCG
		}
	}
}