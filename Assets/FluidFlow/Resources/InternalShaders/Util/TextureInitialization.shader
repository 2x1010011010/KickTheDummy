Shader "Hidden/FluidFlow/TextureInitialization"
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

			#include "../FluidFlow.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			#if defined(FF_UV1)
				float2 uv2 : TEXCOORD1;
			#endif
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _FF_MainTex;

			v2f vert(appdata v)
			{
				v2f o;
			#if defined(FF_UV1)
				o.vertex = ProjectUVToClipSpace(AtlasTransformUV(v.uv2));
			#else
				o.vertex = ProjectUVToClipSpace(AtlasTransformUV(v.uv));
			#endif
				o.uv = v.uv;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				return tex2D(_FF_MainTex, i.uv);
			}
			ENDCG
		}
	}
}