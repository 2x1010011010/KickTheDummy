using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace FluidFlow
{
    [CreateAssetMenu(fileName = "NewTextureChannelFormat", menuName = "Fluid Flow/Texture Channel Format")]
    public class TextureChannelFormat : InitializableScriptableObject
    {
        [Tooltip("List of possible formats. First one available on the target platform will be chosen.")]
        [SerializeField] private GraphicsFormat[] Formats;
        public string Identifier { get => name; }
        [System.NonSerialized] private GraphicsFormat format = GraphicsFormat.None;
        public GraphicsFormat Format {
            get {
                if (format == GraphicsFormat.None) {
                    format = Formats.GetSupportedFormat();
                }
                return format;
            }
        }
        public bool IsValid => Format != GraphicsFormat.None;

        public override void Initialize()
        {
            Register(this);
        }

        private static readonly Dictionary<string, TextureChannelFormat> formats = new Dictionary<string, TextureChannelFormat>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitDomain()    // when domain reload is disabled, we have to reset static state manually
        {
            formats.Clear();
        }

        private static void Register(TextureChannelFormat channel)
        {
            if (formats.ContainsKey(channel.Identifier)) {
                Debug.LogWarningFormat("FluidFlow: TextureChannelFormat with identifier '{0}' already registered!", channel.Identifier);
                return;
            }
            // Debug.LogFormat("Register {0} ({1})", channel.identifier, channel.GetInstanceID());
            formats.Add(channel.Identifier, channel);
        }

        public static TextureChannelFormat Register(string identifier, GraphicsFormat[] targetFormats)
        {
            if (formats.ContainsKey(identifier)) {
                Debug.LogWarningFormat("FluidFlow: Identifier '{0}' already registered!", identifier);
                return formats[identifier];
            }
            var instance = CreateInstance<TextureChannelFormat>();
            instance.name = identifier;
            instance.Formats = targetFormats;
            Register(instance);
            return instance;
        }

        public static bool TryResolve(string name, out TextureChannelFormat format) => formats.TryGetValue(name, out format);
    }

    [System.Serializable]
    public struct TextureChannelFormatReference
    {
        public enum Mode
        {
            DIRECT,
            INDIRECT
        }
        [SerializeField] private Mode mode;
        [SerializeField] private string identifier;
        [SerializeField] private TextureChannelFormat format;
        public bool IsValid => mode == Mode.DIRECT ? format != null : TryResolve(out var _);
        public string Identifier => mode == Mode.DIRECT ? (format ? format.Identifier : "<Invalid Direct Reference>") : identifier;

        private TextureChannelFormatReference(Mode m, string id, TextureChannelFormat fmt)
        {
            mode = m;
            identifier = id;
            format = fmt;
        }

        public bool TryResolve(out TextureChannelFormat fmt)
        {
            if (mode == Mode.INDIRECT) {
                if (TextureChannelFormat.TryResolve(identifier, out fmt)) {
                    format = fmt;
                    mode = Mode.DIRECT;
                    return true;
                }
            }
            fmt = format;
            return format != null;
        }

        public static TextureChannelFormatReference Direct(TextureChannelFormat channel) => new TextureChannelFormatReference(Mode.DIRECT, channel.Identifier, channel);
        public static TextureChannelFormatReference Indirect(string identifier) => new TextureChannelFormatReference(Mode.INDIRECT, identifier, null);
        public static implicit operator TextureChannelFormatReference(TextureChannelFormat channel) => TextureChannelFormatReference.Direct(channel);
        public static implicit operator TextureChannelFormatReference(string name) => TextureChannelFormatReference.Indirect(name);
    }

    public static class TextureChannelFormatUtil
    {
        public static GraphicsFormat GetSupportedFormat(this GraphicsFormat[] formats)
        {
            for (var i = 0; i < formats.Length; i++) {
                if (SystemInfo.IsFormatSupported(formats[i], FormatUsage.Render)) {
                    return formats[i];
                }
            }
            Debug.LogWarning("FluidFlow: Non of the given format fallbacks available on the current platform. Trying to get similar compatible format.");
            for (var i = 0; i < formats.Length; i++) {
                var f = SystemInfo.GetCompatibleFormat(formats[i], FormatUsage.Render);
                if (f != GraphicsFormat.None) {
                    return f;
                }
            }
            Debug.LogWarning("FluidFlow: Fallback to the platform default RenderTexture format.");
            return SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
        }
    }
}
