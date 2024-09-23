Shader "Hidden/FluidFlow/Extensions/QueryPaintConvolution"
{
	Properties
	{
		[HideInInspector] _MainTex("Tex", 2D) = "white" {}
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

			#pragma vertex blitvert
			#pragma fragment frag

			sampler2D_float _MainTex;
			float4 _MainTex_TexelSize;

			float4 frag(blit_v2f i) : SV_Target
			{
				float2 offset = _MainTex_TexelSize.xy * .5f;
				float a = tex2D(_MainTex, i.uv + float2(+offset.x, +offset.y)).x;
				float b = tex2D(_MainTex, i.uv + float2(+offset.x, -offset.y)).x;
				float c = tex2D(_MainTex, i.uv + float2(-offset.x, +offset.y)).x;
				float d = tex2D(_MainTex, i.uv + float2(-offset.x, -offset.y)).x;
				int validCount = (a>=0) + (b>=0) + (c>=0) + (d>=0);
				return (max(0, a) +max(0, b) + max(0, c) + max(0, d)) / validCount;
			}
			ENDCG
		}
	}
}