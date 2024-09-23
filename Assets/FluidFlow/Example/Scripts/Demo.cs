using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FluidFlow
{
    public class Demo : MonoBehaviour
    {
        public Animator[] Animators;

        [System.Serializable]
        public struct Target
        {
            public GameObject GO;
            public FFCanvas Canvas;
            public FFEffects FluidEffects;
            public FFSimulator Simulator;
            public FFSimulatorParticle SimulatorParticle;
        }
        public Target[] Models;
        private int activeModel = 0;

        public SphereBrush Sphere;
        public CapsuleBrush Capsule;
        public DiscBrush Disc;
        public DecalProjector Projector;

        public Dropdown DrawSelect;
        public Toggle ContinuousToggle;
        public Dropdown ModeSelect;
        public Dropdown ColorSelect;
        public Dropdown TextureSelect;
        public Slider FluidAmount;
        public Slider FadeAmount;
        public Slider Size;

        public Dropdown SimulationModeSelect;
        public Texture NormalTex, BloodTex, Blood1Tex, LogoTex, BrushTex, Brush2Tex, CheckerTex;

        private enum DrawShape { Mouse, Sphere, Capsule, Disc, Projector }
        private DrawShape drawShape;
        private enum DrawMode { Fluid, FluidParticles, Color, Normal }
        private DrawMode drawMode;
        private enum ColorMode { Checker, Red, Green, Blue, Pink, White, Gray, Rainbow, }
        private ColorMode colorMode;
        private enum TextureMode { None, Blood, Blood2, Logo, Brush, Brush2 }
        private TextureMode textureMode;
        public enum FluidSimulationMode { None, Texture, Particle }
        private FluidSimulationMode simulationMode;
        private FFDecal decal = new FFDecal(new FFDecal.Channel[1]);

        private Updater continuousDrawUpdater = new Updater(Updater.Mode.FIXED, .015f);
        private GraphicRaycaster graphicsRaycaster;
        private PointerEventData pointerEventData;
        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

        private void Start()
        {
            graphicsRaycaster = FindObjectOfType<GraphicRaycaster>();
            pointerEventData = new PointerEventData(FindObjectOfType<EventSystem>());
            SelectModel(0);
            SetAnimation(false);
            SetEvaporation(false);
            SetSimulationMode((int)FluidSimulationMode.Texture);
            SetSize(Size.value);
            ContinuousToggle.isOn = true;

            DrawSelect.SetOptions(System.Enum.GetNames(typeof(DrawShape)));
            TextureSelect.SetOptions(System.Enum.GetNames(typeof(TextureMode)));
            TextureSelect.value = (int)TextureMode.Brush2;
            ColorSelect.SetOptions(System.Enum.GetNames(typeof(ColorMode)));
            ColorSelect.value = (int)ColorMode.Red;
            SimulationModeSelect.SetOptions(System.Enum.GetNames(typeof(FluidSimulationMode)));
            SimulationModeSelect.onValueChanged.AddListener(SetSimulationMode);
            SimulationModeSelect.value = (int)FluidSimulationMode.Particle;

            DrawSelect.onValueChanged.AddListener((int i) => UpdateDraw());
            ModeSelect.onValueChanged.AddListener((int i) => UpdateDraw());
            ColorSelect.onValueChanged.AddListener((int i) => UpdateDraw());
            TextureSelect.onValueChanged.AddListener((int i) => UpdateDraw());
            SimulationModeSelect.onValueChanged.AddListener((int i) => UpdateDraw());
            Size.onValueChanged.AddListener(SetSize);
            UpdateDraw();

            continuousDrawUpdater.AddListener(DoDraw);
        }

        private bool MouseOverUI()
        {
            pointerEventData.position = Input.mousePosition;
            raycastResults.Clear();
            graphicsRaycaster.Raycast(pointerEventData, raycastResults);
            return raycastResults.Count > 0;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
                Debug.Break();
            if (!MouseOverUI()) {
                if (ContinuousToggle.isOn && Input.GetMouseButton(0))
                    continuousDrawUpdater.Update();
                else if (!ContinuousToggle.isOn && Input.GetMouseButtonDown(0))
                    continuousDrawUpdater.Invoke();
            }
        }

        public void DoDraw()
        {
            var useProjection = drawShape == DrawShape.Mouse || drawShape == DrawShape.Projector;
            // color source can be implicitly created form color or texture
            var colorSource = colorMode switch {
                ColorMode.Red => (FFDecal.ColorSource)Color.red,
                ColorMode.Green => (FFDecal.ColorSource)Color.green,
                ColorMode.Blue => (FFDecal.ColorSource)Color.blue,
                ColorMode.Pink => (FFDecal.ColorSource)Color.magenta,
                ColorMode.White => (FFDecal.ColorSource)Color.white,
                ColorMode.Gray => (FFDecal.ColorSource)Color.gray,
                ColorMode.Checker => (FFDecal.ColorSource)CheckerTex,// if (useProjection) // texture can only be used as color source, when using decal projection mode
                _ => (FFDecal.ColorSource)Color.HSVToRGB(Time.time % 1f, .9f, .9f),
            };
            if (useProjection) {
                Utility.DebugFrustum(getProjection());
                if (Models[activeModel].SimulatorParticle.enabled) {
                    var color = colorSource.SourceType == FFDecal.ColorSource.Type.COLOR ? colorSource.Color : Color.white;
                    Models[activeModel].SimulatorParticle.ProjectParticles(getProjection(), new Vector2(.4f, 1f), new Vector2(.5f, 3f), color, .6f, (int)FluidAmount.value + 1);
                } else {
                    decal.MaskChannel = textureMode switch {
                        TextureMode.Blood => FFDecal.Mask.AlphaMask(BloodTex),
                        TextureMode.Blood2 => FFDecal.Mask.AlphaMask(Blood1Tex),
                        TextureMode.Logo => FFDecal.Mask.AlphaMask(LogoTex),
                        TextureMode.Brush => FFDecal.Mask.TextureMask(BrushTex, ComponentMask.R),
                        TextureMode.Brush2 => FFDecal.Mask.TextureMask(Brush2Tex, ComponentMask.R),
                        _ => FFDecal.Mask.None(),
                    };
                    decal.Channels[0] = drawMode switch {
                        DrawMode.Fluid => FFDecal.Channel.Fluid("Fluid", colorSource, FluidAmount.value),
                        DrawMode.Normal => FFDecal.Channel.Normal("Normal", NormalTex, 2f),
                        _ => FFDecal.Channel.Color("Color", colorSource),
                    };
                    if (drawMode == DrawMode.Normal)
                        decal.MaskChannel = FFDecal.Mask.AlphaMask(Blood1Tex);  // prevent normal map from overriding complete frustum rect
                    if (drawShape == DrawShape.Mouse) {
                        Models[activeModel].Canvas.ProjectDecal(decal, getProjection());
                    } else {
                        Projector.Draw(Models[activeModel].Canvas, decal);
                    }
                }
            } else {
                string targetChannel = drawMode == DrawMode.Fluid ? "Fluid" : "Color";
                FFBrush brush;
                if (drawMode == DrawMode.Fluid) {
                    brush = FFBrush.Fluid(colorSource.Color, FluidAmount.value, FadeAmount.value);
                } else {
                    brush = FFBrush.SolidColor(colorSource.Color, FadeAmount.value);
                }

                switch (drawShape) {
                    case DrawShape.Sphere:
                        Sphere.Draw(Models[activeModel].Canvas, targetChannel, brush);
                        break;
                    case DrawShape.Capsule:
                        Capsule.Draw(Models[activeModel].Canvas, targetChannel, brush);
                        break;
                    case DrawShape.Disc:
                        Disc.Draw(Models[activeModel].Canvas, targetChannel, brush);
                        break;
                }
            }
        }

        public void UpdateDraw()
        {
            System.Enum.TryParse(DrawSelect.options[DrawSelect.value].text, out drawShape);
            System.Enum.TryParse(ModeSelect.options[ModeSelect.value].text, out drawMode);
            System.Enum.TryParse(ColorSelect.options[ColorSelect.value].text, out colorMode);
            System.Enum.TryParse(TextureSelect.options[TextureSelect.value].text, out textureMode);
            System.Enum.TryParse(SimulationModeSelect.options[SimulationModeSelect.value].text, out simulationMode);

            Projector.enabled = drawShape == DrawShape.Projector;
            Sphere.enabled = drawShape == DrawShape.Sphere;
            Capsule.enabled = drawShape == DrawShape.Capsule;
            Disc.enabled = drawShape == DrawShape.Disc;

            var particleSim = simulationMode == FluidSimulationMode.Particle;
            var useDecal = drawShape == DrawShape.Mouse || drawShape == DrawShape.Projector;
            FluidAmount.transform.parent.gameObject.SetActive(ModeSelect.value == 0 || particleSim);
            FadeAmount.transform.parent.gameObject.SetActive(!useDecal && !particleSim);
            ColorSelect.transform.parent.gameObject.SetActive(!(useDecal && ModeSelect.value == 2) || particleSim);
            TextureSelect.transform.parent.gameObject.SetActive(useDecal && ModeSelect.value != 2 && !particleSim);
            ModeSelect.transform.parent.gameObject.SetActive(!particleSim);

            {
                var modeOpts = new List<string>() {
                    DrawMode.Fluid.ToString(),
                    DrawMode.Color.ToString()
                };
                if (useDecal)
                    modeOpts.Add(DrawMode.Normal.ToString());
                ModeSelect.SetOptions(modeOpts);
                var index = modeOpts.IndexOf(drawMode.ToString());
                ModeSelect.SetValueWithoutNotify(index >= 0 ? index : 0);
                System.Enum.TryParse(ModeSelect.options[ModeSelect.value].text, out drawMode);
            }

            if (simulationMode == FluidSimulationMode.Particle) {
                DrawSelect.SetValueWithoutNotify((int)DrawShape.Mouse);
                if (colorMode == ColorMode.Checker)
                    colorMode = ColorMode.Gray;
            }
        }

        public void SetSize(float size)
        {
            Projector.ProjectionSize = Size.value;
            Sphere.Radius = Size.value * 3;
            Capsule.Radius = Size.value * .4f;
            Capsule.Height = Size.value * 8;
            Disc.Thickness = Size.value * .04f;
            Disc.Radius = Size.value * 6;
        }

        public void SelectModel(int index)
        {
            activeModel = index;
            for (int i = 0; i < Models.Length; i++)
                Models[i].GO.SetActive(i == activeModel);
        }

        public void SetAnimation(bool enabled)
        {
            for (int i = 0; i < Animators.Length; i++)
                Animators[i].enabled = enabled;
        }

        public void ResetTextures()
        {
            Models[activeModel].Canvas.InitializeTextureChannels();
            Models[activeModel].SimulatorParticle.ClearAllParticles();
        }

        public void SetEvaporation(bool enabled)
        {
            for (int i = 0; i < Models.Length; i++)
                Models[i].FluidEffects.UseEvaporation = enabled;
        }

        public void SetBlur(bool enabled)
        {
            for (int i = 0; i < Models.Length; i++)
                Models[i].FluidEffects.UseBlur = enabled;
        }

        public void SetSimulationMode(int index)
        {
            var mode = (FluidSimulationMode)index;
            for (int i = 0; i < Models.Length; i++) {
                Models[i].Simulator.enabled = mode == FluidSimulationMode.Texture;
                Models[i].SimulatorParticle.enabled = mode == FluidSimulationMode.Particle;
            }
        }

        public void FadeOutFluid()
        {
            Models[activeModel].Simulator.FadeOut(.2f, 20);
        }

        public void SaveFluidTex()
        {
            Models[activeModel].Canvas.SaveTextureChannel("_FluidTex", Application.dataPath + "/export.png");
        }

        private FFProjector getProjection()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var size = Size.value;
            return FFProjector.Orthogonal(ray, Vector3.up, size, size, .01f, 10);
        }
    }

    public static class DemoUtil
    {
        public static void SetOptions(this Dropdown dropdown, string[] newOptions)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>(newOptions));
        }

        public static void SetOptions(this Dropdown dropdown, List<string> newOptions)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(newOptions);
        }
    }
}