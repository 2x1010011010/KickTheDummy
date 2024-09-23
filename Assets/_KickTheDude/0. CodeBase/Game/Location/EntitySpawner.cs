using Game.InteractiveSystem;
using Game.ResourceSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class EntitySpawner : MonoBehaviour
{
    public event Action<InteractableObject> EntitySpawned;

    [SerializeField, BoxGroup("SETUP")] private PropEntity _resourceEntity;
    [SerializeField, BoxGroup("PARAMETERS")] private bool _spawnOnEnable;

    private IEntitiesFactory<InteractableObject> _entitiesFactory;

    [Inject]
    private void Construct(IEntitiesFactory<InteractableObject> entitiesFactory)
    {
        _entitiesFactory = entitiesFactory;
    }

    private void Start()
    {
        if (_spawnOnEnable)
            Spawn();
    }

    [Button("SPAWN", ButtonSizes.Large), BoxGroup("ACTIONS")]
    private async void Spawn()
    {
        InteractableObject entity = null;

        entity = await _entitiesFactory.CreateEntity(_resourceEntity.InteractableObjectReference, transform.position, transform.rotation);

        StartCoroutine(Placing(entity));

        EntitySpawned?.Invoke(entity);
    }

    private IEnumerator Placing(InteractableObject entity)
    {
        yield return new WaitForFixedUpdate();

        entity.transform.position = transform.position;
        entity.transform.rotation = transform.rotation;
    } 
}
