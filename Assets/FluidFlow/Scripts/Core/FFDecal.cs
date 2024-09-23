using UnityEngine;

namespace FluidFlow
{
    [System.Serializable]
    public struct FFDecal
    {
        [Tooltip("Optional mask for masking the decal." +
            "When no mask texture is set, no mask is applied.")]
        public Mask MaskChannel;

        [Tooltip("Texture channels of a canvas affected by this decal.")]
        public Channel[] Channels;

        public FFDecal(Channel channel)
        {
            MaskChannel = new Mask();
            Channels = new Channel[] { channel };
        }

        public FFDecal(Mask maskChannel, Channel channel)
        {
            MaskChannel = maskChannel;
            Channels = new Channel[] { channel };
        }

        public FFDecal(params Channel[] channels)
        {
            MaskChannel = new Mask();
            Channels = channels;
        }

        public FFDecal(Mask maskChannel, params Channel[] channels)
        {
            MaskChannel = maskChannel;
            Channels = channels;
        }

        // allow implicitly converting a channel to a decal without a mask.
        public static implicit operator FFDecal(Channel channel)
        {
            return new FFDecal(new Channel[] { channel });
        }

        // allow implicitly converting an array of channels to a multi-channel decal without a mask.
        public static implicit operator FFDecal(Channel[] channels)
        {
            return new FFDecal(channels);
        }

        [System.Serializable]
        public struct Channel
        {
            public enum Type
            {
                [Tooltip("Channel contains color information.")]
                COLOR,

                [Tooltip("Channel contains a normal map.")]
                NORMAL,

                [Tooltip("Channel contains fluid information.")]
                FLUID
            };

            [Tooltip("TextureChannel this decal channels draws to.")]
            public TextureChannelReference TargetTextureChannel;

            [Tooltip("Type of this decal channel.")]
            public Type ChannelType;

            [Tooltip("Set where this decal channel samples its color from.")]
            public ColorSource Source;

            [Tooltip("Channel type dependent data.")]
            public float Data;

            [Tooltip("Mask which channels of the targeted texture are affected.")]
            public ComponentMask WriteMask;

            public static Channel Color(TextureChannelReference property, ColorSource source, ComponentMask mask = ComponentMask.All)
            {
                return new Channel() {
                    TargetTextureChannel = property,
                    ChannelType = Type.COLOR,
                    Source = source,
                    WriteMask = mask
                };
            }

            public static Channel Fluid(TextureChannelReference property, ColorSource source, float amount = 1, ComponentMask mask = ComponentMask.All)
            {
                return new Channel() {
                    TargetTextureChannel = property,
                    ChannelType = Type.FLUID,
                    Source = source,
                    Data = amount,
                    WriteMask = mask
                };
            }

            public static Channel Normal(TextureChannelReference property, Texture normal, float amount = 1, ComponentMask mask = ComponentMask.All)
            {
                return new Channel() {
                    TargetTextureChannel = property,
                    ChannelType = Type.NORMAL,
                    Source = normal,
                    Data = amount,
                    WriteMask = mask
                };
            }
        }

        [System.Serializable]
        public struct ColorSource
        {
            public enum Type
            {
                [Tooltip("Color sampled from a texture.")]
                TEXTURE,

                [Tooltip("Single color.")]
                COLOR
            }

            [Tooltip("What is used as the color source?")]
            public Type SourceType;

            [Tooltip("Texture used for as the color source.")]
            public Texture Texture;

            [Tooltip("Solid color used as the color source.")]
            public Color Color;

            // allow implicitly converting a color to a color source
            public static implicit operator ColorSource(Color color)
            {
                return new ColorSource() {
                    SourceType = Type.COLOR,
                    Color = color
                };
            }

            // allow implicitly converting a texture to a color source
            public static implicit operator ColorSource(Texture texture)
            {
                return new ColorSource() {
                    SourceType = Type.TEXTURE,
                    Texture = texture
                };
            }
        }

        [System.Serializable]
        public struct Mask
        {
            [Tooltip("Texture defining the shape of the mask.")]
            public Texture Texture;

            [Tooltip("Color channels of the mask texture used for masking.")]
            public ComponentMask Components;

            public static Mask AlphaMask(Texture texture)
            {
                return new Mask() {
                    Texture = texture,
                    Components = ComponentMask.A
                };
            }

            public static Mask TextureMask(Texture texture, ComponentMask components)
            {
                return new Mask() {
                    Texture = texture,
                    Components = components
                };
            }

            public static Mask None()
            {
                return new Mask() {
                    Components = 0
                };
            }
        }
    }

    [System.Flags]
    public enum ComponentMask
    {
        R = 1 << 0,
        G = 1 << 1,
        B = 1 << 2,
        A = 1 << 3,
        All = R | G | B | A,
        None = 0,
    }

    public static class ComponentExtension
    {
        public static Vector4 ToVec4(this ComponentMask components)
        {
            return new Vector4() {
                x = components.HasFlag(ComponentMask.R) ? 1 : 0,
                y = components.HasFlag(ComponentMask.G) ? 1 : 0,
                z = components.HasFlag(ComponentMask.B) ? 1 : 0,
                w = components.HasFlag(ComponentMask.A) ? 1 : 0,
            };
        }

        public static string ToText(this ComponentMask components)
        {
            if (components == ComponentMask.None)
                return "None";
            var builder = new System.Text.StringBuilder(4);
            if (components.HasFlag(ComponentMask.R))
                builder.Append('R');
            if (components.HasFlag(ComponentMask.G))
                builder.Append('G');
            if (components.HasFlag(ComponentMask.B))
                builder.Append('B');
            if (components.HasFlag(ComponentMask.A))
                builder.Append('A');
            return builder.ToString();
        }
    }
}