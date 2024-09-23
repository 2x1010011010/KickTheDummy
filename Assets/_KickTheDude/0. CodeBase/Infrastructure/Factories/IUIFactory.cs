using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IUIFactory : IFactory
{
    UniTask<T> CreateWindow<T>(Transform parent) where T : UIWindow;
    UniTask<T> CreateButton<T>(Transform parent) where T : UIButton;

    void DestroyWindow(UIWindow obj);
}
