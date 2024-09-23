Shader "Hidden/PaintIn3D/CwDilate"
{
	Properties
	{
	}
	SubShader
	{
		Cull Off

		Pass
		{
			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag

				float4 _CwCoord;

				struct a2v
				{
					float2 texcoord0 : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
					float2 texcoord2 : TEXCOORD2;
					float2 texcoord3 : TEXCOORD3;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
				};

				void Vert(a2v i, out v2f o)
				{
					float2 texcoord = i.texcoord0 * _CwCoord.x + i.texcoord1 * _CwCoord.y + i.texcoord2 * _CwCoord.z + i.texcoord3 * _CwCoord.w;
					o.vertex = float4(texcoord.xy * 2.0f - 1.0f, 0.5f, 1.0f);
#if UNITY_UV_STARTS_AT_TOP
					o.vertex.y = -o.vertex.y;
#endif
				}

				float4 Frag(v2f i) : SV_TARGET
				{
					return float4(1, 0, 0, 0);
				}
			ENDCG
		}

		Pass // LQ A
		{
			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag

				#include "../../../PaintCore/Required/Resources/CwShared.cginc"

				sampler2D _CwTexure;
				float2    _CwSize;
				float4    _CwOffsets[255];
				int       _CwSamples;
				float2    _CwScale;

				struct a2v
				{
					float4 vertex    : POSITION;
					float2 texcoord0 : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float2 uv     : TEXCOORD0;
				};

				void Vert(a2v i, out v2f o)
				{
					o.vertex = UnityObjectToClipPos(i.vertex);
					o.uv     = i.texcoord0;
				}

				float4 Frag(v2f i) : SV_TARGET
				{
					float2 uv = CW_SnapToPixel(i.uv, _CwSize);

					if (tex2Dlod(_CwTexure, float4(uv, 0, 0)).x < 1.0f)
					{
						for (int s = 1; s <= _CwSamples; s++)
						{
							if (tex2Dlod(_CwTexure, float4(uv + _CwOffsets[s].xy * _CwScale, 0, 0)).x >= 1.0f)
							{
								return float4(s, 0.0f, 0.0f, 0.0f) / 255.0f;
							}
						}
					}

					return float4(0.0f, 0.0f, 0.0f, 0.0f);
				}
			ENDCG
		}

		Pass // LQ B
		{
			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag

				#include "../../../PaintCore/Required/Resources/CwShared.cginc"
				
				sampler2D _CwTexure;
				sampler2D _CwLookup;
				float2    _CwSize;
				float4    _CwOffsets[255];
				float2    _CwScale;

				struct a2v
				{
					float4 vertex    : POSITION;
					float2 texcoord0 : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float2 uv     : TEXCOORD0;
				};

				void Vert(a2v i, out v2f o)
				{
					o.vertex = UnityObjectToClipPos(i.vertex);
					o.uv     = i.texcoord0;
				}

				float4 Frag(v2f i) : SV_TARGET
				{
					float2 uv    = CW_SnapToPixel(i.uv, _CwSize);
					int    index = tex2D(_CwLookup, uv).x * 255.0f;

					return tex2D(_CwTexure, uv + _CwOffsets[index].xy * _CwScale);
				}
			ENDCG
		}

		Pass // HQ A
		{
			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag

				#include "../../../PaintCore/Required/Resources/CwShared.cginc"

				sampler2D _CwTexure;
				float2    _CwSize;
				float2    _CwScale;

				struct a2v
				{
					float4 vertex    : POSITION;
					float2 texcoord0 : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float2 uv     : TEXCOORD0;
				};

				void Vert(a2v i, out v2f o)
				{
					o.vertex = UnityObjectToClipPos(i.vertex);
					o.uv     = i.texcoord0;
				}

				float4 Frag(v2f i) : SV_TARGET
				{
					float2 uv   = CW_SnapToPixel(i.uv, _CwSize);
					float4 best = float4(tex2Dlod(_CwTexure, float4(uv, 0, 0)).xyz, 1000.0f);

					if (best.x < 1.0f)
					{
						for (int y = -4; y <= 4; y++)
						{
							for (int x = -4; x <= 4; x++)
							{
								int2   offset = int2(x, y);
								float  dist   = length(offset);
								float4 samp   = tex2Dlod(_CwTexure, float4(uv + offset * _CwScale, 0, 0));

								if (samp.x >= 1.0f && dist < best.w)
								{
									best = float4(1, samp.yz + offset, dist);
								}
							}
						}
					}

					return best;
				}
			ENDCG
		}

		Pass // HQ B
		{
			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag

				#include "../../../PaintCore/Required/Resources/CwShared.cginc"
				
				sampler2D _CwTexure;
				sampler2D _CwLookup;
				float2    _CwSize;
				float2    _CwScale;

				struct a2v
				{
					float4 vertex    : POSITION;
					float2 texcoord0 : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float2 uv     : TEXCOORD0;
				};

				void Vert(a2v i, out v2f o)
				{
					o.vertex = UnityObjectToClipPos(i.vertex);
					o.uv     = i.texcoord0;
				}

				float4 Frag(v2f i) : SV_TARGET
				{
					float2 uv     = CW_SnapToPixel(i.uv, _CwSize);
					float2 offset = tex2D(_CwLookup, uv).yz;

					return tex2D(_CwTexure, uv + offset * _CwScale);
				}
			ENDCG
		}
	}
}