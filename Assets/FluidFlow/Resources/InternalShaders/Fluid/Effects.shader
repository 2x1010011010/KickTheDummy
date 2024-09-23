Shader "Hidden/FluidFlow/Fluid/Effects"
{
	Properties
	{
		[HideInInspector] _MainTex("Fluid", 2D) = "white" {}
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

			// Effects
			#pragma multi_compile_local __ FF_EFFECT_STITCH
			#pragma multi_compile_local __ FF_EFFECT_BLUR
			#pragma multi_compile_local __ FF_EFFECT_FADE

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

#if defined(FF_EFFECT_STITCH) || defined(FF_EFFECT_BLUR)
			sampler2D_float _FF_FlowTex;
#endif

#if defined(FF_EFFECT_BLUR)
			float _FF_BlurMinFluid;
			float _FF_BlurFactor;
#endif

#if defined(FF_EFFECT_FADE)
			float _FF_FadeAmount;
			float _FF_FadeMode;
#endif

			float4 frag(blit_v2f i) : SV_Target
			{
#if defined(FF_EFFECT_STITCH) || defined(FF_EFFECT_BLUR)
				float4 flow = tex2D(_FF_FlowTex, i.uv);
				if (IsZero(flow))
					discard;
				bool seam = IsSeam(flow);
				float2 uv = seam ? UnpackSeamUV(flow, _MainTex_TexelSize) : i.uv;
#else
				float2 uv = i.uv;
#endif

				float4 fluid = tex2D(_MainTex, uv);

#if defined(FF_EFFECT_BLUR)
				float3 offset = float3(_MainTex_TexelSize.xy, 0);
				float4 l = tex2D(_MainTex, uv - offset.xz);
				float4 r = tex2D(_MainTex, uv + offset.xz);
				float4 u = tex2D(_MainTex, uv - offset.zy);
				float4 d = tex2D(_MainTex, uv + offset.zy);

				float factorC = max(0, fluid.w - _FF_BlurMinFluid) * (1.0f / 4.0f);
				float4 factorLRUD = max(0, float4(l.w, r.w, u.w, d.w) - _FF_BlurMinFluid) * (1.0f / 4.0f);
				float4 gradient = (factorC - factorLRUD) * _FF_BlurFactor;
				float4 inGradient = max(0, -gradient);
				float inAmount = dot (inGradient, 1);
				float4 outGradient = max(0, gradient);
				float outAmount = dot (outGradient, 1);

				fluid.w -= outAmount;
				float3 color = fluid.rgb * fluid.w + l.rgb * inGradient.x + r.rgb * inGradient.y + u.rgb * inGradient.z + d.rgb * inGradient.w;
				float total = fluid.w + inAmount;
				fluid = float4(total > 0 ? color / total : float3(0, 0, 0), total);
#endif

#if defined(FF_EFFECT_FADE)
				// fluid.w = max(0, fluid.w - _FF_FadeAmount);	// linear fade
				// fluid.w = max(0, fluid.w - fluid.w * _FF_FadeAmount);	// exponential fade
				fluid.w = max(0, fluid.w - _FF_FadeAmount * (_FF_FadeMode > .5f ? fluid.w : 1));	// exponential fade
#endif
				return fluid;
			}
			ENDCG
		}
	}
}