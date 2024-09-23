using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class PlayerInstaller : MonoInstaller
{
    [SerializeField, BoxGroup("SETUP")] private Transform _spawnPoint;

    private IPlayer _player;

    public override void InstallBindings()
    {
        BindInstantiate();
    }

    private void BindInstantiate()
    {
        _player = Container.InstantiatePrefabForComponent<IPlayer>(Resources.Load("Player"));

        Container.Bind<IPlayer>().FromInstance(_player).AsSingle().NonLazy();

        _player.Root.position = _spawnPoint.position;
        _player.Root.rotation = _spawnPoint.rotation;
        _player.Root.parent = _spawnPoint;
    }
}
