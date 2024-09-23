#include "../../../../PaintCore/Required/Resources/CwShared.cginc"
#include "../../../../PaintCore/Required/Resources/CwMasking.cginc"
#include "../../../../PaintCore/Required/Resources/CwBlendModes.cginc"
#include "../../../../PaintCore/Required/Resources/CwExtrusions.cginc"
#include "../../../../PaintCore/Required/Resources/CwOverlap.cginc"

float4    _Coord;
float4    _Channels;
float4x4  _Matrix;
float3    _Direction;
sampler2D _Texture;
sampler2D _Shape;
float4    _ShapeChannel;
float4    _Color;
float     _Opacity;
float     _Hardness;
float     _Wrapping;
float     _In3D;
float2    _NormalFront;
float2    _NormalBack;

sampler2D _TileTexture;
float4x4  _TileMatrix;
float     _TileOpacity;
float     _TileTransition;

struct a2v
{
	float4 vertex    : POSITION;
	float3 normal    : NORMAL;
	float3 tangent   : TANGENT;
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
	float2 dot_rot  : TEXCOORD2;
	float3 tile     : TEXCOORD3;
	float3 weights  : TEXCOORD4;
	float3 mask     : TEXCOORD5;
	float4 vpos     : TEXCOORD6;
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
	float3 worldTangent = normalize(mul((float3x3)unity_ObjectToWorld, i.tangent));

	o.vertex   = float4(texcoord.xy * 2.0f - 1.0f, 0.5f, 1.0f);
	o.texcoord = texcoord;
	o.position = lerp(float3(texcoord, 0.0f), worldPos.xyz, _In3D);
	o.position = mul((float3x3)_Matrix, o.position);
	o.tile     = mul(_TileMatrix, worldPos).xyz;
	o.mask     = mul(_MaskMatrix, worldPos).xyz;
	o.vpos     = mul(_DepthMatrix, worldPos);

	o.weights = pow(abs(worldNormal), _TileTransition);
	o.weights /= o.weights.x + o.weights.y + o.weights.z;

	float3 loc_tan = mul((float3x3)_Matrix, worldTangent);
	o.dot_rot.x = dot(worldNormal, _Direction);
	o.dot_rot.y = -atan2(loc_tan.y, loc_tan.x);

#if UNITY_UV_STARTS_AT_TOP
	o.vertex.y = -o.vertex.y;
#endif
}

float CW_GetStrength(float3 position, float normal)
{
	// Fade OOB
	float3 box = saturate(abs(position));
	box.xy = pow(box.xy, 1000.0f);
	box.z = pow(box.z, _Hardness);

	// Fade slopes
	float front = (_NormalFront.x - normal) * _NormalFront.y;
	float back  = (_NormalBack.x - normal) * _NormalBack.y;
	float fade  = saturate(max(front, back));

	// Shape
	float2 coord = position.xy * 0.5f + 0.5f;
	float  shape = dot(tex2D(_Shape, coord), _ShapeChannel);

	// Combine
	float strength = 1.0f;
	strength -= max(box.x, max(box.y, box.z));
	strength *= smoothstep(0.0f, 1.0f, fade);
	strength *= _Opacity;
	strength *= shape;
	return strength;
}

float CW_GetStrength(v2f i, float3 position)
{
	float strength = CW_GetStrength(position, i.dot_rot.x);
	#if CW_LINE_CLIP || CW_QUAD_CLIP
		#if CW_LINE_CLIP
			float3 f_position = i.position - _Position;
		#elif CW_QUAD_CLIP
			float3 f_position = i.position - CW_GetClosestPosition_Edge(_Position, _EndPosition, i.position);
		#endif
		float  f_depth    = f_position.z * _Wrapping; f_position.xy /= 1.0f - f_depth * f_depth;
		float  f_strength = CW_GetStrength(f_position, i.dot_rot.x);

		return CW_GetOverlapStrength(strength, f_strength);
	#else
		return strength;
	#endif
}

void Frag(v2f i, out f2g o)
{
	float3 position = i.position - CW_GetClosestPosition(i.position);
	float3 absPos   = abs(position);

	// You can remove this to improve performance if you don't care about overlapping UV support
	if (max(max(absPos.x, absPos.y), absPos.z) > 1.0f)
	{
		discard;
	}
	
	float  depth    = position.z * _Wrapping; position.xy /= 1.0f - depth * depth;
	float  strength = CW_GetStrength(i, position);
	float2 coord    = position.xy * 0.5f + 0.5f;
	float4 color    = tex2D(_Texture, coord) * _Color;

	// Fade mask
	strength *= CW_GetMask(i.mask);

	// Fade local mask
	strength *= CW_GetLocalMask(i.texcoord);

	// Fade depth
	strength *= CW_GetDepthMask(i.vpos);

	// Mix in tiling
	float4 textureX = tex2D(_TileTexture, i.tile.yz) * i.weights.x;
	float4 textureY = tex2D(_TileTexture, i.tile.xz) * i.weights.y;
	float4 textureZ = tex2D(_TileTexture, i.tile.xy) * i.weights.z;
	color *= lerp(float4(1.0f, 1.0f, 1.0f, 1.0f), textureX + textureY + textureZ, _TileOpacity);

	o.color = CW_Blend(color, strength, i.texcoord, i.dot_rot.y, _Channels);
}