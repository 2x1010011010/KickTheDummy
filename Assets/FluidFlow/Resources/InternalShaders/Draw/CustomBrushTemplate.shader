// this is the name for finding the shader on the C# side
Shader "Hidden/FluidFlow/Draw/CustomBrush"
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

			// put necessary variables here
			// float3 _MyPositionVar;

			float distance(v2f_draw i)
			{
				// return the distance from 'i.vertex' to your shape here
				return 0;
			}
			ENDCG
		}
	}
}