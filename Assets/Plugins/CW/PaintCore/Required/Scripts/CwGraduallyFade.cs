using UnityEngine;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component allows you to fade the pixels of the specified CwPaintableTexture.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwGraduallyFade")]
	[AddComponentMenu(CwCommon.ComponentMenuPrefix + "Gradually Fade")]
	public class CwGraduallyFade : MonoBehaviour
	{
		/// <summary>This allows you to choose which paintable texture will be modified by this component.</summary>
		public CwPaintableTexture PaintableTexture { set { paintableTexture = value; } get { return paintableTexture; } } [SerializeField] private CwPaintableTexture paintableTexture;

		/// <summary>Once this component has accumulated this amount of fade, it will be applied to the <b>PaintableTexture</b>. The lower this value, the smoother the fading will appear, but also the higher the performance cost.</summary>
		public float Threshold { set { threshold = value; } get { return threshold; } } [Range(0.0f, 1.0f)] [SerializeField] private float threshold = 0.02f;

		/// <summary>The speed of the fading.
		/// 1 = 1 Second.
		/// 2 = 0.5 Seconds.</summary>
		public float Speed { set { speed = value; } get { return speed; } } [SerializeField] private float speed = 1.0f;

		/// <summary>This component will paint using this blending mode.
		/// NOTE: See <b>CwBlendMode</b> documentation for more information.</summary>
		public CwBlendMode BlendMode { set { blendMode = value; } get { return blendMode; } } [SerializeField] private CwBlendMode blendMode = CwBlendMode.ReplaceOriginal(Vector4.one);

		/// <summary>The texture that will be faded toward.</summary>
		public Texture BlendTexture { set { blendTexture = value; } get { return blendTexture; } } [SerializeField] private Texture blendTexture;

		/// <summary>The paintable texture that will be faded toward.</summary>
		public CwPaintableTexture BlendPaintableTexture { set { blendPaintableTexture = value; } get { return blendPaintableTexture; } } [SerializeField] private CwPaintableTexture blendPaintableTexture;

		/// <summary>The color that will be faded toward.</summary>
		public Color BlendColor { set { blendColor = value; } get { return blendColor; } } [SerializeField] private Color blendColor = Color.white;

		/// <summary>If you want the gradually fade effect to be masked by a texture, then specify it here.</summary>
		public Texture MaskTexture { set { maskTexture = value; } get { return maskTexture; } } [SerializeField] private Texture maskTexture;

		/// <summary>If you want the gradually fade effect to be masked by a paintable texture, then specify it here.</summary>
		public CwPaintableTexture MaskPaintableTexture { set { maskPaintableTexture = value; } get { return maskPaintableTexture; } } [SerializeField] private CwPaintableTexture maskPaintableTexture;

		/// <summary>This allows you to specify the channel of the mask.</summary>
		public CwChannel MaskChannel { set { maskChannel = value; } get { return maskChannel; } } [SerializeField] private CwChannel maskChannel;

		[SerializeField]
		private float counter;

		protected virtual void Update()
		{
			if (paintableTexture != null && paintableTexture.Activated == true)
			{
				if (speed > 0.0f)
				{
					counter += speed * Time.deltaTime;
				}

				if (counter >= threshold)
				{
					var step = Mathf.FloorToInt(counter * 255.0f);

					if (step > 0)
					{
						var change             = step / 255.0f;
						var finalMaskTexture   = default(Texture);
						var finalTargetTexture = default(Texture);

						counter -= change;

						if (blendPaintableTexture != null && blendPaintableTexture.Activated == true)
						{
							finalTargetTexture = blendPaintableTexture.Current;
						}
						else if (blendTexture != null)
						{
							finalTargetTexture = blendTexture;
						}

						CwCommandFill.Instance.SetState(false, 0);
						CwCommandFill.Instance.SetMaterial(blendMode, finalTargetTexture, blendColor, Mathf.Min(change, 1.0f), Mathf.Min(change, 1.0f));

						var command = CwPaintableManager.Submit(CwCommandFill.Instance, paintableTexture.Model, paintableTexture);

						if (maskPaintableTexture != null && maskPaintableTexture.Activated == true)
						{
							finalMaskTexture = maskPaintableTexture.Current;
						}
						else if (maskTexture != null)
						{
							finalMaskTexture = maskTexture;
						}

						command.LocalMaskTexture = finalMaskTexture;
						command.LocalMaskChannel = CwCommon.IndexToVector((int)maskChannel);
					}
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwGraduallyFade;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwGraduallyFade_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.PaintableTexture == null));
				Draw("paintableTexture", "This allows you to choose which paintable texture will be modified by this component.");
			EndError();
			BeginError(Any(tgts, t => t.Threshold <= 0.0f));
				Draw("threshold", "Once this component has accumulated this amount of fade, it will be applied to the PaintableTexture. The lower this value, the smoother the fading will appear, but also the higher the performance cost.");
			EndError();
			BeginError(Any(tgts, t => t.Speed <= 0.0f));
				Draw("speed", "The speed of the fading.\n\n1 = 1 Second.\n\n2 = 0.5 Seconds.");
			EndError();

			Separator();

			Draw("blendMode", "This component will paint using this blending mode.\n\nNOTE: See CwBlendMode documentation for more information.");
			Draw("blendTexture", "The texture that will be faded toward.");
			Draw("blendPaintableTexture", "The paintable texture that will be faded toward.");
			if (Any(tgts, t => t.BlendTexture != null && t.BlendPaintableTexture != null))
			{
				Warning("You have set two blend textures. Only the BlendPaintableTexture will be used.");
			}
			if (Any(tgts, t => t.PaintableTexture != null && t.PaintableTexture == t.BlendTexture))
			{
				Error("The PaintableTexture and BlendPaintableTexture are the same.");
			}
			Draw("blendColor", "The color that will be faded toward.");

			Separator();

			EditorGUILayout.BeginHorizontal();
				Draw("maskTexture", "If you want the gradually fade effect to be masked by a texture, then specify it here.");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maskChannel"), GUIContent.none, GUILayout.Width(50));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
				Draw("maskPaintableTexture", "If you want the gradually fade effect to be masked by a paintable texture, then specify it here.");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maskChannel"), GUIContent.none, GUILayout.Width(50));
			EditorGUILayout.EndHorizontal();
			if (Any(tgts, t => t.MaskTexture != null && t.MaskTexture != null && t.MaskPaintableTexture != null))
			{
				Warning("You have set two mask textures. Only the MaskPaintableTexture will be used.");
			}
			if (Any(tgts, t => t.PaintableTexture != null && t.PaintableTexture == t.MaskPaintableTexture))
			{
				Error("The PaintableTexture and MaskPaintableTexture are the same.");
			}
			if (Any(tgts, t => t.BlendPaintableTexture != null && t.BlendPaintableTexture == t.MaskPaintableTexture))
			{
				Error("The TargetPaintableTexture and MaskPaintableTexture are the same.");
			}
		}
	}
}
#endif