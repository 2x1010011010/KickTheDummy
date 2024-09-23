using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SurfaceDataSystem
{
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public class SurfaceDataPhysicsMapping : GlobalScriptableObject
	{
		private static SurfaceDataPhysicsMapping _instance;


#if UNITY_EDITOR
		static SurfaceDataPhysicsMapping()
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
				_instance = GetOrCreateInstance<SurfaceDataPhysicsMapping>();
		}



		[SerializeField] private List<SurfaceDataPhysicMaterial> m_materials = new();
		private Dictionary<PhysicMaterial, Surface> _cache = new();


		private void Initialization()
		{

		}


		public bool TryGetSurface( PhysicMaterial physicsMaterial, out Surface surface )
		{
			surface = default;

			if( physicsMaterial == null )
				return false;

			if( _cache.TryGetValue( physicsMaterial, out surface ) )
				return true;

			foreach( var material in m_materials )
			{
				if( material.material.name == physicsMaterial.name )
				{
					surface = material.surface;
					_cache.Add( physicsMaterial, surface );
					return true;
				}
			}

			return false;
		}
	}
}