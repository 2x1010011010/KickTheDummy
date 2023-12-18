Shader "KriptoFX/BFX/BFX_Decal"
{
	Properties
	{
		[HDR] _TintColor("Tint Color", Color) = (1,1,1,1)
		_MainTex("NormalAlpha", 2D) = "white" {}
		_LookupFade("Lookup Fade Texture", 2D) = "white" {}
		_Cutout("Cutout", Range(0, 1)) = 1
		_CutoutTex("CutoutDepth(XZ)", 2D) = "white" {}
	[Space]
		_SunPos("Sun Pos", Vector) = (1, 0.5, 1, 0)
		//[Toggle(CLAMP_SIDE_SURFACE)] _ClampSideSurface("Clamp side surface", Int) = 0
	}


	SubShader
	{
		Tags{ "Queue" = "AlphaTest"}
		Blend DstColor SrcColor
		//Blend SrcAlpha OneMinusSrcAlpha
		Cull Front
		ZTest Always
		ZWrite Off

		Pass
		{

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#pragma multi_compile_fog
				#pragma multi_compile_instancing
				#pragma multi_compile _ USE_CUSTOM_DECAL_LAYERS
				#pragma shader_feature CLAMP_SIDE_SURFACE
				#pragma multi_compile _ USE_CUSTOM_DECAL_LAYERS_IGNORE_MODE

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _Flowmap;
				sampler2D _LookupFade;
				sampler2D _CutoutTex;

				float4 _MainTex_ST;
				float4 _MainTex_NextFrame;
				float4 _CutoutTex_ST;

				UNITY_INSTANCING_BUFFER_START(Props)
					UNITY_DEFINE_INSTANCED_PROP(half4, _TintColor)
					UNITY_DEFINE_INSTANCED_PROP(half, _Cutout)
					UNITY_DEFINE_INSTANCED_PROP(float, _LightIntencity)
				UNITY_INSTANCING_BUFFER_END(Props)


				half4 _CutoutColor;
				half4 _FresnelColor;
				half4 _DistortionSpeedScale;
				UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
				UNITY_DECLARE_DEPTH_TEXTURE(_LayerDecalDepthTexture);
				half InterpolationValue;
				half _AlphaPow;
				half _DistortSpeed;
				half _DistortScale;
				float4 _SunPos;
				half _DepthMul;
				half3 _DecalForwardDir;

				struct appdata_t {
					float4 vertex : POSITION;
					float4 normal : NORMAL;
					half4 color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					half4 color : COLOR;

					float4 screenUV : TEXCOORD0;
					float4 screenPos : TEXCOORD1;
					float3 viewDir : TEXCOORD2;
					UNITY_FOG_COORDS(3)
					float4x4 inverseVP : TEXCOORD4;

					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				float4x4 inverse(float4x4 m) {
					float
						a00 = m[0][0], a01 = m[0][1], a02 = m[0][2], a03 = m[0][3],
						a10 = m[1][0], a11 = m[1][1], a12 = m[1][2], a13 = m[1][3],
						a20 = m[2][0], a21 = m[2][1], a22 = m[2][2], a23 = m[2][3],
						a30 = m[3][0], a31 = m[3][1], a32 = m[3][2], a33 = m[3][3],

						b00 = a00 * a11 - a01 * a10,
						b01 = a00 * a12 - a02 * a10,
						b02 = a00 * a13 - a03 * a10,
						b03 = a01 * a12 - a02 * a11,
						b04 = a01 * a13 - a03 * a11,
						b05 = a02 * a13 - a03 * a12,
						b06 = a20 * a31 - a21 * a30,
						b07 = a20 * a32 - a22 * a30,
						b08 = a20 * a33 - a23 * a30,
						b09 = a21 * a32 - a22 * a31,
						b10 = a21 * a33 - a23 * a31,
						b11 = a22 * a33 - a23 * a32,

						det = b00 * b11 - b01 * b10 + b02 * b09 + b03 * b08 - b04 * b07 + b05 * b06;

					return float4x4(
						a11 * b11 - a12 * b10 + a13 * b09,
						a02 * b10 - a01 * b11 - a03 * b09,
						a31 * b05 - a32 * b04 + a33 * b03,
						a22 * b04 - a21 * b05 - a23 * b03,
						a12 * b08 - a10 * b11 - a13 * b07,
						a00 * b11 - a02 * b08 + a03 * b07,
						a32 * b02 - a30 * b05 - a33 * b01,
						a20 * b05 - a22 * b02 + a23 * b01,
						a10 * b10 - a11 * b08 + a13 * b06,
						a01 * b08 - a00 * b10 - a03 * b06,
						a30 * b04 - a31 * b02 + a33 * b00,
						a21 * b02 - a20 * b04 - a23 * b00,
						a11 * b07 - a10 * b09 - a12 * b06,
						a00 * b09 - a01 * b07 + a02 * b06,
						a31 * b01 - a30 * b03 - a32 * b00,
						a20 * b03 - a21 * b01 + a22 * b00) / det;
				}

				float3 WorldSpacePositionFromDepth(float2 positionNDC, float deviceDepth, float4x4 inverseVP)
				{
					float4 positionCS = float4(positionNDC * 2.0 - 1.0, deviceDepth, 1.0);
#if UNITY_UV_STARTS_AT_TOP
					positionCS.y = -positionCS.y;
#endif
					float4 hpositionWS = mul(inverseVP, positionCS);
					return hpositionWS.xyz / hpositionWS.w;
				}

				v2f vert(appdata_t v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;

					o.screenPos = ComputeScreenPos(o.vertex);
					o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
					UNITY_TRANSFER_FOG(o,o.vertex);

					o.inverseVP = inverse(UNITY_MATRIX_VP);

					return o;
				}

			

				half4 frag(v2f i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);
					float2 screenUV = i.screenPos.xy / i.screenPos.w;

#if USE_CUSTOM_DECAL_LAYERS
					float depth = SAMPLE_DEPTH_TEXTURE(_LayerDecalDepthTexture, screenUV);
					float depthMask = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
					float fade = depth < depthMask - 0.0005 ? 0 : 1;
	#if USE_CUSTOM_DECAL_LAYERS_IGNORE_MODE
						fade = 1 - fade;
						depth = depthMask;
	#endif
#else
					float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
#endif

					float3 worldPos = WorldSpacePositionFromDepth(screenUV, depth, i.inverseVP);
					float3 decalSpaceScenePos = mul(unity_WorldToObject, float4(worldPos, 1));

					float3 stepVal = saturate((0.5 - abs(decalSpaceScenePos.xyz)) * 10000);
					half lookupHeight = tex2D(_LookupFade, float2(decalSpaceScenePos.y + 0.5, 0));

					float projClipFade = stepVal.x * stepVal.y * stepVal.z * lookupHeight;
#if USE_CUSTOM_DECAL_LAYERS
					projClipFade *= fade;
#endif


#if CLAMP_SIDE_SURFACE
#ifdef UNITY_UV_STARTS_AT_TOP
					half3 n = normalize(cross(ddx(decalSpaceScenePos), ddy(decalSpaceScenePos) * _ProjectionParams.x));
#else
					half3 n = normalize(cross(ddx(decalSpaceScenePos), -ddy(decalSpaceScenePos) * _ProjectionParams.x));
#endif

					half angle = abs(dot(n, _DecalForwardDir));

					angle = angle > 0.1 ? 1 : 0;
					projClipFade *= angle;
					//return float4(saturate(angle), 0, 0, 1);

#endif


					float2 uv = decalSpaceScenePos.xz + 0.5;
					float2 uvMain = uv * _MainTex_ST.xy + _MainTex_ST.zw;
					float2 uvCutout = (decalSpaceScenePos.xz + 0.5) * _CutoutTex_ST.xy + _CutoutTex_ST.zw;

					half4 normAlpha = tex2D(_MainTex, uvMain);
					half4 res = 0;
					res.a = saturate(normAlpha.w * 2);
					if (res.a < 0.1 || projClipFade < 0.1) discard;

					normAlpha.xy = normAlpha.xy * 2 - 1;
					float3 normal = normalize(float3(normAlpha.x, 1, normAlpha.y));

					half3 mask = tex2D(_CutoutTex, uvCutout).xyz;
					half cutout = 0.5 + UNITY_ACCESS_INSTANCED_PROP(Props, _Cutout) * i.color.a * 0.5;

					half alphaMask = saturate((mask.r - (cutout * 2 - 1)) * 20) * res.a;
					half colorMask = saturate((mask.r - (cutout * 2 - 1)) * 5) * res.a;
					res.a = alphaMask;
					res.a = saturate(res.a * projClipFade);


					float intencity = UNITY_ACCESS_INSTANCED_PROP(Props, _LightIntencity);
					float light = max(0.001, dot(normal, normalize(_SunPos.xyz)));
					light = pow(light, 150) * 3 * intencity;
					light *= (1 - mask.z * colorMask);

					float4 tintColor = UNITY_ACCESS_INSTANCED_PROP(Props, _TintColor);
					#if !UNITY_COLORSPACE_GAMMA
							tintColor = tintColor * 1.35;
					#endif
					res.rgb = lerp(tintColor.rgb, tintColor.rgb * 0.25, mask.z * colorMask) + light;


					half fresnel = (1 - dot(normal, normalize(i.viewDir)));
					fresnel = pow(fresnel + 0.1, 5);

					UNITY_APPLY_FOG_COLOR(i.fogCoord, res, half4(0.5, 0.5, 0.5, 0.5));
					return saturate(lerp(0.5, res, res.a * tintColor.a));

				}

			ENDCG
	}

	}

}
