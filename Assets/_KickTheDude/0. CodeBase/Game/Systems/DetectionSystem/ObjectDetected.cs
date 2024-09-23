using Micosmo.SensorToolkit;
using NodeCanvas.Framework;
using UnityEngine;

public class ObjectDetected : ConditionTask<Sensor>
{
    protected override void OnEnable()
    {
        _objectDetected = false;
        agent.OnDetected.AddListener(ReturnConditionAfterDetect);
    }

    protected override void OnDisable()
    {
        _objectDetected = false;
        agent.OnDetected.RemoveListener(ReturnConditionAfterDetect);
    }

    bool _objectDetected;

    private void ReturnConditionAfterDetect(GameObject detectedObject, Sensor sensor)
    {
        _objectDetected = true;
    }

    protected override bool OnCheck()
    {
        return _objectDetected;
    }
}
