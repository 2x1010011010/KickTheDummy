Shader "Hidden/FluidFlow/EncodePadding"
{
	Properties
	{
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#include "../FluidFlow.cginc"
			#include "Padding.cginc"

			#pragma vertex blitvert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			float4 frag (blit_v2f i) : SV_Target
			{
				int2 result = ClosestOffsetInside (_MainTex, _MainTex_TexelSize.xy, i.uv);
				return float4(result.x > 0 ? EncodeOffset (result) : 0, 0, 0, 0);
			}
			ENDCG
		}
	}
}