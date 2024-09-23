#ifndef FFPADDING_CG_INCLUDED
#define FFPADDING_CG_INCLUDED

#define PADDING_DIRECTIONS 9
#define PADDING_STEPS 3
#define PADDING_RESOLUTION (PADDING_DIRECTIONS * PADDING_STEPS)
#define PADDING_RESOLUTION_INV (1.0 / PADDING_RESOLUTION)

static const float2 FF_PADDING_OFFSETS[PADDING_DIRECTIONS] = {
	float2(0, 0),

	float2 (1, 0),
	float2 (-1, 0),
	float2 (0, 1),
	float2 (0, -1),

	float2 (1, 1),
	float2 (-1, -1),
	float2 (1, -1),
	float2 (-1, 1),
};

inline float EncodeOffset(int2 offset) {
	return ((offset.x * PADDING_STEPS) + offset.y - .5) * PADDING_RESOLUTION_INV;
}

inline float2 DecodeOffset(float encoded) {
	float dir = floor(encoded * PADDING_DIRECTIONS);
	float offset = floor(encoded * PADDING_RESOLUTION - dir * PADDING_STEPS);
	return FF_PADDING_OFFSETS[dir] * (offset + 1);
}

inline float2 OffsetToUV(int2 offset, float2 startUV, float2 texelSize) {
	return startUV + FF_PADDING_OFFSETS[offset.x] * texelSize * offset.y;
}

inline int2 ClosestOffsetInside(sampler2D uvMap, float2 uvMapTexelSize, float2 startUV) {
	int2 result = 0;
	float found = tex2D(uvMap, startUV).x;
	for (int padding = 1; padding < PADDING_STEPS; padding++) {
		for (int dir = 1; dir < PADDING_DIRECTIONS; dir++) {
			float inside = tex2D(uvMap, OffsetToUV(int2(dir, padding), startUV, uvMapTexelSize)).x;
			result = (found == 0 && inside > 0) ? int2(dir, padding) : result;
			found += inside;
		}
	}
	return result;
}

#endif