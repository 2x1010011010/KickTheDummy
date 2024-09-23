using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SurfaceDataSystem.Player
{
	[System.Serializable]
	public class GroundChecker
	{
		[SerializeField] private Vector3 m_position;
		[SerializeField] private float m_radius;

		[SerializeField] private LayerMask m_groundMask;


		private Transform transform;
		private Rigidbody _rigidbody;


		public event System.Action<Vector3> OnLanded;
		public event System.Action OnLeaveGround;


		private bool _grounded;
		private Vector3 _velocity;
		private Vector3 _previousPosition;


		public bool IsGrounded => _grounded;


		public void Init( Transform transform )
		{
			this.transform = transform;
			_rigidbody = transform.GetComponentInChildren<Rigidbody>();
		}


		public void FixedUpdate()
		{
			UpdateGroundStatus();

			if( _rigidbody ? !_rigidbody.isKinematic : false )
				_velocity = _rigidbody.velocity * _rigidbody.mass;
			else
				_velocity = ( transform.position - _previousPosition ) / Time.fixedDeltaTime * 3;

			_previousPosition = transform.position;
		}


		public void UpdateGroundStatus()
		{
			bool wasStatus = _grounded;
			_grounded = CheckIsGrounded();

			if( _grounded == wasStatus )
				return;

			if( _grounded && !wasStatus )
				OnLanded?.Invoke( _velocity );

			if( !_grounded && wasStatus )
				OnLeaveGround?.Invoke();
		}


		private bool CheckIsGrounded()
		{
			Collider[] colliders = Physics.OverlapSphere( transform.TransformPoint( m_position ), m_radius, m_groundMask );

			foreach( Collider collider in colliders )
			{
				if( collider.CompareTag( "Player" ) )
					continue;
				return true;
			}

			return false;
		}
	}
}