
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    public static class ParticlesExtension
    {
        public static readonly ShaderPropertyIdentifier SeedPropertyID = "_FF_Seed";
        public static readonly ShaderPropertyIdentifier ProbabilityPropertyID = "_FF_Probability";
        public static readonly ShaderPropertyIdentifier SpeedPropertyID = "_FF_Speed";
        public static readonly ShaderPropertyIdentifier AmountPropertyID = "_FF_Amount";

        private static void SetPositionOnly(Material material, bool positionOnly) => material.SetKeyword("FF_TARGET_POSITION_ONLY", positionOnly);
        private static void SetColorOnly(Material material, bool colorOnly) => material.SetKeyword("FF_TARGET_COLOR_ONLY", colorOnly);
        public static readonly MaterialCache ParticleDrawUV = new MaterialCache(InternalShaders.RootPath + "/Fluid/Particles/ProjectParticles", InternalShaders.SetSecondaryUV, SetPositionOnly, SetColorOnly, FFParticleUtil.SetPositionTexSigned);
        public static PerRenderTargetVariant DrawParticleVariant(bool onlyPos, bool onlyColor, bool signed) => new PerRenderTargetVariant(ParticleDrawUV, Utility.SetBit(1, onlyPos) + Utility.SetBit(2, onlyColor) + Utility.SetBit(3, signed));
        public static readonly MaterialCache ParticleDrawDirect = new MaterialCache(InternalShaders.RootPath + "/Fluid/Particles/DrawParticlesDirect", SetPositionOnly, SetColorOnly, FFParticleUtil.SetPositionTexSigned);
        public static Material DrawParticlesDirect(bool onlyPos, bool onlyColor, bool signed) => ParticleDrawDirect.Get(Utility.SetBit(0, onlyPos) + Utility.SetBit(1, onlyColor) + Utility.SetBit(2, signed));

        private static RenderTargetBinding MRTBindingCache = new RenderTargetBinding(
                    new RenderTargetIdentifier[] { new RenderTargetIdentifier(), new RenderTargetIdentifier() },    // placeholder
                    new RenderBufferLoadAction[] { RenderBufferLoadAction.Load, RenderBufferLoadAction.Load },
                    new RenderBufferStoreAction[] { RenderBufferStoreAction.Store, RenderBufferStoreAction.Store },
                    new RenderTargetIdentifier(0),
                    RenderBufferLoadAction.Load,
                    RenderBufferStoreAction.DontCare);

        private static readonly RenderBuffer[] MRTBufferCache = new RenderBuffer[2];

        public static void ProjectParticles(this FFSimulatorParticle sim, FFProjector projector, float speed, float amount, Color color, float probability = .5f, int rectSize = 4)
        {
            sim.ProjectParticles(projector, Vector2.one * speed, Vector2.one * amount, color, probability, rectSize);
        }

        public static void ProjectParticles(this FFSimulatorParticle sim, FFProjector projector, Vector2 speedRange, Vector2 amountRange, Color color, float probability = .5f, int rectSize = 4)
        {
            Shader.SetGlobalFloat(SeedPropertyID, Random.value * 1000);
            Shader.SetGlobalFloat(ProbabilityPropertyID, probability);
            Shader.SetGlobalColor(InternalShaders.ColorPropertyID, color);
            Shader.SetGlobalVector(SpeedPropertyID, speedRange);
            Shader.SetGlobalVector(AmountPropertyID, amountRange);

            var cb = Shared.CommandBuffer();
            cb.SetViewProjectionMatrices(Matrix4x4.identity, projector);
            var particleData = sim.Particles;
            cb.GetTemporaryRT(0, particleData.Size.x, particleData.Size.y, 1, FilterMode.Point, RenderTextureFormat.Depth);
            var viewportRect = particleData.Request(Vector2Int.one * rectSize);
            if (sim.UseMRT && InternalShaders.SupportedRenderTargetCount >= 2) {
                MRTBindingCache.colorRenderTargets[0] = particleData.PositionTexture;
                MRTBindingCache.colorRenderTargets[1] = particleData.ColorTexture;
                cb.SetRenderTarget(MRTBindingCache);
                cb.SetViewport(viewportRect);
                cb.ClearRenderTarget(true, false, Color.clear);
                cb.DrawRenderTargets(sim.GravityMap.Canvas.Surfaces, DrawParticleVariant(false, false, particleData.PositionTexSigned), true);
            } else {
                cb.SetRenderTarget(particleData.PositionTexture, new RenderTargetIdentifier(0));
                cb.SetViewport(viewportRect);
                cb.ClearRenderTarget(true, false, Color.clear);
                cb.DrawRenderTargets(sim.GravityMap.Canvas.Surfaces, DrawParticleVariant(true, false, particleData.PositionTexSigned), true);

                cb.SetRenderTarget(particleData.ColorTexture, new RenderTargetIdentifier(0));
                cb.SetViewport(viewportRect);
                cb.ClearRenderTarget(true, false, Color.clear);
                cb.DrawRenderTargets(sim.GravityMap.Canvas.Surfaces, DrawParticleVariant(false, true, particleData.PositionTexSigned), true);
            }
            cb.ReleaseTemporaryRT(0);
            Graphics.ExecuteCommandBuffer(cb);
            cb.Clear();
            sim.ResetTimeout();
        }

        public static void AddParticle(this FFSimulatorParticle sim, Vector2 uv, float amount, float speed, Color color)
        {
            var particles = new NativeArray<Particle>(1, Allocator.Temp, NativeArrayOptions.ClearMemory);
            particles[0] = new Particle() { Position = uv, Amount = amount, Speed = speed, Color = color };
            sim.AddParticles(particles);
            particles.Dispose();
        }

        public static bool AddParticle(this FFSimulatorParticle sim, Transform target, int submesh, Vector2 uv, float amount, float speed, Color color)
        {
            // transform uv to FFCanvas texture atlas
            if (sim.GravityMap.Canvas.AtlasTransformUV(target, submesh, uv, out var transformedUV)) {
                var particles = new NativeArray<Particle>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                particles[0] = new Particle() { Position = transformedUV, Amount = amount, Speed = speed, Color = color };
                sim.AddParticles(particles);
                particles.Dispose();
                return true;
            }
            return false;
        }

        // only works with MeshColliders
        public static bool AddParticle(this FFSimulatorParticle sim, RaycastHit hit, float amount, float speed, Color color)
        {
            if (sim.GravityMap.Canvas.TryGetCanvasUV(hit, out var uv)) {
                var particles = new NativeArray<Particle>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                particles[0] = new Particle() { Position = uv, Amount = amount, Speed = speed, Color = color };
                sim.AddParticles(particles);
                particles.Dispose();
                return true;
            }
            return false;
        }

        public static void AddParticles(this FFSimulatorParticle sim, NativeArray<Particle> particles)
        {
            var mesh = ParticleDrawMesh.Instance();
            var buffer = new NativeArray<ParticleDrawMesh.Particle>(particles.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var particleData = sim.Particles;
            var texelSize = particleData.PositionTexture.texelSize;
            for (var i = 0; i < particles.Length; i++) {
                var rect = particleData.Request(Vector2Int.one);
                buffer[i] = new ParticleDrawMesh.Particle(new Vector2((rect.xMin + .5f) * texelSize.x, (rect.yMin + .5f) * texelSize.y), particles[i]);
            }
            mesh.SetData(buffer);
            buffer.Dispose();

            if (sim.UseMRT && InternalShaders.SupportedRenderTargetCount >= 2) {
                MRTBufferCache[0] = particleData.PositionTexture.colorBuffer;
                MRTBufferCache[1] = particleData.ColorTexture.colorBuffer;
                Graphics.SetRenderTarget(MRTBufferCache, particleData.PositionTexture.depthBuffer);
                DrawParticlesDirect(false, false, particleData.PositionTexSigned).SetPass(0);
                mesh.DrawNow();
            } else {
                Graphics.SetRenderTarget(particleData.PositionTexture);
                DrawParticlesDirect(true, false, particleData.PositionTexSigned).SetPass(0);
                mesh.DrawNow();
                Graphics.SetRenderTarget(particleData.ColorTexture);
                DrawParticlesDirect(false, true, particleData.PositionTexSigned).SetPass(0);
                mesh.DrawNow();
            }
            sim.ResetTimeout();
        }

        class ParticleDrawMesh : System.IDisposable
        {
            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
            public struct Particle
            {
                public Vector2 Position;
                public Color ParticleColor;
                public Vector2 ParticlePosition;
                public float Amount;
                public float Speed;

                public Particle(Vector2 position, FluidFlow.Particle data)
                {
                    Position = position;
                    ParticleColor = data.Color;
                    ParticlePosition = data.Position;
                    Amount = data.Amount;
                    Speed = data.Speed;
                }
            }

            private int Capacity;
            private readonly Mesh Mesh;
            private const MeshUpdateFlags UpdateFlags = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds;

            private static ParticleDrawMesh instance = null;

            public static ParticleDrawMesh Instance()
            {
                if (instance == null) {
                    instance = new ParticleDrawMesh(64);
                    Application.quitting += instance.Dispose;
                }
                return instance;
            }

            public ParticleDrawMesh(int capacity)
            {
                Mesh = new Mesh();
                SetCapacity(capacity);
            }

            private void SetCapacity(int capacity)
            {
                Capacity = capacity;
                Mesh.Clear();
                Mesh.SetVertexBufferParams(capacity, new VertexAttributeDescriptor[] {
                    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2),
                    new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 4),
                });
                Mesh.SetIndexBufferParams(capacity, IndexFormat.UInt16);
                var indices = new NativeArray<ushort>(capacity, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                for (var i = 0; i < capacity; i++)
                    indices[i] = (ushort)i;
                Mesh.SetIndexBufferData(indices, 0, 0, Capacity, UpdateFlags);
                indices.Dispose();
                Mesh.subMeshCount = 1;
                Mesh.SetSubMesh(0, new SubMeshDescriptor(0, Capacity, MeshTopology.Points), UpdateFlags);
            }

            public void SetData(NativeArray<Particle> particles)
            {
                var count = particles.Length;
                if (count > Capacity)
                    SetCapacity(count);
                Mesh.SetVertexBufferData(particles, 0, 0, count, 0, UpdateFlags);
                var submeshDescr = new SubMeshDescriptor(0, count, MeshTopology.Points) {
                    bounds = new Bounds(Vector3.zero, Vector3.one),
                    firstVertex = 0,
                    vertexCount = count
                };
                Mesh.SetSubMesh(0, submeshDescr, UpdateFlags);
            }

            public void DrawNow()
            {
                Graphics.DrawMeshNow(Mesh, Matrix4x4.identity, 0);
            }

            public void Dispose()
            {
                Object.Destroy(Mesh);
            }
        }
    }
}
