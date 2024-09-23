using System.Collections.Generic;

public class Pool
{
    private Queue<IPoolable> _poolablesQueue;
    private List<IPoolable> _poolables;

    public int PoolSize { get; private set; }

    public Pool(int poolSize)
    {
        PoolSize = poolSize;
    }

    public void Initialize()
    {
        _poolables = new List<IPoolable>(PoolSize);
        _poolablesQueue = new Queue<IPoolable>(PoolSize);
    }

    public void Expand(IPoolable poolable)
    {
        _poolables.Add(poolable);
        Refresh();
    }

    public bool TryGetPoolable(out IPoolable poolable)
    {
        if (_poolablesQueue.TryDequeue(out IPoolable foundedPoolable))
        {
            poolable = foundedPoolable;
        }
        else
        {
            if(_poolables.Count == PoolSize)
            {
                Refresh();
                poolable = _poolablesQueue.Dequeue();
            }
            else
            {
                poolable = null;
            }
        }

        poolable?.OnSpawn();

        return poolable != null;
    }

    private void Refresh()
    {
        _poolablesQueue?.Clear();

        foreach (var poolable in _poolables)
            _poolablesQueue.Enqueue(poolable);
    }
}
