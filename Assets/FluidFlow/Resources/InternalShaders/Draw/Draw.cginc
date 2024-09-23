#ifndef DRAW_CG_INCLUDED
#define DRAW_CG_INCLUDED

// basic shader setup, to render each pixels world position to the uv unwrap of the model
#include "../FluidFlow.cginc"
#include "UnityCG.cginc"

struct appdata_draw
{
	float4 vertex : POSITION;
#if defined(FF_UV1)
	float2 uv : TEXCOORD1;
#else
	float2 uv : TEXCOORD0;
#endif
};

struct v2f_draw
{
	float4 uv : SV_POSITION;
	float3 vertex : TEXCOORD0;
	float2 texcoord : TEXCOORD1;
};

v2f_draw vert_draw(appdata_draw v)
{
	v2f_draw o;
	o.texcoord = AtlasTransformUV(v.uv);
	o.uv = ProjectUVToClipSpace(o.texcoord);
	o.vertex = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
	return o;
}

// blend old texture with current brush stroke, depending on brush type
sampler2D _FF_OtherTex;
half4 _FF_WriteMask;
float4 blend_draw(float2 uv, float4 color, float factor)
{
	float4 old = tex2D(_FF_OtherTex, uv);
#if defined(FF_FLUID)
	float3 alpha = _FF_WriteMask.xyz * clamp(color.w * factor, 0, 1);
	return float4(old.xyz * (1 - alpha) + color.xyz * alpha, old.w + color.w * factor * _FF_WriteMask.w);
#else
	float4 mask = _FF_WriteMask * clamp(factor, 0, 1);
	return lerp(old, color, mask);
#endif
}

// basic fragment shader setup, for brush shaders
half4 _FF_Color;
float _FF_Data;
float _FF_Fade;
float _FF_FadeInv;

// predeclare distance function for the brushes
float distance(v2f_draw i);

float smoothstep(float x) {
	x = clamp(x, 0.0, 1.0);
	return x * x * x * (x * (x * 6 - 15) + 10);
}

float4 frag_draw(v2f_draw i) : SV_Target
{
	float dist = distance(i);

	// generate color from brush data and distance
	float amount = (1 - smoothstep((dist - _FF_Fade) * _FF_FadeInv)) * (dist <= 1);
#if defined(FF_FLUID)
	// set fluid amount
	amount *= _FF_Data;
#endif
	// blend color with old texture
	return blend_draw(i.texcoord, _FF_Color, amount);
}

#endif