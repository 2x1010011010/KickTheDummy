using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SurfaceDataSystem
{
	public enum ImpactMode
	{
		Multiple,
		Single
	}

	public enum CollisionType
	{
		Enter, Exit, Stay, TriggerEnter, TriggerExit
	}


	public static class RaycastHitExtensions
	{
		/// <summary>
		/// Play footstep sound from hit and strength
		/// </summary>
		/// <param name="strength">Force of step from 0 to 1</param>
		public static bool PlayFootstep( this RaycastHit hit, float strength = 1 )
		{
			if( Time.timeSinceLevelLoad < 0.1f )
				return false;

			if( !SurfaceData.TryGetSurface( hit, out Surface surface ) )
			{
				PlayFootstepTerrain( hit, strength );
				return false;
			}

			if( !surface.TryGetModule( out SurfaceFootstepsModule footstepsModule ) )
				return false;

			footstepsModule.PlaySound( hit.point, strength );
			return true;
		}


		private static bool PlayFootstepTerrain( this RaycastHit hit, float strength = 1 )
		{
			if( !hit.collider.TryGetComponent( out SurfaceDataTerrain terrain ) )
				return false;

			var surfacesLayers = terrain.GetSurfacesLayersData( hit.point );

			foreach( var surfaceLayer in surfacesLayers.Keys )
			{
				float alpha = surfacesLayers[ surfaceLayer ];
				if( alpha < 0.05f )
					continue;

				if( !surfaceLayer.surface )
					continue;

				if( !surfaceLayer.surface.TryGetModule( out SurfaceFootstepsModule footstepsModule ) )
					continue;

				footstepsModule.PlaySound( hit.point, strength * alpha );
			}

			return true;
		}


		public static void PlayFootstep( this Collision collision, float strength = 1 ) => PlayFootstep( collision.ToRaycastHit(), strength );


		public static void PlayImpact( this Collision collision, CollisionMode collisionMode = CollisionMode.Single, float forceMultiplier = 1 )
		{
			if( Time.timeSinceLevelLoad < 0.1f )
				return;

			CollisionData collisionData = new( collision, collisionMode );

			float volumeMultiplier = 1f / collisionData.ContactsCount;

			for( int i = 0; i < collisionData.ContactsCount; i++ )
			{
				Vector3 position = collisionData.Contacts[ i ].point;
				ContactPointData contactData = collisionData.ContactsData[ i ];
				SurfaceImpactsModule impactsModule = default;


				float intensity = GetCollisionIntensity( contactData, CollisionType.Enter );
				intensity /= 200f;
				intensity *= forceMultiplier;


				if( !contactData.OtherBody )
				{
					if( contactData.OtherSurface ? contactData.OtherSurface.TryGetModule( out impactsModule ) : false )
						impactsModule.PlaySound( position, intensity, volumeMultiplier );
				}

				if( contactData.ThisSurface ? contactData.ThisSurface.TryGetModule( out impactsModule ) : false )
					impactsModule.PlaySound( position, intensity, volumeMultiplier );
			}
		}


		public static void PlaySlide( this Collision collision, CollisionMode collisionMode, Dictionary<ContinuousData, AudioSourcePoolable> continuousAudioSources, float forceMultiplier = 1 ) =>
			new CollisionData( collision, collisionMode ).PlaySlide( collisionMode, continuousAudioSources, forceMultiplier );

		public static void PlaySlide( this CollisionData collisionData, CollisionMode collisionMode, Dictionary<ContinuousData, AudioSourcePoolable> continuousAudioSources, float forceMultiplier = 1 )
		{
			if( Time.timeSinceLevelLoad < 0.1f )
				return;

			float volumeMultiplier = 1f / collisionData.ContactsCount;

			for( int i = 0; i < collisionData.ContactsCount; i++ )
			{
				Vector3 position = collisionData.Contacts[ i ].point;
				ContactPointData contactData = collisionData.ContactsData[ i ];
				SurfaceSlidingModule slideModule = default;


				if( collisionMode == CollisionMode.Single )
					position = contactData.ThisBody.transform.position;


				float intensity = GetCollisionIntensity( contactData, CollisionType.Stay );
				intensity /= 200f;
				intensity *= forceMultiplier;


				if( contactData.OtherSurface ? contactData.OtherSurface.TryGetModule( out slideModule ) : false )
				{
					ContinuousData data = new
					(
						position,
						contactData.OtherCollider.transform.InverseTransformPoint( position ),
						intensity,
						contactData.OtherSurface,
						contactData.ThisCollider,
						contactData.OtherCollider,
						collisionMode
					);
					slideModule.PlaySound( data, continuousAudioSources, volumeMultiplier );
				}

				if( contactData.ThisSurface ? contactData.ThisSurface.TryGetModule( out slideModule ) : false )
				{
					ContinuousData data = new
					(
						position,
						contactData.ThisCollider.transform.InverseTransformPoint( position ),
						intensity,
						contactData.ThisSurface,
						contactData.ThisCollider,
						contactData.OtherCollider,
						collisionMode
					);
					slideModule.PlaySound( data, continuousAudioSources, volumeMultiplier );
				}
			}
		}


		public static float PlayRoll( this Collision collision, CollisionMode collisionMode, Dictionary<ContinuousData, AudioSourcePoolable> continuousAudioSources, float forceMultiplier = 1 ) =>
			new CollisionData( collision, collisionMode ).PlayRoll( collisionMode, continuousAudioSources, forceMultiplier );

		public static float PlayRoll( this CollisionData collisionData, CollisionMode collisionMode, Dictionary<ContinuousData, AudioSourcePoolable> continuousAudioSources, float forceMultiplier = 1 )
		{
			if( Time.timeSinceLevelLoad < 0.1f )
				return 0;

			float volumeMultiplier = 1f / collisionData.ContactsCount;
			float maxRoll = 0;

			for( int i = 0; i < collisionData.ContactsCount; i++ )
			{
				Vector3 position = collisionData.Contacts[ i ].point;
				ContactPointData contactData = collisionData.ContactsData[ i ];
				SurfaceRollingModule module = default;


				Vector3 relativeContactPointVelocity = contactData.ThisVelocityData.TotalPointForce - contactData.OtherVelocityData.TotalPointForce;
				Vector3 rollVelocity = Vector3.zero;
				float roll = 1 - Mathf.Clamp01( relativeContactPointVelocity.magnitude * 0.1f );

				if( roll > maxRoll )
					maxRoll = roll;

				if( roll > 0 )
					rollVelocity = contactData.ThisVelocityData.TangentialForce * roll;


				if( collisionMode == CollisionMode.Single )
					position = contactData.ThisBody.transform.position;


				float intensity = rollVelocity.magnitude;
				intensity /= 200f;
				intensity *= forceMultiplier;


				/*
				if( contactData.OtherSurface ? contactData.OtherSurface.TryGetModule( out slideModule ) : false )
				{
					ContinuousData data = new
					(
						position,
						contactData.OtherCollider.transform.InverseTransformPoint( position ),
						intensity,
						contactData.OtherSurface,
						contactData.ThisCollider,
						contactData.OtherCollider,
						collisionMode
					);
					slideModule.PlaySound( data, continuousAudioSources, volumeMultiplier );
				}
				*/

				if( contactData.ThisSurface ? contactData.ThisSurface.TryGetModule( out module ) : false )
				{
					ContinuousData data = new
					(
						position,
						contactData.ThisCollider.transform.InverseTransformPoint( position ),
						intensity,
						contactData.ThisSurface,
						contactData.ThisCollider,
						contactData.OtherCollider,
						collisionMode
					);
					module.PlaySound( data, continuousAudioSources, volumeMultiplier );
				}
			}

			return maxRoll;
		}


		public static float GetCollisionIntensity( this ContactPointData data, CollisionType collisionType )
		{
			float dotProduct;
			Vector3 velocity = data.RelativeVelocity;
			float velocityMagnitude = velocity.magnitude;

			if( data.Normal.sqrMagnitude == 0 )
				dotProduct = 1;
			else
			{
				Vector3 normalizedVelocity = velocityMagnitude == 0 ? Vector3.zero : velocity / velocityMagnitude;

				if( collisionType == CollisionType.Enter )
					dotProduct = Mathf.Abs( Vector3.Dot( normalizedVelocity, data.Normal ) );
				else
					dotProduct = 1 - Mathf.Abs( Vector3.Dot( normalizedVelocity, data.Normal ) );
			}

			float intensity = dotProduct * velocityMagnitude;
			// float intensity = ( dotProduct + ( 1 - dotProduct ) * ( 1 - coef ) ) * velocityMagnitude;

			return Mathf.Abs( intensity );
		}


		public static RaycastHit ToRaycastHit( this Collision collision, bool reversed = false ) =>
			collision.GetContact( 0 ).ToRaycastHit( reversed );


		public static RaycastHit ToRaycastHit( this ContactPoint contactPoint, bool reversed = false )
		{
			Ray ray = new();
			float range = 0.2f;
			float radius = 0.05f;
			LayerMask layerMask = 1 << contactPoint.otherCollider.gameObject.layer;
			Collider targetCollider = contactPoint.otherCollider;

			if( reversed )
			{
				ray.origin = contactPoint.point - contactPoint.normal * ( range / 2 );
				ray.direction = contactPoint.normal;

				layerMask = 1 << contactPoint.thisCollider.gameObject.layer;
				targetCollider = contactPoint.thisCollider;
			}
			else
			{
				ray.origin = contactPoint.point + contactPoint.normal * ( range / 2 );
				ray.direction = -contactPoint.normal;
			}

			RaycastHit[] hits = new RaycastHit[ 2 ];
			Physics.SphereCastNonAlloc( ray, radius, hits, range, layerMask );

			foreach( RaycastHit hit in hits )
			{
				if( hit.collider == targetCollider )
					return hit;
			}

			return default;
		}
	}
}