Shader "Hidden/PaintCore/CwFill"
{
	Properties
	{
		_ReplaceTexture("Replace Texture", 2D) = "white" {}
		_Texture("Texture", 2D) = "white" {}
		_MaskTexture("Mask Texture", 2D) = "white" {}
		_LocalMaskTexture("Local Mask Texture", 2D) = "white" {}
	}

	SubShader
	{
		Blend Off
		Cull Off
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 0 // ALPHA_BLEND
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 1 // ALPHA_BLEND_INVERSE
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 2 // PREMULTIPLIED
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 3 // ADDITIVE
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 4 // ADDITIVE_SOFT
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 5 // SUBTRACTIVE
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 6 // SUBTRACTIVE_SOFT
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 7 // REPLACE
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 8 // REPLACE_ORIGINAL
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 9 // REPLACE_CUSTOM
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 10 // MULTIPLY_INVERSE_RGB
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 11 // BLUR
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 12 // NORMAL_BLEND
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 13 // NORMAL_REPLACE
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 14 // FLOW
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 15 // NORMAL_REPLACE_ORIGINAL
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 16 // NORMAL_REPLACE_CUSTOM
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 17 // MIN
			#include "CwFill.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#define BLEND_MODE_INDEX 18 // MAX
			#include "CwFill.cginc"
			ENDCG
		}
	} // SubShader
} // Shader