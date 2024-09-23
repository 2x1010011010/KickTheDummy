using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SurfaceDataSystem
{
	public static class SurfaceData
	{
		internal const string ResourcesPath = "SurfaceData";

		private static SurfaceDataPhysicsMapping _physicsMapping;
		private static SurfaceDataMaterialsMapping _materialsMapping;


		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
		private static void Init()
		{
			_physicsMapping = GlobalScriptableObject.GetOrCreateInstance<SurfaceDataPhysicsMapping>();
			_materialsMapping = GlobalScriptableObject.GetOrCreateInstance<SurfaceDataMaterialsMapping>();
		}


		public static bool TryGetSurface( RaycastHit hit, out Surface surface )
		{
			if( _physicsMapping.TryGetSurface( hit.collider.sharedMaterial, out surface ) ) return true;

			if( _materialsMapping.TryGetSurface( hit, out surface ) ) return true;

			return false;
		}


		public static bool TryGetSurface( ContactPoint contactPoint, out Surface surface, bool reversed = false )
		{
			if( reversed )
			{
				if( _physicsMapping.TryGetSurface( contactPoint.thisCollider.sharedMaterial, out surface ) )
					return true;
			}
			else
			{
				if( _physicsMapping.TryGetSurface( contactPoint.otherCollider.sharedMaterial, out surface ) )
					return true;
			}

			RaycastHit raycast = contactPoint.ToRaycastHit( reversed );

			if( raycast.collider == null )
				return false;

			if( _materialsMapping.TryGetSurface( raycast, out surface ) ) return true;

			return false;
		}
	}
}