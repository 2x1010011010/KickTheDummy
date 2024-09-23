Shader "FluidFlow/Fluid (Surface Shader)"
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
		half _FluidSmoothness;
		half _FluidMetallic;
		half _FluidNormal;
		half _FluidColorScatter;
		half _FluidMinColorScale;

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			// macro for initializing internal input variables
			FF_INITIALIZE_OUTPUT(v, o, _FluidTex_ST);

			// move vertices in normal direction, to simulate thickness of fluid
			// float d = min(2, tex2Dlod(_FluidTex, float4(o.FF_UV_NAME.xy, 0, 0)).a) * .01;
            // v.vertex.xyz += v.normal * d;
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

			// sample fluid
			IN.FF_UV_NAME = FFDistortUV(IN.FF_UV_NAME, _FluidNoiseScale, _FluidNoiseAmount * _FluidTex_TexelSize.x);
			fixed4 fluid = tex2D(_FluidTex, IN.FF_UV_NAME);

			// approximate fluid normal, based on fluid height of neighbouring texels
			float3 fluidNormal = FF_UNPACK_FLUID_NORMAL(IN, _FluidTex, fluid.FF_FLUID_AMOUNT, _FluidNormal);

			// influence of fluid on the current pixel
			float fluidHeight = min(fluid.FF_FLUID_AMOUNT * _FluidScale, 1);
			float3 fluidColor = fluid.FF_FLUID_COLOR * lerp(_FluidMinColorScale, 1, pow(.5f, fluid.FF_FLUID_AMOUNT * _FluidColorScatter));	// FIXME: this is expensive

			// interpolate material properties depending on fluid influence on the current pixel
			o.Albedo = lerp(main, fluidColor, fluidHeight);
			// partially keep underlying surface normal
			o.Normal = lerp(normal, fluidNormal, fluidHeight * .7f);
			o.Metallic = lerp(_Metallic, _FluidMetallic, fluidHeight);
			o.Smoothness = lerp(_Smoothness, _FluidSmoothness, fluidHeight);
		}
		ENDCG
	}
	FallBack "Diffuse"
}