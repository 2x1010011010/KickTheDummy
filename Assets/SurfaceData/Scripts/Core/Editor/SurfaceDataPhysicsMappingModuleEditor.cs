#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


namespace SurfaceDataSystem
{
    [CustomEditor( typeof( SurfaceDataPhysicsMapping ) )]
    public class SurfaceDataPhysicsMappingModuleEditor : Editor
    {
        private SerializedProperty _materials;

        private int _selectedIndex;


		private void OnEnable()
        {
            if( target == null ) return;

            _materials = serializedObject.FindProperty( "m_materials" );
        }

		public override void OnInspectorGUI()
		{
            serializedObject.Update();

            EditorUtils.SerializeList( this, _materials, ref _selectedIndex );

            serializedObject.ApplyModifiedProperties();

		}
	}
}

#endif