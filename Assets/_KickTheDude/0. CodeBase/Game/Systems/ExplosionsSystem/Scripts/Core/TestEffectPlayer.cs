using Game.InteractiveSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class TestEffectPlayer : SerializedMonoBehaviour
{
    [SerializeField] private InteractableObject _explodeable;
    [SerializeField] private EffectEntity _effectEntity;

    private IEffectsFactory _effectsFactory;

    [Inject]
    private void Construct(IEffectsFactory effectsFactory)
    {
        _effectsFactory = effectsFactory;
    }

    private void OnEnable()
    {
        _explodeable.GetInteractive<IExplodeable>().Exploded += Exploded;
    }

    private void OnDisable()
    {
        _explodeable.GetInteractive<IExplodeable>().Exploded -= Exploded;
    }

    private void Exploded(ExplosionData explosionData)
    {
        _effectsFactory.CreateEffect(_effectEntity, transform.position, Quaternion.identity);
    }
}
