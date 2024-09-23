using UnityEngine;

namespace FluidFlow
{
    /// <summary>
    /// Adds fluid simulation to a specified TextureChannel of a FluidCanvas.
    /// </summary>
    public class FFSimulator : MonoBehaviour
    {
        #region Public Properties

        [Tooltip("Target Fluid Canvas.")]
        [SerializeField] private Tracked<FFGravityMap> gravityMap;  // tracks the reference, and automatically un/initializes
        public FFGravityMap GravityMap {
            get => gravityMap.Target;
            set => gravityMap.Target = value;
        }

        [Tooltip("Name of the TextureChannel being simulated.")]
        public TextureChannelReference TextureChannelReference = TextureChannelReference.Indirect("Fluid");

        [Tooltip("Keep updating fluid simulation, when no RenderTarget is visible?")]
        public bool UpdateInvisible = false;

        [Tooltip("Halt simulation after inactivity?")]
        public bool UseTimeout = true;

        [Min(0)]
        [Tooltip("Inactivity time (seconds) after which the simulation is halted.")]
        public float Timeout = 5;

        [Header("Fluid")]
        [Tooltip("Set when or how often the fluid simulation is updated.")]
        public Updater FluidUpdater = new Updater(Updater.Mode.FIXED, .025f);

        [Min(0)]
        [Tooltip("Amount of fluid retained in each pixel of the fluid texture depending on the angle of the surface.")]
        public float FluidRetainedFlat = 1.1f;

        [Min(0)]
        [Tooltip("Amount of fluid retained in each pixel of the fluid texture depending on the angle of the surface.")]
        public float FluidRetainedSteep = 0.9f;

        [Tooltip("Fluid color influence of inflowing fluid compared to fluid color in texel.")]
        public float InflowColorInfluence = 2f;

        [Tooltip("For simulation, the fluid texture is written to a back-buffer. Also simulate when reading back to front-buffer, or just copy contents? More computation, but fluid will flow faster.")]
        public bool DoubleSimulationStep = true;

        #endregion Public Properties

        #region Private Variables
        private bool initialized;
        private TextureChannel targetTextureChannel;
        private float remainingSimulationTime = 0;

        #endregion Private Variables

        #region Public Methods

        /// <summary>
        /// Manually reset the evaporation and simulation timeout timers.
        /// </summary>
        public void ResetTimeout()
        {
            remainingSimulationTime = Timeout;
        }

        /// <summary>
        /// Manually simulate the target fluid texture one step.
        /// </summary>
        public void UpdateFluid()
        {
            if (!initialized || !GravityMap.Initialized)
                return;
            using (var paintScope = GravityMap.Canvas.BeginPaintScope(targetTextureChannel, false)) {
                if (paintScope.IsValid)
                    FFFluidUtil.Simulate(paintScope.Target, GravityMap.FlowTexture, new Vector2(FluidRetainedSteep, FluidRetainedFlat), InflowColorInfluence, DoubleSimulationStep);
            }
        }

        #endregion Public Methods

        #region Private

        private void Initialize()
        {
            if (initialized || !GravityMap || !GravityMap.Initialized)
                return;
            if (!TextureChannelReference.IsValid) {
                Debug.LogWarning("FluidFlow: Unable to initialize. TextureChannelReference is invalid.");
                return;
            }
            targetTextureChannel = TextureChannelReference.Resolve();
            GravityMap.Canvas.OnTextureChannelUpdated.AddListener(OnTextureChannelUpdated);
            initialized = true;
        }

        private void OnTextureChannelUpdated(TextureChannel channel)
        {
            if (channel == targetTextureChannel)
                ResetTimeout();
        }

        private void Uninitialize()
        {
            if (!initialized)
                return;
            GravityMap.Canvas.OnTextureChannelUpdated.RemoveListener(OnTextureChannelUpdated);
            initialized = false;
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
            FluidUpdater.AddListener(UpdateFluid);
            FluidUpdater.RandomizeTimer();
        }

        private void Update()
        {
            if (!initialized)
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

        #endregion Private
    }

    public static class FFFluidUtil
    {
        public static readonly ShaderPropertyIdentifier FluidRetainedPropertyID = "_FF_FluidRetained";
        public static readonly ShaderPropertyIdentifier InflowColorInfluencePropertyID = "_FF_InflowColorInfluence";
        public static readonly MaterialCache FluidSimulate = new MaterialCache(InternalShaders.RootPath + "/Fluid/Simulate", FFFlowTextureUtil.SetFlowTexCompressed);


        /// <summary>
        /// Move fluid in a fluid texture one step depending on a gravity-based flow map.
        /// </summary>
        public static void Simulate(RenderTexture target, RenderTexture flowTex, Vector2 fluidRetained, float inflowColorInfluence, bool doubleSimulate)
        {
            using (target.SetTemporaryFilterMode(FilterMode.Point)) {
                Shader.SetGlobalTexture(FFFlowTextureUtil.FlowTexPropertyID, flowTex);
                Shader.SetGlobalVector(FluidRetainedPropertyID, fluidRetained);
                Shader.SetGlobalFloat(InflowColorInfluencePropertyID, inflowColorInfluence);
                using (var cpy = new TmpRenderTexture(target.descriptor)) {
                    Graphics.Blit(target, cpy, FFFlowTextureUtil.FlowTextureVariant(FluidSimulate, flowTex));
                    if (doubleSimulate) {
                        Graphics.Blit(cpy, target, FFFlowTextureUtil.FlowTextureVariant(FluidSimulate, flowTex));
                    } else {
                        InternalShaders.CopyTexture(cpy, target);
                    }
                }
            }
        }
    }
}