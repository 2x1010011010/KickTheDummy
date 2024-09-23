using System.Collections.Concurrent;


namespace SurfaceDataSystem
{
	public abstract class ObjectPoolBase<T>
	{
		private readonly ConcurrentBag<T> _objects;
		public abstract T GenerateObject();
	
	    public T Get() => _objects.TryTake( out T item ) ? item : GenerateObject();
		public void Return( T item ) => _objects.Add( item );
	}
}