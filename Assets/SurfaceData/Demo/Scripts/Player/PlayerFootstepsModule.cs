using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SurfaceDataSystem.Player
{
	[RequireComponent( typeof( PlayerMovementModule ) )]
    public class PlayerFootstepsModule : PlayerModule
    {
		[SerializeField] private LayerMask m_groundLayer;


		private Vector2 _velocity;

		private float _speed;
		private float _footstepTimer;


		protected override void OnInit()
		{
			PlayerMovementModule movementModule = GetComponent<PlayerMovementModule>();

			movementModule.OnMove += SetMoveVelocity;
			movementModule.OnJumped += OnJumped;
			movementModule.OnLanded += OnLanded;
			movementModule.OnSpeedChange += SetSpeed;

			SetSpeed( movementModule.Speed );
		}


		public override void OnLateUpdate()
		{
			if( _footstepTimer <= 0 && _velocity.magnitude > 0.1f )
			{
				if( Raycast( out RaycastHit hit ) )
				{
					hit.PlayFootstep( _speed > 25 ? 1f : 0.45f );
					_footstepTimer = 0.2f + ( 1 / _speed ) * 6f;
				}
			}

			if( _footstepTimer > 0 )
				_footstepTimer -= Time.deltaTime;
		}


		private bool Raycast( out RaycastHit hit ) => Physics.SphereCast( transform.position, 0.1f, -transform.up, out hit, 1f, m_groundLayer );


		private void SetSpeed( float speed )
		{
			_speed = speed;
		}

		private void SetMoveVelocity( Vector2 velocity )
		{
			_velocity = velocity;
		}


		private void OnJumped()
		{
			if( Raycast( out RaycastHit hit ) )
				hit.PlayFootstep( 0.45f );
		}


		private void OnLanded( Vector3 velocity )
		{
			float dot = Vector3.Dot( velocity, -transform.up );
			dot /= 16f;
			if( dot <= 0.05 )
				return;

			if( Raycast( out RaycastHit hit ) )
				hit.PlayFootstep( dot );
		}
	}
}