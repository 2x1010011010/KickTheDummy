using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SurfaceEntity", menuName = "StaticData/SurfaceEntity", order = 1)]
public class SurfaceEntity : ScriptableObject
{
    [field: SerializeField, BoxGroup("DATA")] public PhysicMaterial PhysicMaterial { get; private set; }
    [field: SerializeField, BoxGroup("DATA")] public EffectEntity VisualEffect { get; private set; }
}
