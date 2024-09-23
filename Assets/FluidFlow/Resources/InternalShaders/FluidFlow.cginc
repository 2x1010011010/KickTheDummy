#ifndef FLUIDFLOW_CG_INCLUDED
#define FLUIDFLOW_CG_INCLUDED

#include "UnityCG.cginc"

#define PI (3.14159265358979323846)
#define PI_INV (1.0 / PI)
#define PI2 (PI * 2.0)
#define PI2_INV (1.0 / PI2)

#define FF_TANGENT_SPACE_TRANSFORMATION TEXCOORD2

float4 _FF_AtlasTransform;

inline float2 AtlasTransformUV(in float2 uv)
{
	return uv * _FF_AtlasTransform.xy + _FF_AtlasTransform.zw;
}

inline float4 ProjectUVToClipSpace(in float2 uv)
{
#if defined(UNITY_UV_STARTS_AT_TOP)
	return float4(uv.x * 2 - 1, 1 - uv.y * 2, 0, 1);
#else
	return float4(uv.x * 2 - 1, uv.y * 2 - 1, 0, 1);
#endif
}

inline float3 ProjectToClipSpace(in float3 pos, in float4x4 projection)	
{
	float4 projected = mul(projection, float4(pos, 1));
	return ((projected.xyz / projected.w) + 1) * .5f;	// (0,0,0) - (1,1,1)
}

inline float VectorToMask(in float4 vec, in float4 maskChannel, in float4 maskInfo)
{
	float mask = dot(vec, maskChannel) * maskInfo.x;
	return mask <= maskInfo.y ? 0 : clamp(0, 1, mask + maskInfo.z);
}

inline float2x2 AngleToRotation(in float a) 
{
	float s = sin(a);
	float c = cos(a);
	return float2x2(c, -s, s, c);
}

inline float2 PolarToDirection(in float a) 
{
	return float2(cos(a), sin(a));
}

inline float2 SnapDirection(in float2 direction) 
{
	return abs(direction.x) > abs(direction.y) 
		? float2(sign(direction.x), 0) 
		: float2(0, sign(direction.y));
}

inline float3x3 CreateTangentToWorld(in float3 normal, in float4 tangent)
{
	float3 binormal = cross(normal, tangent.xyz) * tangent.w; // * unity_WorldTransformParams.w 
	return float3x3(tangent.xyz, binormal, normal);
}

inline float2 SnapToTexel(in float2 uv, in float4 texelSize)
{
	return (floor(uv * texelSize.zw) + .5f) * texelSize.xy;
}

inline float2 ClampLength(in float2 vec, in float maxLength)
{
	float len = length(vec);
	return vec * (min(maxLength, len) / (len == 0 ? 1 : len));
}

inline float4 PackNormal(in float3 normal)
{
	normal = normalize(normal);
#if defined(UNITY_NO_DXT5nm)
	return float4(normal * .5 + .5, 1);
#else
	return float4(1, normal.y * .5 + .5, 1, normal.x * .5 + .5);
#endif
}

// blit vertex shader base

struct blit_data
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
};

struct blit_v2f
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
};

blit_v2f blitvert(blit_data v)
{
	blit_v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv = v.uv;
	return o;
}

#endif