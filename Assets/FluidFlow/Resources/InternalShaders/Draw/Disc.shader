Shader "Hidden/FluidFlow/Draw/Disc"
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

			// Disc
			float3 _FF_Position;
			float3 _FF_Normal;
			float _FF_Radius;
			float _FF_ThicknessInv;

			float distance(v2f_draw i)
			{
				float3 v = i.vertex - _FF_Position;
				float distPlane = dot(v, _FF_Normal);
				float distCenter = length(v - _FF_Normal * distPlane);
				return abs(distPlane) * _FF_ThicknessInv + (distCenter > _FF_Radius);
			}
			ENDCG
		}
	}
}