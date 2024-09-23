using Lean.Pool;
using UnityEngine;

public class SimplePoolable : MonoBehaviour, IPoolable
{
    public void OnDespawn()
    {
        gameObject.SetActive(false);
    }

    public void OnSpawn()
    {
        gameObject.SetActive(true);
    }
}
