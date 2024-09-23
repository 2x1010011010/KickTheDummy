using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;


namespace SurfaceDataSystem
{
    public class ModularScriptableObject<M> : ScriptableObject where M : ScriptableObject
	{
		[SerializeField, HideInInspector] protected bool m_allowSameModules;
		[SerializeField, HideInInspector] protected List<M> m_modules = new();
		[SerializeField, HideInInspector] private Dictionary<string, M> _modulesCache = new();
		

		public List<M> Modules
		{
			get
			{
				m_modules ??= new();

				return m_modules;
			}
		}

		protected Dictionary<string, M> ModulesCache
		{
			get
			{
				if( _modulesCache == null || _modulesCache.Count != m_modules.Count )
					SyncCache();

				return _modulesCache;
			}
		}


		private bool _inited;
		private void Init()
		{
			if( !Application.isPlaying )
				return;

			if( _inited )
				return;

			_inited = true;
			OnInit();
		}

		protected virtual void OnInit() { }



		public void SyncCache()
		{
			_modulesCache = new();
			foreach( M m in Modules )
			{
				string moduleName = m.GetType().Name;
				_modulesCache.Add( moduleName, m );
			}
		}


		public void AddModule( M module )
		{
			Modules.Add( module );
			_modulesCache.Add( module.GetType().Name, module );
		}

		public void RemoveModule( M module )
		{
			_modulesCache.Remove( module.GetType().Name );
			Modules.Remove( module );
			DestroyImmediate( module, true );
#if UNITY_EDITOR
			EditorUtility.SetDirty( this );
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
#endif
		}

		public void RemoveModule( int index )
		{
			M module = Modules[ index ];
			RemoveModule( module );
		}


		public bool ContainsModule( Type moduleType )
		{
			string typeName = moduleType.Name;
			return ModulesCache.ContainsKey( typeName );
		}

		public bool ContainsModule<T>() where T : M
		{
			string typeName = typeof( T ).Name;
			return ModulesCache.ContainsKey( typeName );
		}

		public bool TryGetModule<T>( out T module ) where T : M
		{
			Init();

			string typeName = typeof( T ).Name;
			if( ModulesCache.TryGetValue( typeName, out M m ) )
			{
				module = m as T;
				return true;
			}

			module = null;
			return false;
		}

		public bool TryGetModule( int index, out M module )
		{
			Init();

			if( index < 0 || index >= Modules.Count )
			{
				module = null;
				return false;
			}
			else
			{
				module = Modules[ index ];
				return true;
			}
		}
	}
}