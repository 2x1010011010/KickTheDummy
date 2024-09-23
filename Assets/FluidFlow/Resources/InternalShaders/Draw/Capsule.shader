Shader "Hidden/FluidFlow/Draw/Capsule"
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
			float4 _FF_Line;
			float _FF_RadiusInv;

			float distance(v2f_draw i)
			{
				float3 proj = _FF_Position + clamp(dot(i.vertex - _FF_Position, _FF_Line.xyz) * _FF_Line.w, 0, 1) * _FF_Line.xyz;
				return length(proj - i.vertex) * _FF_RadiusInv;
			}
			ENDCG
		}
	}
}