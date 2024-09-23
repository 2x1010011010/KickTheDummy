using System;
using _KickTheDude._0._CodeBase.UI.SettingsWindow.Enums;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace _KickTheDude._0._CodeBase.UI.SettingsWindow
{
  public class UIScreenSettingsPanel : UIPanel
  {
    [SerializeField, BoxGroup("RESOLUTION")] private UIButton _resolutionIncrease;
    [SerializeField, BoxGroup("RESOLUTION")] private UIButton _resolutionDecrease;
    [SerializeField, BoxGroup("RESOLUTION")] private TMP_Text _currentResolutionText;

    [SerializeField, BoxGroup("FPS")] private UIButton _fpsIncrease;
    [SerializeField, BoxGroup("FPS")] private UIButton _fpsDecrease;
    [SerializeField, BoxGroup("FPS")] private TMP_Text _currentFPSText;
    
    private int _currentResolutionIndex;
    private int _currentFPSIndex;
    private Resolutions _resolutions;
    private FPSValues _fpsValues;
    private int _resolutionsCount;
    private int _fpsValuesCount;
    private const string FrameRateIndex = "FPS";
    private const string ResolutionIndex = "Resolution";
    
    public override void Show()
    {
      _resolutionIncrease.ButtonClicked += IncreaseResolution;
      _resolutionDecrease.ButtonClicked += DecreaseResolution;
      _fpsIncrease.ButtonClicked += IncreaseFPS;
      _fpsDecrease.ButtonClicked += DecreaseFPS;
      
      GetStartValues();

      base.Show();
    }
    
    public override void Hide()
    {
      _resolutionIncrease.ButtonClicked -= IncreaseResolution;
      _resolutionDecrease.ButtonClicked -= DecreaseResolution;
      _fpsIncrease.ButtonClicked -= IncreaseFPS;
      _fpsDecrease.ButtonClicked -= DecreaseFPS;
      
      base.Hide();
    }

    private void GetStartValues()
    {
      _currentResolutionIndex = PlayerPrefs.HasKey(ResolutionIndex) ? PlayerPrefs.GetInt(ResolutionIndex) : QualitySettings.GetQualityLevel();
      SetQuality(_currentResolutionIndex);

      _currentFPSIndex = PlayerPrefs.HasKey(FrameRateIndex) ? PlayerPrefs.GetInt(FrameRateIndex) : 0;
      SetFPS(_currentFPSIndex);
      
      _resolutionsCount = Enum.GetNames(typeof(Resolutions)).Length;
      _fpsValuesCount = Enum.GetNames(typeof(FPSValues)).Length;
    }

    private void DecreaseFPS(UIButton obj)
    {
      _currentFPSIndex--;
      if (_currentFPSIndex < 0)
      {
        _currentFPSIndex = _fpsValuesCount-1;
      }
      SetFPS(_currentFPSIndex);
    }

    private void IncreaseFPS(UIButton obj)
    {
      _currentFPSIndex++;
      if (_currentFPSIndex > _fpsValuesCount - 1)
      {
        _currentFPSIndex = 0;
      }
      SetFPS(_currentFPSIndex);
    }

    private void DecreaseResolution(UIButton obj)
    {
      _currentResolutionIndex--;
      if (_currentResolutionIndex < 0)
      {
        _currentResolutionIndex = _resolutionsCount - 1;
      }
      SetQuality(_currentResolutionIndex);
    }

    private void IncreaseResolution(UIButton obj)
    {
      _currentResolutionIndex++;
      if (_currentResolutionIndex > _resolutionsCount - 1)
      {
        _currentResolutionIndex = 0;
      }
      SetQuality(_currentResolutionIndex);
    }

    private void SetQuality(int qualityIndex)
    {
      QualitySettings.SetQualityLevel(qualityIndex);
      _currentResolutionText.text = Enum.GetValues(typeof(Resolutions)).GetValue(qualityIndex).ToString();
      
      PlayerPrefs.SetInt(ResolutionIndex, qualityIndex);
    }

    private void SetFPS(int fpsIndex)
    {
      Application.targetFrameRate = (int)Enum.GetValues(typeof(FPSValues)).GetValue(fpsIndex); 
      _currentFPSText.text = Enum.GetValues(typeof(FPSValues)).GetValue(fpsIndex).ToString();
      
      PlayerPrefs.SetInt(FrameRateIndex, fpsIndex);
    }
  }
}