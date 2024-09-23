using UnityEngine;

namespace FluidFlow
{
    /// <summary>
    /// Example snippets for drawing to a FFCanvas
    /// </summary>
    public class DrawExamples : MonoBehaviour
    {
        public FFCanvas Canvas;
        public TextureChannelReference ColorChannel = "Color";
        public TextureChannelReference FluidChannel = "Fluid";
        public TextureChannelReference NormalChannel = "Normal";

        public KeyCode ActivationKey = KeyCode.Return;
        public KeyCode LeftKey = KeyCode.LeftArrow;
        public KeyCode RightKey = KeyCode.RightArrow;
        public KeyCode ClearKey = KeyCode.Space;

        [Header("Decal")]
        public Texture DecalColorTexture;

        public Texture DecalNormalTexture;

        public Color DecalColor = Color.red;

        public Texture MaskTexture;

        public FFDecalSO DecalScriptableObject;

        [Header("Decal Projection")]
        public Transform Projector;

        public bool Perspective = true;

        [Header("Sphere Brush")]
        public Transform Brush;

        public float BrushSize = .1f;

        public Color BrushColor = Color.red;

        private int testCase = 0;
        private const int testCaseCount = 10;

        [Range(0f, 1f)]
        public float BrushFade = .5f;

        private void Update()
        {
            // change test case
            var change = (Input.GetKeyDown(LeftKey) ? -1 : 0) + (Input.GetKeyDown(RightKey) ? 1 : 0);
            if (change != 0) {
                testCase += change;
                if (testCase >= testCaseCount)
                    testCase = 0;
                if (testCase < 0)
                    testCase = testCaseCount - 1;
                Debug.Log("Current test case: " + (testCase + 1));
            }

            if (Input.GetKeyDown(ClearKey))
                Canvas.InitializeTextureChannels(); // reinitialize textures

            var projector = getProjector();
            Utility.DebugFrustum(projector);

            if (!Input.GetKeyDown(ActivationKey))
                return;

            // use texture as color source for decal channel
            var textureChannel = FFDecal.Channel.Color(ColorChannel, DecalColorTexture);
            // use solid color as color source for decal channel
            var colorChannel = FFDecal.Channel.Color(ColorChannel, DecalColor);
            // decal channel for drawing fluid (allows to specify fluid amount)
            var fluidChannel = FFDecal.Channel.Fluid(FluidChannel, DecalColorTexture, 2f);
            // decal channel for drawing normal maps (allows to specify normal scale)
            var normalChannel = FFDecal.Channel.Normal(NormalChannel, DecalNormalTexture);

            switch (testCase) {
                // DECAL PROJECTION
                case 0:
                    Debug.Log("Project decal texture on the color channel of the canvas.");
                    Canvas.ProjectDecal(textureChannel, projector);
                    break;

                case 1:
                    Debug.Log("Project solid color masked by the ALPHA channel of a texture on the color channel of the canvas.");
                    Canvas.ProjectDecal(new FFDecal(FFDecal.Mask.AlphaMask(DecalColorTexture), colorChannel), projector);
                    break;

                case 2:
                    Debug.Log("Project solid color masked by the RED channel of a texture on the color channel of the canvas.");
                    Canvas.ProjectDecal(new FFDecal(FFDecal.Mask.TextureMask(MaskTexture, ComponentMask.R), colorChannel), projector);
                    break;

                case 3:
                    Debug.Log("Project a normal texture on the normal-map channel of the canvas.");
                    Canvas.ProjectDecal(normalChannel, projector);
                    break;

                case 4:
                    Debug.Log("Project a normal texture and a color texture on the canvas using a single decal.");
                    Canvas.ProjectDecal(new FFDecal(normalChannel, textureChannel), projector);
                    break;

                case 5:
                    Debug.Log("Project a fluid texture onto the fluid channel of the canvas.");
                    Canvas.ProjectDecal(fluidChannel, projector);
                    break;

                case 6:
                    Debug.Log("Project a decal defined by a scriptableObject onto the canvas.");
                    Canvas.ProjectDecal(DecalScriptableObject, projector);
                    break;

                // BRUSHES (sphere used as example, others work similarly)
                case 7:
                    Debug.Log("Draw a sphere of solid color onto the color channel of the canvas.");
                    Canvas.DrawSphere(ColorChannel, BrushColor, Brush.position, BrushSize);
                    break;

                case 8:
                    Debug.Log("Draw a sphere of color with fade onto the color channel of the canvas.");
                    Canvas.DrawSphere(ColorChannel, FFBrush.SolidColor(BrushColor, BrushFade), Brush.position, BrushSize);
                    break;

                case 9:
                    Debug.Log("Draw a sphere of fluid with fade onto the color channel of the canvas.");
                    Canvas.DrawSphere(FluidChannel, FFBrush.Fluid(BrushColor, 2f, BrushFade), Brush.position, BrushSize);
                    break;
            }
        }

        private FFProjector getProjector()
        {
            if (Perspective) {
                return FFProjector.Perspective(Projector, 45, 1, .1f, 1f);
            } else {
                return FFProjector.Orthogonal(Projector, .1f, .1f, .1f, 1f);
            }
        }
    }
}