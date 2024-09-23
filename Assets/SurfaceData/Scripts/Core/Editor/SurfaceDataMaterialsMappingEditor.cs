using UnityEngine;
using UnityEditor;


namespace SurfaceDataSystem
{
    [CustomEditor( typeof( SurfaceDataMaterialsMapping ) )]
    public class SurfaceDataMaterialsMappingEditor : Editor
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