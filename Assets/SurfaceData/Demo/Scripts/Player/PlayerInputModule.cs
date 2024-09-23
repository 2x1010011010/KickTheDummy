using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SurfaceDataSystem.Player
{
	public class PlayerInputModule : PlayerModule
	{
		public event System.Action OnFirePressed;
		public event System.Action OnFireReleased;

		public event System.Action OnSecondaryFirePressed;
		public event System.Action OnSecondaryFireReleased;


		public event System.Action OnSprintPressed;
		public event System.Action OnSprintReleased;


		public event System.Action OnJumpPressed;

		public event System.Action<Vector2> OnLook;

		public event System.Action<Vector2> OnMove;


		public static bool InputIsLocked() => Cursor.lockState != CursorLockMode.Locked;


		public override void OnUpdate()
		{
			if( InputIsLocked() )
			{
				Move( Vector2.zero );
				Look( Vector2.zero );
				Fire( false );
				Sprint( false );

				if( Input.GetMouseButtonDown( 0 ) ) // Hide Cursor and unlock input
					Cursor.lockState = CursorLockMode.Locked;
				return;
			}


			if( Input.GetKeyDown( KeyCode.Escape ) ) // Show Cursor and lock input
				Cursor.lockState = CursorLockMode.None;


			Vector2 moveInput = new( Input.GetAxis( "Horizontal" ), Input.GetAxis( "Vertical" ) );
			Move( moveInput );

			Vector2 lookInput = new( Input.GetAxis( "Mouse X" ), Input.GetAxis( "Mouse Y" ) );
			Look( lookInput );


			if( Input.GetKeyDown( KeyCode.Space ) )
				Jump();


			if( Input.GetMouseButtonDown( 0 ) )
				Fire( true );

			if( Input.GetMouseButtonUp( 0 ) )
				Fire( false );


			if( Input.GetKeyDown( KeyCode.LeftShift ) )
				Sprint( true );
			if( Input.GetKeyUp( KeyCode.LeftShift ) )
				Sprint( false );


			if( Input.GetMouseButtonDown( 1 ) )
				SecondaryFire( true );

			if( Input.GetMouseButtonUp( 1 ) )
				SecondaryFire( false );
		}



		private void Move( Vector2 rawInput )
		{
			OnMove?.Invoke( rawInput );
		}

		private void Look( Vector2 rawInput )
		{
			OnLook?.Invoke( rawInput );
		}

		private void Sprint( bool value )
		{
			if( value )
				OnSprintPressed?.Invoke();
			else
				OnSprintReleased?.Invoke();
		}


		private void Jump()
		{
			OnJumpPressed?.Invoke();
		}

		private void Fire( bool value )
		{
			if( value )
				OnFirePressed?.Invoke();
			else
				OnFireReleased?.Invoke();
		}

		private void SecondaryFire( bool value )
		{
			if( value )
				OnSecondaryFirePressed?.Invoke();
			else
				OnSecondaryFireReleased?.Invoke();
		}
	}
}