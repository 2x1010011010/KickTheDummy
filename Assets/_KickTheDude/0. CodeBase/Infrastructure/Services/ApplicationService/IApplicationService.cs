using System;

public interface IApplicationService
{
    event Action ApplicationLowMemory;
    event Action<bool> InternetConnectionChanged;
    event Action<bool> ApplicationPausedChanged;

    string ApplicationVersion { get; }
    bool IsInternetConnectionEnabled { get; }
    bool IsPaused { get; }

    void QuitGame();
}