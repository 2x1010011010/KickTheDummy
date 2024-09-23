using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIService : IUIService
{
    private Dictionary<Type, UIWindow> _windows;
    private List<Type> _windowsInProcess;

    private Transform _windowsParent;
    private UIFactory _uiFactory;
    private AudioSource _uiSoundsSource;

    public UIService(UIFactory uiFactory, AudioSource uiSoundsSource, Transform windowsParent)
    {
        _uiFactory = uiFactory;
        _windowsParent = windowsParent;
        _uiSoundsSource = uiSoundsSource;

        _windows = new Dictionary<Type, UIWindow>();
        _windowsInProcess = new List<Type>();
    }

    public async Task OpenWindow<T>(params object[] parameters) where T : UIWindow
    {
        if (_windows.TryGetValue(typeof(T), out UIWindow uiWindow))
        {
            await uiWindow.Open(parameters);
        }
        else
        {
            if (_windowsInProcess.Contains(typeof(T)))
            {
                return;
            }
            else
            {
                _windowsInProcess.Add(typeof(T));

                var createdWindow = await RegisterWindow<T>();

                await createdWindow.Open(parameters);
            }
        }
    }

    public void CloseWindow<T>() where T : UIWindow
    {
        if (_windows.TryGetValue(typeof(T), out UIWindow uiWindow))
        {
            CloseWindow(uiWindow);
        }
    }

    private void CloseWindow(UIWindow uiWindow)
    {
        Debug.Log("Close Window " + uiWindow.GetType());

        _windows.Remove(uiWindow.GetType());
        uiWindow.Close();

        _uiFactory.DestroyWindow(uiWindow);

        GameObject.Destroy(uiWindow.gameObject);
    }

    public void CloseAllWindows()
    {
        foreach(var window in _windows.Values.ToArray())
        {
            CloseWindow(window);
        }
    }

    public async Task<UIWindow> GetWindow<T>() where T : UIWindow
    {
        if(_windows.TryGetValue(typeof(T), out UIWindow uiWindow))
        {
            return uiWindow;
        }
        else
        {
            return await RegisterWindow<T>();
        }
    }

    private async Task<UIWindow> RegisterWindow<T>() where T : UIWindow
    {
        var createdWindow = await _uiFactory.CreateWindow<T>(_windowsParent);

        Debug.Log($"[UI SERVICE] Window " + typeof(T) + " is registered!");

        _windows.Add(typeof(T), createdWindow);
        _windowsInProcess.Remove(typeof(T));

        return createdWindow;
    }


    private PointerEventData pointerEventData;
    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    public bool IsPointerOverUI()
    {
        pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        return raycastResults.Count > 0;
    }

    public void PlayUISound(AudioClip audioClip)
    {
        _uiSoundsSource.PlayOneShot(audioClip);
    }
}
