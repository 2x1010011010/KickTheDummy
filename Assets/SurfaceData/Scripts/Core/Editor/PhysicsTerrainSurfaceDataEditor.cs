using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace SurfaceDataSystem
{
	[CustomEditor( typeof( SurfaceDataTerrain ) )]
	public class SurfaceDataTerrainEditor : Editor
	{
		private SurfaceDataTerrain m_physicsTerrainSurfaceData;
		private SerializedProperty m_soundLayers;

		void OnEnable()
		{
			m_physicsTerrainSurfaceData = target as SurfaceDataTerrain;
			m_physicsTerrainSurfaceData.OnEnable();

			m_soundLayers = serializedObject.FindProperty( "m_soundLayers" );
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField( m_soundLayers );

			EditorGUILayout.Space( 10 );
			if( GUILayout.Button( "Sync Terrain Layers" ) )
				m_physicsTerrainSurfaceData.SyncLayers();

			serializedObject.ApplyModifiedProperties();
		}
	}
}