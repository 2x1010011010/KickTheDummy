using Game.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingsService
{
    public GraphicsSettings GraphicsSettings { get; }
    public PlayerSettings PlayerSettings { get; }
    public GoreSettings GoreSettings { get; }

    public GameSettingsService()
    {
        GraphicsSettings = new GraphicsSettings();
        PlayerSettings = new PlayerSettings();
        GoreSettings = new GoreSettings();
    }
}
