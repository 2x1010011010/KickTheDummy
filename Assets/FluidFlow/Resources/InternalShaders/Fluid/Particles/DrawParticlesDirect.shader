Shader "Hidden/FluidFlow/Fluid/Particles/DrawParticlesDirect"
{
    SubShader
    {
        Cull Off
        ZTest ALWAYS
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // multiple rendertarget (MRT) support
            #pragma multi_compile_local __ FF_TARGET_POSITION_ONLY FF_TARGET_COLOR_ONLY
            #pragma multi_compile_local __ FF_PARTICLETEX_SIGNED

            #include "Particle.cginc"
            #include "../../FluidFlow.cginc"
           
            struct appdata
			{
				float2 vertex : POSITION;
                float4 particle : TEXCOORD0;
                float4 color : COLOR0;
			};

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 particle : TEXCOORD0;
                float4 color : COLOR;
            };

            struct FragmentOutput
            {
#if FF_TARGET_POSITION_ONLY
                float4 particle : SV_Target0;
#elif FF_TARGET_COLOR_ONLY
                float4 color : SV_Target0;
#else
                float4 particle : SV_Target0;
                float4 color : SV_Target1;
#endif
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = ProjectUVToClipSpace(v.vertex);
                o.particle = v.particle;
                o.color = v.color;
                return o;
            }

            FragmentOutput frag (v2f i)
            {
                FragmentOutput o;
#if !FF_TARGET_COLOR_ONLY
                o.particle = PackParticle(i.particle.xy, i.particle.z, clamp(i.particle.w, 0, 1));
#endif
#if !FF_TARGET_POSITION_ONLY
                o.color = i.color;
#endif
                return o;
            }

            ENDCG
        }
    }
}
