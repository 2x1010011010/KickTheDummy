sampler2D _Buffer;
float2    _BufferSize;

float2 P3D_SnapToPixel(float2 coord, float2 size)
{
	float2 pixel = floor(coord * size);
#ifndef UNITY_HALF_TEXEL_OFFSET
	pixel += 0.5f;
#endif
	return pixel / size;
}

float4 P3D_SampleMip0(sampler2D s, float2 coord)
{
	return tex2Dbias(s, float4(coord.x, coord.y, 0, -15.0));
}

float4 P3D_PackNormal(float3 v)
{
#if defined(SHADER_API_MOBILE) && UNITY_VERSION > 202200 && !defined(UNITY_NO_DXT5nm)
	v = v * 0.5f + 0.5f; return float4(v.x, v.y, v.z, v.x);
#else
	v = v * 0.5f + 0.5f; return float4(v.x, v.y, v.z, 1.0f);
#endif
}

float3 P3D_RotateNormal(float3 v, float a)
{
	float s = sin(a); float c = cos(a); return float3(v.x * c - v.y * s, v.x * s + v.y * c, v.z);
}