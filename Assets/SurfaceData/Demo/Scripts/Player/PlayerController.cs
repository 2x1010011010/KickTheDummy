using UnityEngine;


namespace SurfaceDataSystem.Player
{
	public class PlayerController : MonoBehaviour
	{
		private PlayerModule[] _modules;
	
	
		private void Awake()
		{
			_modules = GetComponents<PlayerModule>();
	
			foreach( PlayerModule module in _modules )
				module.Init( this );
		}
	
	
		private void Update()
		{
			foreach( PlayerModule module in _modules )
				module.OnUpdate();
		}
	
	
		private void LateUpdate()
		{
			foreach( PlayerModule module in _modules )
				module.OnLateUpdate();
		}
	
	
		private void FixedUpdate()
		{
			foreach( PlayerModule module in _modules )
				module.OnFixedUpdate();
		}
	}
}