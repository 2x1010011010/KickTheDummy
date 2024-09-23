using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace SurfaceDataSystem
{
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public class SurfaceDataMaterialsMapping : GlobalScriptableObject
	{
		private static SurfaceDataMaterialsMapping _instance;


#if UNITY_EDITOR
		static SurfaceDataMaterialsMapping()
		{
			EditorApplication.update += CheckInstance;
		}
#endif


		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
		private static void Init()
		{
			CheckInstance();
			_instance.Initialization();
		}


		private static void CheckInstance()
		{
			if( _instance == null )
				_instance = GetOrCreateInstance<SurfaceDataMaterialsMapping>();
		}


		[SerializeField] private List<SurfaceDataMaterial> m_materials = new();
		public List<SurfaceDataMaterial> Materials => m_materials;


		private readonly Dictionary<GameObject, MeshRenderer> _cache = new();
		private readonly Dictionary<MeshRenderer, Mesh> _cachedMeshes = new();


		private void Initialization()
		{

		}


		public bool TryGetSurface( RaycastHit hit, out Surface surface )
		{
			surface = null;
			if( !TryGetRenderer( hit.collider, out MeshRenderer renderer ) )
				return false;

			Material material;
			if( renderer.sharedMaterials.Length > 1 )
			{
				if( !TryGetMesh( renderer, out Mesh mesh ) )
					return false;

				if( !renderer.TryGetMaterial( mesh, hit, out material ) )
					return false;
			}
			else
				material = renderer.sharedMaterial;

			if( material == null )
				return false;


			foreach( var mat in m_materials )
			{
				if( mat.material.Equals( material ) )
				{
					surface = mat.surface;
					return surface != null;
				}
			}

			return false;
		}

		public bool TryGetSurface( Collision collision, out Surface surface ) => TryGetSurface( collision.ToRaycastHit(), out surface );



		private bool TryGetRenderer( Collider collider, out MeshRenderer renderer )
		{
			if( _cache.TryGetValue( collider.gameObject, out renderer ) )
				return true;

			if( collider.transform.parent != null )
				if( _cache.TryGetValue( collider.transform.parent.gameObject, out renderer ) )
					return true;

			renderer = collider.GetComponentInParent<MeshRenderer>();
			if( !renderer )
				return false;

			_cache.Add( collider.gameObject, renderer );
			return true;
		}


		private bool TryGetMesh( MeshRenderer renderer, out Mesh mesh )
		{
			if( !_cachedMeshes.TryGetValue( renderer, out mesh ) )
			{
				if( !renderer.TryGetComponent( out MeshFilter meshFilter ) )
					return false;
				else
				{
					mesh = meshFilter.sharedMesh;
					return mesh != null;
				}
			}

			return false;
		}
	}
}