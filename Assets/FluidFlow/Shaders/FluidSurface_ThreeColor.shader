Shader "FluidFlow/Fluid Three Color (Surface Shader)"
{
	Properties
	{
		[Header(Main)]
		_Color("Color", Color) = (1,1,1,1)
		[NoScaleOffset] _MainTex("Albedo (RGB)", 2D) = "white" {}
		[MaterialToggle] _DrawMain("Draw On Main", Float) = 0

		_Smoothness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		[NoScaleOffset] [Normal] _NormalTex("Normal", 2D) = "bump" {}
		[MaterialToggle] _DrawNormal("Draw On Normal", Float) = 0
		_NormalScale("Normal Scale", Float) = 1
		

		[Header(Fluid)] 
		_FluidScale("Fluid Scale", Float) = 1
		_FluidNoiseScale("Fluid Noise Scale", Float) = 1
		_FluidNoiseAmount("Fluid Noise Amount", Float) = 1
		_FluidColorScatter("Fluid Color Scatter", Float) = 1
		_FluidMinColorScale("Fluid Min Color Scale", Float) = .05

		[Header(Red)] 
		_FluidColorR("Fluid Color", Color) = (1,1,1,1)
		_FluidSmoothnessR("Fluid Smoothness", Range(0,1)) = 0.5
		_FluidMetallicR("Fluid Metallic", Range(0,1)) = 0.0
		_FluidNormalR("Fluid Normal Scale", Float) = 1

		[Header(Green)]
		_FluidColorG("Fluid Color G", Color) = (1,1,1,1)
		_FluidSmoothnessG("Fluid Smoothness", Range(0,1)) = 0.5
		_FluidMetallicG("Fluid Metallic", Range(0,1)) = 0.0
		_FluidNormalG("Fluid Normal Scale", Float) = 1

		[Header(Blue)]
		_FluidColorB("Fluid Color B", Color) = (1,1,1,1)
		_FluidSmoothnessB("Fluid Smoothness", Range(0,1)) = 0.5
		_FluidMetallicB("Fluid Metallic", Range(0,1)) = 0.0
		_FluidNormalB("Fluid Normal Scale", Float) = 1

		// will be set automatically by the FFCanvas (if '_FluidTex' is specified as a texture channel)
		[HideInInspector] _FluidTex("Fluid", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#include "UnityCG.cginc"
			
		// helper functions for writing custom fluid flow surface shaders
		#include "FFShaderUtil.cginc"

		#pragma target 3.0
		#pragma surface surf Standard fullforwardshadows vertex:vert
		// needed so fluid flow shader properties are initialized ^

		// needed when secondary uv is used for drawing/ fluid simulation (keyword set automatically by FFCanvas)
		#pragma multi_compile_local __ FF_UV1

		struct Input
		{
			float2 uv_MainTex;
			// macro adds default input variables, necessary for drawing on the model
			FF_SURFACE_INPUT
		};

		// default shader stuff..
		fixed4 _Color;
		sampler2D _MainTex;
		half _DrawMain;
		sampler2D _NormalTex;
		half _DrawNormal;
		half _NormalScale;
		half _Smoothness;
		half _Metallic;

		// properties defining the look of the fluid
		sampler2D _FluidTex;
		float4 _FluidTex_ST;
		float4 _FluidTex_TexelSize;
		half _FluidScale;
		half _FluidNoiseScale;
		half _FluidNoiseAmount;
		half _FluidColorScatter;
		half _FluidMinColorScale;

		half4 _FluidColorR;
		half _FluidSmoothnessR;
		half _FluidMetallicR;
		half _FluidNormalR;

		half4 _FluidColorG;
		half _FluidSmoothnessG;
		half _FluidMetallicG;
		half _FluidNormalG;

		half4 _FluidColorB;
		half _FluidSmoothnessB;
		half _FluidMetallicB;
		half _FluidNormalB;

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			// macro for initializing internal input variables
			FF_INITIALIZE_OUTPUT(v, o, _FluidTex_ST);
		}

		inline float3 FFUnpackFluidNormal(in Input IN, in sampler2D fluidTex, in float2 texelSize, in float3 scale)
		{
			float2 uv = IN.FF_UV_NAME;
#if defined(FF_UV1)
			float2 up = IN.FF_TRANSFORMATION_NAME.xz * texelSize;
			float2 right = IN.FF_TRANSFORMATION_NAME.yw * texelSize;
#else
			float2 up = float2(0, texelSize.y);
			float2 right = float2(texelSize.x, 0);
#endif
			half4 fl = tex2D(fluidTex, uv + right);
			half4 fr = tex2D(fluidTex, uv - right);
			half4 fu = tex2D(fluidTex, uv + up);
			half4 fd = tex2D(fluidTex, uv - up);
			half l = dot(fl.FF_FLUID_COLOR, scale) * fl.FF_FLUID_AMOUNT;
			half r = dot(fr.FF_FLUID_COLOR, scale) * fr.FF_FLUID_AMOUNT;
			half u = dot(fu.FF_FLUID_COLOR, scale) * fu.FF_FLUID_AMOUNT;
			half d = dot(fd.FF_FLUID_COLOR, scale) * fd.FF_FLUID_AMOUNT;
			return normalize(float3(r - l, d - u, 1));
		}


		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			// when drawing on a texture channel, it has to be sampled with the atlas transformed uv (-> automatically calculated in IN.FF_UV_NAME)
			float2 mainUV = _DrawMain > 0 ? IN.FF_UV_NAME : IN.uv_MainTex;
			float2 normalUV = _DrawNormal > 0 ? IN.FF_UV_NAME : IN.uv_MainTex;

			// default shader stuff..
			fixed4 main = tex2D(_MainTex, mainUV) * _Color;
			float3 normal = UnpackNormal(tex2D(_NormalTex, normalUV));
			normal = normalize(float3(normal.xy * _NormalScale, normal.z));
			normal = _DrawNormal > 0 ? FF_TRANSFORM_NORMAL(IN, normal) : normal;

			// sample fluid
			float4 fluid = tex2D(_FluidTex, FFDistortUV(IN.FF_UV_NAME, _FluidNoiseScale, _FluidNoiseAmount * _FluidTex_TexelSize.x));

			// approximate fluid normal
			float3 fluidNormal = FFUnpackFluidNormal(IN, _FluidTex, _FluidTex_TexelSize.xy, float3(_FluidNormalR, _FluidNormalG, _FluidNormalB));

			// influence of fluid on the current pixel
			float fluidHeight = min(fluid.FF_FLUID_AMOUNT * _FluidScale, 1);

			float total = fluid.FF_FLUID_COLOR.r + fluid.FF_FLUID_COLOR.g + fluid.FF_FLUID_COLOR.b;
			float3 mask = fluid.FF_FLUID_COLOR * (total > 0 ? 1.0f / total : 1);
			// half3 mask = total > 1 ? fluid.FF_FLUID_COLOR / total : fluid.FF_FLUID_COLOR;
			float4 fluidColor = _FluidColorR * mask.r + _FluidColorG * mask.g + _FluidColorB * mask.b; 
			fluidColor.rgb = fluidColor.rgb * lerp(_FluidMinColorScale, 1, pow(.5f, fluid.FF_FLUID_AMOUNT * _FluidColorScatter));

			o.Albedo = lerp(main, fluidColor, fluidHeight * fluidColor.a);
			// partially keep underlying surface normal
			o.Normal = lerp(normal, fluidNormal, fluidHeight * .7f);
			o.Metallic = lerp(_Metallic, dot(float3(_FluidMetallicR, _FluidMetallicG, _FluidMetallicB), mask), fluidHeight);
			o.Smoothness = lerp(_Smoothness, dot(float3(_FluidSmoothnessR, _FluidSmoothnessG, _FluidSmoothnessB), mask), fluidHeight);
		}
		ENDCG
	}
	FallBack "Diffuse"
}