using UnityEngine;

public class VisualEffect : MonoBehaviour, IPoolable
{
    public void OnSpawn()
    {
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void OnDespawn()
    {
        
    }
}
