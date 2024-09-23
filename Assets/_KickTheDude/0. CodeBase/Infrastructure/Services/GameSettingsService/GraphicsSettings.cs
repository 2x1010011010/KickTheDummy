using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Settings
{
    public class GraphicsSettings : ISettings
    {
        public static event Action GraphicsSettingsChanged;
        public event Action Changed;

        public const int QUALITY_LEVEL_COUNT = 3;
        public const string TARGET_FPS = "ID_PREFS_TARGET_FPS";
        public const string QUALITY_LEVEL = "ID_QUALITY_LEVEL";
        public const string SHOW_FPS = "ID_SHOW_FPS";

        private int[] fpsValues = new int[4] { -1, 30, 60, 300 };

        public GraphicsSettings()
        {
            if (!PlayerPrefs.HasKey(TARGET_FPS)) PlayerPrefs.SetInt(TARGET_FPS, 60);
#if UNITY_IOS
            if (!PlayerPrefs.HasKey(QUALITY_LEVEL)) PlayerPrefs.SetInt(QUALITY_LEVEL, 2);
#else
            if (!PlayerPrefs.HasKey(QUALITY_LEVEL)) PlayerPrefs.SetInt(QUALITY_LEVEL, 2);
#endif
            if (!PlayerPrefs.HasKey(SHOW_FPS)) PlayerPrefs.SetInt(SHOW_FPS, 0);

            QualitySettings.vSyncCount = 0;
            //Application.targetFrameRate = PlayerPrefs.GetInt(TARGET_FPS);
            //QualitySettings.SetQualityLevel(PlayerPrefs.GetInt(QUALITY_LEVEL));
            //Application.targetFrameRate = 60;
            QualitySettings.SetQualityLevel(2);
        }

        public void IncreaseTargetFPS()
        {
            QualitySettings.vSyncCount = 0;

            var curentFPS = PlayerPrefs.GetInt(TARGET_FPS); Debug.Log("CURENT FPS " + curentFPS);
            var index = Array.IndexOf(fpsValues, curentFPS); Debug.Log("FOUNDED INDEX " + index);
            var targetFPS = index == fpsValues.Length - 1 ? fpsValues[0] : fpsValues[index + 1];

            Application.targetFrameRate = targetFPS;
            PlayerPrefs.SetInt(TARGET_FPS, targetFPS);

            GraphicsSettingsChanged?.Invoke();
        }

        public void DecreaseTargetFPS()
        {
            QualitySettings.vSyncCount = 0;

            var curentFPS = PlayerPrefs.GetInt(TARGET_FPS);
            var index = Array.IndexOf(fpsValues, curentFPS);
            var targetFPS = index == 0 ? fpsValues[fpsValues.Length - 1] : fpsValues[index - 1];

            Application.targetFrameRate = targetFPS;
            PlayerPrefs.SetInt(TARGET_FPS, targetFPS);

            GraphicsSettingsChanged?.Invoke();
        }

        public void IncreaseQualityLevel()
        {
            var currentQualityLevel = QualitySettings.GetQualityLevel();

            var targetQualityLevel = currentQualityLevel == QUALITY_LEVEL_COUNT ? 0 : currentQualityLevel + 1;

            QualitySettings.SetQualityLevel(targetQualityLevel);

            PlayerPrefs.SetInt(QUALITY_LEVEL, targetQualityLevel);
            GraphicsSettingsChanged?.Invoke();
        }

        public void DecreaseQualityLevel()
        {
            var currentQualityLevel = QualitySettings.GetQualityLevel();

            var targetQualityLevel = currentQualityLevel == 0 ? QUALITY_LEVEL_COUNT : currentQualityLevel - 1;

            QualitySettings.SetQualityLevel(targetQualityLevel);

            PlayerPrefs.SetInt(QUALITY_LEVEL, targetQualityLevel);
            GraphicsSettingsChanged?.Invoke();
        }

        public void SwitchShowFPSState()
        {
            var curentShowFPSState = PlayerPrefs.GetInt(SHOW_FPS) == 1;

            if (curentShowFPSState) PlayerPrefs.SetInt(SHOW_FPS, 0); else PlayerPrefs.SetInt(SHOW_FPS, 1);

            GraphicsSettingsChanged?.Invoke();
        }

        public bool GetShowFPSState()
        {
            return PlayerPrefs.GetInt(SHOW_FPS) == 1;
        }
    }
}