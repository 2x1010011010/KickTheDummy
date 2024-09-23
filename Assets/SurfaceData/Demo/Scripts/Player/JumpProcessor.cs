using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SurfaceDataSystem.Player
{
	public class JumpProcessor
	{
		private GroundChecker _groundChecker;
		private Rigidbody _rigidbody;
		private Transform _transform;


		private bool _wannaJump;
		private float _jumpPressedTime;
		private float _listenToJumpDuration = 0.1f;
		private float _leaveGroundTime;
		private float _wannaJumpForce;
		private float _timeJumped;


		public event System.Action OnJumped;
		public event System.Action<Vector3> OnLanded;


		public JumpProcessor( GroundChecker groundChecker, Rigidbody rigidbody )
		{
			_groundChecker = groundChecker;
			_rigidbody = rigidbody;
			_transform = rigidbody.transform;

			_groundChecker.OnLanded += OnLandedProcessing;
			_groundChecker.OnLeaveGround += OnLeaveGroundProcessing;
		}


		private void OnLandedProcessing( Vector3 velocity )
		{
			OnLanded?.Invoke( velocity );
			if( _wannaJump && Time.time - _jumpPressedTime < _listenToJumpDuration )
				Jump( _wannaJumpForce );
		}

		private void OnLeaveGroundProcessing()
		{
			_leaveGroundTime = Time.time;
		}


		public void Jump( float force )
		{
			if( Time.time - _timeJumped < _listenToJumpDuration )
				return;

			bool grounded = _groundChecker.IsGrounded;
			if( !_groundChecker.IsGrounded )
				grounded = Time.time - _leaveGroundTime < _listenToJumpDuration / 2;

			if( grounded )
			{
				ApplyJumpForce( force );
				_wannaJump = false;
				OnJumped?.Invoke();
			}
			else
			{
				_jumpPressedTime = Time.time;
				_wannaJump = true;
				_wannaJumpForce = force;
			}
		}


		private void ApplyJumpForce( float force )
		{
			_timeJumped = Time.time;

			Vector3 velocity = _rigidbody.velocity;
			velocity.y = 0;
			_rigidbody.velocity = velocity;

			_rigidbody.AddForce( _transform.up * force, ForceMode.Impulse );
		}
	}
}