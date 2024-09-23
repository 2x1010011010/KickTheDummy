using UnityEngine;

namespace FluidFlow
{
    [System.Serializable]
    public struct FFBrush
    {
        public enum Type
        {
            [Tooltip("Brush for drawing color.")]
            COLOR,

            [Tooltip("Brush for drawing fluid.")]
            FLUID
        }

        [Tooltip("What is the brush used for?")]
        public Type BrushType;

        [Tooltip("Color of the brush.")]
        public Color Color;

        [Tooltip("Optional data. Currently only used for fluid amount.")]
        public float Data;

        [Tooltip("Intensity reduction of the brush's edges.")]
        [Range(0f, 1f)]
        public float Fade;

        public FFBrush(Type brushType, Color color, float data, float fade)
        {
            BrushType = brushType;
            Color = color;
            Data = data;
            Fade = fade;
        }

        // allow implicitly converting a color to a solid color brush
        public static implicit operator FFBrush(Color color)
        {
            return SolidColor(color);
        }

        /// <summary>
        /// Convenience function for creating a solid color brush.
        /// </summary>
        public static FFBrush SolidColor(Color color, float fade = 0f)
        {
            return new FFBrush(Type.COLOR, color, 0, fade);
        }

        /// <summary>
        /// Convenience function for creating a fluid brush.
        /// </summary>
        public static FFBrush Fluid(Color color, float fluidAmount, float fade = 0f)
        {
            return new FFBrush(Type.FLUID, color, fluidAmount, fade);
        }
    }
}