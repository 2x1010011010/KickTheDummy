using System;
using System.Collections;
using UnityEngine;

public class ApplicationService : MonoBehaviour, IApplicationService
{
    public event Action ApplicationLowMemory;
    public event Action<bool> InternetConnectionChanged;
    public event Action<bool> ApplicationPausedChanged;

    public string ApplicationVersion => Application.version;
    public bool IsInternetConnectionEnabled { get; private set; } = true;
    public bool IsPaused { get; private set; }

    public void Init()
    {
        Debug.Log("[APPLICATION SERVICE] Init");

        StartCoroutine(CheckInternet());

        Application.lowMemory += OnApplicationLowMemory;
    }

    public void Dispose()
    {
        Application.lowMemory -= OnApplicationLowMemory;
    }

    private IEnumerator CheckInternet()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(5f);

            UpdateInternetConnection();
        }
    }

    public bool GetInternetConnection()
    {
        IsInternetConnectionEnabled = Application.internetReachability != NetworkReachability.NotReachable;

        return IsInternetConnectionEnabled;
    }

    public void UpdateInternetConnection()
    {
        Debug.Log("UPDATE INTERNET CONNECTION");

        var oldStatus = IsInternetConnectionEnabled;
        IsInternetConnectionEnabled = GetInternetConnection();

        if(oldStatus != IsInternetConnectionEnabled)
            InternetConnectionChanged?.Invoke(IsInternetConnectionEnabled);
    }

    private void OnApplicationLowMemory()
    {
        Resources.UnloadUnusedAssets();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        IsPaused = !hasFocus;

        ApplicationPausedChanged?.Invoke(IsPaused);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        //IsPaused = pauseStatus;

        //ApplicationPausedChanged?.Invoke(IsPaused);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
