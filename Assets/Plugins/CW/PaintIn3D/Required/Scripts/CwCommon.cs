using CW.Common;
using UnityEngine;

namespace PaintIn3D
{
	/// <summary>This class contains some useful methods used by this asset.</summary>
	internal static class CwCommon
	{
		public const string HelpUrlPrefix = "https://carloswilkes.com/Documentation/PaintIn3D#";

		public const string ComponentMenuPrefix = "CW/Paint in 3D/CW ";

#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/CW/Paint in 3D/Preset")]
		private static void CreateAsset()
		{
			PaintCore.CwPreset_Editor.CreateAsset(go =>
				{
					go.AddComponent<MeshFilter>();
					go.AddComponent<MeshRenderer>();
					go.AddComponent<CwPaintableMesh>();
				});
		}
#endif
	}
}