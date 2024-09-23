using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Micosmo.SensorToolkit;

namespace Micosmo.SensorToolkit.Editors {

    [CustomPropertyDrawer(typeof(NavMeshMaskAttribute))]
    public class NavMeshMaskDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty serializedProperty, GUIContent label) {

            using (new GUILayout.HorizontalScope()) {

                position = EditorGUI.PrefixLabel(position, label);

                EditorGUI.BeginChangeCheck();

                string[] areaNames = GameObjectUtility.GetNavMeshAreaNames();
                List<string> completeAreaNames = new List<string>();

                foreach (string name in areaNames) {
                    var id = GameObjectUtility.GetNavMeshAreaFromName(name);
                    while (id >= completeAreaNames.Count) {
                        completeAreaNames.Add("");
                    }
                    completeAreaNames[id] = name;
                }

                int mask = serializedProperty.intValue;

                mask = EditorGUI.MaskField(position, mask, completeAreaNames.ToArray());
                if (EditorGUI.EndChangeCheck()) {
                    serializedProperty.intValue = mask;
                }
            }
        }
    }

}