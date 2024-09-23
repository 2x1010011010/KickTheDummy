Shader "Hidden/FluidFlow/Gravity"
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
			#pragma multi_compile_local __ FF_UV1
			#pragma multi_compile_local __ USE_NORMAL

			#include "Flow.cginc"

			#include "../Noise.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			#if defined(FF_UV1)
				float2 uv1 : TEXCOORD1;
				float4 transform : FF_TANGENT_SPACE_TRANSFORMATION;
			#endif
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float4 tangent : TEXCOORD2;
			#if defined(FF_UV1)
				float4 transform : TEXCOORD3;
			#endif
			};

			sampler2D_half _FF_NormalTex;
			float _FF_NormalStrength;
			float _FF_NormalFromAtlas;

			float4 _FF_Gravity;
			float4 _FF_TexelSize;

			float4 _FF_Noise;
			float4 _FF_NoiseLayerShift;

			v2f vert(appdata v)
			{
				v2f o;
			#if defined(FF_UV1)
				float2 altasUV = AtlasTransformUV(v.uv1);
				o.transform = v.transform;
			#else
				float2 altasUV = AtlasTransformUV(v.uv);
			#endif
				o.vertex = ProjectUVToClipSpace(altasUV);
				o.uv = _FF_NormalFromAtlas > 0 ? altasUV : v.uv;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
				return o;
			}


			float4 frag(v2f i) : SV_Target
			{
				float3x3 tangentToWorld;
			#if defined(USE_NORMAL)
				// deform tangent space depending on _NormalTex
				// assuming _NormalTex is relative to uv0 tangent space
				float3 binormal = cross(i.normal, i.tangent.xyz) * i.tangent.w; // * unity_WorldTransformParams.w
				float3 normal = UnpackNormal(tex2D(_FF_NormalTex, i.uv));
				// scale normal
				normal = normalize(float3(normal.xy * _FF_NormalStrength, normal.z));
				// convert to world space
				normal = mul(normal, float3x3(i.tangent.xyz, binormal, i.normal));
				// recalculate tangent to world matrix using the normal map
				float3 tangent = cross(binormal, normal) * i.tangent.w;
				tangentToWorld = CreateTangentToWorld(normal, float4(tangent,  i.tangent.w));
			#else
				tangentToWorld = CreateTangentToWorld(i.normal, i.tangent);
			#endif

				// world space gravity to local uv0 tangent space
				float3 g = mul(tangentToWorld, _FF_Gravity.xyz).xyz;
			#if defined(FF_UV1)
				// transform gravity to uv1 tangent space, if necessary
				g.xy = mul(float2x2(i.transform), g.xy);
			#endif

				if (_FF_Noise.w > 0) {
					float2 pos = i.uv * _FF_Noise.z + _FF_Noise.xy;
					float2 rnd = float2(ff_fnoise(pos, _FF_NoiseLayerShift.xy),  ff_fnoise(pos + 12.34f, _FF_NoiseLayerShift.xy));
					return PackFlowGravity(normalize(float3(g.xy + rnd * _FF_Noise.w, g.z)));
				} else {
					return PackFlowGravity(normalize(g));
				}
			}
			ENDCG
		}
	}
}