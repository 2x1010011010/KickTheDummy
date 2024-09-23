using UnityEngine;


namespace SurfaceDataSystem.Player
{
	public class PlayerModule : MonoBehaviour
	{
		protected PlayerController _controller;


		public void Init( PlayerController controller )
		{
			_controller = controller;
			OnInit();
		}

		protected virtual void OnInit() { }


		public virtual void OnUpdate() { }
		public virtual void OnLateUpdate() { }
		public virtual void OnFixedUpdate() { }
	}
}