using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;

namespace FluidFlow
{
    public class FFGravityMap : MonoBehaviour
    {
        [Tooltip("Target Fluid Canvas.")]
        [SerializeField] private Tracked<FFCanvas> canvas;
        public FFCanvas Canvas {
            get => canvas.Target;
            set => canvas.Target = value;
        }

        [Tooltip("Compress flow texture to reduce video memory usage, but requires some operations for packing/unpacking during simulation.")]
        public bool Compressed = true;

        [Tooltip("Set when or how often the gravity map is updated. The gravity map is only updated when requesting the FlowTexture.")]
        public Updater GravityUpdater = new Updater(Updater.Mode.CONTINUOUS, .1f);

        [Header("Normal Map")]
        [Tooltip("Is the gravity map influenced by the RenderTargets' normal map?")]
        public bool UseNormalMaps = false;

        [Tooltip("Name of the normal map texture property of the RenderTargets' materials.")]
        public TextureChannelReference NormalTextureChannel = "Normal";

        [Tooltip("Influence of the normal map on the generation of the gravity map.")]
        public float NormalInfluence = 1;

        [Header("Noise")]
        [Tooltip("Influence of random noise on the flow map.")]
        public float NoiseAmount = .4f;

        [Tooltip("Scale of the random noise.")]
        public float NoiseScale = 50;

        [Tooltip("Offset of the random noise.")]
        public Vector2 NoiseOffset = Vector2.zero;

        [Tooltip("Amount of time-based scrolling of the noise for less uniform flow simulation.")]
        public float NoiseScrollAmount = 1;

        [Tooltip("Speed of time-based scrolling of the noise for less uniform flow simulation.")]
        public float NoiseScrollSpeed = 1;

        private InitializationState state = InitializationState.UNINITIALIZED;
        public bool Initialized { get => state == InitializationState.INITIALIZED && Canvas && Canvas.Initialized; }
        [HideInInspector] public UnityEvent OnInitialized = new UnityEvent();
        [HideInInspector] public UnityEvent OnUninitialized = new UnityEvent();

        private double lastUpdate;
        private float scroll;
        private Vector3 worldGravity = Vector3.down;
        private RenderTexture flowTexture;

        /// <summary>
        /// Lazily update internal flow texture upon request, set depending on update mode.
        /// </summary>
        public RenderTexture FlowTexture {
            get {
                switch (GravityUpdater.UpdateMode) {
                    case Updater.Mode.CONTINUOUS:
                        if (lastUpdate != Time.timeSinceLevelLoadAsDouble)
                            UpdateGravity();
                        break;
                    case Updater.Mode.FIXED:
                        if ((float)(Time.timeSinceLevelLoadAsDouble - lastUpdate) >= GravityUpdater.FixedUpdateInterval)
                            UpdateGravity();
                        break;
                    default:
                        break;
                }
                return flowTexture;
            }
        }

        public void Initialize()
        {
            if (state != InitializationState.UNINITIALIZED)
                return;
            if (!Canvas || !Canvas.Initialized)
                return;
            if (Compressed) {
                if (Application.platform == RuntimePlatform.WebGLPlayer) {       // TODO: WebGL does not support uint shader arithmetic 
                    Debug.LogWarning("FluidFlow: WebGL does not support compressed flow textures. Falling back to uncompressed flow texture.");
                    Compressed = false;
                } else if (FFFlowTextureUtil.CompressedFlowFormat == GraphicsFormat.None) {
                    Debug.LogWarning("FluidFlow: Compressed flow texture format not supported on current platform. Falling back to uncompressed flow texture.");
                    Compressed = false;
                }
            }
            var flowFormat = Compressed ? FFFlowTextureUtil.CompressedFlowFormat : InternalTextures.HighPrecisionRGBA;
            if (flowFormat == GraphicsFormat.None) {
                Debug.LogWarning("FluidFlow: Unable to initialize. Required RenderTexture format not supported on the current platform.");
                return;
            }
            state = InitializationState.INITIALIZING;
            flowTexture = InternalTextures.CreateRenderTexture(flowFormat, Canvas.Resolution);
            flowTexture.filterMode = FilterMode.Point;
            // flowTexture.DebugTexture();

            var handles = new List<Stitcher.GenerationHandle>();    // request generation of stitch data (if not already cached)
            for (var i = 0; i < Canvas.Surfaces.Count; i++) {
                var optRequest = Cache.Request(Canvas.Surfaces[i].Mesh, Canvas.Surfaces[i].UVSet);
                if (optRequest.TryGet(out var handle))
                    handles.Add(handle);
            }
            if (Canvas.InitializeAsync) {
                Cache.CompleteStitchRequestsAsync(handles, completeInitialization);
            } else {
                Cache.CompleteStitchRequests(handles);
                completeInitialization();
            }

            void completeInitialization()
            {
                StitchMap.Draw(Canvas.Surfaces, flowTexture);
                state = InitializationState.INITIALIZED;
                OnInitialized.Invoke();
            }
        }

        public void Uninitialize()
        {
            if (state != InitializationState.INITIALIZED)
                return;
            flowTexture.Release();
            state = InitializationState.UNINITIALIZED;
            OnUninitialized.Invoke();
        }

        /// <summary>
        /// Set a new world space gravity direction, and update the internal flow map.
        /// </summary>
        public void UpdateGravity(Vector3 worldGravity)
        {
            this.worldGravity = worldGravity.normalized;
            UpdateGravity();
        }

        /// <summary>
        /// Update the internal flow map.
        /// </summary>
        public void UpdateGravity()
        {
            var time = Time.timeSinceLevelLoadAsDouble;
            var dTime = time - lastUpdate;
            lastUpdate = time;
            scroll = (scroll + (float)dTime * NoiseScrollSpeed) % (Mathf.PI * 2f);
            var layerShift = NoiseScrollAmount > 0 ? new Vector2(Mathf.Sin(scroll), Mathf.Cos(scroll)) * NoiseScrollAmount : Vector2.zero;

            Graphics.SetRenderTarget(flowTexture);
            new FFFlowTextureUtil.NoiseData(NoiseOffset, NoiseScale, NoiseAmount, layerShift).SetGlobals();
            Shader.SetGlobalVector(FFFlowTextureUtil.GravityPropertyID, worldGravity.normalized);
            Shader.SetGlobalVector(InternalShaders.TexelSizePropertyID, flowTexture.GetTexelSize());
            if (UseNormalMaps) {
                Shader.SetGlobalFloat(FFFlowTextureUtil.NormalStrengthPropertyID, NormalInfluence);

                if (Canvas.TextureChannels.TryGetValue(NormalTextureChannel, out var normalTexture)) {
                    Shader.SetGlobalFloat(FFFlowTextureUtil.NormalFromTextureChannelAtlasPropertyID, 1);
                    Shader.SetGlobalTexture(FFFlowTextureUtil.NormalTexPropertyID, normalTexture);
                    FFFlowTextureUtil.DrawFlowTex(Canvas.Surfaces, true);
                } else {
                    Shader.SetGlobalFloat(FFFlowTextureUtil.NormalFromTextureChannelAtlasPropertyID, 0);
                    FFFlowTextureUtil.DrawFlowTexWithPerMaterialNormalMap(Canvas.Surfaces, NormalTextureChannel);
                }
            } else {
                FFFlowTextureUtil.DrawFlowTex(Canvas.Surfaces, false);
            }
        }

        // TODO: also render motion vectors to uv space to simulate e.g. centrifugal force

        private void Awake()
        {
            canvas.OnBeforeChanged += target => {
                if (target) {
                    target.OnInitialized.RemoveListener(Initialize);
                    target.OnUninitialized.RemoveListener(Uninitialize);
                }
                Uninitialize();
            };
            canvas.OnAfterChanged += target => {
                if (target) {
                    target.OnInitialized.AddListener(Initialize);
                    target.OnUninitialized.AddListener(Uninitialize);
                }
                Initialize();
            };
            canvas.Initialize();    // assign reference set in inspector, and thus trigger initialization

            worldGravity = Physics.gravity.normalized;
        }

        private void OnDestroy()
        {
            canvas.Reset();
        }
    }

    public static class FFFlowTextureUtil
    {
        public static GraphicsFormat CompressedFlowFormat => SystemInfo.GetCompatibleFormat(GraphicsFormat.R8G8B8A8_UNorm, FormatUsage.Render);

        public static readonly ShaderPropertyIdentifier FlowTexPropertyID = "_FF_FlowTex";
        public static void SetFlowTexCompressed(Material material, bool compressed) => material.SetKeyword("FF_FLOWTEX_COMPRESSED", compressed);
        public static bool IsCompressed(RenderTexture rt) => rt.graphicsFormat == CompressedFlowFormat;
        public static Material FlowTextureVariant(MaterialCache cache, RenderTexture rt) => cache.Get(Utility.SetBit(0, IsCompressed(rt)));
        public static readonly ShaderPropertyIdentifier NormalTexPropertyID = "_FF_NormalTex";
        public static readonly ShaderPropertyIdentifier GravityPropertyID = "_FF_Gravity";
        public static readonly ShaderPropertyIdentifier NormalStrengthPropertyID = "_FF_NormalStrength";
        public static readonly ShaderPropertyIdentifier NormalFromTextureChannelAtlasPropertyID = "_FF_NormalFromAtlas";

        public static readonly MaterialCache Gravity = new MaterialCache(InternalShaders.RootPath + "/Gravity", InternalShaders.SetSecondaryUV, InternalShaders.SetKeyword("USE_NORMAL"));
        public static PerRenderTargetVariant GravityVariant(bool useNormal) => new PerRenderTargetVariant(Gravity, Utility.SetBit(1, useNormal));

        public readonly struct NoiseData
        {
            public static readonly ShaderPropertyIdentifier NoisePropertyID = "_FF_Noise";
            public static readonly ShaderPropertyIdentifier NoiseLayerShiftPropertyID = "_FF_NoiseLayerShift";
            private readonly Vector4 Noise;
            private readonly Vector2 LayerShift;

            public NoiseData(Vector2 offset, float scale, float amount, Vector2 layerShift)
            {
                Noise = new Vector4(offset.x, offset.y, scale, amount);
                LayerShift = layerShift;
            }

            public void SetGlobals()
            {
                Shader.SetGlobalVector(NoisePropertyID, Noise);
                Shader.SetGlobalVector(NoiseLayerShiftPropertyID, LayerShift);
            }
        }

        // FIXME: not set by unity, while doing custom graphics stuff 
        // private static readonly ShaderPropertyIdentifier worldTransformParams = "unity_WorldTransformParams";
        // command.SetGlobalVector(worldTransformParams, new Vector4(0, 0, 0, 1));
        public static void DrawFlowTex(List<Surface> surfaces, bool normal)
        {
            var cb = Shared.CommandBuffer();
            cb.DrawRenderTargets(surfaces, GravityVariant(normal), false);
            Graphics.ExecuteCommandBuffer(cb);
            cb.Clear();
        }

        public static void DrawFlowTexWithPerMaterialNormalMap(List<Surface> surfaces, TextureChannel normalTextureChannel)
        {
            var cb = Shared.CommandBuffer();
            for (var i = surfaces.Count - 1; i >= 0; i--) {
                var materials = Shared.MaterialList();
                surfaces[i].Renderer.GetSharedMaterials(materials);
                for (var s = surfaces[i].SubmeshDescriptors.Length - 1; s >= 0; s--) {
                    cb.SetGlobalVector(DrawExtensions.AtlasTransformPropertyID, surfaces[i].SubmeshDescriptors[s].AtlasTransform);
                    for (var it = surfaces[i].SubmeshDescriptors[s].SubmeshMask.IterateFlags(); it.Valid(); it.Next()) {
                        var hasNormalTex = normalTextureChannel.TryGet(materials[it.Index()], out var normalTex);
                        if (hasNormalTex) {
                            cb.SetGlobalTexture(NormalTexPropertyID, normalTex);
                        }
                        cb.DrawRenderer(surfaces[i].Renderer, GravityVariant(hasNormalTex).Get(surfaces[i].UVSet), it.Index(), 0);
                    }
                }
            }
            Graphics.ExecuteCommandBuffer(cb);
            cb.Clear();
        }
    }
}
