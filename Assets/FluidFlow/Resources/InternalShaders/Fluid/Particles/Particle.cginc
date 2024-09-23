#ifndef PARTICLE_CG_INCLUDED
#define PARTICLE_CG_INCLUDED

#define FF_PARTICLE_MIN_AMOUNT (0.01f)
#define FF_PARTICLE_AMOUNT_SCALE (128.0f)
inline float4 PackParticle(in float2 position, in float amount, in float speed)
{
	float4 particle = float4(position, amount * (1.0f / FF_PARTICLE_AMOUNT_SCALE), speed);
#if defined(FF_PARTICLETEX_SIGNED)
	return float4(position, amount, speed) * 2 - 1;	// also use negative values for additional precision
#endif
	return particle;
}

inline float UnpackParticleAmount(in float4 particle)
{
#if defined(FF_PARTICLETEX_SIGNED)
	float amount = particle.z * .5f + .5f;
#else
	float amount = particle.z;
#endif
	return amount * FF_PARTICLE_AMOUNT_SCALE;
}

inline float2 UnpackParticlePosition(in float4 particle)
{
#if defined(FF_PARTICLETEX_SIGNED)
	return particle.xy * .5f + .5f;
#else
	return particle.xy;
#endif
}

inline float2 UnpackParticleSpeed(in float4 particle)
{
#if defined(FF_PARTICLETEX_SIGNED)
	return particle.w * .5f + .5f;
#else
	return particle.w;
#endif
}

#endif