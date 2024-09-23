using UnityEngine;


namespace SurfaceDataSystem
{
    public struct VelocityData
    {
        /// <summary>
        /// The linear velocity of the entire body.
        /// </summary>
        public Vector3 LinearForce { get; private set; }

        /// <summary>
        /// The velocity of the point derived from the angular velocity of the body.
        /// </summary>
        public Vector3 TangentialForce { get; private set; }


        /// <summary>
        /// The sum of LinearForce and TangentialForce. This represents the actual force of the point.
        /// </summary>
        public Vector3 TotalPointForce { get; private set; }

        public VelocityData( Vector3 linearVelocity, float mass = 0 )
        {
            LinearForce = mass == 0 ? linearVelocity : VelocityToImpulse( linearVelocity, mass );
            TangentialForce = Vector3.zero;
            TotalPointForce = linearVelocity;
        }

        public VelocityData( Vector3 linearVelocity, Vector3 tangentialVelocity )
        {
            LinearForce = linearVelocity;
            TangentialForce = tangentialVelocity;
            TotalPointForce = LinearForce + TangentialForce;
        }

        public override string ToString()
        {
            return "[Linear Velocity=" + LinearForce + "] [TangentialVelocity=" + TangentialForce + "] [Total=" + TotalPointForce + "]";
        }


        /// <summary>
        /// Transform Velocity to Force
        /// </summary>
        /// <param name="v">velocity</param>
        /// <param name="m">mass</param>
        /// <returns></returns>
        public static Vector3 VelocityToImpulse( Vector3 v, float m ) => m * ( v / Time.fixedDeltaTime );


        public static Vector3 CalculateTangentialVelocity( Vector3 point, Vector3 angularVelocity, Vector3 centerOfRotation )
        {
            Vector3 p = point - centerOfRotation;
            Vector3 velocity = Vector3.Cross( angularVelocity, p );
            return velocity;
        }
    }
}
