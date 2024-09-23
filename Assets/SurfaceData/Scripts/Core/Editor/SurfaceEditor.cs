using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace SurfaceDataSystem
{
	[CustomEditor( typeof( Surface ) )]
	public class SurfaceEditor : ModularScriptableObjectEditor<SurfaceModule>
	{
		private SerializedProperty m_icon;


		protected override void OnEnable()
		{
			base.OnEnable();

			m_icon = serializedObject.FindProperty( "m_icon" );
		}


		public override void OnInspectorGUI()
		{
			EditorGUILayout.PropertyField( m_icon );
			serializedObject.ApplyModifiedProperties();
			base.OnInspectorGUI();
		}
	}
}