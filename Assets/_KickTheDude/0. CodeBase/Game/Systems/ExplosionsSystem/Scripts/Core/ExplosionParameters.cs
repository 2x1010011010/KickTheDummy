using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ExplosionParameters", menuName = "Parameters/ExplosionParameters", order = 1)]
public class ExplosionParameters : ScriptableObject
{
    [SerializeField] private float _explodeRadius;
    [SerializeField] private bool _checkLandWhenExplode;
    [SerializeField] private LayerMask _explodeLayerMask;
    [SerializeField, ShowIf("_checkLandWhenExplode")] private LayerMask _landLayerMask;
    [SerializeField, ShowIf("_checkLandWhenExplode")] private float _checkLandDistance;

    public float ExplodeRadius => _explodeRadius;
    public LayerMask LandLayerMask => _landLayerMask;
    public LayerMask ExplodeLayerMask => _explodeLayerMask;
    public float CheckLandDistance => _checkLandDistance;
    public bool CheckLandWhenExplode => _checkLandWhenExplode;
}
