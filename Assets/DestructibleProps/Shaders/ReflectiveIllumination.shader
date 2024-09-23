Shader "DestructibleProps/ReflectiveBumpedIllum" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
    _Shininess ("Shininess", Range (0.01, 1)) = 0.078125
    _ReflectVel ("Reflect Velocity", Range (0.01, 3)) = 0.5
    _ReflectColor("Reflection Color", Color) = (1,1,1,0.5)
    _IllumColor("Illumination Color", Color) = (0,0.5,0.7,0.5)
    _MainTex ("Base (RGB) Basic Illumination (A)", 2D) = "white" {}
  
    //_SecondTex ("Second Texture (RGBA)", 2D) = ""
	_Mask ("Illumination Mask (A)", 2D) = ""

	
    _Cube ("Reflection Cubemap", Cube) = "" { TexGen CubeReflect }
    _BumpMap ("Normalmap", 2D) = "bump" {}
}
 
SubShader {
    Tags { "RenderType"="Opaque" }
    Cull off
    LOD 400
	CGPROGRAM
	#pragma surface surf Lambert
	#pragma target 3.0
 
sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _Mask;
samplerCUBE _Cube;
 
float4 _Color;
float4 _IllumColor;
float4 _ReflectColor;
float _Shininess;
float _ReflectVel;
 
struct Input {
    float2 uv_MainTex;
    float2 uv_BumpMap;
    float2 uv_Mask;
    float3 worldRefl;
    INTERNAL_DATA
};
 
void surf (Input IN, inout SurfaceOutput o) {
    half4 tex = tex2D(_MainTex, IN.uv_MainTex);
    half4 mask = tex2D(_Mask, IN.uv_Mask);
    //half4 illum = tex2D(_Illum, IN.uv_MainTex);
    half4 c = tex * _Color;
    o.Albedo = c.rgb;
   
    o.Gloss = tex.a;
    o.Specular = _Shininess;
   
    o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
   
    float3 worldRefl = WorldReflectionVector (IN, o.Normal);
    half4 reflcol = texCUBE (_Cube, worldRefl);
    reflcol *= _ReflectVel * tex.a;
    o.Emission = reflcol.rgb * _ReflectColor.rgb + mask.rgb * _IllumColor;
    o.Alpha = reflcol.a * _ReflectColor.a;
}
ENDCG
}
 
FallBack "Reflective/Bumped Diffuse"
}