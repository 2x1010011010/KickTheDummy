Shader "Hidden/FluidFlow/ApplyPadding"
{
	Properties
	{
		[HideInInspector] _MainTex("Texture", 2D) = "white" {}
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
			#pragma multi_compile_local __ PRECALCULATED_OFFSET

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _FF_SeamTex;

			inline float2 paddedUV(sampler2D seamTex, float2 texelSize, float2 uv)
			{
			#if defined(PRECALCULATED_OFFSET)
				return uv + DecodeOffset(tex2D(seamTex, uv).x) * texelSize;
			#else
				return OffsetToUV(ClosestOffsetInside(seamTex, texelSize, uv), uv, texelSize);
			#endif
			}

			float4 frag(blit_v2f i) : SV_Target
			{
				return tex2D(_MainTex, paddedUV(_FF_SeamTex, _MainTex_TexelSize.xy, i.uv));
			}
			ENDCG
		}
	}
}