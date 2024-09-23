Shader "Hidden/PaintIn3D/CwRemap"
{
	Properties
	{
	}
	SubShader
	{
		Pass
		{
			Cull Off
			ZWrite Off

			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag

				sampler2D _CwTexure;

				struct a2v
				{
					float4 texcoord0 : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float2 coord  : TEXCOORD0;
				};

				void Vert(a2v i, out v2f o)
				{
					o.vertex = float4(i.texcoord0.xy * 2.0f - 1.0f, 0.5f, 1.0f);
					o.coord  = i.texcoord0.zw;
#if UNITY_UV_STARTS_AT_TOP
					o.vertex.y = -o.vertex.y;
#endif
				}

				float4 Frag(v2f i) : SV_TARGET
				{
					return tex2D(_CwTexure, i.coord);
				}
			ENDCG
		} // Pass
	} // SubShader
} // Shader