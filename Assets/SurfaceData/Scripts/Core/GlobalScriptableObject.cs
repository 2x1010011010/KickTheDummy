using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace SurfaceDataSystem
{
	public class GlobalScriptableObject : ScriptableObject
	{
		public static T GetOrCreateInstance<T>() where T : ScriptableObject
		{
			Object obj = Resources.Load( SurfaceData.ResourcesPath + "/" + typeof( T ).Name );
			if( obj )
				return obj as T;

#if UNITY_EDITOR
			T instance = ScriptableObject.CreateInstance<T>();

			if( !Directory.Exists( Application.dataPath + "/Resources/" + SurfaceData.ResourcesPath ) )
				Directory.CreateDirectory( Application.dataPath + "/Resources/" + SurfaceData.ResourcesPath );

			AssetDatabase.CreateAsset( instance, "Assets/Resources/" + SurfaceData.ResourcesPath + "/" + typeof( T ).Name + ".asset" );
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			return instance;
#else
            return null;
#endif
        }
	}
}