using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SurfaceDataSystem
{
    public enum CollisionMode
    {
        Single,
        Multiple
    }

    public struct CollisionData
    {
        public float              Impulse       { get; private set; }
        public ContactPoint[]     Contacts      { get; private set; }
        public int                ContactsCount { get; private set; }
        public ContactPointData[] ContactsData  { get; private set; }


		public CollisionData( Collision collision, CollisionMode collisionMode = CollisionMode.Multiple )
        {
            Impulse = ( collision.impulse / Time.fixedDeltaTime ).magnitude;

            if( collisionMode == CollisionMode.Single )
            {
                Contacts = new ContactPoint[ 1 ];
                Contacts[ 0 ] = collision.GetContact( 0 );
            }
            else
                Contacts = collision.contacts;

			ContactsCount = Contacts.Length;


            ContactsData = new ContactPointData[ ContactsCount ];
            for( int i = 0; i < ContactsCount; i++ )
                ContactsData[ i ] = new( Contacts[ i ] );
		}
    }


    public struct ContactPointData
    { 
        public Surface         ThisSurface       { get; private set; }
        public Surface         OtherSurface      { get; private set; }
        public SurfaceDataBody ThisBody          { get; private set; }
		public SurfaceDataBody OtherBody         { get; private set; }
		public VelocityData    ThisVelocityData  { get; private set; }
		public VelocityData    OtherVelocityData { get; private set; }
        public Collider        ThisCollider      { get; private set; }
		public Collider        OtherCollider     { get; private set; }
		public Vector3         RelativeVelocity  { get; private set; }
        public Vector3         Point             { get; private set; }
		public Vector3         Normal            { get; private set; }


		public ContactPointData( ContactPoint contactPoint )
        {
            Point = contactPoint.point;
            Normal = contactPoint.normal;


			SurfaceData.TryGetSurface( contactPoint, out Surface otherSurface );
			OtherSurface = otherSurface;

			SurfaceData.TryGetSurface( contactPoint, out Surface thisSurface, true );
			ThisSurface = thisSurface;

			ThisVelocityData = GetVelocityData( Point, contactPoint.thisCollider, out SurfaceDataBody thisBody );
			OtherVelocityData = GetVelocityData( Point, contactPoint.otherCollider, out SurfaceDataBody otherBody );

            ThisCollider = contactPoint.thisCollider;
			OtherCollider = contactPoint.otherCollider;

			ThisBody = thisBody;
            OtherBody = otherBody;

			RelativeVelocity = ThisVelocityData.TotalPointForce - OtherVelocityData.TotalPointForce;
		}


        private static VelocityData GetVelocityData( Vector3 point, Collider collider, out SurfaceDataBody body )
        {
            VelocityData velocityData = new( Vector3.zero );
			Rigidbody rigidbody = collider.attachedRigidbody;
			body = null;
			if( rigidbody )
			{
				if( rigidbody.TryGetComponent( out body ) )
					velocityData = body.GetVelocityData( point );
				else
					velocityData = new
						(
							rigidbody.velocity,
							VelocityData.CalculateTangentialVelocity( point, rigidbody.angularVelocity, rigidbody.worldCenterOfMass )
						);
			}

            return velocityData;
		}



		public override string ToString()
		{
            string log = "[Contact Data]: ";
			log += "\n" + ThisCollider + " -> " + OtherCollider;
			log += "\n" + RelativeVelocity;
			log += "\n" + ThisSurface + " -> " + OtherSurface;
			return log;
		}
	}
}