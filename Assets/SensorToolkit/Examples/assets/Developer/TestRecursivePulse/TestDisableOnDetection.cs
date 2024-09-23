using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit.Example.Developer {
    public class TestDisableOnDetection : MonoBehaviour {

        public Sensor Sensor;

        void Awake() {
            Sensor.OnSignalAdded += (Signal signal, Sensor sensor) => {
                Debug.Log("Signal added: " + signal.Object.name);
                signal.Object.SetActive(false);
            };
            Sensor.OnSignalLost += (Signal signal, Sensor sensor) => {
                Debug.Log("Signal lost: " + signal.Object.name);
            };
        }

    }
}

