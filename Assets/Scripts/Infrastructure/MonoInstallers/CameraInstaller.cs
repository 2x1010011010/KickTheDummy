using UnityEngine;
using Zenject;

namespace Infrastructure.MonoInstallers
{
  public class CameraInstaller : MonoInstaller
  {
    [SerializeField] private CameraSettings _settings;

    public override void InstallBindings()
    {
      Container.Bind<CameraSettings>().FromInstance(_settings).AsSingle().NonLazy();
    }
  }
}
