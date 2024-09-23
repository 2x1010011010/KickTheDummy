using UnityEngine;

[CreateAssetMenu(fileName = "GrabbableParameters", menuName = "StaticData/GrabbableParameters", order = 1)]
public class GrabbableParameters : ScriptableObject
{
    [field: SerializeField] public AnimatorOverrideController OverrideAnimator { get; private set; }
}
