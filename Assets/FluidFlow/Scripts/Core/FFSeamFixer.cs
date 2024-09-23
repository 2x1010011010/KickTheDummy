using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace FluidFlow
{
    /// <summary>
    /// Fix UV seam artifacts, by expanding the color of the uv islands edges by a few pixels.
    /// When using your FFCanvas in combination with a FFGravityMap, use FFEffects instead!
    /// </summary>
    public class FFSeamFixer : MonoBehaviour
    {
        #region Public Properties

        [Tooltip("Target canvas.")]
        [SerializeField] private Tracked<FFCanvas> canvas;
        public FFCanvas Canvas {
            get => canvas.Target;
            set => canvas.Target = value;
        }

        [Tooltip("Texture channels of the canvas this component affects.")]
        public List<TextureChannelReference> TargetTextureChannels = new List<TextureChannelReference>() { };

        [Tooltip("Cache padding offsets in a texture, insead of recalculating them for each fix? " +
            "Tradeoff between performance and vram usage.")]
        public bool UseCache = true;

        [Tooltip("Set when or how often modified textures are checked for.")]
        public Updater SeamUpdater = new Updater(Updater.Mode.CONTINUOUS);

        public RenderTexture PaddingCache {
            get {
                if (UseCache) {
                    if (paddingCache == null)
                        paddingCache = SeamFixerUtil.CreatePaddingCache(Canvas);
                } else {
                    ClearCache();
                }
                return paddingCache;
            }
        }

        #endregion Public Properties

        #region Private Variables
        private bool initialized = false;
        private RenderTexture paddingCache;
        private List<TextureChannel> targetChannels = new List<TextureChannel>();
        private List<TextureChannel> modifiedChannels = new List<TextureChannel>(); // a HashSet may fit better here, but as there will only be very few TextureChannels a plan List will be faster

        #endregion Private Variables

        #region Public Methods

        public void Initialize()
        {
            if (initialized)
                return;
            if (!Canvas || !Canvas.Initialized)
                return;
            if (InternalTextures.R8MinFormat == GraphicsFormat.None) {
                Debug.LogWarning("FluidFlow: Unable to initialize. Required RenderTexture format not supported on the current platform.");
                return;
            }
            initialized = true;
            Canvas.OnTextureChannelUpdated.AddListener(MarkModified);
            // automatically recalculate cache when RenderTargets or TextureChannels are updated
            Canvas.OnTextureChannelsUpdated.AddListener(UpdateCache);
            Canvas.OnSurfacesUpdated.AddListener(UpdateCache);
            UpdateTargetTextureChannels();
            UpdateCache();
        }

        public void Uninitialize()
        {
            if (!initialized)
                return;
            Canvas.OnTextureChannelUpdated.RemoveListener(MarkModified);
            Canvas.OnTextureChannelsUpdated.RemoveListener(UpdateCache);
            Canvas.OnSurfacesUpdated.RemoveListener(UpdateCache);
            ClearCache();
        }

        /// <summary>
        /// Manually update internal cache, if enabled.
        /// </summary>
        public void UpdateCache()
        {
            ClearCache();
            paddingCache = PaddingCache;
        }

        /// <summary>
        /// Update which TextureChannels are targeted by this component from the list of TargetTextureChannel names.
        /// </summary>
        public void UpdateTargetTextureChannels()
        {
            targetChannels.Clear();
            foreach (var channel in TargetTextureChannels)
                if (channel.IsValid)
                    targetChannels.Add(channel.Resolve());
        }

        /// <summary>
        /// Marks a TextureChannel as modified.
        /// </summary>
        public void MarkModified(TextureChannel textureChannel)
        {
            if (!modifiedChannels.Contains(textureChannel))
                modifiedChannels.Add(textureChannel);
        }

        /// <summary>
        /// Fix seams of all TextureChannels marked as modified and contained in the target TextureChannels.
        /// </summary>
        public void FixModifiedChannels()
        {
            if (!initialized)
                return;
            foreach (var channel in modifiedChannels) {
                if (targetChannels.Contains(channel)) {
                    using (var paintScope = Canvas.BeginPaintScope(channel, false)) {
                        if (paintScope.IsValid) {
                            if (UseCache)
                                SeamFixerUtil.FixSeams(PaddingCache, paintScope.Target, true);
                            else
                                Canvas.Surfaces.FixSeams(paintScope.Target);
                        }
                    }
                }
            }
            modifiedChannels.Clear();
        }

        /// <summary>
        /// Clear cache and release internal resources (if present).
        /// </summary>
        public void ClearCache()
        {
            if (paddingCache != null && paddingCache.IsCreated()) {
                paddingCache.Release();
                paddingCache = null;
            }
        }

        #endregion Public Methods

        #region Private

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
            SeamUpdater.AddListener(FixModifiedChannels);
            SeamUpdater.RandomizeTimer();
        }

        private void Start()
        {
            canvas.Initialize();
        }

        private void LateUpdate()
        {
            SeamUpdater.Update();
        }

        private void OnDestroy()
        {
            canvas.Reset();
        }

        #endregion Private
    }


    public static class SeamFixerUtil
    {
        public static readonly ShaderPropertyIdentifier SeamTexPropertyID = "_FF_SeamTex";
        public static readonly MaterialCache EncodePadding = new MaterialCache(InternalShaders.RootPath + "/EncodePadding");
        public static readonly MaterialCache ApplyPadding = new MaterialCache(InternalShaders.RootPath + "/ApplyPadding",
                                                                              InternalShaders.SetKeyword("PRECALCULATED_OFFSET"));
        public static Material ApplyPaddingVariant(bool precalculated) => ApplyPadding.Get(Utility.SetBit(0, precalculated));

        /// <summary>
        /// Fix seams of all specified RenderTextures
        /// </summary>
        public static void FixSeams(this List<Surface> surfaces, IEnumerable<RenderTexture> targets, Vector2Int resolution)
        {
            using (var uvMap = new TmpRenderTexture(InternalTextures.R8MinFormat, resolution.x, resolution.y)) {
                surfaces.EncodeSeamFixMap(uvMap);
                foreach (var target in targets)
                    FixSeams(uvMap, target, true);
            }
        }

        /// <summary>
        /// Fix seams of the specified RenderTexture, witout padding cache.
        /// </summary>
        public static void FixSeams(this List<Surface> surfaces, RenderTexture target)
        {
            using (var uvMap = new TmpRenderTexture(InternalTextures.R8MinFormat, target.width, target.height)) {
                surfaces.DrawUVMap(uvMap);
                FixSeams(uvMap, target, false);
            }
        }

        /// <summary>
        /// Fix seams of the specified RenderTexture, using an existing uv unwrap, or padding cache.
        /// </summary>
        public static void FixSeams(RenderTexture seamTex, RenderTexture target, bool precalcualtedOffset)
        {
            Shader.SetGlobalTexture(SeamTexPropertyID, seamTex);
            using (var tmp = new TmpRenderTexture(target.descriptor)) {
                InternalShaders.CopyTexture(target, tmp);
                Graphics.Blit(tmp, target, ApplyPaddingVariant(precalcualtedOffset));
            }
        }

        /// <summary>
        /// Calculate and store padding offsets to the target texture.
        /// </summary>
        public static void EncodeSeamFixMap(this List<Surface> surfaces, RenderTexture target)
        {
            using (RestoreRenderTarget.RestoreActive()) {
                using (var tmp = new TmpRenderTexture(target.descriptor)) {
                    // draw uv map of mesh
                    surfaces.DrawUVMap(tmp);
                    // calculate and store padding in the target texture
                    Graphics.Blit(tmp, target, EncodePadding);
                }
            }
        }

        /// <summary>
        /// Create a rendertexture with cached padding offsets.
        /// </summary>
        public static RenderTexture CreatePaddingCache(FFCanvas canvas)
        {
            var rt = InternalTextures.CreateRenderTexture(InternalTextures.R8MinFormat, canvas.Resolution);
            rt.filterMode = FilterMode.Point;
            canvas.Surfaces.EncodeSeamFixMap(rt);
            return rt;
        }
    }
}