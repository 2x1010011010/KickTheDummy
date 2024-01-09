using System;
using UnityEngine;
using TMPro;
using UI.Enums;
using Unity.VisualScripting;
using UnityEngine.Events;

namespace UI
{
  public class SettingsWindow : MonoBehaviour
  {
    [SerializeField] private TMP_Text _qualityName;
    [SerializeField] private TMP_Text _fpsName;
    private QualityLevels _qualityLevels;
    private FrameRates _fps;
    private int _currentFpsIndex = 0;
    private int _currentIndex = 0;
    private const string QualityIndex = "QualityIndex";
    private const string FrameRate = "FPS";

    private void Start()
    {
      _currentIndex = PlayerPrefs.HasKey(QualityIndex) ? PlayerPrefs.GetInt(QualityIndex) : QualitySettings.GetQualityLevel();
      SetQuality(_currentIndex);

      _currentFpsIndex = PlayerPrefs.HasKey(FrameRate) ? PlayerPrefs.GetInt(FrameRate) : 0;
      SetFPS(_currentFpsIndex);
    }

    public void IncreaseQuality()
    {
      _currentIndex++;
      if (_currentIndex > 3)
      {
        _currentIndex = 0;
      }
      SetQuality(_currentIndex);
    }
    
    public void DecreaseQuality()
    {
      _currentIndex--;
      if (_currentIndex < 0)
      {
        _currentIndex = 3;
      }
      SetQuality(_currentIndex);
    }

    public void IncreaseFPS()
    {
      _currentFpsIndex++;
      if (_currentFpsIndex > 2)
      {
        _currentFpsIndex = 0;
      }
      SetFPS(_currentFpsIndex);
    }

    public void DecreaseFPS()
    {
      _currentFpsIndex--;
      if (_currentFpsIndex < 0)
      {
        _currentFpsIndex = 2;
      }
      SetFPS(_currentFpsIndex);
    }

    private void SetQuality(int index)
    {
      QualitySettings.SetQualityLevel(index);
      _qualityName.text = Enum.GetValues(typeof(QualityLevels)).GetValue(index).ToString();
      PlayerPrefs.SetInt(QualityIndex, index);
    }

    private void SetFPS(int index)
    {
      Application.targetFrameRate = (int)Enum.GetValues(typeof(FrameRates)).GetValue(index); 
      _fpsName.text = Enum.GetValues(typeof(FrameRates)).GetValue(index).ToString();
      PlayerPrefs.SetInt(QualityIndex, index);
    }
  }
}