using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GoreSettings : ISettings
{
    public static event Action GoreSettingsChanged;
    public event Action Changed;

    public const string ADVANCED_GORE = "ID_ADVANCED_GORE";
    public const string ADVANCED_GORE_SHADER_KEYWORD = "ADVANCED_GORE";

    public GoreSettings()
    {
        if (!PlayerPrefs.HasKey(ADVANCED_GORE)) PlayerPrefs.SetInt(ADVANCED_GORE, 0);

        var advancedGoreEnabled = PlayerPrefs.GetInt(ADVANCED_GORE) == 1 ? true : false;

        ApplyAdvancedGore(advancedGoreEnabled);
    }

    public void SwitchAdvancedGoreState()
    {
        var advancedGoreEnabled = GetAdvancedGoreState();

        if (advancedGoreEnabled)
            PlayerPrefs.SetInt(ADVANCED_GORE, 0);
        else
            PlayerPrefs.SetInt(ADVANCED_GORE, 1);

        ApplyAdvancedGore(!advancedGoreEnabled);

        GoreSettingsChanged?.Invoke();
    }

    private void ApplyAdvancedGore(bool state)
    {
        if (state)
        {
            Shader.EnableKeyword(ADVANCED_GORE_SHADER_KEYWORD);
        }
        else
        {
            Shader.DisableKeyword(ADVANCED_GORE_SHADER_KEYWORD);
        }
    }

    public bool GetAdvancedGoreState()
        => PlayerPrefs.GetInt(ADVANCED_GORE) == 1? true : false;
}
