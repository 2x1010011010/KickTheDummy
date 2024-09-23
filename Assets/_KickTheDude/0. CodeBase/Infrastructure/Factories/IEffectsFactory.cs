using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IEffectsFactory
{
    UniTask<VisualEffect> CreateEffect(EffectEntity effectEntity, Vector3 position, Quaternion rotation, Transform parent = null);
    UniTask CreateEffectByPhysicsMaterial(PhysicMaterial physicsMaterial, Vector3 position, Quaternion rotation, Transform parent = null);
}
