using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit.Example.Developer {
    public class TestRecursivePulse : MonoBehaviour {

        public Sensor Sensor;
        public GameObject TestObject;

        bool flip = true;

        void Awake() {
            Sensor.OnSignalAdded += (Signal signal, Sensor sensor) => {
                Debug.Log("Signal added: " + signal.Shape.size.magnitude);
                if (flip) {
                    flip = false;
                    TestObject.SetActive(!TestObject.activeSelf);
                    Sensor.Pulse();
                }
            };
            Sensor.OnSignalChanged += (Signal signal, Sensor sensor) => {
                Debug.Log("Signal changed: " + signal.Shape.size.magnitude);
            };
            Sensor.OnSignalLost += (Signal signal, Sensor sensor) => {
                Debug.Log("Signal lost: " + signal.Shape.size.magnitude);
            };
        }

        IEnumerator Start() {
            yield return new WaitForSeconds(1f);
            Sensor.Pulse();
        }

    }
}