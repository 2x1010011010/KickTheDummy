using UnityEngine;


namespace SurfaceDataSystem.Player
{
    public class PlayerDragAndDropModule : PlayerModule
    {
		[SerializeField] private float m_range = 3f;
		[SerializeField] private LayerMask m_draggableMask = 1 << 0;


		private Vector3 _up;
		private Vector3 _forward;
		private Rigidbody _draggedBody;
		private ConfigurableJoint _joint;
		private Rigidbody _hand;
		private Camera _camera;


		protected override void OnInit()
		{
			_camera = Camera.main;
			CreateHand();

			PlayerInputModule input = GetComponent<PlayerInputModule>();
			input.OnFirePressed += OnFirePressed;
			input.OnFireReleased += OnFireReleased;
		}


		public void OnFirePressed()
		{
			OnFire( true );
		}

		public void OnFireReleased()
		{
			OnFire( false );
		}
		

		public void OnFire( bool value )
		{
			if( value )
				TryDrag();
			else
				Drop();
		}

		public override void OnFixedUpdate()
		{
			if( _joint )
			{
				Quaternion rotation = Quaternion.LookRotation( _forward, _up );
				_joint.targetRotation = rotation;
			}
		}


		private Rigidbody CreateHand()
		{
			GameObject handGO = new( "hand" );
			// handGO.hideFlags = HideFlags.HideAndDontSave;
			_hand = handGO.AddComponent<Rigidbody>();
			_hand.isKinematic = true;
			_hand.mass = 15f;

			handGO.transform.SetParent( _camera.transform );
			handGO.transform.localPosition = Vector3.forward;

			return _hand;
		}


		private bool TryDrag()
		{
			Ray ray = _camera.ViewportPointToRay( Vector2.one / 2 );

			if( !Physics.Raycast( ray, out RaycastHit hit, m_range, m_draggableMask ) )
				return false;

			if( !hit.collider.attachedRigidbody )
				return false;

			_hand.transform.localPosition = Vector3.forward * hit.distance;

			_draggedBody = hit.collider.attachedRigidbody;
			_joint = _draggedBody.gameObject.AddComponent<ConfigurableJoint>();
			_joint.connectedBody = _hand;

			_joint.autoConfigureConnectedAnchor = false;
			_joint.connectedAnchor = Vector3.zero;
			_joint.anchor = _draggedBody.transform.InverseTransformPoint( hit.point );
			// _joint.connectedAnchor = _draggedBody.transform.InverseTransformPoint( hit.point );
			// _joint.anchor = Vector3.zero;

			_forward = _camera.transform.InverseTransformDirection( _draggedBody.transform.forward );
			_up = _camera.transform.InverseTransformDirection( _draggedBody.transform.up );

			JointDrive positionDrive = new()
			{
				maximumForce = Mathf.Infinity,
				positionDamper = 1000f,
				positionSpring = 15000f
			};

			_joint.xDrive = positionDrive;
			_joint.yDrive = positionDrive;
			_joint.zDrive = positionDrive;

			_joint.rotationDriveMode = RotationDriveMode.Slerp;
			_joint.slerpDrive = positionDrive;

			return true;
		}


		private void Drop()
		{
			if( !_joint )
				return;

			Destroy( _joint );
			_joint = null;
			_draggedBody = null;
		}


		private void OnDrawGizmos()
		{
			if( !_joint )
				return;

			Vector3 position = _draggedBody.transform.TransformPoint( _joint.connectedAnchor );
			Gizmos.DrawSphere( position, 0.1f );
		}
	}
}