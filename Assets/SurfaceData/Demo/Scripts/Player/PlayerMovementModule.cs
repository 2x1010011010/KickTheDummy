using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SurfaceDataSystem.Player
{
	public class PlayerMovementModule : PlayerModule
	{
		[Header( "General" )]
		[SerializeField] private float m_gravityScale = 1;
		[SerializeField] private bool m_useGravity = true;
		[SerializeField] private GroundChecker m_groundChecker;

		[SerializeField] private float m_jumpForce = 9f;


		[Header( "Movement" )]
		[SerializeField] private float sprintSpeed = 3f;
		[SerializeField] private float walkingSpeed = 2f;


		private Vector2 _rawInput;
		private float _speed;
		public float Speed
		{
			get => _speed;
			protected set
			{
				_speed = value;
				OnSpeedChange?.Invoke( _speed );
			}
		}


		private Vector3 _velocity;


		private Rigidbody _rigidbody;
		private JumpProcessor _jumpProcessor;



		public event System.Action<Vector2> OnMove;
		public event System.Action OnJumped;
		public event System.Action<Vector3> OnLanded;
		public event System.Action<float> OnSpeedChange;



		protected override void OnInit()
		{
			_rigidbody = GetComponentInChildren<Rigidbody>();

			_rigidbody.transform.parent = null;


			_jumpProcessor = new( m_groundChecker, _rigidbody );

			_jumpProcessor.OnJumped += () => OnJumped?.Invoke();
			_jumpProcessor.OnLanded += ( velocity ) => OnLanded?.Invoke( velocity );

			m_groundChecker.Init( transform );

			BindInput( GetComponent<PlayerInputModule>() );


			Speed = walkingSpeed;
		}


		private void BindInput( PlayerInputModule input )
		{
			input.OnMove += Move;
			input.OnSprintPressed += SprintPressed;
			input.OnSprintReleased += SprintReleased;

			input.OnJumpPressed += Jump;
		}


		private void Jump()
		{
			_jumpProcessor.Jump( m_jumpForce );
		}


		private void SprintPressed()
		{
			Speed = sprintSpeed;
		}

		private void SprintReleased()
		{
			Speed = walkingSpeed;
		}


		public void Move( Vector2 input )
		{
			_rawInput = input;
		}


		public override void OnUpdate()
		{
			UpdateMovement();
		}


		private void UpdateMovement()
		{
			float moveX = _rawInput.x;
			float moveY = _rawInput.y;


			Vector2 normInput = new( moveX, moveY );
			normInput.Normalize();


			Vector3 forwardInput = transform.TransformVector( new Vector3( moveX, 0, moveY ) );
			_velocity = _speed * forwardInput;
		}


		public override void OnFixedUpdate()
		{
			m_groundChecker.FixedUpdate();
			ApplyGravity();

			if( !m_groundChecker.IsGrounded )
			{
				Vector3 targetVelocity = _velocity / 5f;
				targetVelocity.y = _rigidbody.velocity.y;
				_rigidbody.velocity = Vector3.Lerp( _rigidbody.velocity, targetVelocity, 2 * Time.deltaTime );
			}
			else
				ApplyMovement();
		}


		private void ApplyGravity()
		{
			if( !m_useGravity )
				return;

			if( Mathf.Approximately( m_gravityScale, 0 ) )
				return;

			_rigidbody.AddForce( Physics.gravity * m_gravityScale, ForceMode.Acceleration );
		}


		private void ApplyMovement()
		{
			Vector3 newVelocity = new( _velocity.x, 0, _velocity.z );

			Vector3 horizonalVelocity = transform.InverseTransformVector( _rigidbody.velocity );

			float speed = horizonalVelocity.magnitude * 5f ;
			if( speed < _speed )
				_rigidbody.AddForce( newVelocity, ForceMode.Acceleration );


			horizonalVelocity = transform.InverseTransformVector( _rigidbody.velocity );

			OnMove?.Invoke( new( newVelocity.x, newVelocity.z ) );
		}
	}
}