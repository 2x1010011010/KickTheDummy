Shader "Hidden/FluidFlow/Fluid/UVSeamStitch"
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

			#pragma multi_compile_local __ FF_FLOWTEX_COMPRESSED

			#include "Flow.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 data : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = ProjectUVToClipSpace(v.vertex.xy);
				// uv position of connected uv seam
				o.uv = v.uv.xy; 
				// seam-normals point in opposite directions -> no rotation needed
				float rot = v.uv.w - v.uv.z + PI;
				// orientation of other seam (xy), and rotation difference for gravity conversion (zw)
				o.data = float4(PolarToDirection(v.uv.w), clamp((rot * PI2_INV + 2) % 1.0f, 0, 1), 0);
				return o;
			}

			float4 _FF_TexelSize;

			float4 frag(v2f i) : SV_Target
			{
				// calculate uv position of other seam texel
				float2 sampleUV = floor(i.uv * _FF_TexelSize.zw + i.data.xy * 0.5f) + .5f;
				return PackFlowSeam(sampleUV, i.data.z, _FF_TexelSize);
			}
			ENDCG
		}
	}
}