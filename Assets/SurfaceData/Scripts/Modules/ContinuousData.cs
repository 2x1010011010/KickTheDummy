using System;
using UnityEngine;


namespace SurfaceDataSystem
{
    public struct ContinuousData
    {
        public Vector3 WorldPosition  { get; private set; }
		public Vector3 LocalPosition  { get; private set; }
		public float   Force		  { get; private set; }
		public Surface Surface		  { get; private set; }

		public Collider ThisCollider  { get; private set; }
		public Collider OtherCollider { get; private set; }


		private readonly int _hashCode;
		public int CollisionHashCode { get; private set; }


		public ContinuousData( Vector3 position, Vector3 localPosition, float force, Surface surface, Collider thisCollider, Collider otherCollider, CollisionMode collisionMode )
		{
			WorldPosition = position;
			LocalPosition = localPosition;
			Force = force;
			Surface = surface;

			ThisCollider = thisCollider;
			OtherCollider = otherCollider;

			Vector3 roundedPosition = localPosition;
			roundedPosition.x = Mathf.Round( roundedPosition.x * 10f ) / 10f;
			roundedPosition.y = Mathf.Round( roundedPosition.y * 10f ) / 10f;
			roundedPosition.z = Mathf.Round( roundedPosition.z * 10f ) / 10f;

			CollisionHashCode = HashCode.Combine( ThisCollider.GetHashCode(), OtherCollider.GetHashCode() );
			if( collisionMode == CollisionMode.Single )
				_hashCode = HashCode.Combine( ThisCollider.GetHashCode(), OtherCollider.GetHashCode(), Surface.GetHashCode() );
			else
				_hashCode = HashCode.Combine( roundedPosition.GetHashCode(), Surface.GetHashCode() );
		}


		public override readonly int GetHashCode()
		{
			return _hashCode;
		}

		public override readonly bool Equals( object obj )
		{
			return GetHashCode().Equals( obj.GetHashCode() );
		}
	}
}