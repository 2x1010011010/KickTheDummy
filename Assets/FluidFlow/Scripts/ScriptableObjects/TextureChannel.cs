using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    [System.Serializable]
    public struct TextureProperty
    {
        public Optional<Shader> Shader;
        public string PropertyName;
    };

    [CreateAssetMenu(fileName = "NewTextureChannel", menuName = "Fluid Flow/Texture Channel")]
    public class TextureChannel : InitializableScriptableObject, System.IComparable<TextureChannel>
    {
        [SerializeField] private TextureProperty[] targets = new TextureProperty[0];

        public string Identifier { get => name; }
        private TextureProperty[] Targets { get => targets; }

        public override void Initialize()
        {
            Register(this);
        }

        public int CompareTo(TextureChannel other) => GetInstanceID() - other.GetInstanceID();

        public void Apply(Material material, MaterialPropertyBlock block, Texture texture)
        {
            for (var i = 0; i < targets.Length; i++) {
                if (targets[i].Shader.TryGet(out var targetShader) && targetShader != material.shader)
                    continue;
                var nameId = Shader.PropertyToID(targets[i].PropertyName);  // TODO: cache this
                if (material.HasProperty(nameId)) {
                    block.SetTexture(nameId, texture);
                }
            }
        }

        public bool TryGet(Material material, out Texture texture)
        {
            for (var i = 0; i < targets.Length; i++) {
                if (targets[i].Shader.TryGet(out var targetShader) && targetShader != material.shader)
                    continue;
                var nameId = Shader.PropertyToID(targets[i].PropertyName);  // TODO: cache this
                var propertyId = material.shader.FindPropertyIndex(targets[i].PropertyName);
                if (propertyId != -1 && material.shader.GetPropertyType(propertyId) == ShaderPropertyType.Texture) {
                    texture = material.GetTexture(nameId);
                    if (texture != null)
                        return true;
                    texture = material.GetDefaultTexture(targets[i].PropertyName);
                    if (texture != null)
                        return true;
                }
            }
            texture = default;
            return false;
        }

        public static implicit operator TextureChannel(TextureChannelReference identifier) => identifier.Resolve();
        public static implicit operator TextureChannel(string identifier) => Resolve(identifier);


        private static readonly Dictionary<string, TextureChannel> channels = new Dictionary<string, TextureChannel>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitDomain()    // when domain reload is disabled, we have to reset static state manually
        {
            channels.Clear();
        }

        private static void Register(TextureChannel channel)
        {
            if (channels.ContainsKey(channel.Identifier)) {
                Debug.LogWarningFormat("FluidFlow: TextureChannel with identifier '{0}' already registered!", channel.Identifier);
                return;
            }
            // Debug.LogFormat("Register {0} ({1})", channel.identifier, channel.GetInstanceID());
            channels.Add(channel.Identifier, channel);
        }

        public static TextureChannel Register(string identifier, TextureProperty[] targets)
        {
            if (channels.ContainsKey(identifier)) {
                Debug.LogWarningFormat("FluidFlow: Identifier '{0}' already registered!", identifier);
                return channels[identifier];
            }
            var instance = CreateInstance<TextureChannel>();
            instance.name = identifier;
            instance.targets = targets;
            Register(instance);
            return instance;
        }

        public static TextureChannel Resolve(string name)
        {
            if (channels.TryGetValue(name, out var channel)) {
                return channel;
            } else {
                Debug.LogWarningFormat("FluidFlow: Resolving TextureChannel with name '{0}' failed. Creating new empty channel.", name);
                return Register(name, new TextureProperty[0]);
            }
        }

        public static bool TryResolve(string name, out TextureChannel channel) => channels.TryGetValue(name, out channel);
    }

    [System.Serializable]
    public struct TextureChannelReference
    {
        public enum Mode
        {
            DIRECT,
            INDIRECT
        }
        [SerializeField] private Mode mode;
        [SerializeField] private string identifier;
        [SerializeField] private TextureChannel channel;
        public bool IsValid => mode == Mode.DIRECT ? channel != null : TryResolve(out var _);
        public string Identifier => mode == Mode.DIRECT ? (channel ? channel.Identifier : "<Invalid Direct Reference>") : identifier;

        private TextureChannelReference(Mode m, string id, TextureChannel ch)
        {
            mode = m;
            identifier = id;
            channel = ch;
        }

        public TextureChannel Resolve()
        {
            if (mode == Mode.INDIRECT) {
                channel = TextureChannel.Resolve(identifier);
                mode = Mode.DIRECT;
            }
            return channel;
        }

        public bool TryResolve(out TextureChannel textureChannel)
        {
            if (mode == Mode.INDIRECT) {
                if (TextureChannel.TryResolve(identifier, out textureChannel)) {
                    channel = textureChannel;
                    mode = Mode.DIRECT;
                    return true;
                }
            }
            textureChannel = channel;
            return channel != null;
        }

        public static TextureChannelReference Direct(TextureChannel channel) => new TextureChannelReference(Mode.DIRECT, channel.Identifier, channel);
        public static TextureChannelReference Indirect(string identifier) => new TextureChannelReference(Mode.INDIRECT, identifier, null);
        public static implicit operator TextureChannelReference(TextureChannel channel) => TextureChannelReference.Direct(channel);
        public static implicit operator TextureChannelReference(string name) => TextureChannelReference.Indirect(name);
    }
}
