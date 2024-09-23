Shader "Hidden/FluidFlow/Fluid/StitchSeams"
{
	Properties
	{
		[HideInInspector] _MainTex("Fluid", 2D) = "black" {}
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

			#pragma vertex blitvert
			#pragma fragment frag

			#pragma multi_compile_local __ FF_FLOWTEX_COMPRESSED

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D_float _FF_FlowTex;

			float4 frag(blit_v2f i) : SV_Target
			{
				float4 flow = tex2D(_FF_FlowTex, i.uv);
				return tex2D(_MainTex, IsSeam(flow) ? UnpackSeamUV(flow, _MainTex_TexelSize) : i.uv);
			}
			ENDCG
		}
	}
}