using UnityEngine;
using CW.Common;

namespace PaintIn3D
{
	/// <summary>This component renders scene depth to a RenderTexture. This scene depth can be set in a <b>P3dPaint___</b> component's <b>Advanced/DepthMask</b> setting, which allows you to paint on the first surface in the view of the specified camera.</summary>
	[HelpURL(P3dCommon.HelpUrlPrefix + "P3dRenderDepth")]
	[AddComponentMenu(P3dCommon.ComponentMenuPrefix + "Render Depth")]
	public class P3dRenderDepth : MonoBehaviour
	{
		/// <summary>The camera whose depth information will be read.</summary>
		public Camera SourceCamera { set { sourceCamera = value; } get { return sourceCamera; } } [SerializeField] private Camera sourceCamera;

		/// <summary>The transformation matrix of the camera when the depth texture was generated.</summary>
		public Matrix4x4 SourceMatrix { set { sourceMatrix = value; } get { return sourceMatrix; } } [SerializeField] private Matrix4x4 sourceMatrix;

		/// <summary>The RenderTexture where the depth information will be stored.</summary>
		public RenderTexture TargetTexture { set { targetTexture = value; } get { return targetTexture; } } [SerializeField] private RenderTexture targetTexture;

		/// <summary>If this is 0, the RenderTexture size will match the viewport. If it's above 0, then the RenderTexture size will be set to the viewport size divided by this value.</summary>
		public int ResizeAndDownscale { set { resizeAndDownscale = value; } get { return resizeAndDownscale; } } [SerializeField] private int resizeAndDownscale;

		/// <summary>The rendered depth must be at least this mant units different from the painted surface for the paint to be masked out.</summary>
		public float Bias { set { bias = value; } get { return bias; } } [SerializeField] private float bias = 0.0000001f;

		/// <summary>Should the scene depth be rendered in <b>Start</b>?</summary>
		public bool ReadInStart { set { readInStart = value; } get { return readInStart; } } [SerializeField] private bool readInStart = true;

		/// <summary>Should the scene depth be rendered every frame in <b>Update</b>?</summary>
		public bool ReadInUpdate { set { readInUpdate = value; } get { return readInUpdate; } } [SerializeField] private bool readInUpdate;

		private Shader cachedShader;

		private static int _P3D_DepthMatrix = Shader.PropertyToID("_P3D_DepthMatrix");

		/// <summary>This method will update the <b>TargetTexture</b> with what the <b>SourceCamera</b> currently sees.</summary>
		[ContextMenu("Read Now")]
		public void ReadNow()
		{
			if (cachedShader == null)
			{
				cachedShader = Shader.Find("Hidden/Paint in 3D/RenderDepth");
			}

			if (sourceCamera != null && targetTexture != null)
			{
				if (resizeAndDownscale > 0)
				{
					var newWidth  = sourceCamera.pixelWidth  / resizeAndDownscale;
					var newHeight = sourceCamera.pixelHeight / resizeAndDownscale;

					if (targetTexture.width != newWidth || targetTexture.height != newHeight)
					{
						if (targetTexture.IsCreated() == true)
						{
							targetTexture.Release();
						}

						targetTexture.width  = newWidth;
						targetTexture.height = newHeight;

						targetTexture.Create();
					}
				}

				var oldDTM = sourceCamera.depthTextureMode;
				var oldTT  = sourceCamera.targetTexture;
				var oldBG  = sourceCamera.backgroundColor;
				var oldCF  = sourceCamera.clearFlags;

				sourceMatrix = Matrix4x4.Translate(new Vector3(0.5f, 0.5f, 0.5f)) * Matrix4x4.Scale(new Vector3(0.5f, 0.5f, 0.5f)) * sourceCamera.projectionMatrix * sourceCamera.worldToCameraMatrix;

				Shader.SetGlobalMatrix(_P3D_DepthMatrix, sourceMatrix);
				
				sourceCamera.clearFlags      = CameraClearFlags.SolidColor;
				sourceCamera.backgroundColor = Color.black;
				sourceCamera.targetTexture   = targetTexture;
				sourceCamera.RenderWithShader(cachedShader, "RenderType");

				sourceCamera.clearFlags       = oldCF;
				sourceCamera.backgroundColor  = oldBG;
				sourceCamera.targetTexture    = oldTT;
				sourceCamera.depthTextureMode = oldDTM;
			}
		}

		protected virtual void Start()
		{
			if (readInStart == true)
			{
				ReadNow();
			}
		}

		protected virtual void Update()
		{
			if (readInUpdate == true)
			{
				ReadNow();
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			var m = sourceMatrix.inverse;
			Gizmos.color = Color.white;
			Gizmos.DrawLine(m.MultiplyPoint(new Vector3(0.0f, 0.0f, 0.0f)), m.MultiplyPoint(new Vector3(1.0f, 0.0f, 0.0f)));
			Gizmos.DrawLine(m.MultiplyPoint(new Vector3(0.0f, 0.0f, 0.0f)), m.MultiplyPoint(new Vector3(0.0f, 1.0f, 0.0f)));
				
			Gizmos.DrawLine(m.MultiplyPoint(new Vector3(1.0f, 1.0f, 0.0f)), m.MultiplyPoint(new Vector3(0.0f, 1.0f, 0.0f)));
			Gizmos.DrawLine(m.MultiplyPoint(new Vector3(1.0f, 1.0f, 0.0f)), m.MultiplyPoint(new Vector3(1.0f, 0.0f, 0.0f)));

			Gizmos.color = Color.black;
			Gizmos.DrawLine(m.MultiplyPoint(new Vector3(0.0f, 0.0f, 1.0f)), m.MultiplyPoint(new Vector3(1.0f, 0.0f, 1.0f)));
			Gizmos.DrawLine(m.MultiplyPoint(new Vector3(0.0f, 0.0f, 1.0f)), m.MultiplyPoint(new Vector3(0.0f, 1.0f, 1.0f)));
				
			Gizmos.DrawLine(m.MultiplyPoint(new Vector3(1.0f, 1.0f, 1.0f)), m.MultiplyPoint(new Vector3(0.0f, 1.0f, 1.0f)));
			Gizmos.DrawLine(m.MultiplyPoint(new Vector3(1.0f, 1.0f, 1.0f)), m.MultiplyPoint(new Vector3(1.0f, 0.0f, 1.0f)));
		}
#endif
	}
}

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = P3dRenderDepth;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class P3dRenderDepth_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.SourceCamera == null));
				Draw("sourceCamera", "The camera whose depth information will be read.");
			EndError();
			BeginError(Any(tgts, t => t.TargetTexture == null));
				Draw("targetTexture", "The RenderTexture where the depth information will be stored.");
			EndError();
			Draw("resizeAndDownscale", "If this is 0, the RenderTexture size will match the viewport. If it's above 0, then the RenderTexture size will be set to the viewport size divided by this value.");
			Draw("bias", "The rendered depth must be at least this mant units different from the painted surface for the paint to be masked out.");
			Draw("readInStart", "Should the scene depth be rendered in <b>Start</b>?");
			Draw("readInUpdate", "Should the scene depth be rendered every frame in <b>Update</b>?");

			Separator();

			if (Button("Read Now") == true)
			{
				Each(tgts, t => t.ReadNow(), true, true);
			}
		}
	}
}
#endif