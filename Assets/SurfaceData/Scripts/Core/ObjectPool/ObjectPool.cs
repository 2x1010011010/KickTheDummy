using System;
using System.Collections.Concurrent;


namespace SurfaceDataSystem
{
	public class ObjectPool<T> where T : Poolable
	{
		private readonly ConcurrentBag<T> _objects;
		private readonly Func<T> _objectGenerator;
	

		public ObjectPool( Func<T> objectGenerator )
		{
			_objectGenerator = objectGenerator ?? throw new ArgumentNullException( nameof( objectGenerator ) );
			_objects = new();
		}

	
		public T Get( bool activeState = true )
		{
			if( _objects.TryTake( out T item ) )
			{
				item.gameObject.SetActive( activeState );
				return item;
			}

			return _objectGenerator();
		}

	
		public void Return( T item )
		{
			item.Reset();
			_objects.Add( item );
		}
	}
}