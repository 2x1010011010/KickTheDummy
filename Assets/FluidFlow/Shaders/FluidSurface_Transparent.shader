Shader "FluidFlow/Fluid Transparent (Surface Shader)"
{
	Properties
	{
		[Header(Fluid)] 
		_FluidScale("Fluid Scale", Float) = 1
		_FluidNoiseScale("Fluid Noise Scale", Float) = 1
		_FluidNoiseAmount("Fluid Noise Amount", Float) = 1
		_FluidSmoothness("Fluid Smoothness", Range(0,1)) = 0.5
		_FluidMetallic("Fluid Metallic", Range(0,1)) = 0.0
		_FluidNormal("Fluid Normal Scale", Float) = 1
		_FluidColorScatter("Fluid Color Scatter", Float) = 1
		_FluidMinColorScale("Fluid Min Color Scale", Float) = .05
		// will be set automatically by the FFCanvas (if '_FluidTex' is specified as a texture channel)
		[HideInInspector] _FluidTex("Fluid", 2D) = "black" {}
	}
	SubShader
	{
		Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
		LOD 200

		CGPROGRAM
		#include "UnityCG.cginc"
			
		// helper functions for writing custom fluid flow surface shaders
		#include "FFShaderUtil.cginc"

		#pragma target 3.0
		#pragma surface surf Standard fullforwardshadows vertex:vert alpha:fade
		// needed so fluid flow shader properties are initialized ^

		// needed when secondary uv is used for drawing/ fluid simulation (keyword set automatically by FFCanvas)
		#pragma multi_compile_local __ FF_UV1

		struct Input
		{
			float2 uv_MainTex;
			// macro adds default input variables, necessary for drawing on the model
			FF_SURFACE_INPUT
		};

		// properties defining the look of the fluid
		sampler2D _FluidTex;
		float4 _FluidTex_TexelSize;
		float4 _FluidTex_ST;
		half _FluidScale;
		half _FluidNoiseScale;
		half _FluidNoiseAmount;
		half _FluidSmoothness;
		half _FluidMetallic;
		half _FluidNormal;
		half _FluidColorScatter;
		half _FluidMinColorScale;

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			// macro for initializing internal input variables
			FF_INITIALIZE_OUTPUT(v, o, _FluidTex_ST);
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			// sample fluid
			IN.FF_UV_NAME = FFDistortUV(IN.FF_UV_NAME, _FluidNoiseScale, _FluidNoiseAmount * _FluidTex_TexelSize.x);
			fixed4 fluid = tex2D(_FluidTex, IN.FF_UV_NAME);

			// approximate fluid normal
			float3 fluidNormal = FF_UNPACK_FLUID_NORMAL(IN, _FluidTex, fluid.FF_FLUID_AMOUNT, _FluidNormal);

			// influence of fluid on the current pixel
			float fluidHeight = min(fluid.FF_FLUID_AMOUNT * _FluidScale, 1);
			float3 fluidColor = fluid.FF_FLUID_COLOR * lerp(_FluidMinColorScale, 1, pow(.5f, fluid.FF_FLUID_AMOUNT * _FluidColorScatter));	// FIXME: this is expensive

			o.Albedo = fluidColor;
			o.Normal = fluidNormal;
			o.Metallic = _FluidMetallic;
			o.Smoothness = _FluidSmoothness;
			o.Alpha = fluidHeight;
		}
		ENDCG
	}
	FallBack "Diffuse"
}