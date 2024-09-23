Shader "Hidden/FluidFlow/Fluid/Particles/DrawParticles"
{
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha One
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_local __ FF_PARTICLETEX_SIGNED
            
            static const float4 DEGENERATE_POSITION = float4(-10, -10, -10, 0);

            static const int VERTEX_COUNT = 3;
            static const float4 vertexOffsets[VERTEX_COUNT] = {
                float4(-0.5f * sqrt(12), -1, 0, 0),
                float4(0.5f * sqrt(12), -1, 0, 0),
                float4(0, 2, 0, 0),
            };

            #include "Particle.cginc"
            #include "../../FluidFlow.cginc"

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR0;
                float2 offset : TEXCOORD0;
                float amount : TEXCOORD1;
            };

            sampler2D _FF_ParticlesTex;
            float4 _FF_ParticlesTex_TexelSize;
            sampler2D _FF_ParticleColorsTex;
            float4 _FF_TexelSize;

            float _FF_ParticleSize;
            float _FF_ParticleStrength;
            float _FF_ParticleSmoothness;
            float _FF_FluidColorScatter;
		    float _FF_FluidMinColorScale;

            v2f vert (uint vertexId : SV_VertexID)
            {
                v2f o;
                uint id = (uint)(vertexId / VERTEX_COUNT);
                int y = (int)(id * _FF_ParticlesTex_TexelSize.x);
                int x = id - (int) (y * _FF_ParticlesTex_TexelSize.z);
                float4 sample =  float4(float2(x, y) * _FF_ParticlesTex_TexelSize.xy, 0, 0);
                float4 p = tex2Dlod(_FF_ParticlesTex, sample);
                o.color = tex2Dlod(_FF_ParticleColorsTex, sample);
                o.amount = UnpackParticleAmount(p);
                
                o.offset = vertexOffsets[(int)vertexId - (int)id * VERTEX_COUNT].xy;
                float2 position = UnpackParticlePosition(p);
                o.vertex = p.z > 0 ?  ProjectUVToClipSpace(position + o.offset * _FF_TexelSize.xy * _FF_ParticleSize) : DEGENERATE_POSITION;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float o = dot(i.offset, i.offset);
                float t = clamp(lerp(_FF_ParticleSmoothness, 0, o), 0, 1);
                float fluidAmount = i.color.a * t * min(i.amount, 1);

                float3 fluidColor = i.color.rgb * lerp(_FF_FluidMinColorScale, 1, pow(.5f, fluidAmount * _FF_FluidColorScatter));
                return float4(fluidColor, fluidAmount * _FF_ParticleStrength);
            }
            ENDCG
        }
    }
}
