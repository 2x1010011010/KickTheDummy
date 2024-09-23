Shader "Hidden/FluidFlow/Draw/Sphere"
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

			#pragma vertex vert_draw
			#pragma fragment frag_draw

			#pragma multi_compile_local __ FF_UV1
			#pragma multi_compile_local __ FF_FLUID

			#include "Draw.cginc"

			float3 _FF_Position;
			float _FF_RadiusInv;

			float distance(v2f_draw i)
			{
				return length(i.vertex - _FF_Position) * _FF_RadiusInv;
			}
			ENDCG
		}
	}
}