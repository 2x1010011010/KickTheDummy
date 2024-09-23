Shader "Hidden/FluidFlow/Fluid/Simulate"
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
			sampler2D_float _FF_FlowTex;
			float4 _FF_FlowTex_TexelSize;
			float4 _FF_FluidRetained;
			float _FF_InflowColorInfluence;

			static const float MAX_FLUID = 10000;
			static const float SEAM_DAMPEN = 0.95f;

			inline float4 calcFlow(in float2 uv, in float4 flow, in float2 proj)
			{
				float2 gravity;
				float retained;
				bool seam = IsSeam(flow);
				if (seam) {
					// uv position of other uv island
					uv = UnpackSeamUV(flow, _FF_FlowTex_TexelSize);
					// sample gravity from seam
					float4 otherFlow = tex2D(_FF_FlowTex, uv);
					seam = IsSeam(otherFlow); // ensure sampled proper position
					proj = mul(UnpackSeamRotation(flow), proj);	// rotate projection, to match other uv island
					gravity = UnpackGravity(otherFlow);
					gravity *= SEAM_DAMPEN;	// reduce fluid flowing over seam, to prevent rare numeric 'explosion' of fluid. Fluid flowing back-and-forth over a seam, increasing in value
					retained = UnpackRetainedFluid(otherFlow);
				} else {
					gravity = UnpackGravity(flow);
					retained = UnpackRetainedFluid(flow);
				}
				float4 fluid = tex2D(_MainTex, uv);
				float flowingAmount = max(0, fluid.w - lerp(_FF_FluidRetained.x, _FF_FluidRetained.y, retained));
				float2 flowing = gravity * gravity * sign(gravity) * flowingAmount;	// TODO: limit max flow?
				return float4(fluid.xyz, max(0, dot(flowing, proj)) * (!seam));
			}

			float4 frag(blit_v2f i) : SV_Target
			{
				// float2 uv = i.uv + _FF_FlowTex_TexelSize.xy * .5f;
				float2 uvs[4] = { 
					i.uv - float2(1, 0) * _FF_FlowTex_TexelSize.xy, 
					i.uv + float2(1, 0) * _FF_FlowTex_TexelSize.xy, 
					i.uv + float2(0, 1) * _FF_FlowTex_TexelSize.xy, 
					i.uv - float2(0, 1) * _FF_FlowTex_TexelSize.xy 
				};

				// sample fluid
				float4 flow = tex2D(_FF_FlowTex, i.uv);
				if (IsZero(flow)) // early out for performance and stability
					return float4(0, 0, 0, 0);
				if (IsSeam(flow))
					return tex2D(_MainTex, UnpackSeamUV(flow, _FF_FlowTex_TexelSize));
				float4 texel = tex2D(_MainTex, i.uv);
				float2 gravity = UnpackGravity(flow);
				float retained = lerp(_FF_FluidRetained.x, _FF_FluidRetained.y, UnpackRetainedFluid(flow));
				float flowingAmount = max(0, texel.w - retained);
				float2 outflow = abs(gravity * gravity * flowingAmount);
				float remainingFluid = texel.w - outflow.x - outflow.y;

				// sample flow
				float4 inflows[4] = {
					calcFlow(uvs[0], tex2D(_FF_FlowTex, uvs[0]), float2(-1, 0)),
					calcFlow(uvs[1], tex2D(_FF_FlowTex, uvs[1]), float2(1, 0)),
					calcFlow(uvs[2], tex2D(_FF_FlowTex, uvs[2]), float2(0, 1)),
					calcFlow(uvs[3], tex2D(_FF_FlowTex, uvs[3]), float2(0, -1))
				};

				// fluid flowing into this texel
				float totalInflow = inflows[0].w + inflows[1].w + inflows[2].w + inflows[3].w;
				float4 relativeInFlow = float4(inflows[0].w, inflows[1].w, inflows[2].w, inflows[3].w) * (totalInflow > 0 ? 1.0f / totalInflow : 1);

				// weighted average color of the fluid flowing into this texel 
				float3 inflowColor =  inflows[0].xyz * relativeInFlow.x + inflows[1].xyz * relativeInFlow.y + inflows[2].xyz * relativeInFlow.z + inflows[3].xyz * relativeInFlow.w;

				// calculate new color and fluid amount of this texel
				float3 fluidColor = lerp(texel.xyz, inflowColor, min(totalInflow * _FF_InflowColorInfluence + (remainingFluid < .1f && totalInflow > 0), 1));
				return float4(fluidColor, clamp(remainingFluid + totalInflow, 0, MAX_FLUID));
			}
			ENDCG
		}
	}
}