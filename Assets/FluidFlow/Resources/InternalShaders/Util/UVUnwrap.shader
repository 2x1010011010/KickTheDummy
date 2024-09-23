Shader "Hidden/FluidFlow/UVUnwrap"
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
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = ProjectUVToClipSpace(AtlasTransformUV(v.uv));
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(1, 1, 1, 1);
			}
			ENDCG
		}
	}
}