using UnityEngine;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component implements the replace paint mode, which will replace all pixels in the specified texture.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwPaintReplace")]
	[AddComponentMenu(CwCommon.ComponentHitMenuPrefix + "Paint Replace")]
	public class CwPaintReplace : MonoBehaviour, IHitCoord
	{
		/// <summary>Only the <b>CwPaintableTexture</b> components with a matching group will be painted by this component.</summary>
		public CwGroup Group { set { group = value; } get { return group; } } [SerializeField] private CwGroup group;

		/// <summary>The texture that will be painted.</summary>
		public Texture Texture { set { texture = value; } get { return texture; } } [SerializeField] private Texture texture;

		/// <summary>The color of the paint.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color = Color.white;

		/// <summary>This stores a list of all modifiers used to change the way this component applies paint (e.g. <b>CwModifyColorRandom</b>).</summary>
		public CwModifierList Modifiers { get { if (modifiers == null) modifiers = new CwModifierList(); return modifiers; } } [SerializeField] private CwModifierList modifiers;

		public void HandleHitCoord(bool preview, int priority, float pressure, int seed, CwHit hit, Quaternion rotation)
		{
			var model = hit.Transform.GetComponentInParent<CwModel>();

			if (model != null)
			{
				var paintableTextures = model.FindPaintableTextures(group);

				if (paintableTextures.Count > 0)
				{
					var finalColor   = color;
					var finalTexture = texture;

					if (modifiers != null && modifiers.Count > 0)
					{
						CwHelper.BeginSeed(seed);
							modifiers.ModifyColor(ref finalColor, preview, pressure);
							modifiers.ModifyTexture(ref finalTexture, preview, pressure);
						CwHelper.EndSeed();
					}

					CwCommandReplace.Instance.SetState(preview, priority);
					CwCommandReplace.Instance.SetMaterial(finalTexture, finalColor);

					for (var i = paintableTextures.Count - 1; i >= 0; i--)
					{
						var paintableTexture = paintableTextures[i];

						CwPaintableManager.Submit(CwCommandReplace.Instance, model, paintableTexture);
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
	using TARGET = CwPaintReplace;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwPaintReplace_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("group", "Only the CwPaintableTexture components with a matching group will be painted by this component.");

			Separator();

			Draw("texture", "The texture that will be painted.");
			Draw("color", "The color of the paint.");

			Separator();

			tgt.Modifiers.DrawEditorLayout(serializedObject, target, "Color", "Texture");
		}
	}
}
#endif