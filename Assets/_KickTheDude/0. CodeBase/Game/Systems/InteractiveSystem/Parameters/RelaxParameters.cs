using UnityEngine;

[CreateAssetMenu(fileName = "RelaxParameters", menuName = "Parameters/RelaxParameters", order = 1)]
public class RelaxParameters : ScriptableObject
{
    [field: SerializeField, Range(0, 10)] public float Unpin { get; private set; }
    [field: SerializeField, Range(0, 1500)] public float Force { get; private set; }
}
