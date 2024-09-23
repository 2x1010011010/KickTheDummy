Shader "Hidden/FluidFlow/Fluid/Particles/SimulateParticles"
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

			#pragma multi_compile_local __ FF_FLOWTEX_COMPRESSED
			#pragma multi_compile_local __ FF_PARTICLETEX_SIGNED

			#include "Particle.cginc"
			#include "../Flow.cginc"

			#pragma vertex blitvert
			#pragma fragment frag

			sampler2D_float _MainTex;
			sampler2D_float _FF_FlowTex;
			float4 _FF_FlowTex_TexelSize;
			float _FF_Amount;
			float _FF_MinGravity;

			float4 frag(blit_v2f i) : SV_Target
			{
				float4 particle = tex2D(_MainTex, i.uv);
				if (particle.z == 0)
					return float4(0, 0, 0, 0);
				float2 position = UnpackParticlePosition(particle);
				float amount = UnpackParticleAmount(particle);
				float speed = UnpackParticleSpeed(particle);
				float4 flow = tex2D(_FF_FlowTex, position);
				if (IsZero(flow))
					return float4(0, 0, 0, 0);
				bool isSeam = IsSeam(flow);
				if (isSeam) {
					float2 seamUV = UnpackSeamUV(flow, _FF_FlowTex_TexelSize);
					float4 otherFlow = tex2D(_FF_FlowTex, seamUV);
					if (IsSeam(otherFlow))
						return float4(0, 0, 0, 0);
					particle = PackParticle(seamUV, amount, speed) ;
				} else {
					float2 gravity = UnpackGravity(flow);
					float gravityPull = dot(gravity, gravity);
					position -= gravity * speed * _FF_FlowTex_TexelSize.xy;
					amount -= _FF_Amount;
					amount = amount < FF_PARTICLE_MIN_AMOUNT || gravityPull < _FF_MinGravity ? 0 : amount;

					particle = PackParticle(position, amount, speed);
				}
				return particle;
			}
			ENDCG
		}
	}
}