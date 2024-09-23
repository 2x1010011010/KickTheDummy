using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SurfaceDataSystem
{
	[System.Serializable]
	public class TerrainSurface
    {
		public TerrainLayer layer;
		public Surface surface;

		public TerrainSurface( TerrainLayer _layer )
		{
			layer = _layer;
			surface = null;
		}
	}
}