Shader "Hidden/FluidFlow/Fluid/Particles/ProjectParticles"
{
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           
            #pragma multi_compile_local __ FF_UV1

            // multiple rendertarget (MRT) support?
            #pragma multi_compile_local __ FF_TARGET_POSITION_ONLY FF_TARGET_COLOR_ONLY
            #pragma multi_compile_local __ FF_PARTICLETEX_SIGNED

            #include "Particle.cginc"
            #include "../../FluidFlow.cginc"
           
            struct appdata
			{
				float3 vertex : POSITION;
				
			#if defined(FF_UV1)
				float2 uv : TEXCOORD1;
            #else
                float2 uv : TEXCOORD0;
			#endif
			};

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
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
                float depth : SV_Depth;
            };

            float _FF_Seed;
            float4 _FF_Color;
            float _FF_Probability;
            float2 _FF_Speed;
            float2 _FF_Amount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(float4(v.vertex, 1));
                o.uv = v.uv;
                return o;
            }

            float noise2d(in float2 uv)
            {
	            return frac(abs(sin(dot(uv, float2(127.1f, 311.7f)))) * 43758.5453123f);
            }

            FragmentOutput frag (v2f i)
            {
                FragmentOutput o;
                float2 seed = i.uv.xy + _FF_Seed;
                if (noise2d(seed) > _FF_Probability)
                    discard;

#if !FF_TARGET_COLOR_ONLY
                float2 position = AtlasTransformUV(i.uv);
                float amount = lerp(_FF_Amount.x, _FF_Amount.y, noise2d(seed + 1));
                float speed = lerp(_FF_Speed.x, _FF_Speed.y, noise2d(seed + 2));
                o.particle = PackParticle(position, amount, clamp(speed, 0, 1));
#endif
#if !FF_TARGET_POSITION_ONLY
                o.color = _FF_Color;
#endif
                o.depth = i.vertex.z;
                return o;
            }
            ENDCG
        }
    }
}
