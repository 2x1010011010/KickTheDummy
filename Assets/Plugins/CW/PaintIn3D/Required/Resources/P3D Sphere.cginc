#include "../../../PaintCore/Required/Resources/P3dShared.cginc"
#include "../../../PaintCore/Required/Resources/P3dMasking.cginc"
#include "../../../PaintCore/Required/Resources/P3dBlendModes.cginc"
#include "../../../PaintCore/Required/Resources/P3dExtrusions.cginc"
#include "../../../PaintCore/Required/Resources/P3dOverlap.cginc"

float4    _Coord;
float4    _Channels;
float4x4  _Matrix;
float4    _Color;
float     _Opacity;
float     _Hardness;
float     _In3D;

sampler2D _TileTexture;
float4x4  _TileMatrix;
float     _TileOpacity;
float     _TileTransition;



struct a2v
{
	float4 vertex    : POSITION;
	float3 normal    : NORMAL;
	float2 texcoord0 : TEXCOORD0;
	float2 texcoord1 : TEXCOORD1;
	float2 texcoord2 : TEXCOORD2;
	float2 texcoord3 : TEXCOORD3;
};

struct v2f
{
	float4 vertex   : SV_POSITION;
	float2 texcoord : TEXCOORD0;
	float3 position : TEXCOORD1;
	float3 tile     : TEXCOORD2;
	float3 weights  : TEXCOORD3;
	float3 mask     : TEXCOORD4;
	float4 vpos     : TEXCOORD5;
};

struct f2g
{
	float4 color : SV_TARGET;
};

void Vert(a2v i, out v2f o)
{
	float2 texcoord = i.texcoord0 * _Coord.x + i.texcoord1 * _Coord.y + i.texcoord2 * _Coord.z + i.texcoord3 * _Coord.w;
	float4 worldPos = mul(unity_ObjectToWorld, i.vertex);
	float3 worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, i.normal));

	o.vertex   = float4(texcoord.xy * 2.0f - 1.0f, 0.5f, 1.0f);
	o.position = lerp(float3(texcoord, 0.0f), worldPos.xyz, _In3D);
	o.position = mul((float3x3)_Matrix, o.position);
	o.texcoord = texcoord;
	o.tile     = mul(_TileMatrix, worldPos).xyz;
	o.mask     = mul(_MaskMatrix, worldPos).xyz;
	o.vpos     = mul(_DepthMatrix, worldPos);

	o.weights = pow(abs(worldNormal), _TileTransition);
	o.weights /= o.weights.x + o.weights.y + o.weights.z;

#if UNITY_UV_STARTS_AT_TOP
	o.vertex.y = -o.vertex.y;
#endif
}

float P3D_GetStrength(float distance)
{
	float strength = 1.0f;
	strength -= pow(saturate(distance), _Hardness);
	strength *= _Opacity;
	return strength;
}

float P3D_GetStrength(v2f i, float distance)
{
	float strength = P3D_GetStrength(distance);
	#if P3D_LINE_CLIP || P3D_QUAD_CLIP
		#if P3D_LINE_CLIP
			float3 f_position = i.position - _Position;
		#elif P3D_QUAD_CLIP
			float3 f_position = i.position - P3D_GetClosestPosition_Edge(_Position, _EndPosition, i.position);
		#endif
		float f_strength = P3D_GetStrength(length(f_position));

		return P3D_GetOverlapStrength(strength, f_strength);
	#else
		return strength;
	#endif
}

void Frag(v2f i, out f2g o)
{
	float3 position = i.position - P3D_GetClosestPosition(i.position);
	float  distance = length(position);

	// You can remove this to improve performance if you don't care about overlapping UV support
	if (distance > 1.0f)
	{
		discard;
	}

	float  strength = P3D_GetStrength(i, distance);
	float4 color    = _Color;

	// Fade mask
	strength *= P3D_GetMask(i.mask);

	// Fade local mask
	strength *= P3D_GetLocalMask(i.texcoord);

	// Fade depth
	strength *= P3D_GetDepthMask(i.vpos);

	// Mix in tiling
	float4 textureX = tex2D(_TileTexture, i.tile.yz) * i.weights.x;
	float4 textureY = tex2D(_TileTexture, i.tile.xz) * i.weights.y;
	float4 textureZ = tex2D(_TileTexture, i.tile.xy) * i.weights.z;
	color *= lerp(float4(1.0f, 1.0f, 1.0f, 1.0f), textureX + textureY + textureZ, _TileOpacity);

	o.color = P3D_Blend(color, strength, i.texcoord, _Channels);
}