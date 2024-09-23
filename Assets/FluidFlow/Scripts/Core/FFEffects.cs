using UnityEngine;

namespace FluidFlow
{
    public class FFEffects : MonoBehaviour
    {
        [Tooltip("Target Fluid Canvas.")]
        [SerializeField] private Tracked<FFGravityMap> gravityMap;
        public FFGravityMap GravityMap {
            get => gravityMap.Target;
            set => gravityMap.Target = value;
        }

        [Tooltip("Name of the fluid TextureChannel.")]
        public TextureChannelReference TextureChannelReference;

        [Tooltip("Keep updating fluid simulation, when no RenderTarget is visible?")]
        public bool UpdateInvisible = false;

        [Tooltip("Set when or how often the amount of fluid is reduced.")]
        public Updater EffectUpdater = new Updater(Updater.Mode.FIXED, .05f);

        [Tooltip("Halt effects after inactivity?")]
        public bool UseTimeout = true;

        [Min(0)]
        [Tooltip("Inactivity time (seconds) after which the simulation is halted.")]
        public float Timeout = 5;

        [Header("Seams")]
        [Tooltip("Hides uv seam artifacts. When using a FFGravityMap, use this instead of FFSeamFixer.")]
        public bool UseSeamStitching = true;

        [Header("Evaporation")]
        [Tooltip("Enable evaporation of fluid over time?")]
        public bool UseEvaporation = true;

        [Min(float.Epsilon)]
        [Tooltip("Amount of fluid evaporating in each evaporation update.")]
        public float EvaporationAmount = .01f;
        public enum DecayMode
        {
            [Tooltip("Fixed amount each step.")]
            LINEAR,
            [Tooltip("Factor of remaining fluid.")]
            EXPONENTIAL
        }
        public DecayMode EvaporationMode = DecayMode.EXPONENTIAL;

        [Header("Blur")]
        [Tooltip("Enable fluid spreading over time?")]
        public bool UseBlur = false;

        [Tooltip("Minimum amount of fluid required in a pixel, before blur is applied.")]
        public float BlurMinimumFluid = 1.5f;

        [Tooltip("Minimum amount of fluid required in a pixel, before blur is applied."), Range(0f, 1f)]
        public float BlurFactor = .5f;

        private bool initialized = false;
        private TextureChannel targetTextureChannel;
        private float remainingEffectTime = 0;

        public void UpdateEffects()
        {
            if (!initialized || !GravityMap.Initialized)
                return;
            using (var paintScope = GravityMap.Canvas.BeginPaintScope(targetTextureChannel, false)) {
                if (paintScope.IsValid) {
                    var flowTex = GravityMap.FlowTexture;
                    Shader.SetGlobalTexture(FFFlowTextureUtil.FlowTexPropertyID, flowTex);
                    if (UseEvaporation) {
                        Shader.SetGlobalFloat(FFEffectsUtil.FadeAmountPropertyID, EvaporationAmount);
                        Shader.SetGlobalFloat(FFEffectsUtil.FadeModePropertyID, EvaporationMode == DecayMode.LINEAR ? 0 : 1);
                    }
                    if (UseBlur) {
                        Shader.SetGlobalFloat(FFEffectsUtil.BlurMinFluidPropertyID, BlurMinimumFluid);
                        Shader.SetGlobalFloat(FFEffectsUtil.BlurFactorPropertyID, BlurFactor);
                    }

                    var targetTex = paintScope.Target;
                    using (var tmp = new TmpRenderTexture(targetTex.descriptor)) {
                        InternalShaders.CopyTexture(targetTex, tmp);
                        Graphics.Blit(tmp, targetTex, FFEffectsUtil.EffectsVariant(flowTex, UseSeamStitching, UseBlur, UseEvaporation));
                    }
                }
            }
        }

        private void OnTextureChannelUpdated(TextureChannel channel)
        {
            if (channel == targetTextureChannel)
                ResetTimeout();
        }

        /// <summary>
        /// Manually reset the evaporation and simulation timeout timers.
        /// </summary>
        public void ResetTimeout()
        {
            remainingEffectTime = Timeout;
        }

        public void Initialize()
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

        public void Uninitialize()
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
            EffectUpdater.AddListener(UpdateEffects);
            EffectUpdater.RandomizeTimer();
        }

        private void Start()
        {
            gravityMap.Initialize();
        }

        private void Update()
        {
            if (!initialized)
                return;
            if (UpdateInvisible || GravityMap.Canvas.IsVisible()) {
                if (!UseTimeout || remainingEffectTime > 0) {
                    EffectUpdater.Update();
                    remainingEffectTime -= Time.deltaTime;
                }
            }
        }

        private void OnDestroy()
        {
            gravityMap.Reset();
        }
    }

    public static class FFEffectsUtil
    {
        public static readonly ShaderPropertyIdentifier BlurMinFluidPropertyID = "_FF_BlurMinFluid";
        public static readonly ShaderPropertyIdentifier BlurFactorPropertyID = "_FF_BlurFactor";
        public static readonly ShaderPropertyIdentifier FadeAmountPropertyID = "_FF_FadeAmount";
        public static readonly ShaderPropertyIdentifier FadeModePropertyID = "_FF_FadeMode";

        public static readonly MaterialCache FluidEffects =
            new MaterialCache(InternalShaders.RootPath + "/Fluid/Effects",
                              FFFlowTextureUtil.SetFlowTexCompressed,
                              InternalShaders.SetKeyword("FF_EFFECT_STITCH"),
                              InternalShaders.SetKeyword("FF_EFFECT_BLUR"),
                              InternalShaders.SetKeyword("FF_EFFECT_FADE"));

        public static Material EffectsVariant(RenderTexture fluidTex, bool stitch, bool blur, bool fade)
                => FluidEffects.Get(Utility.SetBit(0, FFFlowTextureUtil.IsCompressed(fluidTex))
                                    + Utility.SetBit(1, stitch)
                                    + Utility.SetBit(2, blur)
                                    + Utility.SetBit(3, fade));
    }
}
