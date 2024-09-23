using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimeService
{
    event Action<float> TimeValueChanged;

    float CurentTimeScale { get; }
    float CurentTimeValue { get; }

    void SetTimeValue(float value);
    void ResetTimeValue();
}
