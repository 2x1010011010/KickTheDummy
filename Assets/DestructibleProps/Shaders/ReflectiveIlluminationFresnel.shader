Shader "DestructibleProps/ReflectiveBumpedIllumFresnel" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
    _Shininess ("Shininess", Range (0.01, 1)) = 0.078125
    _ReflectVel ("Reflect Velocity", Range (0.01, 3)) = 0.5
    _RimPower ("Fresnel Falloff", Range(0.1, 3)) = 2
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
	#pragma surface surf BlinnPhong
	#pragma target 3.0
 
sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _Mask;
samplerCUBE _Cube;
 
float4 _Color;
float3 _IllumColor;
float3 _ReflectColor;
float _Shininess;
float _ReflectVel;
float _RimPower;
 
struct Input {
    float2 uv_MainTex;
    float3 worldRefl;
    float3 viewDir;
    INTERNAL_DATA
};
 
void surf (Input IN, inout SurfaceOutput o) {
    half4 tex = tex2D(_MainTex, IN.uv_MainTex);
    half4 mask = tex2D(_Mask, IN.uv_MainTex);
    half4 c = tex * _Color;
    
    o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
    
    float3 worldRefl = WorldReflectionVector (IN, o.Normal);
    half4 reflcol = texCUBE (_Cube, worldRefl);
    reflcol *= _ReflectVel * tex.a;
     float rim = 1.0 - saturate(dot(o.Normal, normalize(IN.viewDir)));
    rim = pow(rim, _RimPower);
    o.Emission = (reflcol.rgb * _ReflectColor.rgb * rim) + mask.rgb * _IllumColor;
    o.Albedo = c.rgb;
    o.Gloss = tex.a;
    o.Specular = _Shininess;
    o.Alpha = c.a;
}
ENDCG
}
 
FallBack "Reflective/Bumped Diffuse"
}