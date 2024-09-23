using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;


namespace SurfaceDataSystem
{
	[CanEditMultipleObjects]
    [CustomEditor( typeof( ModularScriptableObjectEditor<> ), true )]
    public class ModularScriptableObjectEditor<T> : Editor where T : ScriptableObject
	{
		private SerializedProperty _allowSame;

		private ModularScriptableObject<T> _modular;
		private ReorderableList _reorderableList;
		private int _selectedIndex;
		private Editor _selectedEditor;
		private List<Type> _types;


		protected virtual void OnEnable()
		{
			_modular = target as ModularScriptableObject<T>;

			_allowSame = serializedObject.FindProperty( "m_allowSameModules" );

			_reorderableList = new ReorderableList( serializedObject, serializedObject.FindProperty( "m_modules" ),
				true, true, true, true );

			_reorderableList.drawHeaderCallback += DrawHeader;
			_reorderableList.drawElementCallback += DrawElement;
			_reorderableList.onSelectCallback += OnSelectElement;
			_reorderableList.onRemoveCallback += OnRemoveCallback;
			_reorderableList.onAddCallback += OnAddCallback;

			EditorUtility.SetDirty( target );
			Repaint();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawDefaultInspector( serializedObject );
			EditorGUILayout.PropertyField( _allowSame );

			EditorGUILayout.Space();
			_reorderableList.DoLayoutList();

			RenderItem();

			serializedObject.ApplyModifiedProperties();
		}


		private void DrawHeader( Rect rect )
		{	
			GUI.Label( rect, "Modules" );
		}


		private void DrawElement( Rect rect, int index, bool isActive, bool isFocused )
		{
			rect.y += 2;

			string moduleName = GetName( index );
			

			GUI.Label( new Rect( rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight ), moduleName );
		}


		protected virtual string GetName( int index )
		{
			var element = _reorderableList.serializedProperty.GetArrayElementAtIndex( index );
			Type type = element.objectReferenceValue.GetType();
			string moduleName = type.Name;
			moduleName = SubstringStart( moduleName, _modular.GetType().Name );
			moduleName = SubstringEnd( moduleName, "Module" );
			return moduleName;
		}


		private void OnSelectElement( ReorderableList list )
		{
			_selectedIndex = list.index;

			if( !_modular.TryGetModule( _selectedIndex, out T module ) )
				return;

			_selectedEditor = CreateEditor( module );
		}

		private void OnAddCallback( ReorderableList list )
		{
			_types = GetSubClasses<T>( !_allowSame.boolValue );

			GUIContent[] menuOptions = new GUIContent[ _types.Count ];

			for( int i = 0; i < _types.Count; i++ )
			{
				string moduleName = _types[ i ].Name;
				moduleName = SubstringStart( moduleName, _modular.GetType().Name );
				moduleName = SubstringEnd( moduleName, "Module" );
				menuOptions[ i ] = new GUIContent( moduleName );
			}

			EditorUtility.DisplayCustomMenu( new Rect( Event.current.mousePosition, Vector2.zero ), menuOptions,
				-1, OnMenuOptionSelected, null );
		}

		private void OnRemoveCallback( ReorderableList list )
		{
			//Debug.Log("Removed element from list");
			_selectedEditor = null;
			_modular.RemoveModule( list.index );
		}

		private void OnMenuOptionSelected( object userData, string[] options, int selected )
		{
			//Debug.Log("Selected menu option: " + options[selected]);

			var selectedType = _types[ selected ];

			T module = CreateInstance( selectedType ) as T;
			string moduleName = selectedType.Name;
			moduleName = SubstringEnd( moduleName, "Module" );
			module.name = moduleName;

			AssetDatabase.AddObjectToAsset( module, _modular );
			AssetDatabase.SaveAssets();

			EditorUtility.SetDirty( module );
			EditorUtility.SetDirty( _modular );

			_modular.AddModule( module );

			EditorUtility.SetDirty( target );
			Repaint();
		}


		// Renders the inspector of currently selected Layer
		private void RenderItem()
		{
			if( _selectedEditor == null )
				return;

			EditorGUILayout.BeginVertical( "box" );

			EditorGUILayout.LabelField( "Module", EditorStyles.boldLabel );
			EditorGUILayout.BeginVertical( EditorStyles.helpBox );

			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color( 0.9f, 0.9f, 0.9f );

			// Display the Inspector for a component that is a member of the component being edited
			_selectedEditor.OnInspectorGUI();

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndVertical();

			// Reset the background color
			GUI.backgroundColor = oldColor;
		}


		private List<Type> GetSubClasses<M>( bool unique = true ) where M : T
		{
			List<Type> subClasses = new();
			Assembly assembly = Assembly.GetAssembly( typeof( M ) );
			Type[] types = assembly.GetTypes();

			foreach( Type type in types )
			{
				if( type.IsSubclassOf( typeof( M ) ) )
				{
					if( unique && _modular.ContainsModule( type ) )
						continue;

					subClasses.Add( type );
				}
			}
			return subClasses;
		}



		public static string SubstringStart( string name, string key )
		{
			if( name.Length > key.Length )
			{
				if( name[ ..key.Length ] == key )
					name = name[ key.Length.. ];
			}

			return name;
		}

		public static string SubstringEnd( string name, string key )
		{
			if( name.Length > key.Length )
			{
				if( name.Substring( name.Length - key.Length ) == key )
					name = name.Substring( 0, name.Length - key.Length );
			}

			return name;
		}


		public static void DrawDefaultInspector( SerializedObject serializedObject )
		{
			SerializedProperty property = serializedObject.GetIterator();

			while( property.NextVisible( true ) )
			{
				if( property.name == "m_Script" )
					continue;

				EditorGUILayout.PropertyField( property, true );
			}
		}
	}
}