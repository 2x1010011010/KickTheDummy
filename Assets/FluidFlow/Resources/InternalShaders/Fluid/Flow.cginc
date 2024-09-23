#ifndef FLOW_CG_INCLUDED
#define FLOW_CG_INCLUDED

#include "../FluidFlow.cginc"

inline float4 PackFlowGravity(in float3 grav) 
{
	return float4(grav.xy * (-.5f) + .5f, grav.z * grav.z, 0);
}

#define FF_BIT_MASK(x) ((1 << x) - 1)
#define FF_FLOW_PRECISION 8
#define FF_FLOW_MAX (FF_BIT_MASK(FF_FLOW_PRECISION))
#define FF_FLOW_MAX_INV (1.0f / (float)FF_FLOW_MAX)

inline float4 PackFlowSeam(in float2 target, in float rotation, in float4 texelSize)
{
#if defined(FF_FLOWTEX_COMPRESSED)
	// Using an RGBA32 RenderTexture each channel has 8bit precision,
	// each target uv component is stored using 13bits, using some bit-magic to spread it over multiple 8bit texture channels
	// due to the 13bit precision of the target uv, (8192, 8192) is the maximum possible pixel coordinate that can be encoded
	uint2 uv = (uint2) (target);
	uint2 low = uv & FF_BIT_MASK(FF_FLOW_PRECISION);
	uint2 high = (uv >> FF_FLOW_PRECISION) & FF_BIT_MASK(5); 							// 5 high-bits of the target x/y coordinate
	uint rotEncoded = (uint)(rotation * FF_BIT_MASK(5)); 								// encode seam rotation in 5 bits
	uint packedZ = high.x | (rotEncoded & FF_BIT_MASK(3)) << 5;							// 5 high-bits of uv.x, and 3 low-bits of rotation
	uint packedW = high.y | ((rotEncoded & (FF_BIT_MASK(2) << 3)) << 2) | (1 << 7);		// 5 high-bits of uv.y, and 2 high-bit of rotation, and 1 high bit to ensure >0 for detecting seam
	return uint4(low, packedZ, packedW) * FF_FLOW_MAX_INV;
#else
	return float4(target * texelSize.xy, rotation, 1);
#endif
}

inline bool IsSeam(in float4 flow)
{
	return flow.w > 0;
}

inline bool IsZero(in float4 flow)
{
	return flow.x == 0 && flow.y == 0 && flow.z == 0 && flow.w == 0;
}

inline float2 UnpackSeamUV(in float4 flow, in float4 texelSize)
{
#if defined(FF_FLOWTEX_COMPRESSED)
	uint4 raw = flow * FF_FLOW_MAX;
	uint2 target = uint2(raw.x + ((raw.z & FF_BIT_MASK(5)) << FF_FLOW_PRECISION),  raw.y + ((raw.w & FF_BIT_MASK(5)) << FF_FLOW_PRECISION));
	return target * texelSize.xy + texelSize.xy * .5f;
#else
	return flow.xy;
#endif
}

inline float2x2 UnpackSeamRotation(in float4 flow)
{
#if defined(FF_FLOWTEX_COMPRESSED)
	uint2 raw = flow.zw * FF_FLOW_MAX;
	float r = ((raw.x >> 5) | ((raw.y & (FF_BIT_MASK(2) << 5)) >> 2)) * (PI2 / (float)FF_BIT_MASK(5)); 	// unpack rotation to the 0-2*PI range
	return AngleToRotation(r);
#else
	return AngleToRotation(flow.z * PI2);
#endif
}

inline float2 UnpackGravity(in float4 flow) 
{
	return flow.xy * 2 - 1;
}

inline float UnpackRetainedFluid(in float4 flow)
{
	return flow.z;
}

#endif