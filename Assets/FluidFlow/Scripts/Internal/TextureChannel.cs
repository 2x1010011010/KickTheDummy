using UnityEngine;

namespace FluidFlow
{
    [System.Serializable]
    public struct TextureChannelDescriptor
    {
        [Tooltip("Name of the shader property, controlled by this TextureChannel.")]
        public TextureChannelReference TextureChannelReference;

        [Tooltip("Channel count and precision of the texture.")]
        public TextureChannelFormatReference FormatReference;

        [Tooltip("Initial color of the texture.")]
        public InitializationMode Initialization;

        public TextureChannelDescriptor(TextureChannelReference channel, TextureChannelFormatReference format, InitializationMode initializationMode)
        {
            TextureChannelReference = channel;
            FormatReference = format;
            Initialization = initializationMode;
        }

        public enum InitializationMode
        {
            [Tooltip("The content of the texture channel is copied from the current texture in the defined texture channel.")]
            COPY,
            [Tooltip("The texture is initialized with a black color (0, 0, 0, 0).")]
            BLACK,
            [Tooltip("The texture is initialized with a gray color (.5, .5, .5, .5).")]
            GRAY,
            [Tooltip("The texture is initialized with a white color (1, 1, 1, 1).")]
            WHITE,
            [Tooltip("The texture is initialized with a default bump map (.5, .5, 1, .5).")]
            BUMP,
            [Tooltip("The texture is initialized with a red color (1, 0, 0, 0).")]
            RED
        }
    }
}