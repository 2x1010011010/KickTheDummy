using UnityEngine;
using UnityEngine.Events;
using Unity.Collections;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component allows you to read the paint color at a hit point. A hit point can be found using a companion component like: CwHitScreen, CwHitBetween.
	/// NOTE: This component only works when you hit a non-convex MeshCollider that has UV data.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwReadColor")]
	[AddComponentMenu(CwCommon.ComponentHitMenuPrefix + "Read Color")]
	public class CwReadColor : MonoBehaviour, IHitCoord
	{
		public enum ReadType
		{
			Immediate,
			Async
		}

		[System.Serializable] public class ColorEvent : UnityEvent<Color> {}

		/// <summary>Only the <b>CwPaintableTexture</b> components with a matching group will be painted by this component.</summary>
		public CwGroup Group { set { group = value; } get { return group; } } [SerializeField] private CwGroup group;

		/// <summary>Should the color be read during preview painting too?</summary>
		public bool Preview { set { preview = value; } get { return preview; } } [SerializeField] private bool preview;

		/// <summary>How should the texture be read?
		/// Immediate = The reading method will block until the pixel is fetched from the GPU.
		/// Async = The pixel value will be read after some time, giving you better performance.</summary>
		public ReadType Read { set { read = value; } get { return read; } } [SerializeField] private ReadType read;

		/// <summary>The last read color value.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color;

		/// <summary>When a color is read, this event will be invoked.
		/// Color = The color that was read.</summary>
		public ColorEvent OnColor { get { if (onColor == null) onColor = new ColorEvent(); return onColor; } } [SerializeField] private ColorEvent onColor;

		[SerializeField]
		private CwReader reader;

		[SerializeField]
		private RenderTexture buffer;

		public void HandleHitCoord(bool preview, int priority, float pressure, int seed, CwHit hit, Quaternion rotation)
		{
			if (preview == true && this.preview == false)
			{
				return;
			}

			var model = hit.Transform.GetComponent<CwModel>();

			if (model != null)
			{
				var paintableTextures = model.FindPaintableTextures(group);

				for (var i = paintableTextures.Count - 1; i >= 0; i--)
				{
					var paintableTexture = paintableTextures[i];
					var coord            = paintableTexture.GetCoord(ref hit);

					switch (read)
					{
						case ReadType.Immediate:
						{
							color = CwCommon.GetPixel(paintableTexture.Current, coord);

							if (onColor != null)
							{
								onColor.Invoke(color);
							}
						}
						break;

						case ReadType.Async:
						{
							if (reader == null)
							{
								reader = new CwReader();

								reader.OnComplete += HandleComplete;
							}

							if (buffer == null)
							{
								buffer = new RenderTexture(1, 1, 0);
							}

							if (reader.Requested == false)
							{
								var x = coord.x * paintableTexture.Current.width;
								var y = coord.y * paintableTexture.Current.height;

								Graphics.CopyTexture(paintableTexture.Current, 0, 0, (int)x, (int)y, 1, 1, buffer, 0, 0, 0, 0);
							
								reader.Request(buffer, 0, true);
							}
						}
						break;
					}
				}
			}
		}

		protected virtual void OnEnable()
		{
			if (reader != null)
			{
				reader.OnComplete += HandleComplete;
			}
		}

		protected virtual void OnDisable()
		{
			if (reader != null)
			{
				reader.OnComplete -= HandleComplete;
			}
		}

		protected virtual void OnDestroy()
		{
			if (reader != null)
			{
				reader.Release();
			}
		}

		protected virtual void Update()
		{
			if (reader != null)
			{
				reader.UpdateRequest();
			}
		}

		private void HandleComplete(NativeArray<Color32> pixels)
		{
			if (onColor != null)
			{
				onColor.Invoke(pixels[0]);
			}
		}
	}
}

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwReadColor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwReadColor_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("group", "Only the CwPaintableTexture components with a matching group will be read by this component.");
			Draw("preview", "Should the color be read during preview painting too?");
			Draw("read", "How should the texture be read?\nImmediate = The reading method will block until the pixel is fetched from the GPU.\nAsync = The pixel value will be read after some time, giving you better performance.");
			Draw("color", "The last read color value.");

			Separator();

			Draw("onColor");
		}
	}
}
#endif