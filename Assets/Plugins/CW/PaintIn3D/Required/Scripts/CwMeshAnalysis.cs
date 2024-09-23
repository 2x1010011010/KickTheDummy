#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CW.Common;

namespace PaintIn3D
{
	/// <summary>This window allows you to examine the UV data of a mesh. This can be accessed from the context menu (⋮ button at top right) of any mesh in the inspector.</summary>
	public class CwMeshAnalysis : EditorWindow
	{
		private Mesh mesh;

		private int coord;

		private int submesh;

		private int mode;

		private float pitch;

		private float yaw;

		private bool ready;

		private int triangleCount;

		private int invalidCount;

		private int partiallyCount;

		private float utilizationPercent;

		private float overlapPercent;

		private Texture2D overlapTex;

		private int outsideCount;

		private int overlapCount;

		private Material overlapMaterial;

		private string[] submeshNames = new string[0];

		private List<int> indices = new List<int>();

		private List<Vector3> positions = new List<Vector3>();

		private List<Vector2> coords = new List<Vector2>();

		private Vector3[] arrayA;

		private List<Vector3> listA = new List<Vector3>();

		private Vector3[] arrayB;

		private List<Vector3> listB = new List<Vector3>();

		private List<float> ratioList = new List<float>();

		private static int _Coord = Shader.PropertyToID("_Coord");

		[MenuItem("Window/CW/Mesh Analysis")]
		public static void OpenWindow()
		{
			OpenWith(null);
		}

#if UNITY_EDITOR
		[MenuItem("CONTEXT/Mesh/Analyze Mesh (Paint in 3D)")]
		public static void Create(MenuCommand menuCommand)
		{
			var mesh = menuCommand.context as Mesh;

			if (mesh != null)
			{
				OpenWith(mesh, 0);
			}
		}
#endif

		public static void OpenWith(GameObject gameObject, Mesh mesh = null, int channel = 0)
		{
			if (mesh == null && gameObject != null)
			{
				var mf = gameObject.GetComponent<MeshFilter>();

				if (mf != null && mf.sharedMesh != null)
				{
					OpenWith(mf.sharedMesh, channel); return;
				}

				var smr = gameObject.GetComponent<SkinnedMeshRenderer>();

				if (smr != null && smr.sharedMesh != null)
				{
					OpenWith(smr.sharedMesh, channel); return;
				}
			}

			OpenWith(null, 0);
		}

		public static void OpenWith(Mesh mesh, int channel)
		{
			var window = GetWindow<CwMeshAnalysis>("Mesh Analysis", true);

			window.mesh  = mesh;
			window.ready = false;
			window.coord = channel;
		}

		private static bool IsOutside(Vector2 coord)
		{
			return coord.x < 0.0f || coord.x > 1.0f || coord.y < 0.0f || coord.y > 1.0f;
		}

		protected virtual void OnDestroy()
		{
			CwHelper.Destroy(overlapTex);
		}

		private static Vector4 IndexToVector(int index)
		{
			switch (index)
			{
				case 0: return new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
				case 1: return new Vector4(0.0f, 1.0f, 0.0f, 0.0f);
				case 2: return new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
				case 3: return new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
			}

			return default(Vector4);
		}

		private static Texture2D GetReadableCopy(RenderTexture texture)
		{
			var newTexture = default(Texture2D);

			if (texture != null)
			{
				newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBAFloat, false, false);

				CwHelper.BeginActive(texture);
					newTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
				CwHelper.EndActive();

				newTexture.Apply();
			}

			return newTexture;
		}

		private void BakeOverlap()
		{
			var desc          = new RenderTextureDescriptor(1024, 1024, RenderTextureFormat.ARGBFloat, 0);
			var renderTexture = RenderTexture.GetTemporary(desc);

			if (overlapMaterial == null)
			{
				overlapMaterial = CwHelper.CreateTempMaterial("Overlap", "Hidden/PaintIn3D/CwOverlap");
			}

			overlapMaterial.SetVector(_Coord, IndexToVector(coord));

			var oldActive = RenderTexture.active;

			RenderTexture.active = renderTexture;

			GL.Clear(true, true, Color.black);

			overlapMaterial.SetPass(0);

			Graphics.DrawMeshNow(mesh, Matrix4x4.identity, submesh);

			foreach (var obj in Selection.objects)
			{
				var otherMesh = obj as Mesh;

				if (otherMesh != null && otherMesh != mesh)
				{
					Graphics.DrawMeshNow(otherMesh, Matrix4x4.identity, submesh);
				}
			}

			RenderTexture.active = oldActive;

			overlapTex = GetReadableCopy(renderTexture);

			RenderTexture.ReleaseTemporary(renderTexture);

			var utilizationCount = 0;
			var overlapCount     = 0;

			for (var y = 0; y < overlapTex.height; y++)
			{
				for (var x = 0; x < overlapTex.width; x++)
				{
					var pixel = CwHelper.ToLinear(overlapTex.GetPixel(x, y));

					if (pixel.r > 0.0f)
					{
						if (pixel.r > 2.0f)
						{
							pixel = Color.red;

							overlapCount += 1;
						}
						else
						{
							pixel = Color.gray;
						}

						utilizationCount += 1;

						overlapTex.SetPixel(x, y, pixel);
					}
				}
			}

			var total = overlapTex.width * overlapTex.height * 0.01f;

			utilizationPercent = utilizationCount / total;
			overlapPercent     = overlapCount / total;

			overlapTex.Apply();
		}

		protected virtual void OnGUI()
		{
			CwEditor.ClearStacks();

			EditorGUILayout.BeginHorizontal();
				var newMesh = (Mesh)EditorGUILayout.ObjectField("Mesh", mesh, typeof(Mesh), false);
				EditorGUI.BeginDisabledGroup(newMesh == null);
					if (GUILayout.Button("Refresh", EditorStyles.miniButton, GUILayout.Width(60)) == true)
					{
						ready = false;
					}
					if (GUILayout.Button("Fix", GUILayout.ExpandWidth(false)) == true)
					{
						CwMeshFixer_Editor.CreateMeshFixerAsset(mesh);
					}
				EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			if (newMesh != mesh)
			{
				ready = false;
				mesh  = newMesh;
			}

			if (mesh != null)
			{
				if (mesh.subMeshCount != submeshNames.Length)
				{
					var submeshNamesList = new List<string>();

					for (var i = 0; i < mesh.subMeshCount; i++)
					{
						submeshNamesList.Add(i.ToString());
					}

					submeshNames = submeshNamesList.ToArray();
				}

				EditorGUILayout.Separator();

				var newSubmesh  = EditorGUILayout.Popup("Submesh", submesh, submeshNames);
				var newCoord    = EditorGUILayout.Popup("Coord", coord, new string[] { "UV0", "UV1", "UV2", "UV3" });
				var newMode     = EditorGUILayout.Popup("Mode", mode, new string[] { "Texcoord", "Triangles" });

				if (mode == 1) // Triangles
				{
					EditorGUILayout.BeginHorizontal();
						var newPitch = EditorGUILayout.FloatField("Pitch", pitch);
						var newYaw   = EditorGUILayout.FloatField("Yaw", yaw);
					EditorGUILayout.EndHorizontal();

					if (newPitch != pitch || newYaw != yaw)
					{
						ready = false;
						pitch = newPitch;
						yaw   = newYaw;
					}
				}

				EditorGUILayout.Separator();

				EditorGUILayout.LabelField("Triangles", EditorStyles.boldLabel);
				EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.BeginHorizontal();
						EditorGUILayout.IntField("Total", triangleCount);
						CwEditor.BeginError(invalidCount > 0);
							EditorGUILayout.IntField("With No UV", invalidCount);
						CwEditor.EndError();
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
						CwEditor.BeginError(outsideCount > 0);
							EditorGUILayout.IntField("Out Of Bounds", outsideCount);
						CwEditor.EndError();
						CwEditor.BeginError(partiallyCount > 0);
							EditorGUILayout.IntField("Partially Out Of Bounds", partiallyCount);
						CwEditor.EndError();
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
						CwEditor.BeginError(utilizationPercent < 40.0f);
							EditorGUILayout.FloatField("Utilization %", utilizationPercent);
						CwEditor.EndError();
						CwEditor.BeginError(overlapPercent > 0);
							EditorGUILayout.FloatField("Overlap %", overlapPercent);
						CwEditor.EndError();
					EditorGUILayout.EndHorizontal();
				EditorGUI.EndDisabledGroup();

				if (coord != newCoord || newSubmesh != submesh || newMode != mode || ready == false)
				{
					ready   = true;
					coord   = newCoord;
					submesh = newSubmesh;
					mode    = newMode;
					
					listA.Clear();
					listB.Clear();
					ratioList.Clear();
					mesh.GetTriangles(indices, submesh);
					mesh.GetVertices(positions);
					mesh.GetUVs(coord, coords);

					triangleCount      = indices.Count / 3;
					invalidCount       = 0;
					outsideCount       = 0;
					partiallyCount     = 0;
					overlapTex         = CwHelper.Destroy(overlapTex);
					utilizationPercent = 0.0f;
					overlapPercent     = 0.0f;

					if (coords.Count > 0)
					{
						if (mode == 0) // Texcoord
						{
							BakeOverlap();
						}

						var rot  = Quaternion.Euler(pitch, yaw, 0.0f);
						var off  = -mesh.bounds.center;
						var mul  = CwHelper.Reciprocal(mesh.bounds.size.magnitude);
						var half = Vector3.one * 0.5f;

						for (var i = 0; i < indices.Count; i += 3)
						{
							var positionA = positions[indices[i + 0]];
							var positionB = positions[indices[i + 1]];
							var positionC = positions[indices[i + 2]];
							var coordA    = coords[indices[i + 0]];
							var coordB    = coords[indices[i + 1]];
							var coordC    = coords[indices[i + 2]];
							var outside   = 0; outside += IsOutside(coordA) ? 1 : 0; outside += IsOutside(coordB) ? 1 : 0; outside += IsOutside(coordC) ? 1 : 0;
							var area      = Vector3.Cross(coordA - coordB, coordA - coordC).sqrMagnitude;
							var invalid   = area <= float.Epsilon;

							if (invalid == true)
							{
								invalidCount++;
							}

							if (outside == 3)
							{
								outsideCount++;
							}

							if (outside == 1 || outside == 2)
							{
								partiallyCount++;
							}

							if (mode == 0) // Texcoord
							{
								listA.Add(coordA); listA.Add(coordB);
								listA.Add(coordB); listA.Add(coordC);
								listA.Add(coordC); listA.Add(coordA);
							}

							if (mode == 1) // Triangles
							{
								positionA = half + rot * (off + positionA) * mul;
								positionB = half + rot * (off + positionB) * mul;
								positionC = half + rot * (off + positionC) * mul;

								positionA.z = positionB.z = positionC.z = 0.0f;

								listA.Add(positionA); listA.Add(positionB);
								listA.Add(positionB); listA.Add(positionC);
								listA.Add(positionC); listA.Add(positionA);

								if (invalid == true)
								{
									listB.Add(positionA); listB.Add(positionB);
									listB.Add(positionB); listB.Add(positionC);
									listB.Add(positionC); listB.Add(positionA);
								}
							}
						}
					}
					else
					{
						invalidCount = triangleCount;
					}

					arrayA = listA.ToArray();
					arrayB = listB.ToArray();
				}

				var rect = EditorGUILayout.BeginVertical(); GUILayout.FlexibleSpace(); EditorGUILayout.EndVertical();
				var pos  = rect.min;
				var siz  = rect.size;

				GUI.Box(rect, "");

				if (mode == 0 && overlapTex != null) // Texcoord
				{
					GUI.DrawTexture(rect, overlapTex);
				}

				Handles.BeginGUI();
					if (listA.Count > 0)
					{
						for (var i = listA.Count - 1; i >= 0; i--)
						{
							var coord = listA[i];

							coord.x = pos.x + siz.x * coord.x;
							coord.y = pos.y + siz.y * (1.0f - coord.y);

							arrayA[i] = coord;
						}

						Handles.DrawLines(arrayA);

						for (var i = listB.Count - 1; i >= 0; i--)
						{
							var coord = listB[i];

							coord.x = pos.x + siz.x * coord.x;
							coord.y = pos.y + siz.y * (1.0f - coord.y);

							arrayB[i] = coord;
						}

						Handles.color = Color.red;
						Handles.DrawLines(arrayB);
					}
				Handles.EndGUI();
			}
			else
			{
				EditorGUILayout.HelpBox("No Mesh Selected.\nTo select a mesh, click Analyze Mesh from the CwPaintable component, or from the Mesh inspector context menu (gear icon at top right).", MessageType.Info);
			}
		}
	}
}
#endif