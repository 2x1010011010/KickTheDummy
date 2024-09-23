using UnityEngine;

[CreateAssetMenu(fileName = "HealthParameters", menuName = "Parameters/HealthParameters", order = 1)]
public class HealthParameters : ScriptableObject
{
    [SerializeField] private int _maxHealth;

    public int MaxHealth => _maxHealth;
}
