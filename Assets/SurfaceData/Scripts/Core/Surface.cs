using UnityEngine;


namespace SurfaceDataSystem
{
    [CreateAssetMenu( fileName = "new_surface", menuName = "SurfaceData/Surface" )]
    public class Surface : ModularScriptableObject<SurfaceModule>
    {
		[SerializeField, HideInInspector] private Texture m_icon;
		public Texture Icon => m_icon;


		protected override void OnInit()
		{
			foreach( var module in Modules )
				module.Init( this );
		}
	}
}