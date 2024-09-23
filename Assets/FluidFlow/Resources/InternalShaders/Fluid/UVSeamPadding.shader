Shader "Hidden/FluidFlow/Fluid/UVSeamPadding"
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
			#include "Flow.cginc"
			#include "../Padding/Padding.cginc"

			#pragma vertex blitvert
			#pragma fragment frag

			sampler2D_float _MainTex;
			float4 _MainTex_TexelSize;

			inline float4 PadFlowTexture(sampler2D uvMap, float4 uvMapTexelSize, float2 startUV) {
				float4 pixel = tex2D(uvMap, startUV);
				float seam = IsSeam(pixel);
				float4 result = (seam > 0) ? pixel : 0;
				for (int dir = 1; dir < PADDING_DIRECTIONS; dir++) {
					pixel = tex2D(uvMap, OffsetToUV(int2(dir, 1), startUV, uvMapTexelSize.xy));
					float inside = IsSeam(pixel);
					result = (seam == 0 && inside > 0) ? pixel : result;
					seam += inside;
				}
				return result;
			}

			float4 frag (blit_v2f i) : SV_Target
			{
				return PadFlowTexture(_MainTex, _MainTex_TexelSize, i.uv);
			}
			ENDCG
		}
	}
}