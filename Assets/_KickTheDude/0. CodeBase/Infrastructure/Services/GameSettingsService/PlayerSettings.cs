using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Settings
{
    public class PlayerSettings : ISettings
    {
        public static event Action PlayerSettingsChanged;
        public event Action Changed;

        public const string CAMERA_SENSITIVITY = "ID_PREFS_CAMERA_SENSITIVITY";
        public const string PLAYER_SANTA_SKIN = "ID_PREFS_PLAYER_SANTA_SKIN";

        public PlayerSettings()
        {
            if (!PlayerPrefs.HasKey(CAMERA_SENSITIVITY)) PlayerPrefs.SetInt(CAMERA_SENSITIVITY, 20);
            if (!PlayerPrefs.HasKey(PLAYER_SANTA_SKIN)) PlayerPrefs.SetInt(PLAYER_SANTA_SKIN, 0);
        }

        public void IncreaseCameraSensitivity()
        {
            var curentSensitivity = PlayerPrefs.GetInt(CAMERA_SENSITIVITY, 20);

            if (curentSensitivity < 100)
            {
                curentSensitivity += 1;
                PlayerPrefs.SetInt(CAMERA_SENSITIVITY, curentSensitivity);
            }

            PlayerSettingsChanged?.Invoke();
            Changed?.Invoke();
        }

        public void DecreaseCameraSensitivity()
        {
            var curentSensitivity = PlayerPrefs.GetInt(CAMERA_SENSITIVITY, 20);

            if (curentSensitivity > 1)
            {
                curentSensitivity -= 1;
                PlayerPrefs.SetInt(CAMERA_SENSITIVITY, curentSensitivity);
            }

            PlayerSettingsChanged?.Invoke();
            Changed?.Invoke();
        }

        public void SwitchPlayerSantaSkin()
        {
            var currentPlayerSkinIsSanta = GetPlayerSantaSkinState();

            if (currentPlayerSkinIsSanta)
                PlayerPrefs.SetInt(PLAYER_SANTA_SKIN, 0);
            else
                PlayerPrefs.SetInt(PLAYER_SANTA_SKIN, 1);

            PlayerSettingsChanged?.Invoke();
            Changed?.Invoke();
        }

        public int GetCurentCameraSensitivity()
            => PlayerPrefs.GetInt(CAMERA_SENSITIVITY, 20);

        public bool GetPlayerSantaSkinState()
            => PlayerPrefs.GetInt(PLAYER_SANTA_SKIN) == 1 ? true : false;
    }
}