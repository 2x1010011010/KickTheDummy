#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;


namespace SurfaceDataSystem
{
    [CustomPropertyDrawer( typeof( SurfaceDataMaterial ) )]
    public class SurfaceDataMaterialDrawer : PropertyDrawer
    {
        private float height = 0;

		private Editor _previewEditor;
		private Material _lastMaterial;
		private Texture2D _texture;




		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			EditorGUI.BeginProperty( position, label, property );
			
			SerializedProperty materialObject = property.FindPropertyRelative( "material" );
			Material material = ( materialObject.objectReferenceValue as Material );
		
			if( material != null )
			{
				if( _lastMaterial != material )
				{
					_previewEditor = Editor.CreateEditor( material );

					Texture materialTexture = material.mainTexture;
					if( materialTexture )
					{
						_texture = new( materialTexture.width, materialTexture.height );
						EditorUtility.CopySerialized( materialTexture, _texture );
					}
					else
						_texture = null;
					_lastMaterial = material;
				}

				Rect textureRect = new( position.x, position.y, 64, 64 );
				if( _texture )
					EditorGUI.DrawPreviewTexture( textureRect, _texture );
				// Editor.CreateEditor( material ).OnPreviewGUI( textureRect, EditorStyles.whiteLabel );
			}
			height = 64;
			float width = 64;
		
			float h = height / 2 - height / 8;
			float w = position.width - width - width / 4;
			Rect materialRect = new( position.x + width + 8, position.y + height / 16, w, h );
			EditorGUI.PropertyField( materialRect, materialObject, GUIContent.none );
		
			Rect surfaceRect = new( position.x + 64 + 8, position.y + height - h - height / 16, w, h );
			EditorGUI.PropertyField( surfaceRect, property.FindPropertyRelative( "surface" ), GUIContent.none );
		
			EditorGUI.EndProperty();
		}
		
		public override float GetPropertyHeight ( SerializedProperty property, GUIContent label )
		{
			return height;
		}
    }
#endif
}