using System;
using UnityEngine;
using TMPro;

namespace UI
{
  public class SettingsWindow : MonoBehaviour
  {
    [SerializeField] private TMP_Text _qualityName; 
    private QualityLevels _qualityLevels;
    private int _currentIndex;
    private const string QualityIndex = "QualityIndex";

    private void Start()
    {
      _currentIndex = PlayerPrefs.HasKey(QualityIndex) ? PlayerPrefs.GetInt(QualityIndex) : QualitySettings.GetQualityLevel();
      SetQuality(_currentIndex);
      _qualityName.text = Enum.GetValues(typeof(QualityLevels)).GetValue(_currentIndex).ToString();
    }

    public void IncreaseQuality()
    {
      _currentIndex++;
      if (_currentIndex > 3)
      {
        _currentIndex = 3;
      }
      SetQuality(_currentIndex);
    }
    
    public void DecreaseQuality()
    {
      _currentIndex--;
      if (_currentIndex < 0)
      {
        _currentIndex = 0;
      }
      SetQuality(_currentIndex);
    }

    private void SetQuality(int index)
    {
      QualitySettings.SetQualityLevel(index);
      _qualityName.text = Enum.GetValues(typeof(QualityLevels)).GetValue(index).ToString();
      PlayerPrefs.SetInt(QualityIndex, index);
    }
  }
}