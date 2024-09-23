using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using PaintCore;

namespace PaintIn3D
{
	/// <summary>This tool allows you to process any mesh so that it can be painted.
	/// The fixed meshes will be placed as a child of this tool in your Project window.
	/// To use the fixed mesh, drag and drop it into your MeshFilter or SkinnedMeshRenderer.
	/// This tool can be accessed from the context menu (⋮ button at top right) of any mesh/model inspector.</summary>
	[ExecuteInEditMode]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwMeshFixer")]
	public class CwMeshFixer : ScriptableObject
	{
		[System.Serializable]
		public class Pair
		{
			/// <summary>The original mesh.</summary>
			public Mesh Source;

			/// <summary>The fixed mesh.</summary>
			public Mesh Output;
		}

		private class Ring
		{
			public List<Point> Points = new List<Point>();

			public Point GetPoint(int index)
			{
				if (index < 0)
				{
					index = Points.Count - 1;
				}
				else if (index >= Points.Count)
				{
					index = 0;
				}

				return Points[index];
			}
		}

		private class Edge
		{
			public bool  Used;
			public Point PointA;
			public Point PointB;
			public bool  Flip;

			public bool Match(Point a, Point b)
			{
				if (PointA == a && PointB == b) return true;
				if (PointA == b && PointB == a) return true;

				return false;
			}
		}

		private class Point
		{
			public int       Index;
			public Vector2   Coord;
			public Insertion Outer;

			public List<Edge> Edges = new List<Edge>();
		}

		private class Insertion
		{
			public int     Index;
			public int     NewIndex;
			public Vector2 NewCoord;
		}

		// LEGACY
		[SerializeField] private Mesh source;

		// LEGACY
		[SerializeField] private Mesh mesh;

		/// <summary>The meshes we will fix.</summary>
		public List<Pair> Meshes { get { if (meshes == null) meshes = new List<Pair>(); return meshes; } } [SerializeField] private List<Pair> meshes;

		/// <summary>The UV channel whose seams will be fixed.</summary>
		public CwCoord Coord { set { coord = value; } get { return coord; } } [SerializeField] private CwCoord coord;

		/// <summary>Generate UV data for the meshes?</summary>
		public bool GenerateUV { set { generateUV = value; } get { return generateUV; } } [SerializeField] private bool generateUV;

		/// <summary>Maximum allowed angle distortion (0..1).</summary>
		public float AngleError { set { angleError = value; } get { return angleError; } } [SerializeField] [Range(0.01f, 1.0f)] private float angleError = 0.08f;

		/// <summary>Maximum allowed area distortion (0..1).</summary>
		public float AreaError { set { areaError = value; } get { return areaError; } } [SerializeField] [Range(0.01f, 1.0f)] private float areaError = 0.15f;

		/// <summary>This angle (in degrees) or greater between triangles will cause seam to be created.</summary>
		public float HardAngle { set { hardAngle = value; } get { return hardAngle; } } [SerializeField] [Range(10, 180)] private float hardAngle = 88;

		/// <summary>How much uv-islands will be padded.</summary>
		public float PackMargin { set { packMargin = value; } get { return packMargin; } } [SerializeField] [Range(0.0001f, 0.1f)] private float packMargin = 0.00390625f;

		/// <summary>If UV data is shifted out of the 0..1 range (e.g. 3..4), it will be wrapped back to 0..1.
		/// However, if these wrapped triangles still go outside the 0..1 range, should they be wrapped to the other side so it can still be fully painted?</summary>
		public bool FixOverflow { set { fixOverflow = value; } get { return fixOverflow; } } [SerializeField] private bool fixOverflow = true;

		/// <summary>Fix the seams of the meshes?</summary>
		public bool FixSeams { set { fixSeams = value; } get { return fixSeams; } } [SerializeField] private bool fixSeams = true;

		/// <summary>The thickness of the UV borders in the fixed mesh.</summary>
		public float Border { set { border = value; } get { return border; } } [SerializeField] private float border = 0.005f;

#if UNITY_EDITOR
		public static UnityEditor.UnwrapParam UnwrapParams = new UnityEditor.UnwrapParam();
#endif

		private static Dictionary<Mesh, Mesh> cacheFirst   = new Dictionary<Mesh, Mesh>();
		private static Dictionary<Mesh, Mesh> cacheSecond  = new Dictionary<Mesh, Mesh>();
		private static Dictionary<Mesh, Mesh> cacheThird   = new Dictionary<Mesh, Mesh>();
		private static Dictionary<Mesh, Mesh> cacheFourth  = new Dictionary<Mesh, Mesh>();

		public static Mesh GetCachedMesh(Mesh source, CwCoord coord, bool allowGeneration = true)
		{
			switch (coord)
			{
				case CwCoord.First:  return TryGetCachedMesh(cacheFirst , source, coord, allowGeneration);
				case CwCoord.Second: return TryGetCachedMesh(cacheSecond, source, coord, allowGeneration);
				case CwCoord.Third:  return TryGetCachedMesh(cacheThird , source, coord, allowGeneration);
				case CwCoord.Fourth: return TryGetCachedMesh(cacheFourth, source, coord, allowGeneration);
			}

			return default(Mesh);
		}

		private static Mesh TryGetCachedMesh(Dictionary<Mesh, Mesh> cache, Mesh source, CwCoord coord, bool allowGeneration = true)
		{
			var fixedMesh = default(Mesh);

			if (source != null && cache.TryGetValue(source, out fixedMesh) == false && allowGeneration == true)
			{
				fixedMesh = new Mesh();

				fixedMesh.name = source.name + " (Auto Fixed Seams)";

				Generate(source, fixedMesh, false, true, true, coord, 0.005f);

				cache.Add(source, fixedMesh);
			}

			return fixedMesh;
		}

		/// <summary>This allows you to add a mesh to the seam fixer.
		/// NOTE: You must later call <b>Generate</b> to seam fix the added meshes.</summary>
		public void AddMesh(Mesh mesh)
		{
			if (mesh != null)
			{
				Meshes.Add(new Pair() { Source = mesh });
			}
		}

		public void ConvertLegacy()
		{
			if (source != null)
			{
				Meshes.Add(new Pair() { Source = source, Output = mesh });

				source = null;
				mesh   = null;
			}
		}

#if UNITY_EDITOR
		protected virtual void Reset()
		{
			fixSeams = false;
		}
#endif

		[ContextMenu("Generate")]
		public void Generate()
		{
#if UNITY_EDITOR
			UnityEditor.Undo.RecordObject(this, "Generate Seam Fix");

			UnwrapParams.angleError = angleError;
			UnwrapParams.areaError  = areaError;
			UnwrapParams.hardAngle  = hardAngle;
			UnwrapParams.packMargin = packMargin;
#endif

			if (meshes != null)
			{
				foreach (var pair in meshes)
				{
					if (pair.Source != null)
					{
						if (pair.Output == null)
						{
							pair.Output = new Mesh();
						}

						pair.Output.name = pair.Source.name + " (Fixed)";

						Generate(pair.Source, pair.Output, generateUV, fixOverflow, fixSeams, coord, border);
					}
					else
					{
						DestroyImmediate(pair.Output);

						pair.Output = null;
					}
				}
			}
#if UNITY_EDITOR
			if (CwHelper.IsAsset(this) == true)
			{
				var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(this));

				for (var i = 0; i < assets.Length; i++)
				{
					var assetMesh = assets[i] as Mesh;

					if (assetMesh != null)
					{
						if (meshes == null || meshes.Exists(p => p.Output == assetMesh) == false)
						{
							DestroyImmediate(assetMesh, true);
						}
					}
				}

				if (meshes != null)
				{
					foreach (var pair in meshes)
					{
						if (pair.Output != null && CwHelper.IsAsset(pair.Output) == false)
						{
							UnityEditor.AssetDatabase.AddObjectToAsset(pair.Output, this);

							UnityEditor.AssetDatabase.SaveAssets();
						}
					}
				}
			}

			if (CwHelper.IsAsset(this) == true)
			{
				CwHelper.ReimportAsset(this);
			}

			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}

		/// <summary>This static method allows you to fix any mesh at runtime.
		/// NOTE: The <b>generateUV</b> setting is only available in the editor.</summary>
		public static void Generate(Mesh source, Mesh output, bool generateUV, bool fixOverflow, bool fixSeams, CwCoord coord, float border)
		{
#if UNITY_EDITOR
			if (generateUV == true)
			{
				var clone = Instantiate(source);

				if (coord == CwCoord.Second)
				{
					UnityEditor.Unwrapping.GenerateSecondaryUVSet(clone, UnwrapParams);
				}
				else // NOTE: Contents of uv2 lost, recover it?
				{
					UnityEditor.Unwrapping.GenerateSecondaryUVSet(clone, UnwrapParams);

					switch (coord)
					{
						case CwCoord.First:  clone.uv  = clone.uv2; break;
						case CwCoord.Third:  clone.uv3 = clone.uv2; break;
						case CwCoord.Fourth: clone.uv4 = clone.uv2; break;
					}
				}

				DoGenerate(clone, output, fixOverflow, fixSeams, coord, border);

				DestroyImmediate(clone);

				return;
			}
#endif
			DoGenerate(source, output, fixOverflow, fixSeams, coord, border);
		}

		
		private static void DoGenerate(Mesh source, Mesh output, bool fixOverflow, bool fixSeams, CwCoord coord, float border)
		{
			if (source != null && output != null && border != 0.0f)
			{
				output.Clear(false);

				var points     = new Dictionary<Vector2, Point>();
				var insertions = new List<Insertion>();
				var submeshes  = new List<List<int>>();
				var coords     = default(Vector2[]);

				switch (coord)
				{
					case CwCoord.First : coords = source.uv ; break;
					case CwCoord.Second: coords = source.uv2; break;
					case CwCoord.Third : coords = source.uv3; break;
					case CwCoord.Fourth: coords = source.uv4; break;
				}

				if (coords.Length > 0)
				{
					var sumX = 0.0;
					var sumY = 0.0;

					for (var i = 0; i < coords.Length; i++)
					{
						sumX += coords[i].x;
						sumY += coords[i].y;
					}

					sumX /= coords.Length;
					sumY /= coords.Length;

					var shiftX = Mathf.FloorToInt((float)sumX);
					var shiftY = Mathf.FloorToInt((float)sumY);

					if (shiftX != 0 || shiftY != 0)
					{
						var delta = new Vector2(-shiftX, -shiftY);

						for (var i = 0; i < coords.Length; i++)
						{
							coords[i] += delta;
						}
					}
				}

				if (fixSeams == true)
				{
					for (var i = 0; i < coords.Length; i++)
					{
						var uv = coords[i];

						uv *= 16384.0f;
						uv.x = Mathf.Floor(uv.x);
						uv.y = Mathf.Floor(uv.y);
						uv /= 16384.0f;

						coords[i] = uv;

						AddPoint(points, uv, i);
					}

					var vertexIndex = source.vertexCount;

					for (var i = 0; i < source.subMeshCount; i++)
					{
						var edges   = new List<Edge>();
						var indices = new List<int>(); source.GetTriangles(indices, i);

						foreach (var pair in points)
						{
							pair.Value.Edges.Clear();
						}

						if (coords.Length > 0)
						{
							for (var j = 0; j < indices.Count; j += 3)
							{
								var pointA = points[coords[indices[j + 0]]];
								var pointB = points[coords[indices[j + 1]]];
								var pointC = points[coords[indices[j + 2]]];

								AddTriangle(edges, pointA, pointB, pointC);
							}
						}

						for (var j = edges.Count - 1; j >= 0; j--)
						{
							var edge = edges[j];

							if (edge.Used == false)
							{
								edge.Used = true;

								var ring = new Ring();

								ring.Points.Add(edge.PointA);
								ring.Points.Add(edge.PointB);

								TraceEdges(ring, edge.PointB);

								if (ring.Points.Count > 2)
								{
									for (var k = 0; k < ring.Points.Count; k++)
									{
										var pointA = ring.GetPoint(k - 1);
										var pointB = ring.GetPoint(k    );
										var pointC = ring.GetPoint(k + 1);

										var normalA   = (pointA.Coord - pointB.Coord).normalized; normalA = new Vector2(-normalA.y, normalA.x);
										var normalB   = (pointB.Coord - pointC.Coord).normalized; normalB = new Vector2(-normalB.y, normalB.x);
										var average   = normalA + normalB;
										var magnitude = average.sqrMagnitude;
										var insertion = new Insertion();

										insertion.Index    = pointB.Index;
										insertion.NewCoord = pointB.Coord;
										insertion.NewIndex = vertexIndex++;

										pointB.Outer = insertion;

										if (magnitude > 0.0f)
										{
											magnitude = Mathf.Sqrt(magnitude);

											insertion.NewCoord += (average / magnitude) * border;
										}

										insertions.Add(insertion);
									}

									for (var k = 0; k < ring.Points.Count; k++)
									{
										var pointA = ring.GetPoint(k    );
										var pointB = ring.GetPoint(k + 1);
										var indexA = pointA.Index;
										var indexB = pointB.Index;
										var indexC = pointA.Outer.NewIndex;
										var indexD = pointB.Outer.NewIndex;

										if (edge.Flip == true)
										{
											indices.Add(indexA);
											indices.Add(indexC);
											indices.Add(indexB);

											indices.Add(indexD);
											indices.Add(indexB);
											indices.Add(indexC);
										}
										else
										{
											indices.Add(indexA);
											indices.Add(indexB);
											indices.Add(indexC);

											indices.Add(indexD);
											indices.Add(indexC);
											indices.Add(indexB);
										}
									}
								}
							}
						}

						submeshes.Add(indices);
					}
				}
				else
				{
					for (var i = 0; i < source.subMeshCount; i++)
					{
						var indices = new List<int>(); source.GetTriangles(indices, i);

						submeshes.Add(indices);
					}
				}

				AddFixSeamData(source, output, submeshes, insertions, coord);
			}
		}

		private static void AddCoord(List<Vector4> coords, Insertion insertion, bool write)
		{
			var uv = coords[insertion.Index];

			if (write == true)
			{
				uv.x = insertion.NewCoord.x;
				uv.y = insertion.NewCoord.y;
			}

			coords.Add(uv);
		}

		private static void AddFixSeamData(Mesh source, Mesh output, List<List<int>> submeshes, List<Insertion> insertions, CwCoord coord)
		{
			output.bindposes    = source.bindposes;
			output.bounds       = source.bounds;
			output.subMeshCount = source.subMeshCount;
			output.indexFormat  = source.indexFormat;

			if (source.vertexCount + insertions.Count >= 65535)
			{
				output.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			}

			var weights      = new List<BoneWeight>(); source.GetBoneWeights(weights);
			var colors       = new List<Color32>();    source.GetColors(colors);
			var normals      = new List<Vector3>();    source.GetNormals(normals);
			var tangents     = new List<Vector4>();    source.GetTangents(tangents);
			var coords0      = new List<Vector4>();    source.GetUVs(0, coords0);
			var coords1      = new List<Vector4>();    source.GetUVs(1, coords1);
			var coords2      = new List<Vector4>();    source.GetUVs(2, coords2);
			var coords3      = new List<Vector4>();    source.GetUVs(3, coords3);
			var positions    = new List<Vector3>();    source.GetVertices(positions);
			var boneVertices = new List<byte>(source.GetBonesPerVertex());
			var boneWeights  = new List<BoneWeight1>(source.GetAllBoneWeights());
			var boneIndices  = new List<int>();

			if (boneVertices.Count > 0)
			{
				var total = 0;

				foreach (var count in boneVertices)
				{
					boneIndices.Add(total);
				
					total += count;
				}

				weights.Clear();
			}

			foreach (var insertion in insertions)
			{
				if (boneVertices.Count > 0)
				{
					var index = boneIndices[insertion.Index];
					var count = boneVertices[insertion.Index];

					boneVertices.Add(count);

					for (var i = 0; i < count; i++)
					{
						boneWeights.Add(boneWeights[index + i]);
					}
				}

				if (weights.Count > 0) weights.Add(weights[insertion.Index]);

				if (colors.Count > 0) colors.Add(colors[insertion.Index]);

				if (normals.Count > 0) normals.Add(normals[insertion.Index]);

				if (tangents.Count > 0) tangents.Add(tangents[insertion.Index]);

				if (coords0.Count > 0) AddCoord(coords0, insertion, coord == CwCoord.First);

				if (coords1.Count > 0) AddCoord(coords1, insertion, coord == CwCoord.Second);

				if (coords2.Count > 0) AddCoord(coords2, insertion, coord == CwCoord.Third);

				if (coords3.Count > 0) AddCoord(coords3, insertion, coord == CwCoord.Fourth);

				positions.Add(positions[insertion.Index]);
			}

			output.SetVertices(positions);

			if (weights.Count > 0)
			{
				output.boneWeights = weights.ToArray();
			}

			if (boneVertices.Count > 0)
			{
				var na1 = new Unity.Collections.NativeArray<byte>(boneVertices.ToArray(), Unity.Collections.Allocator.Temp);
				var na2 = new Unity.Collections.NativeArray<BoneWeight1>(boneWeights.ToArray(), Unity.Collections.Allocator.Temp);
				output.SetBoneWeights(na1, na2);
				na2.Dispose();
				na1.Dispose();
			}

			output.SetColors(colors);
			output.SetNormals(normals);
			output.SetTangents(tangents);
			output.SetUVs(0, coords0);
			output.SetUVs(1, coords1);
			output.SetUVs(2, coords2);
			output.SetUVs(3, coords3);

			var deltaVertices = new List<Vector3>();
			var deltaNormals = new List<Vector3>();
			var deltaTangents = new List<Vector3>();

			if (source.blendShapeCount > 0)
			{
				var tempDeltaVertices = new Vector3[source.vertexCount];
				var tempDeltaNormals  = new Vector3[source.vertexCount];
				var tempDeltaTangents = new Vector3[source.vertexCount];

				for (var i = 0; i < source.blendShapeCount; i++)
				{
					var shapeName  = source.GetBlendShapeName(i);
					var frameCount = source.GetBlendShapeFrameCount(i);

					for (var j = 0; j < frameCount; j++)
					{
						source.GetBlendShapeFrameVertices(i, j, tempDeltaVertices, tempDeltaNormals, tempDeltaTangents);

						deltaVertices.Clear();
						deltaNormals.Clear();
						deltaTangents.Clear();

						deltaVertices.AddRange(tempDeltaVertices);
						deltaNormals.AddRange(tempDeltaNormals);
						deltaTangents.AddRange(tempDeltaTangents);

						foreach (var insertion in insertions)
						{
							deltaVertices.Add(deltaVertices[insertion.Index]);
							deltaNormals.Add(deltaNormals[insertion.Index]);
							deltaTangents.Add(deltaTangents[insertion.Index]);
						}

						output.AddBlendShapeFrame(shapeName, source.GetBlendShapeFrameWeight(i, j), deltaVertices.ToArray(), deltaNormals.ToArray(), deltaTangents.ToArray());
					}
				}
			}

			for (var i = 0; i < submeshes.Count; i++)
			{
				output.SetTriangles(submeshes[i], i);
			}
		}

		private static void TraceEdges(Ring ring, Point point)
		{
			var edges = point.Edges;

			for (var i = 0; i < edges.Count; i++)
			{
				var edge = edges[i];

				if (edge.Used == false && edge.PointA == point)
				{
					edge.Used = true;

					if (edge.PointB == ring.Points[0])
					{
						return;
					}

					ring.Points.Add(edge.PointB);
					
					point = edge.PointB;
					edges = edge.PointB.Edges;
					i     = -1;
				}
			}
		}

		private static void AddPoint(Dictionary<Vector2, Point> points, Vector2 coord, int index)
		{
			var point = default(Point);

			if (points.TryGetValue(coord, out point) == false)
			{
				point = new Point();

				point.Coord = coord;
				point.Index = index;

				points.Add(coord, point);
			}
		}

		private static void AddTriangle(List<Edge> edges, Point pointA, Point pointB, Point pointC)
		{
			var ab = pointB.Coord - pointA.Coord;
			var ac = pointC.Coord - pointA.Coord;

			// Ignore degenerate triangles
			if (Vector3.Cross(ab, ac).sqrMagnitude >= 0.0f)
			{
				// Clockwise?
				if (((pointB.Coord.x - pointA.Coord.x) * (pointC.Coord.y - pointA.Coord.y) - (pointC.Coord.x - pointA.Coord.x) * (pointB.Coord.y - pointA.Coord.y)) >= 0.0f)
				{
					AddTriangle2(edges, pointA, pointB, pointC, true);
				}
				else
				{
					AddTriangle2(edges, pointC, pointB, pointA, false);
				}
			}
		}

		private static void AddTriangle2(List<Edge> edges, Point pointA, Point pointB, Point pointC, bool flip)
		{
			RemoveOrAddEdge(edges, pointA, pointB, flip);
			RemoveOrAddEdge(edges, pointB, pointC, flip);
			RemoveOrAddEdge(edges, pointC, pointA, flip);
		}

		private static void RemoveOrAddEdge(List<Edge> edges, Point pointA, Point pointB, bool flip)
		{
			for (var i = 0; i < pointA.Edges.Count; i++)
			{
				var edge = pointA.Edges[i];

				if (edge.Match(pointA, pointB) == true)
				{
					edge.Used = true; return;
				}
			}

			for (var i = 0; i < pointB.Edges.Count; i++)
			{
				var edge = pointB.Edges[i];

				if (edge.Match(pointA, pointB) == true)
				{
					edge.Used = true; return;
				}
			}

			var newEdge = new Edge();

			newEdge.PointA = pointA;
			newEdge.PointB = pointB;
			newEdge.Flip   = flip;

			pointA.Edges.Add(newEdge);
			pointB.Edges.Add(newEdge);

			edges.Add(newEdge);
		}
	}
}

#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwMeshFixer;

	[CustomEditor(typeof(TARGET))]
	public class CwMeshFixer_Editor : CwEditor
	{
		private Texture2D sourceTexture;

		private enum SquareSizes
		{
			Square64    =    64,
			Square128   =   128,
			Square256   =   256,
			Square512   =   512,
			Square1024  =  1024,
			Square2048  =  2048,
			Square4096  =  4096,
			Square8192  =  8192,
			Square16384 = 16384,
		}

		private SquareSizes newSize = SquareSizes.Square1024;

		private Dictionary<Mesh, Mesh> pairs = new Dictionary<Mesh, Mesh>();

		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			EditorGUILayout.HelpBox("This tool allows you to process any mesh so that it can be painted. The fixed meshes will be placed as a child of this tool in your Project window. To use the fixed mesh, drag and drop it into your MeshFilter or SkinnedMeshRenderer.", MessageType.Info);

			Separator();

			Each(tgts, t => t.ConvertLegacy()); serializedObject.Update();

			var sMeshes = serializedObject.FindProperty("meshes");
			var sDel    = -1;
			var missing = false;

			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Meshes");
				if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
				{
					sMeshes.InsertArrayElementAtIndex(sMeshes.arraySize);
				}
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel++;
				for (var i = 0; i < tgt.Meshes.Count; i++)
				{
					var sSource = sMeshes.GetArrayElementAtIndex(i).FindPropertyRelative("Source");

					EditorGUILayout.BeginHorizontal();
						BeginError(sSource.objectReferenceValue == null);
							EditorGUILayout.PropertyField(sSource, GUIContent.none);
						EndError();
						var sourceMesh = tgt.Meshes[i].Source;
						BeginDisabled(sourceMesh == null);
							if (GUILayout.Button("Analyze Old", EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
							{
								CwMeshAnalysis.OpenWith(sourceMesh, 0);
							}
						EndDisabled();
						var outputMesh = tgt.Meshes[i].Output;
						if (outputMesh == null)
						{
							missing = true;
						}
						BeginDisabled(outputMesh == null);
							if (GUILayout.Button("Analyze New", EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
							{
								CwMeshAnalysis.OpenWith(outputMesh, 0);
							}
							//EditorGUILayout.ObjectField(GUIContent.none, outputMesh, typeof(Mesh), false, GUILayout.Width(80));
						EndDisabled();
						if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.ExpandWidth(false)) == true)
						{
							sDel = i;
						}
					EditorGUILayout.EndHorizontal();
				}
			EditorGUI.indentLevel--;

			if (sDel >= 0)
			{
				sMeshes.DeleteArrayElementAtIndex(sDel);
			}

			Separator();

			Draw("coord", "The UV channel whose seams will be fixed.");

			Separator();

			BeginDisabled();
				EditorGUILayout.Toggle(new GUIContent("Recenter UV", "If UV data is shifted out of the 0..1 range (e.g. 3..4), it will be wrapped back to 0..1."), true);
			EndDisabled();

			Separator();

			Draw("generateUV", "Generate UV data for the meshes?");
			if (Any(tgts, t => t.GenerateUV == true))
			{
				BeginIndent();
					Draw("angleError", "Maximum allowed angle distortion (0..1).");
					Draw("areaError", "Maximum allowed area distortion (0..1).");
					Draw("hardAngle", "This angle (in degrees) or greater between triangles will cause seam to be created.");
					Draw("packMargin", "How much uv-islands will be padded.");
				EndIndent();
			}

			//Separator();

			//Draw("fixOverflow", "If UV data is shifted out of the 0..1 range (e.g. 3..4), it will be wrapped back to 0..1.\n\nHowever, if these wrapped triangles still go outside the 0..1 range, should they be wrapped to the other side so it can still be fully painted?");

			Separator();

			Draw("fixSeams", "Fix the seams of the meshes?");
			if (Any(tgts, t => t.FixSeams == true))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.Border <= 0.0f));
						Draw("border", "The thickness of the UV borders in the fixed mesh.");
					EndError();
				EndIndent();
			}

			Separator();

			BeginColor(Color.green, missing);
				if (Button("Generate") == true)
				{
					Each(tgts, t => t.Generate());
				}
			EndColor();

			Separator();
			Separator();

			EditorGUILayout.LabelField("REMAP TEXTURE", EditorStyles.boldLabel);

			sourceTexture = (Texture2D)EditorGUILayout.ObjectField(sourceTexture, typeof(Texture2D), false);

			if (sourceTexture != null)
			{
				newSize = (SquareSizes)EditorGUILayout.EnumPopup("New Size", newSize);

				Separator();

				EditorGUILayout.LabelField("REMAP WITH MESH", EditorStyles.boldLabel);

				for (var i = 0; i < tgt.Meshes.Count; i++)
				{
					var pair = tgt.Meshes[i];

					if (pair != null && pair.Source != null && pair.Output != null)
					{
						if (GUILayout.Button(pair.Source.name) == true)
						{
							Remap(sourceTexture, pair.Source, pair.Output, (int)newSize);
						}
					}
				}
			}

			if (tgts.Length == 1)
			{
				Separator();
				Separator();

				EditorGUILayout.LabelField("SWAP MESHES", EditorStyles.boldLabel);

				pairs.Clear();

				foreach (var pair in tgt.Meshes)
				{
					if (pair.Source != null && pair.Output != null)
					{
						pairs.Add(pair.Source, pair.Output);
					}
				}

				Mesh output;

				var count = 0;

				foreach (var mm in FindObjectsOfType<CwMeshModel>())
				{
					var mf  = mm.GetComponent<MeshFilter>();
					var smr = mm.GetComponent<SkinnedMeshRenderer>();
					var m   = mf != null ? mf.sharedMesh : (smr != null ? smr.sharedMesh : null);

					if (m != null && pairs.TryGetValue(m, out output) == true)
					{
						EditorGUILayout.BeginHorizontal();
							BeginDisabled();
								EditorGUILayout.ObjectField(mm.gameObject, typeof(GameObject), true);
							EndDisabled();
							BeginColor(Color.green);
								if (GUILayout.Button("Swap", GUILayout.ExpandWidth(false)) == true)
								{
									if (mf != null)
									{
										Undo.RecordObject(mf, "Swap Mesh"); mf.sharedMesh = output; EditorUtility.SetDirty(mf);
									}
									else if (smr != null)
									{
										Undo.RecordObject(smr, "Swap Mesh"); smr.sharedMesh = output; EditorUtility.SetDirty(smr);
									}
								}
							EndColor();
						EditorGUILayout.EndHorizontal();

						count += 1;
					}
				}

				if (count == 0)
				{
					Info("If your scene contains any P3dPaintableMesh/P3dPaintableMeshAtlas components using the original non-fixed mesh, then they will be listed here.");
				}
			}
		}

		private static void Remap(Texture2D sourceTexture, Mesh oldMesh, Mesh newMesh, int newSize)
		{
			var path = AssetDatabase.GetAssetPath(sourceTexture);
			var name = sourceTexture.name;
			var dir  = string.IsNullOrEmpty(path) == false ? System.IO.Path.GetDirectoryName(path) : "Assets";

			if (string.IsNullOrEmpty(path) == false)
			{
				name = System.IO.Path.GetFileNameWithoutExtension(path);
			}

			name += " (Remapped)";

			path = EditorUtility.SaveFilePanelInProject("Export Texture", name, "png", "Export Your Texture", dir);

			if (string.IsNullOrEmpty(path) == false)
			{
				var remapTexture = CwRemap.Remap(sourceTexture, oldMesh, newMesh, newSize);

				CwRemap.Export(remapTexture, path, sourceTexture);

				DestroyImmediate(remapTexture);
			}
		}

		[MenuItem("CONTEXT/Mesh/Fix Mesh (Paint in 3D)")]
		[MenuItem("CONTEXT/ModelImporter/Fix Mesh (Paint in 3D)")]
		public static void Create(MenuCommand menuCommand)
		{
			var sources = new List<Mesh>();
			var mesh    = menuCommand.context as Mesh;
			var name    = "";

			if (mesh != null)
			{
				sources.Add(mesh);

				name = mesh.name;
			}
			else
			{
				var modelImporter = menuCommand.context as ModelImporter;

				if (modelImporter != null)
				{
					var assets = AssetDatabase.LoadAllAssetsAtPath(modelImporter.assetPath);

					for (var i = 0; i < assets.Length; i++)
					{
						var assetMesh = assets[i] as Mesh;

						if (assetMesh != null)
						{
							sources.Add(assetMesh);
						}
					}

					name = System.IO.Path.GetFileNameWithoutExtension(modelImporter.assetPath);
				}
			}
			
			if (sources.Count > 0)
			{
				var path = AssetDatabase.GetAssetPath(menuCommand.context);

				if (string.IsNullOrEmpty(path) == false)
				{
					path = System.IO.Path.GetDirectoryName(path);
				}
				else
				{
					path = "Assets";
				}

				path += "/Mesh Fixer (" + name + ").asset";

				var instance = CreateInstance<CwMeshFixer>();

				foreach (var source in sources)
				{
					instance.AddMesh(source);
				}

				ProjectWindowUtil.CreateAsset(instance, path);
			}
		}

		[MenuItem("Assets/Create/CW/Paint in 3D/Mesh Fixer")]
		private static void CreateAsset()
		{
			var guids = Selection.assetGUIDs;

			CreateMeshFixerAsset(default(Mesh), guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : default(string));
		}

		public static void CreateMeshFixerAsset(Mesh mesh)
		{
			CreateMeshFixerAsset(mesh, AssetDatabase.GetAssetPath(mesh));
		}

		public static void CreateMeshFixerAsset(Mesh mesh, string path)
		{
			var asset = CreateInstance<CwMeshFixer>();
			var name  = "Mesh Fixer";

			if (string.IsNullOrEmpty(path) == true || path.StartsWith("Library/", System.StringComparison.InvariantCultureIgnoreCase))
			{
				path = "Assets";
			}
			else if (AssetDatabase.IsValidFolder(path) == false)
			{
				path = System.IO.Path.GetDirectoryName(path);
			}

			if (mesh != null)
			{
				var meshPath      = AssetDatabase.GetAssetPath(mesh);
				var modelImporter = AssetImporter.GetAtPath(meshPath);

				if (modelImporter != null)
				{
					foreach (var o in AssetDatabase.LoadAllAssetsAtPath(modelImporter.assetPath))
					{
						var assetMesh = o as Mesh;

						if (assetMesh is Mesh)
						{
							asset.AddMesh(assetMesh);
						}
					}

					name += " (" + System.IO.Path.GetFileNameWithoutExtension(modelImporter.assetPath) + ")";
				}
				else
				{
					name += " (" + mesh.name + ")";

					asset.AddMesh(mesh);
				}
			}

			var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".asset");

			AssetDatabase.CreateAsset(asset, assetPathAndName);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();

			Selection.activeObject = asset; EditorGUIUtility.PingObject(asset);
		}
	}
}
#endif