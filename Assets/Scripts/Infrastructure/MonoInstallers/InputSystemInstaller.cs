using System.Net.Mime;
using CameraSystem;
using Infrastructure.Services.InputService;
using UnityEngine;
using Zenject;

namespace Infrastructure.MonoInstallers
{
  public class InputSystemInstaller : MonoInstaller
  {
    [SerializeField] private MobileCameraController _movementController;
    [SerializeField] private MobileCameraController _rotationConroller;
    public override void InstallBindings()
    {
      if(Application.isEditor)
        BindDesktopInput();
      else
      {
        BindMobileInput();
        BindControllers();
      }
    }

    private void BindControllers()
    {
      Container.Bind<MobileCameraController>().FromInstance(_movementController).AsSingle();
      Container.Bind<MobileCameraController>().FromInstance(_rotationConroller).AsSingle();
    }

    private void BindDesktopInput()
    {
      Container.Bind<IInputService>().To<DesktopInputService>().FromNew().AsSingle();
    }

    private void BindMobileInput()
    {
      Container.Bind<IInputService>().To<MobileInputService>().FromNew().AsSingle();
    }
  }
}