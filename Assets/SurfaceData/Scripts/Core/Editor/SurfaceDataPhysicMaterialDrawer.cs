#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;


namespace SurfaceDataSystem
{
    [CustomPropertyDrawer( typeof( SurfaceDataPhysicMaterial ) )]
    public class SurfaceDataPhysicMaterialDrawer : PropertyDrawer
    {
        private float height = 0;
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			EditorGUI.BeginProperty( position, label, property );
			
			SerializedProperty surfaceObject = property.FindPropertyRelative( "surface" );
			Surface surface = ( surfaceObject.objectReferenceValue as Surface );
		
			if( surface != null && surface.Icon != null )
			{
				Rect textureRect = new( position.x, position.y, 64, 64 );
				EditorGUI.DrawPreviewTexture( textureRect, surface.Icon );
			}
			height = 64;
			float width = 64;
		
			float h = height / 2 - height / 8;
			float w = position.width - width - width / 4;
			Rect materialRect = new( position.x + width + 8, position.y + height / 16, w, h );
			EditorGUI.PropertyField( materialRect, property.FindPropertyRelative( "material" ), GUIContent.none );
		
			Rect surfaceRect = new( position.x + 64 + 8, position.y + height - h - height / 16, w, h );
			EditorGUI.PropertyField( surfaceRect, surfaceObject, GUIContent.none );
		
			EditorGUI.EndProperty();
		}
		
		public override float GetPropertyHeight ( SerializedProperty property, GUIContent label )
		{
			return height;
		}
    }
#endif
}