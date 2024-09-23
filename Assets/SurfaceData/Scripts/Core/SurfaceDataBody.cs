using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SurfaceDataSystem
{
    public class SurfaceDataBody : MonoBehaviour
	{
		// [SerializeField] private CollisionMode m_collisionMode;
		[SerializeField] private float m_forceMultiplier = 1;


		protected Vector3 _velocity;
		protected Vector3 _angularVelocity;
		protected Vector3 _worldCenterOfMass;

		private Vector3 _previousPosition;
		private Quaternion _previousRotation;
		private bool _isSlipping;


		private bool _rigidbodyInit;
		private Rigidbody _rigidbody;
		public Rigidbody Rigidbody
		{
			get
			{
				if( !_rigidbodyInit )
				{
					_rigidbodyInit = true;
					_rigidbody = GetComponent<Rigidbody>();
				}

				return _rigidbody;
			}
		}


		private readonly Dictionary<ContinuousData, AudioSourcePoolable> _slidingSources = new Dictionary<ContinuousData, AudioSourcePoolable>();
		private readonly Dictionary<ContinuousData, AudioSourcePoolable> _rollingSources = new Dictionary<ContinuousData, AudioSourcePoolable>();


		private void OnCollisionEnter( Collision collision )
		{
			collision.PlayImpact( CollisionMode.Single, m_forceMultiplier );
		}

        /*
		protected virtual void OnCollisionStay( Collision collision )
		{
			CollisionData collisionData = new( collision, CollisionMode.Single );
			
			collisionData.PlayRoll( CollisionMode.Single, _rollingSources, m_forceMultiplier );
			collisionData.PlaySlide( CollisionMode.Single, _slidingSources, m_forceMultiplier );
		}
        */

		protected virtual void OnCollisionExit( Collision collision )
		{
			UpdateSlidingSources( collision );
			UpdateRollingSources( collision );
		}


		private void UpdateSlidingSources( Collision collision )
		{
			if( _slidingSources.Count <= 0 ) return;

			if( collision.contactCount <= 0 )
				ClearContinous( collision, _slidingSources );
		}


		private void UpdateRollingSources( Collision collision )
		{
			if( _rollingSources.Count <= 0 ) return;

			if( collision.contactCount <= 0 )
				ClearContinous( collision, _rollingSources );
		}


		private void ClearContinous( Collision collision, Dictionary<ContinuousData, AudioSourcePoolable> sources )
		{
			List<ContinuousData> keysToRemove = new List<ContinuousData>();
			foreach( var key in sources.Keys )
			{
				if( key.ThisCollider == collision.collider || key.OtherCollider == collision.collider )
				{
					sources[ key ].FadeAway( 0f );
					keysToRemove.Add( key );
				}
			}

			foreach( var key in keysToRemove )
				sources.Remove( key );
		}



		protected virtual void FixedUpdate()
		{
			if( gameObject.isStatic ) return;


			if( Rigidbody )
			{
				if( Rigidbody.IsSleeping() || Rigidbody.isKinematic )
				{
					if( !_isSlipping )
					{
						_isSlipping = true;
						OnSleep( true );
					}
				}
				else if( !Rigidbody.IsSleeping() && !Rigidbody.isKinematic )
				{
					if( _isSlipping )
					{
						_isSlipping = false;
						OnSleep( false );
					}
				}
			}


			if( Rigidbody ? !Rigidbody.isKinematic : false )
			{
				_velocity = Rigidbody.velocity * Rigidbody.mass;
				_angularVelocity = Rigidbody.angularVelocity * Rigidbody.mass;
				_worldCenterOfMass = Rigidbody.worldCenterOfMass;
			}
			else
			{
				_velocity = ( transform.position - _previousPosition ) / Time.fixedDeltaTime * 3;

				Quaternion deltaRotation = transform.rotation * Quaternion.Inverse( _previousRotation );
				deltaRotation.ToAngleAxis( out var angle, out var axis );
				angle *= Mathf.Deg2Rad;
				_angularVelocity = 1.0f / Time.fixedDeltaTime * angle * axis * 3;

				_worldCenterOfMass = transform.position;
			}

			_previousPosition = transform.position;
			_previousRotation = transform.rotation;
		}



		protected virtual void OnSleep( bool value )
		{
			/*
			if( value )
			{
				foreach( var key in _continuousAudioSources.Keys )
					_continuousAudioSources[ key ].FadeAway( 0f );

				_continuousAudioSources.Clear();
			}
			*/
		}


		public virtual VelocityData GetVelocityData( Vector3 contactPoint ) => new VelocityData
        (
			_velocity,
			VelocityData.CalculateTangentialVelocity( contactPoint, _angularVelocity, _worldCenterOfMass )
		);
	}
}