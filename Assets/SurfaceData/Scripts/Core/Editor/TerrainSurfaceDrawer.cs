using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace SurfaceDataSystem
{
	[CustomPropertyDrawer( typeof( TerrainSurface ) )]
	public class TerrainSurfaceDrawer : PropertyDrawer
	{
		private float height = 0;
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			EditorGUI.BeginProperty( position, label, property );

			SerializedProperty materialObject = property.FindPropertyRelative( "layer" );
			TerrainLayer layer = ( materialObject.objectReferenceValue as TerrainLayer );
			Texture2D texture = layer ? layer.diffuseTexture : Texture2D.blackTexture;

			Rect textureRect = new( position.x, position.y, 64, 64 );
			height = 64;
			float width = 64;
			EditorGUI.DrawPreviewTexture( textureRect, texture );

			float h = height / 2 - height / 8;
			float w = position.width - width - width / 4;
			Rect materialRect = new( position.x + width + 8, position.y + height / 16, w, h );
			EditorGUI.PropertyField( materialRect, materialObject, GUIContent.none );

			Rect surfaceRect = new( position.x + 64 + 8, position.y + height - h - height / 16, w, h );
			EditorGUI.PropertyField( surfaceRect, property.FindPropertyRelative( "surface" ), GUIContent.none );

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
		{
			return height;
		}
	}
}