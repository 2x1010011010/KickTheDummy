using UnityEngine;

[CreateAssetMenu(fileName = "ForceParameters", menuName = "Parameters/ForceParameters", order = 1)]
public class ForceParameters : ScriptableObject
{
    [field: SerializeField] public float Force { get; private set; }
    [field: SerializeField] public ForceMode ForceMode { get; private set; }
}
