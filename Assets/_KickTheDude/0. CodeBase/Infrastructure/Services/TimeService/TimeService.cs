using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

public class TimeService : ITimeService
{
    public event Action<float> TimeValueChanged;

    //private AudioMixerGroup _masterGroup;

    public float CurentTimeScale => Time.timeScale;
    public float CurentTimeValue => Mathf.InverseLerp(0.3f, 1, CurentTimeScale);

    public TimeService()
    {
        //_masterGroup = masterGroup;

        Debug.Log("[TIME SERVICE] Initialized!");
    }

    public void SetTimeValue(float value)
    {
        var targetValue = Mathf.Lerp(0.3f, 1, value);

        ApplyTimeScale(targetValue);
        ApplyAudioPitch(targetValue);

        TimeValueChanged?.Invoke(CurentTimeValue);
    }

    public void ResetTimeValue()
    {
        SetTimeValue(1f);
    }

    private void ApplyTimeScale(float value)
    {
        Time.timeScale = value;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void ApplyAudioPitch(float value)
    {
        //_masterGroup.audioMixer.SetFloat("Pitch", Mathf.Lerp(0.02f, 1f, value));
    }
}
