#ifndef FFSHADERUTIL_CG_INCLUDED
#define FFSHADERUTIL_CG_INCLUDED

// define the layout of the fluid textures
#define FF_FLUID_COLOR xyz
#define FF_FLUID_AMOUNT w

// default defines for a surface shader input struct
#define FF_UV_NAME FF_uv
#define FF_TRANSFORMATION_NAME FF_transformation

#define FF_SURFACE_INPUT_UV0 float2 FF_UV_NAME;
#define FF_SURFACE_INPUT_UV1 FF_SURFACE_INPUT_UV0 float4 FF_TRANSFORMATION_NAME;

#if defined(FF_UV1)
	#define FF_SURFACE_INPUT FF_SURFACE_INPUT_UV1
#else
	#define FF_SURFACE_INPUT FF_SURFACE_INPUT_UV0
#endif

// default defines for initializing a surface shader input struct
#define FF_TANGENT_SPACE_TRANSFORMATION texcoord2
#define FF_INITIALIZE_OUTPUT_UV0(appdata, output, atlasTransformation) {\
	(output).FF_UV_NAME = FFAtlasTransformUV((atlasTransformation), (appdata).texcoord.xy);}

#define FF_INITIALIZE_OUTPUT_UV1(appdata, output, atlasTransformation) {\
	(output).FF_UV_NAME = FFAtlasTransformUV((atlasTransformation), (appdata).texcoord1.xy);\
	(output).FF_TRANSFORMATION_NAME = (appdata).FF_TANGENT_SPACE_TRANSFORMATION;}

#if defined(FF_UV1)
	#define FF_INITIALIZE_OUTPUT FF_INITIALIZE_OUTPUT_UV1
#else
	#define FF_INITIALIZE_OUTPUT FF_INITIALIZE_OUTPUT_UV0
#endif

// convenience macro, for transforming a tangent space normal from uv1 to uv0, when necessary
#if defined(FF_UV1)
	#define FF_TRANSFORM_NORMAL(surface_input, normal) FFTransformNormalFromUV1(normal, (surface_input).FF_TRANSFORMATION_NAME)
#else
	#define FF_TRANSFORM_NORMAL(surface_input, normal) normal
#endif

inline float2 FFAtlasTransformUV(in float4 transformation, in float2 uv) 
{
	return uv * transformation.xy + transformation.zw;
}

#if defined(FF_UV1)
	#define FF_UNPACK_FLUID_NORMAL(surface_input, fluid_sampler, fluid_amount, scale) FFUnpackFluidNormalUV1(fluid_sampler, (surface_input).FF_TRANSFORMATION_NAME, ##fluid_sampler##_TexelSize.xy, fluid_amount, (surface_input).FF_UV_NAME, scale)
#else
	#define FF_UNPACK_FLUID_NORMAL(surface_input, fluid_sampler, fluid_amount, scale) FFUnpackFluidNormal(fluid_sampler, ##fluid_sampler##_TexelSize.xy, fluid_amount, (surface_input).FF_UV_NAME, scale)
#endif

inline float3 FFUnpackFluidNormal(in sampler2D fluidTex, in float2 texelSize, in float fluid_amount, in float2 uv, in float scale)
{
	half r = tex2D(fluidTex, uv + float2(texelSize.x, 0)).FF_FLUID_AMOUNT;
	half u = tex2D(fluidTex, uv + float2(0, texelSize.y)).FF_FLUID_AMOUNT;
	float2 gradient = float2(fluid_amount - r, fluid_amount - u) * scale;
	return normalize(float3(gradient, 1));
}

inline float3 FFUnpackFluidNormalUV1(in sampler2D fluidTex, float4 uv_transform, in float2 texelSize, in float fluid_amount, in float2 uv, in float scale)
{
	float2 up = uv_transform.xz * texelSize;
	float2 right = uv_transform.yw * texelSize;
	half r = tex2D(fluidTex, uv + right).FF_FLUID_AMOUNT;
	half u = tex2D(fluidTex, uv + up).FF_FLUID_AMOUNT;
	float2 gradient = float2(fluid_amount - r, fluid_amount - u) * scale;
	return normalize(float3(gradient, 1));
}

inline float3 FFTransformNormalFromUV1(in float3 normal, in float4 texcoord2)
{
	normal.xy = mul(float2x2(texcoord2.xzyw), normal.xy);
	return normal;
}

float2 hash(float2 p)
{
	p = float2( dot(p,float2(127.1,311.7)), dot(p,float2(269.5,183.3)) );
	return -1.0 + 2.0*frac(sin(p)*43758.5453123);
}

float ff_snoise( in float2 p )
{
    const float K1 = 0.366025404; // (sqrt(3)-1)/2;
    const float K2 = 0.211324865; // (3-sqrt(3))/6;
	float2  i = floor( p + (p.x+p.y)*K1 );
    float2  a = p - i + (i.x+i.y)*K2;
    float m = step(a.y,a.x); 
    float2  o = float2(m,1.0-m);
    float2  b = a - o + K2;
	float2  c = a - 1.0 + 2.0*K2;
    float3  h = max( 0.5-float3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );
	float3  n = h*h*h*h*float3( dot(a,hash(i+0.0)), dot(b,hash(i+o)), dot(c,hash(i+1.0)));
    return dot( n, float3(70.0, 70.0, 70.0) );
}

float ff_fnoise(in float2 p)
{
    return ff_snoise(p) * .5f + ff_snoise(p * 2) * .25f + ff_snoise(p * 4) * .125f;
}

float2 FFDistortUV(float2 uv, float scale, float amount)
{
	float2 p = uv * scale;
	return uv + float2(ff_fnoise(p), ff_fnoise(p + float2(12.3, 23.4))) * amount;
}


#endif