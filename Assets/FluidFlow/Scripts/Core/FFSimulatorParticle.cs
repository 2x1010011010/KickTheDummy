using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace FluidFlow
{
    public struct Particle
    {
        public Vector2 Position;
        public float Amount;
        public float Speed;
        public Color Color;
    }

    public class FFSimulatorParticle : MonoBehaviour
    {
        [Tooltip("Target Fluid Canvas.")]
        [SerializeField] private Tracked<FFGravityMap> gravityMap;
        public FFGravityMap GravityMap {
            get => gravityMap.Target;
            set => gravityMap.Target = value;
        }

        [Tooltip("Reference to the TextureChannel the particles are painted on.")]
        public TextureChannelReference TextureChannelReference = TextureChannelReference.Indirect("Fluid");

        [Tooltip("Keep updating fluid simulation, when no RenderTarget is visible?")]
        public bool UpdateInvisible = false;

        [Tooltip("Halt simulation after inactivity?")]
        public bool UseTimeout = true;

        [Min(0)]
        [Tooltip("Inactivity time (seconds) after which the simulation is halted.")]
        public float Timeout = 3;


        [Header("Fluid")]
        [Tooltip("Set when or how often the fluid simulation is updated.")]
        public Updater FluidUpdater = new Updater(Updater.Mode.FIXED, .025f);

        [Tooltip("Internal size of the textures used for storing particle data.")]
        public int ParticleTextureSize = 64;

        [Tooltip("For simulation, the fluid texture is written to a back-buffer. Also simulate when reading back to front-buffer, or just copy contents? More computation, but fluid will flow faster.")]
        public bool DoubleSimulationStep = true;


        [Header("Particles")]
        [Tooltip("Fluid amount removed from particles in each update cycle.")]
        public float DecayAmountPerStep = .005f;

        [Tooltip("Pixel size of each particle.")]
        public float ParticleSize = 1.5f;

        [Tooltip("Color influence of each particle.")]
        public float ParticleStrength = .5f;

        [Tooltip("Fade amount of particle."), Range(0f, 1f)]
        public float ParticleSmoothness = 1;

        [Tooltip("Darken particle color based on fluid amount.")]
        public float ColorScatter = 1f;

        [Tooltip("Minimum particle color darken factor."), Range(0f, 1f)]
        public float MinColorScale = .1f;

        [Tooltip("Destroy particles with low gravitational pull (flat surface) to prevent massive fluid accumulation."), Range(0f, 1f)]
        public float MinimumGravityPull = .08f;

        [Header("Advanced")]
        [Tooltip("Use MultipleRenderTargets? Reduces draw calls, but requires hardware support.")]
        public bool UseMRT = true;

        private bool initialized = false;
        public bool Initialized { get => initialized && GravityMap && GravityMap.Initialized; }
        private TextureChannel targetTextureChannel;
        private FFParticleUtil.Data particleData;
        public FFParticleUtil.Data Particles { get => particleData; }

        private float remainingSimulationTime = 0;

        public void Initialize()
        {
            if (initialized || !GravityMap || !GravityMap.Initialized)
                return;
            if (!TextureChannelReference.IsValid) {
                Debug.LogWarning("FluidFlow: Unable to initialize. TextureChannelReference is invalid.");
                return;
            }
            var positionFormat = FFParticleUtil.ParticlePositionFormat;
            var colorFormat = InternalTextures.ColorFormatRGBA;
            if (positionFormat == GraphicsFormat.None || colorFormat == GraphicsFormat.None) {
                Debug.LogWarning("FluidFlow: Unable to initialize. Required RenderTexture format not supported on the current platform.");
                return;
            }
            targetTextureChannel = TextureChannelReference.Resolve();
            GravityMap.Canvas.OnTextureChannelUpdated.AddListener(OnTextureChannelUpdated);
            particleData = new FFParticleUtil.Data(Vector2Int.one * ParticleTextureSize, positionFormat, colorFormat);
            initialized = true;
        }

        private void OnTextureChannelUpdated(TextureChannel channel)
        {
            if (channel == targetTextureChannel)
                ResetTimeout();
        }

        public void Uninitialize()
        {
            if (!initialized)
                return;
            GravityMap.Canvas.OnTextureChannelUpdated.RemoveListener(OnTextureChannelUpdated);
            particleData.Dispose();
            initialized = false;
        }

        /// <summary>
        /// Manually reset the simulation timeout timers.
        /// </summary>
        public void ResetTimeout()
        {
            remainingSimulationTime = Timeout;
        }

        void Simulate()
        {
            if (!initialized || !GravityMap.Initialized)
                return;
            using (var paintScope = GravityMap.Canvas.BeginPaintScope(targetTextureChannel)) {
                if (paintScope.IsValid) {
                    var flowTex = GravityMap.FlowTexture;
                    var drawParticlesMat = FFParticleUtil.FluidDrawParticlesVariant(particleData.PositionTexSigned);
                    var simulateParticlesMat = FFParticleUtil.FluidSimulateParticlesVariant(flowTex, particleData.PositionTexSigned);

                    using (var tmp = new TmpRenderTexture(particleData.PositionTexture.descriptor)) {
                        // simulation globals
                        Shader.SetGlobalTexture(FFFlowTextureUtil.FlowTexPropertyID, flowTex);
                        Shader.SetGlobalFloat(FFParticleUtil.AmountPropertyID, DecayAmountPerStep);
                        Shader.SetGlobalFloat(FFParticleUtil.MinGravityPropertyID, MinimumGravityPull);
                        // draw globals
                        Shader.SetGlobalVector(InternalShaders.TexelSizePropertyID, paintScope.Target.GetTexelSize());
                        Shader.SetGlobalTexture(FFParticleUtil.ParticleColorsTexPropertyID, particleData.ColorTexture);
                        Shader.SetGlobalFloat(FFParticleUtil.ParticleSizePropertyID, ParticleSize);
                        Shader.SetGlobalFloat(FFParticleUtil.ParticleStrengthPropertyID, ParticleStrength);
                        Shader.SetGlobalFloat(FFParticleUtil.ParticleSmoothnessPropertyID, ParticleSmoothness > 0 ? 1f / Mathf.Clamp01(ParticleSmoothness) : float.MaxValue);
                        Shader.SetGlobalFloat(FFParticleUtil.FluidColorScatterPropertyID, ColorScatter);
                        Shader.SetGlobalFloat(FFParticleUtil.FluidMinColorScalePropertyID, MinColorScale);

                        Graphics.Blit(particleData.PositionTexture, tmp, simulateParticlesMat);

                        Shader.SetGlobalTexture(FFParticleUtil.ParticlesTexPropertyID, particleData.PositionTexture);
                        Graphics.SetRenderTarget(paintScope.Target);
                        drawParticlesMat.SetPass(0);
                        Graphics.DrawProceduralNow(MeshTopology.Triangles, particleData.Size.x * particleData.Size.y * 3);

                        if (DoubleSimulationStep) {
                            Shader.SetGlobalTexture(FFParticleUtil.ParticlesTexPropertyID, tmp);
                            drawParticlesMat.SetPass(0);
                            Graphics.DrawProceduralNow(MeshTopology.Triangles, particleData.Size.x * particleData.Size.y * 3);

                            Graphics.Blit(tmp, particleData.PositionTexture, simulateParticlesMat); // also simulate on back copy
                        } else {
                            InternalShaders.CopyTexture(tmp, particleData.PositionTexture);
                        }
                    }
                }
            }
        }

        public void ClearAllParticles()
        {
            particleData.ClearAllParticles();
            remainingSimulationTime = 0;
        }

        private void Awake()
        {
            gravityMap.OnBeforeChanged += target => {
                if (target) {
                    target.OnInitialized.RemoveListener(Initialize);
                    target.OnUninitialized.RemoveListener(Uninitialize);
                }
                Uninitialize();
            };
            gravityMap.OnAfterChanged += target => {
                if (target) {
                    target.OnInitialized.AddListener(Initialize);
                    target.OnUninitialized.AddListener(Uninitialize);
                }
                Initialize();
            };
            gravityMap.Initialize();
            FluidUpdater.AddListener(Simulate);
            FluidUpdater.RandomizeTimer();
        }

        private void LateUpdate()
        {
            if (!Initialized)
                return;
            if (UpdateInvisible || GravityMap.Canvas.IsVisible()) {
                if (!UseTimeout || remainingSimulationTime > 0) {
                    FluidUpdater.Update();
                    remainingSimulationTime -= Time.deltaTime;
                }
            }
        }

        private void OnDestroy()
        {
            gravityMap.Reset();
        }
    }

    public static class FFParticleUtil
    {
        private static readonly GraphicsFormat[] ParticlePositionFormats = new GraphicsFormat[] {
            GraphicsFormat.R16G16B16A16_UNorm,
            GraphicsFormat.R16G16B16A16_SNorm,
            GraphicsFormat.R32G32B32A32_SFloat
        };
        private static GraphicsFormat particlePositionFormat = GraphicsFormat.None;
        public static GraphicsFormat ParticlePositionFormat {
            get {
                if (particlePositionFormat == GraphicsFormat.None)
                    particlePositionFormat = ParticlePositionFormats.GetSupportedFormat();
                return particlePositionFormat;
            }
        }

        public class Data : System.IDisposable
        {
            public readonly Vector2Int Size;
            public readonly RenderTexture PositionTexture;
            public readonly RenderTexture ColorTexture;
            public readonly bool PositionTexSigned;
            private Vector3Int index;   // x, y: position in particle texture; z: next unused row

            public Data(Vector2Int size, GraphicsFormat positionFormat, GraphicsFormat colorFormat)
            {
                index = Vector3Int.zero;
                Size = size;
                PositionTexSigned = GraphicsFormatUtility.IsSignedFormat(positionFormat);
                PositionTexture = InternalTextures.CreateRenderTexture(positionFormat, size);

                PositionTexture.filterMode = FilterMode.Point;
                // PositionTexture.DebugTexture();
                ColorTexture = InternalTextures.CreateRenderTexture(colorFormat, size);
                // ColorTexture.DebugTexture();
                ColorTexture.filterMode = FilterMode.Point;
            }

            public Rect Request(Vector2Int size)
            {
                // try and find the next free rect in particle buffer that has been used least recently
                var minPos = Size - size;
                if (minPos.x < 0 || minPos.y < 0) {
                    Debug.LogWarning("FluidFlow: Requested allocation larger than particle data buffer.");
                    return new Rect(0, 0, Size.x, Size.y);
                }
                if (index.x + size.x > Size.x) {    // too wide for this row -> move to next row
                    index.x = 0;
                    index.y = index.z;
                }
                if (index.y + size.y > Size.y) {    // end of texture -> cycle back to beginning
                    index.x = 0;
                    index.y = 0;
                    index.z = 0;
                }
                var rect = new Rect(index.x, index.y, size.x, size.y);
                index.x += size.x;
                index.z = Mathf.Max(index.z, index.y + size.y); // next free row
                return rect;
            }

            public void ClearAllParticles()
            {
                Graphics.SetRenderTarget(PositionTexture);
                GL.Clear(false, true, Color.clear);
                index = Vector3Int.zero;
            }

            public void Dispose()
            {
                if (PositionTexture.IsCreated())
                    PositionTexture.Release();
                if (ColorTexture.IsCreated())
                    ColorTexture.Release();
            }
        }

        public static readonly ShaderPropertyIdentifier ParticlesTexPropertyID = "_FF_ParticlesTex";
        public static readonly ShaderPropertyIdentifier ParticleColorsTexPropertyID = "_FF_ParticleColorsTex";
        public static readonly ShaderPropertyIdentifier ParticleSizePropertyID = "_FF_ParticleSize";
        public static readonly ShaderPropertyIdentifier ParticleStrengthPropertyID = "_FF_ParticleStrength";
        public static readonly ShaderPropertyIdentifier ParticleSmoothnessPropertyID = "_FF_ParticleSmoothness";
        public static readonly ShaderPropertyIdentifier FluidColorScatterPropertyID = "_FF_FluidColorScatter";
        public static readonly ShaderPropertyIdentifier FluidMinColorScalePropertyID = "_FF_FluidMinColorScale";
        public static readonly ShaderPropertyIdentifier MinGravityPropertyID = "_FF_MinGravity";
        public static readonly ShaderPropertyIdentifier AmountPropertyID = "_FF_Amount";

        public static void SetPositionTexSigned(Material material, bool signed) => material.SetKeyword("FF_PARTICLETEX_SIGNED", signed);

        public static readonly MaterialCache FluidDrawParticles = new MaterialCache(InternalShaders.RootPath + "/Fluid/Particles/DrawParticles", SetPositionTexSigned);
        public static Material FluidDrawParticlesVariant(bool floatingPoint) => FluidDrawParticles.Get(Utility.SetBit(0, floatingPoint));
        public static readonly MaterialCache FluidSimulateParticles = new MaterialCache(InternalShaders.RootPath + "/Fluid/Particles/SimulateParticles", FFFlowTextureUtil.SetFlowTexCompressed, SetPositionTexSigned);
        public static Material FluidSimulateParticlesVariant(RenderTexture rt, bool signed)
            => FluidSimulateParticles.Get(Utility.SetBit(0, FFFlowTextureUtil.IsCompressed(rt))
                                        | Utility.SetBit(1, signed));
    }
}
