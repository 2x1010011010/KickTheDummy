using System.Threading.Tasks;
using UnityEngine;

public interface IUIService
{
    Task OpenWindow<T>(params object[] parameters) where T : UIWindow;
    void CloseWindow<T>() where T : UIWindow;
    void CloseAllWindows();
    Task<UIWindow> GetWindow<T>() where T : UIWindow;

    bool IsPointerOverUI();
    void PlayUISound(AudioClip audioClip);
}
