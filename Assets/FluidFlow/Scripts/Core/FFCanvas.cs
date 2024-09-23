using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FluidFlow
{
    public class FFCanvas : MonoBehaviour
    {
        #region Public Properties

        // Render Targets
        [Tooltip("Initialize the canvas automatically?")]
        public bool AutoInitialize = true;

        [Tooltip("Initialize canvas asynchronously? Depending on complexity initialization might take multiple frames. Use caches for faster initialization.")]
        public bool InitializeAsync = true;

        [Tooltip("Description of renderers handled by this canvas.")]
        public List<SurfaceDescriptor> SurfaceDescriptors = new List<SurfaceDescriptor>() { SurfaceDescriptor.Default() };

        [Header("Texture Channels")]
        [Tooltip("Width and height of each atlas tile textures."), Min(1)]
        public Vector2Int Resolution = Vector2Int.one * 512;

        [Tooltip("Description of texture channels used by this canvas.")]
        public List<TextureChannelDescriptor> TextureChannelDescriptors = new List<TextureChannelDescriptor>() { new TextureChannelDescriptor("Color", TextureChannelFormatReference.Indirect("Color"), TextureChannelDescriptor.InitializationMode.COPY) };

        public List<MaterialPropertyOverride> MaterialPropertyOverrides = new List<MaterialPropertyOverride>() { new MaterialPropertyOverride("_MainTex_ST", MaterialPropertyOverride.UVMode.KEYWORD, InternalShaders.SecondaryUVKeyword) };

        // Runtime
        [field: System.NonSerialized] public List<Material> AllocatedMaterials { get; private set; } = new List<Material>();

        /// <summary>
        /// Surfaces list initialized at runtime from the SurfaceDescriptors.
        /// </summary>
        public List<Surface> Surfaces { get; private set; } = new List<Surface>();

        /// <summary>
        /// TextureChannels initialized at runtime from the TextureChannelDescriptors.
        /// </summary>
        public SortedDictionary<TextureChannel, RenderTexture> TextureChannels { get; private set; } = new SortedDictionary<TextureChannel, RenderTexture>();

        /// <summary>
        /// Invoked when a TextureChannel has been drawn on.
        /// </summary>
        [HideInInspector] public UnityEvent<TextureChannel> OnTextureChannelUpdated = new UnityEvent<TextureChannel>();

        /// <summary>
        /// Invoked when the Surfaces list has been updated from the SurfaceDescriptors.
        /// </summary>
        [HideInInspector] public UnityEvent OnSurfacesUpdated = new UnityEvent();

        /// <summary>
        /// Invoked when the TextureChannels have been updated from the TextureChannelDescriptors.
        /// </summary>
        [HideInInspector] public UnityEvent OnTextureChannelsUpdated = new UnityEvent();

        public float TextureChannelAspectRatio => Resolution.x / (float)Resolution.y;

        private InitializationState state = InitializationState.UNINITIALIZED;
        public bool Initialized { get => state == InitializationState.INITIALIZED; }
        [HideInInspector] public UnityEvent OnInitialized = new UnityEvent();
        [HideInInspector] public UnityEvent OnUninitialized = new UnityEvent();

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Sets up the renderers, creates the internal rendertextures and assigns the material properties.
        /// If async initialization is active, initialization might take multiple frames until it is finished.
        /// </summary>
        public void Initialize()
        {
            if (state != InitializationState.UNINITIALIZED)
                return;
            state = InitializationState.INITIALIZING;
            UpdateTextureChannels();
            var handles = UpdateRenderTargets();
            if (InitializeAsync) {
                Cache.CompleteMeshRequestsAsync(handles, completeInitialization);
            } else {
                Cache.CompleteMeshRequests(handles);
                completeInitialization();
            }
            void completeInitialization()
            {
                InitializeTextureChannels();
                UpdateMaterials();
                state = InitializationState.INITIALIZED;
                OnInitialized.Invoke();
            }
        }

        /// <summary>
        /// Releases internal rendertextures and unsets material property blocks.
        /// </summary>
        public void Uninitialize()
        {
            if (state != InitializationState.INITIALIZED)
                return;
            // unset textures from material property bocks
            for (var s = 0; s < Surfaces.Count; s++)
                for (var d = Surfaces[s].SubmeshDescriptors.Length - 1; d >= 0; d--)
                    foreach (var index in Surfaces[s].SubmeshDescriptors[d].SubmeshMask.EnumerateSetBits())
                        Surfaces[s].Renderer.SetPropertyBlock(null, index);
            ReleaseTextures();
            state = InitializationState.UNINITIALIZED;
            OnUninitialized.Invoke();
        }

        public void DeleteRuntimeAllocatedMaterials()
        {
            for (var i = AllocatedMaterials.Count - 1; i >= 0; i--)
                Destroy(AllocatedMaterials[i]);
            AllocatedMaterials.Clear();
        }

        /// <summary>
        /// Update RenderTargets from RenderTargetDescriptors list.
        /// Note that requested mesh data generation has to be completed, before the canvas is fully initialized.
        /// </summary>
        public List<Gravity.GenerationHandle> UpdateRenderTargets()
        {
            Surfaces.Clear();
            var handles = new List<Gravity.GenerationHandle>();
            for (var i = 0; i < SurfaceDescriptors.Count; i++) {
                var (surface, optGenerationHandle) = SurfaceDescriptors[i].ToSurface(TextureChannelAspectRatio);
                Surfaces.Add(surface);
                if (optGenerationHandle.TryGet(out var handle))
                    handles.Add(handle);
            }
            OnSurfacesUpdated.Invoke();
            return handles;
        }

        /// <summary>
        /// Update TextureChannels from TextureChannelDescriptors.
        /// </summary>
        public void UpdateTextureChannels(bool releaseOld = true)
        {
            // release old textures
            if (releaseOld)
                ReleaseTextures();
            TextureChannels.Clear();
            // create textures
            foreach (var channel in TextureChannelDescriptors) {
                if (!channel.TextureChannelReference.IsValid) {
                    Debug.LogWarning("FluidFlow: Invalid TextureChannelReference!");
                    continue;
                }
                var textureChannel = channel.TextureChannelReference.Resolve();
                // ensure each texture property is unique per canvas
                if (!TextureChannels.ContainsKey(textureChannel)) {
                    if (channel.FormatReference.TryResolve(out var textureChannelFormat)) {
                        if (textureChannelFormat.IsValid) {
                            TextureChannels.Add(textureChannel, InternalTextures.CreateRenderTexture(textureChannelFormat.Format, Resolution));
                        } else {
                            Debug.LogWarningFormat("FluidFlow: Required TextureChannelFormat for '{0}' not supported on the current platform!", textureChannel.Identifier);
                        }
                    } else {
                        Debug.LogWarning("FluidFlow: Invalid TextureChannelFormat!");
                    }
                } else {
                    Debug.LogWarningFormat("FluidFlow: Found duplicate texture property '{0}' in texture channels.", textureChannel.Identifier);
                }
            }

            OnTextureChannelsUpdated.Invoke();
        }

        /// <summary>
        /// Apply textures, atlas transformations to renderers as MaterialProperties.
        /// Additionally, this instances all materials which are not instanced already and require updating the secondary uv keyword.
        /// </summary>
        public void UpdateMaterials()
        {
            // apply material properties to renderers
            for (var s = Surfaces.Count - 1; s >= 0; s--) {
                var usesSecondaryUV = Surfaces[s].UVSet == UVSet.UV1;
                var materials = Shared.MaterialList();
                Surfaces[s].Renderer.GetSharedMaterials(materials);
                var materialsModified = false;
                foreach (var submeshTransform in Surfaces[s].EnumerateSubmeshes()) {
                    using (var editMaterial = new ScopedMaterialPropertyBlockEdit(Surfaces[s].Renderer, submeshTransform.Index)) {
                        for (var i = 0; i < MaterialPropertyOverrides.Count; i++) {
                            var propOverride = MaterialPropertyOverrides[i];
                            if (propOverride.Target.Enabled && propOverride.Target.Value != materials[submeshTransform.Index].shader)
                                continue;
                            // set position of the surface in the texture atlas
                            editMaterial.PropertyBlock.SetVector(propOverride.AtlasTransformPropertyName, submeshTransform.AtlasTransform);
                            if (propOverride.UVOverrideMode == MaterialPropertyOverride.UVMode.PROPERTY) {
                                editMaterial.PropertyBlock.SetFloat(propOverride.UVTargetName, usesSecondaryUV ? 1 : 0);
                            } else if (usesSecondaryUV != materials[submeshTransform.Index].IsKeywordEnabled(propOverride.UVTargetName)) {
                                // switch uv set used by the material, if necessary
                                // ensure material is instanced before changing (instanced objects have an instance id <0)
                                if (materials[submeshTransform.Index].GetInstanceID() >= 0) {
                                    materials[submeshTransform.Index] = new Material(materials[submeshTransform.Index]);   // clone material, so the original is not altered
                                    AllocatedMaterials.Add(materials[submeshTransform.Index]);    // runtime allocated materials are not cleaned up automatically, so we have to delete them manually later
                                    materialsModified = true;
                                }
                                materials[submeshTransform.Index].SetKeyword(propOverride.UVTargetName, usesSecondaryUV);
                            }
                        }
                        // apply texture channels to renderers
                        foreach (var channel in TextureChannels)
                            channel.Key.Apply(materials[submeshTransform.Index], editMaterial.PropertyBlock, channel.Value);
                    }
                }
                if (materialsModified)
                    Surfaces[s].Renderer.sharedMaterials = materials.ToArray();
            }
        }

        /// <summary>
        /// Initialize the TextureChannels with the specified color/texture information.
        /// </summary>
        public void InitializeTextureChannels()
        {
            foreach (var descriptor in TextureChannelDescriptors) {
                if (descriptor.TextureChannelReference.IsValid && TextureChannels.TryGetValue(descriptor.TextureChannelReference, out var target))
                    Surfaces.InitializeTextureChannel(target, descriptor);
            }
            Surfaces.FixSeams(TextureChannels.Values, Resolution);
        }

        /// <summary>
        /// Is any of the RenderTargets visible to any camera?
        /// </summary>
        public bool IsVisible()
        {
            for (var i = Surfaces.Count - 1; i >= 0; i--)
                if (Surfaces[i].Renderer.isVisible)
                    return true;
            return false;
        }

        #endregion Public Methods

        #region Private

        private void Start()
        {
            if (AutoInitialize)
                Initialize();
        }

        private void OnDestroy()
        {
            Uninitialize();
        }

        private void ReleaseTextures()
        {
            if (TextureChannels != null) {
                foreach (var texture in TextureChannels.Values)
                    texture.Release();
                TextureChannels.Clear();
            }
        }

        #endregion Private
    }

    public enum InitializationState
    {
        [Tooltip("Not yet initialized.")]
        UNINITIALIZED,

        [Tooltip("Currently being initialized.")]
        INITIALIZING,

        [Tooltip("Fully initialized.")]
        INITIALIZED
    }

    public enum UVSet
    {
        [Tooltip("Default uv set.")]
        UV0,

        [Tooltip("Secondary/lightmap uv set.")]
        UV1
    }

    [System.Serializable]
    public struct MaterialPropertyOverride
    {
        public Optional<Shader> Target;
        public string AtlasTransformPropertyName;
        public enum UVMode
        {
            KEYWORD,
            PROPERTY
        }
        public UVMode UVOverrideMode;
        public string UVTargetName;

        public MaterialPropertyOverride(string atlasTransformPropertyName, UVMode overrideMode, string uvTargetName)
        {
            Target = Optional<Shader>.None;
            AtlasTransformPropertyName = atlasTransformPropertyName;
            UVOverrideMode = overrideMode;
            UVTargetName = uvTargetName;
        }
    }
}