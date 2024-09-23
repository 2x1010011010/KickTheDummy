using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FluidFlow
{
    public static class EditorUtil
    {
        public static readonly float ListElementPadding = 2;
        public static readonly float ListElementHeight = EditorGUIUtility.singleLineHeight + ListElementPadding * 2;

        private static GUIContent menuContentCache = null;
        private static GUIContent MenuContent {
            get {
                if (menuContentCache == null)
                    menuContentCache = EditorGUIUtility.IconContent("_Menu");
                return menuContentCache;
            }
        }

        private static GUIStyle menuButtonStyleCache = null;
        private static GUIStyle MenuButtonStyle {
            get {
                if (menuButtonStyleCache == null) {
                    menuButtonStyleCache = EditorStyles.miniButton;
                    menuButtonStyleCache.padding = new RectOffset(0, 0, 0, 0);
                }
                return menuButtonStyleCache;
            }
        }

        public static bool IsModel(UnityEngine.Object obj)
        {
            return PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.Model;
        }

        public static IEnumerable<string> EnumerateUniqueTexturePropertyNames(List<SurfaceDescriptor> descriptors)
        {
            var unique = new HashSet<string>();
            foreach (var target in descriptors) {
                if (!target.Renderer)
                    continue;
                var materials = Shared.MaterialList();
                target.Renderer.GetSharedMaterials(materials);
                foreach (var material in materials) {
                    foreach (var property in material.GetTexturePropertyNames()) {
                        if (!unique.Contains(property)) {
                            unique.Add(property);
                            yield return property;
                        }
                    }
                }
            }
        }

        public class SubmeshMaskPopupWindow : PopupWindowContent
        {
            private static readonly GUIStyle toggleStyle = EditorStyles.miniBoldLabel;
            private readonly SerializedProperty Property;
            private readonly int SubmeshCount;
            private readonly float Width;
            private int SubmeshMask;

            public SubmeshMaskPopupWindow(SerializedProperty property, Mesh targetMesh, float width)
            {
                Property = property;
                SubmeshCount = targetMesh.subMeshCount;
                SubmeshMask = Property.intValue;
                Width = Mathf.Max(110, width);
            }

            public override Vector2 GetWindowSize()
            {
                return new Vector2(Width, (SubmeshCount + 3) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) + 8);
            }

            public override void OnGUI(Rect rect)
            {
                var offset = new RectOffset(4, 4, 4, 4);
                rect = offset.Remove(rect);
                var newMask = 0;
                using (new GUILayout.AreaScope(rect)) {
                    if (EditorGUILayout.ToggleLeft("Nothing", SubmeshMask == 0, toggleStyle)) {
                        SubmeshMask = 0;
                    }
                    if (EditorGUILayout.ToggleLeft("Everything", SubmeshMask == (1 << SubmeshCount) - 1, toggleStyle)) {
                        SubmeshMask = (1 << SubmeshCount) - 1;
                    }
                    for (var i = 0; i < SubmeshCount; i++) {
                        var set = EditorGUILayout.ToggleLeft(i.ToString(), SubmeshMask.IsBitSet(i), toggleStyle);
                        newMask += Utility.SetBit(i, set);
                    }
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button("Revert", EditorStyles.miniButton)) {
                            newMask = Property.intValue;
                        }
                        if (GUILayout.Button("Apply", EditorStyles.miniButton)) {
                            SubmeshMask = newMask;
                            editorWindow.Close();
                        }
                    }
                }
                SubmeshMask = newMask;
            }

            public override void OnClose()
            {
                Property.intValue = SubmeshMask;
                Property.serializedObject.ApplyModifiedProperties();
                base.OnClose();
            }
        }

        public static void SubmeshMaskField(Rect rect, SerializedProperty maskProperty, Mesh target)
        {
            using (var propertyScope = new EditorGUI.PropertyScope(rect, GUIContent.none, maskProperty)) {
                if (EditorGUI.DropdownButton(rect, new GUIContent(SubmeshMaskToString(maskProperty.intValue, target.subMeshCount)), FocusType.Passive, EditorStyles.popup)) {
                    PopupWindow.Show(rect, new SubmeshMaskPopupWindow(maskProperty, target, rect.width));
                }
            }
        }

        public static string SubmeshMaskToString(int mask, int submeshCount)
        {
            var relevantMask = mask & ((1 << submeshCount) - 1);
            if (relevantMask == 0)
                return "Nothing";
            if (relevantMask == (1 << submeshCount) - 1)
                return "Everything";
            var builder = new System.Text.StringBuilder();
            for (int i = 0; i < submeshCount; i++) {
                if (relevantMask.IsBitSet(i)) {
                    if (builder.Length > 0)
                        builder.Append(", ");
                    builder.Append(i);
                }
            }
            return builder.ToString();
        }

        public static void OptionsTextFieldLayout(SerializedProperty property, System.Func<List<string>> options, bool showLabel = false)
        {
            OptionsTextField(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.textField), property, options, showLabel);
        }

        public static void OptionsTextField(Rect rect, SerializedProperty property, System.Func<List<string>> options, bool showLabel = false)
        {
            rect.xMax -= rect.height + 1;
            if (showLabel)
                EditorGUI.PropertyField(rect, property);
            else
                FullsizePropertyField(rect, property);

            var style = EditorStyles.miniButton;
            style.padding = new RectOffset(0, 0, 0, 0);
            if (GUI.Button(new Rect(rect.xMax + 1, rect.yMin, rect.height, rect.height), MenuContent, style)) {
                GenericMenu.MenuFunction2 callback = (object data) => {
                    property.stringValue = data as string;
                    property.serializedObject.ApplyModifiedProperties();
                };
                GUI.FocusControl(null);
                var popup = new GenericMenu();
                foreach (var name in options())
                    popup.AddItem(new GUIContent(name), false, callback, name);
                popup.ShowAsContext();
            }
        }

        public static void DoLayoutListIndented(this UnityEditorInternal.ReorderableList list)
        {
            EditorGUILayout.Space(list.GetHeight());
            var rect = EditorGUI.IndentedRect(GUILayoutUtility.GetLastRect());
            using (var noIndent = new GUIIndentScope(0))
                list.DoList(rect);
        }

        public static void PropertyComponentsFieldLayout(SerializedProperty texProperty, SerializedProperty vecProperty)
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.objectField);
            PropertyComponentsField(rect, texProperty, vecProperty);
        }

        public static void PropertyComponentsField(Rect rect, SerializedProperty texProperty, SerializedProperty maskProperty, GUIContent content = null)
        {
            const float ComponentsFieldWidth = 75;
            rect.xMax -= ComponentsFieldWidth + 2;
            if (content != null)
                EditorGUI.PropertyField(rect, texProperty, content);
            else
                EditorGUI.PropertyField(rect, texProperty);
            FullsizePropertyField(new Rect(rect.xMax + 2, rect.y - 1, ComponentsFieldWidth, rect.height), maskProperty);
        }

        public static void ToggleableFieldLayout(SerializedProperty toggleProperty, System.Action drawInternal)
        {
            EditorGUILayout.PropertyField(toggleProperty);
            if (toggleProperty.boolValue) {
                using (var indented = new EditorGUI.IndentLevelScope(1))
                    drawInternal.Invoke();
            }
        }

        public static void FullsizePropertyField(Rect rect, SerializedProperty property)
        {
            EditorGUI.PropertyField(rect, property, new GUIContent("", property.tooltip));
        }

        public static void MinMaxSliderLayout(SerializedProperty property, float lower, float upper)
        {
            var range = property.vector2Value;
            float min = range.x;
            float max = range.y;
            EditorGUILayout.MinMaxSlider(property.displayName, ref min, ref max, lower, upper);
            if (min != range.x || max != range.y)
                property.vector2Value = new Vector2(min, max);
        }

        public static bool ButtonFieldLayout(string label, string text)
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(rect, label);
            rect.xMin += EditorGUIUtility.labelWidth;
            return GUI.Button(rect, text);
        }

        public static void ObjectPopup<T>(Rect rect, T value, T[] array, GenericMenu.MenuFunction2 callback) where T : Object
        {
            rect.xMax -= rect.height;

            using (var disabled = new GUIEnableScope(false))
                EditorGUI.ObjectField(rect, value, typeof(T), false);

            if (GUI.Button(new Rect(rect.xMax, rect.yMin, rect.height, rect.height), MenuContent, MenuButtonStyle)) {
                GUI.FocusControl(null);
                var popup = new GenericMenu();
                foreach (var elem in array)
                    popup.AddItem(new GUIContent(elem.ToString()), elem == value, callback, elem);
                popup.ShowAsContext();
            }
        }

        public static void PropertyFieldWithOptions<T>(Rect rect, SerializedProperty property, GUIContent label, System.Func<List<System.Tuple<string, T>>> options) where T : Object
        {
            rect.xMax -= rect.height;
            EditorGUI.PropertyField(rect, property, label);

            if (GUI.Button(new Rect(rect.xMax, rect.yMin, rect.height, rect.height), MenuContent, MenuButtonStyle)) {
                GUI.FocusControl(null);
                var popup = new GenericMenu();
                GenericMenu.MenuFunction2 updatePropertyValue = (object value) => {
                    var tVal = value as T;
                    if (tVal) {
                        property.objectReferenceValue = tVal;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                };
                foreach (var elem in options.Invoke())
                    popup.AddItem(new GUIContent(elem.Item1), false, updatePropertyValue, elem.Item2);
                popup.ShowAsContext();
            }
        }

        public static IEnumerable<T> EnumerateAllAssets<T>() where T : Object
        {
            foreach (var guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
                yield return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }

    public class HeaderGroupScope : System.IDisposable
    {
        public bool expanded { get; private set; }

        public HeaderGroupScope(string name)
        {
            expanded = EditorPrefs.GetBool(FFEditorOnlyUtility.EditorPrefPrefix + name, true);
            var after = EditorGUILayout.BeginFoldoutHeaderGroup(expanded, name);
            if (after != expanded) {
                expanded = after;
                EditorPrefs.SetBool(FFEditorOnlyUtility.EditorPrefPrefix + name, expanded);
            }
            EditorGUI.indentLevel++;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel--;
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }

    public struct GUIEnableScope : System.IDisposable
    {
        private bool tmp;

        public GUIEnableScope(bool enable)
        {
            tmp = GUI.enabled;
            GUI.enabled = enable;
        }

        public void Dispose()
        {
            GUI.enabled = tmp;
        }
    }

    public struct GUIHighlightScope : System.IDisposable
    {
        private Color tmpBg, tmpFg;

        public GUIHighlightScope(bool set, Color color)
        {
            tmpBg = GUI.backgroundColor;
            tmpFg = GUI.color;
            if (set)
                GUI.backgroundColor = color;
        }

        public GUIHighlightScope(bool set, Color bgColor, Color fgColor)
        {
            tmpBg = GUI.backgroundColor;
            tmpFg = GUI.color;
            if (set) {
                GUI.backgroundColor = bgColor;
                GUI.color = fgColor;
            }
        }

        public void Dispose()
        {
            GUI.backgroundColor = tmpBg;
            GUI.color = tmpFg;
        }
    }

    public struct GUIIndentScope : System.IDisposable
    {
        private int tmp;

        public GUIIndentScope(int indent)
        {
            tmp = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indent;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel = tmp;
        }
    }

    public struct LabelWidthScope : System.IDisposable
    {
        private float tmp;

        public LabelWidthScope(float width)
        {
            tmp = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = width;
        }

        public void Dispose()
        {
            EditorGUIUtility.labelWidth = tmp;
        }
    }

    public struct HorizontalLayout
    {
        private Rect rect;
        private float[] elements;

        public HorizontalLayout(Rect rect, params int[] elements)
        {
            this.rect = rect;
            this.elements = new float[elements.Length];
            var sumInv = 1.0f / elements.Sum();
            var tmpSum = 0;
            for (int i = 0; i < elements.Length; i++) {
                tmpSum += elements[i];
                this.elements[i] = tmpSum * sumInv;
            }
        }

        public Rect Get(int index, int line = 0, int padding = 2)
        {
            return Get(index > 0 ? elements[index - 1] : 0f, elements[index], line, padding);
        }

        public Rect Get(float from, float to, int line = 0, int padding = 2)
        {
            var width = rect.width * (to - from) - padding * 2;
            var left = rect.x + rect.width * from + padding;
            var right = left + width;
            var top = rect.y + EditorGUIUtility.singleLineHeight * line;
            return new Rect(left, top, right - left, EditorGUIUtility.singleLineHeight);
        }
    }
}