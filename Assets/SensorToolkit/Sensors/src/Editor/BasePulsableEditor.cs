using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Micosmo.SensorToolkit.Editors {

    public static class EditorState {
        public static event System.Action OnStopTesting;
        public static Observable<BasePulsableSensor> ActivePulsable = new Observable<BasePulsableSensor>();
        public static void StopAllTesting() {
            if (EditorApplication.isPlaying || EditorApplication.isPaused) {
                return;
            }
            OnStopTesting?.Invoke();
            ActivePulsable.Value = null;
        }
    }

    public abstract class BasePulsableEditor<T> : Editor where T : BasePulsableSensor {

        T pulsable;
        protected bool IsTesting = false;
        protected bool IsActivePulsable => EditorState.ActivePulsable.Value == pulsable;
        protected abstract bool canTest { get; }

        bool isInGame => EditorApplication.isPlaying || EditorApplication.isPaused;
        protected bool showDetections => isInGame || IsTesting;

        protected virtual void OnEnable() {
            if (serializedObject == null) {
                return;
            }
            pulsable = serializedObject.targetObject as T;
            pulsable.OnPulsed += OnPulsedHandler;
            EditorState.OnStopTesting += OnStopTestingHandler;
            EditorState.ActivePulsable.OnChanged += ActivePulsableChangedHandler;

            if ((EditorApplication.isPlaying || EditorApplication.isPaused) && EditorState.ActivePulsable.Value == null) {
                EditorState.ActivePulsable.Value = pulsable;
            }
        }

        protected virtual void OnDisable() {
            EditorState.StopAllTesting();
            if (IsActivePulsable) {
                EditorState.ActivePulsable.Value = null;
            }
            pulsable.OnPulsed -= OnPulsedHandler;
            EditorState.OnStopTesting -= OnStopTestingHandler;
            EditorState.ActivePulsable.OnChanged -= ActivePulsableChangedHandler;
        }

        public override void OnInspectorGUI() {
            var mb = pulsable as MonoBehaviour;
            if (mb != null && mb.transform.hasChanged) {
                EditorState.StopAllTesting();
                mb.transform.hasChanged = false;
            }

            serializedObject.Update();

            var rect = EditorGUILayout.BeginVertical();
            rect.xMin -= 12; rect.xMax += 2;
            if (IsActivePulsable) {
                DrawActive(rect);
            }
            OnPulsableGUI();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (showDetections && !IsActivePulsable) {
                if (GUILayout.Button("Show Gizmos", GUILayout.Width(100))) {
                    EditorState.ActivePulsable.Value = pulsable;
                }
            }
            if (canTest && !isInGame) {
                if (GUILayout.Button("Test", GUILayout.Width(100))) {
                    StartTesting();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawActive(Rect rect) {
            var colour = STPrefs.ActiveSensorEditorColour;
            EditorGUI.DrawRect(rect, colour);
        }

        protected abstract void OnPulsableGUI();

        void OnPulsedHandler() {
            Repaint();
            if (Application.isPlaying || pulsable == null) {
                return;
            }
            IsTesting = true;
            SceneView.RepaintAll();
        }

        void StartTesting() {
            if (Application.isPlaying || pulsable == null) {
                return;
            }
            if (IsTesting) {
                EditorState.StopAllTesting();
            }
            pulsable.PulseAll();
            EditorState.ActivePulsable.Value = pulsable;
        }

        void OnStopTestingHandler() {
            if (!IsTesting || Application.isPlaying || pulsable == null) {
                return;
            }
            IsTesting = false;
            SceneView.RepaintAll();
        }

        void ActivePulsableChangedHandler() {
            pulsable.ShowDetectionGizmos = IsActivePulsable;
            SceneView.RepaintAll();
        }
    }

}