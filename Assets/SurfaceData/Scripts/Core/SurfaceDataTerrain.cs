using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace SurfaceDataSystem
{
	[RequireComponent( typeof( Terrain ) )]
	public class SurfaceDataTerrain : MonoBehaviour
	{
		[SerializeField] private List<TerrainSurface> m_soundLayers = new();

#if UNITY_EDITOR
		private readonly Dictionary<TerrainLayer, TerrainSurface> _soundLayersDictionary = new();
#endif

		private Terrain _terrain;


		public void OnEnable() => _terrain = GetComponent<Terrain>();


		public Dictionary<TerrainSurface, float> GetSurfacesLayersData( Vector3 position )
		{
			Vector3Int positionInt = ConvertPosition( position );

			float[,,] alphaMap = _terrain.terrainData.GetAlphamaps( positionInt.x, positionInt.z, 1, 1 );

			Dictionary<TerrainSurface, float> layersData = new();

			for( int i = 0; i < m_soundLayers.Count; i++ )
			{
				float alpha = alphaMap[ 0, 0, i ];
				if( alpha > 0.2f )
					layersData.Add( m_soundLayers[ i ], alpha );
			}

			return layersData;
		}


		private Vector3Int ConvertPosition( Vector3 playerPosition )
		{
			Vector3Int positionInt = new();

			Vector3 terrainPosition = playerPosition - _terrain.transform.position;

			Vector3 mapPosition = new()
			{
				x = terrainPosition.x / _terrain.terrainData.size.x,
				z = terrainPosition.z / _terrain.terrainData.size.z
			};

			float xCoord = mapPosition.x * _terrain.terrainData.alphamapWidth;
			float zCoord = mapPosition.z * _terrain.terrainData.alphamapHeight;
			positionInt.x = (int)xCoord;
			positionInt.z = (int)zCoord;

			return positionInt;
		}


		private Surface GetSurface( Dictionary<TerrainSurface, float> layers )
		{
			Surface surface = null;
			float maxWeight = 0;

			foreach( var terrainSurface in layers.Keys )
			{
				if( layers[ terrainSurface ] <= maxWeight ) continue;

				surface = terrainSurface.surface;
				maxWeight = layers[ terrainSurface ];
			}

			return surface;
		}


#if UNITY_EDITOR
		public void SyncLayers()
		{
			TerrainLayer[] layers = _terrain.terrainData.terrainLayers;

			List<TerrainSurface> soundLayers = new();
			soundLayers.AddRange( m_soundLayers );
			m_soundLayers.Clear();

			foreach( TerrainLayer layer in layers )
			{
				if( Contains( soundLayers, layer ))
					m_soundLayers.Add( Get( soundLayers, layer ) );
				else
					m_soundLayers.Add( new( layer ) );

				if( !Contains( layer ) )
					_soundLayersDictionary.Add( layer, new( layer ) );
			}
		}


		private bool Contains( List<TerrainSurface> list, TerrainLayer terrainLayer )
		{
			foreach( TerrainSurface terrainSurface in list )
			{
				if( terrainSurface.layer.Equals( terrainLayer ) )
					return true;
			}

			return false;
		}

		private TerrainSurface Get( List<TerrainSurface> list, TerrainLayer terrainLayer )
		{
			foreach( TerrainSurface terrainSurface in list )
			{
				if( terrainSurface.layer.Equals( terrainLayer ) )
					return terrainSurface;
			}

			return default;
		}

		private bool Contains( TerrainLayer layer )
		{
			if( _soundLayersDictionary.Count == 0 ) return false;
			if ( _soundLayersDictionary.TryGetValue( layer, out _ ) )
				return true;

			return false;
		}
#endif
    }
}