Shader "Hidden/FluidFlow/Draw/UVDecal"
{
	SubShader
	{
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			// UV Set
			#pragma multi_compile_local __ FF_UV1

			// Source UV Set
			#pragma multi_compile_local __ FF_SOURCE_UV1

			// Color source
			#pragma multi_compile_local __ FF_COLOR

			// Mask (Optional)
			#pragma multi_compile_local __ FF_MASK

			// Decal Mode (Optional)
			#pragma multi_compile_local __ FF_FLUID FF_NORMAL

			#include "Draw.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			#if defined(FF_UV1)
				float2 uv : TEXCOORD1;
			#else
				float2 uv : TEXCOORD0;
			#endif

			#if defined(FF_SOURCE_UV1)
				float2 uv_source : TEXCOORD1;
			#else
				float2 uv_source : TEXCOORD0;
			#endif
			};

			struct v2f
			{
				float4 uv : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float2 uv_source : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.texcoord = AtlasTransformUV(v.uv);
				o.uv = ProjectUVToClipSpace(o.texcoord);
				o.uv_source = v.uv_source;
				return o;
			}

			// decal
			sampler2D _FF_DecalTex;
			// already defined in Draw.cginc
			// float4 _Color;
			// float _Data;

			// mask
			sampler2D _FF_MaskTex;
			float4 _FF_MaskComponents;
			float _FF_MaskComponentsInv;

			float4 frag(v2f i) : SV_Target
			{
				float factor = 1;

			#if defined(FF_MASK)
				float mask = dot(tex2D(_FF_MaskTex, i.uv_source), _FF_MaskComponents) * _FF_MaskComponentsInv;
				factor *= mask;
			#endif

				// sample corresponding color
			#if defined(FF_COLOR)
				float4 colorSample = _FF_Color;
			#else
				float4 colorSample = tex2D(_FF_DecalTex, i.uv_source);
			#endif

			#if defined(FF_FLUID)
				// set fluid amount
				colorSample.w *= _FF_Data;
			#elif defined(FF_NORMAL)
				float3 normal = UnpackNormal(colorSample);
				normal.xy *= _FF_Data;
				colorSample = PackNormal(normal);
			#endif

				// blend color with old texture
				return blend_draw(i.texcoord, colorSample, factor);
			}

			ENDCG
		}
	}
}