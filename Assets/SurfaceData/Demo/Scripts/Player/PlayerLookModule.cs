using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SurfaceDataSystem.Player
{
    public class PlayerLookModule : PlayerModule
    {
		[SerializeField] private Transform m_camera;

		[Space]
		[SerializeField] private float m_sensitivity;


		private Vector2 _rawInput;
		private Vector2 _euler;

		private Rigidbody _rigidbody;


		public Transform Camera => m_camera;



		protected override void OnInit()
		{
			_euler = new( m_camera.eulerAngles.x, transform.eulerAngles.y );

			PlayerInputModule input = GetComponent<PlayerInputModule>();
			input.OnLook += OnLook;

			_rigidbody = GetComponent<Rigidbody>();
		}


		public override void OnUpdate()
		{
			UpdateLook();
		}


		public void OnLook( Vector2 delta )
		{
			_rawInput = delta;
		}


		private void UpdateLook()
		{
			Vector2 delta = _rawInput;

			delta *= m_sensitivity;

			delta.y *= -1;

			_euler.x += delta.y;
			_euler.y += delta.x;

			_euler.x = Mathf.Clamp( _euler.x, -90f, 90f );

			m_camera.localEulerAngles = new( _euler.x, 0, 0 );
			
			// transform.rotation = Quaternion.Euler( 0f, _euler.y, 0f );
			_rigidbody.MoveRotation( Quaternion.Euler( 0f, _euler.y, 0f ) );
		}
	}
}