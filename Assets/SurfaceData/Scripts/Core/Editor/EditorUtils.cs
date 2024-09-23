using UnityEditor;
using UnityEngine;


#if UNITY_EDITOR

namespace SurfaceDataSystem
{
    public static class EditorUtils
    {
        public static void Error( string header, string desription )
        {
            EditorGUILayout.Space( 10 );
			GUIStyle style = new("error")
			{
				alignment = TextAnchor.MiddleCenter,
				wordWrap = true,
				fontSize = 14
			};
			
			GUIStyle descriptionStyle = new("HelpBox")
			{
				alignment = TextAnchor.MiddleCenter,
				wordWrap = true,
				fontSize = 14,
				richText = true
			};
			
			GUILayout.BeginVertical( "ERROR", "window" );
			GUILayout.Label( header, style );
			GUILayout.Label( desription, descriptionStyle );
			GUILayout.EndVertical();
        }

		
		public static void SerializeList( this Editor editor, SerializedProperty list, ref int selectedIndex )
		{
			int lastSelected = selectedIndex;

			for( int i = 0; i < list.arraySize; i++ )
			{
				SerializedProperty element = list.GetArrayElementAtIndex( i );
				SerializeListElement( element, i, ref selectedIndex );
			}
			DrawListControls( list, selectedIndex );

			if( lastSelected != selectedIndex )
				editor.Repaint();
		}

		public static void SerializeListElement( SerializedProperty listElement, int index, ref int selectedIndex )
		{
			GUIStyle style = new( GUI.skin.box );

			if( index == selectedIndex )
			{
				Color color = new();
				color.r = 0.031f;
				color.g = 0.513f;
				color.b = 0.584f;
				color.a = 0.5f;

				style.normal.background = BackgroundStyle.Get( color );
			}

			Rect rect = EditorGUILayout.BeginVertical( style );
			EditorGUILayout.PropertyField( listElement );
			EditorGUILayout.EndVertical();

			Event current = Event.current;

			if ( current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
				selectedIndex = index;
		}


		private static void DrawListControls( SerializedProperty list, int selectedIndex )
		{
			EditorGUILayout.BeginHorizontal();
			
			float miniButtonWidth = EditorGUIUtility.fieldWidth;
			float spaceWidth = EditorGUIUtility.currentViewWidth - 36 - EditorGUIUtility.fieldWidth - miniButtonWidth * 2;

			if( GUILayout.Button( "+", GUILayout.Width( miniButtonWidth ) ) )
				list.InsertArrayElementAtIndex( list.arraySize );
			if( GUILayout.Button( "-", GUILayout.Width( miniButtonWidth ) ) )
				list.DeleteArrayElementAtIndex( selectedIndex );
			
			GUILayout.Space( spaceWidth );

            list.arraySize = EditorGUILayout.IntField( list.arraySize, GUILayout.Width( EditorGUIUtility.fieldWidth ) );
            EditorGUILayout.EndHorizontal();
		}


		public static void DrawDefaultInspector( SerializedObject serializedObject )
		{
			SerializedProperty property = serializedObject.GetIterator();

			while( property.NextVisible(true) )
			{
			    if (property.name == "m_Script")
			        continue;

			    EditorGUILayout.PropertyField(property, true);
			}
		}
    }

	public static class BackgroundStyle
	{
	    private static Texture2D texture = new(1, 1);
	 
	 
	    public static Texture2D Get(Color color)
	    {
			if( texture == null )
				texture = new Texture2D( 1, 1 );

			texture.SetPixel(0, 0, color);
			texture.Apply();
			return texture;
	    }
	}
}

#endif