using UnityEditor;
using UnityEngine;

namespace FluidFlow
{
    [CustomPropertyDrawer(typeof(ComponentMask))]
    public class ComponentMaskPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var Mask = (ComponentMask)property.intValue;
            if (EditorGUI.DropdownButton(position, new GUIContent(Mask.ToText()), FocusType.Keyboard)) {
                PopupWindow.Show(position, new ComponentMaskPopupWindow(property, position.width));
            }
        }

        private class ComponentMaskPopupWindow : PopupWindowContent
        {
            // private static readonly GUIStyle toggleStyle = EditorStyles.miniBoldLabel;
            private readonly SerializedProperty Property;
            private readonly float Width;
            private ComponentMask Mask;

            public ComponentMaskPopupWindow(SerializedProperty property, float width)
            {
                Property = property;
                Mask = (ComponentMask)Property.intValue;
                Width = Mathf.Max(110, width);
            }

            public override Vector2 GetWindowSize()
            {
                return new Vector2(Width, (7) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) + 8);
            }

            public override void OnGUI(Rect rect)
            {
                var offset = new RectOffset(4, 4, 4, 4);
                rect = offset.Remove(rect);
                using (new GUILayout.AreaScope(rect)) {
                    if (EditorGUILayout.ToggleLeft("None", Mask == ComponentMask.None, EditorStyles.miniBoldLabel))
                        Mask = ComponentMask.None;
                    if (EditorGUILayout.ToggleLeft("All", Mask == ComponentMask.All, EditorStyles.miniBoldLabel))
                        Mask = ComponentMask.All;
                    var r = Mask.HasFlag(ComponentMask.R);
                    var g = Mask.HasFlag(ComponentMask.G);
                    var b = Mask.HasFlag(ComponentMask.B);
                    var a = Mask.HasFlag(ComponentMask.A);
                    if (EditorGUILayout.ToggleLeft("R", r, EditorStyles.miniBoldLabel) != r)
                        Mask ^= ComponentMask.R;
                    if (EditorGUILayout.ToggleLeft("G", g, EditorStyles.miniBoldLabel) != g)
                        Mask ^= ComponentMask.G;
                    if (EditorGUILayout.ToggleLeft("B", b, EditorStyles.miniBoldLabel) != b)
                        Mask ^= ComponentMask.B;
                    if (EditorGUILayout.ToggleLeft("A", a, EditorStyles.miniBoldLabel) != a)
                        Mask ^= ComponentMask.A;
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (GUILayout.Button("Revert", EditorStyles.miniButton))
                            Mask = (ComponentMask)Property.intValue;
                        if (GUILayout.Button("Apply", EditorStyles.miniButton))
                            editorWindow.Close();
                    }
                }
            }

            public override void OnClose()
            {
                Property.intValue = (int)Mask;
                Property.serializedObject.ApplyModifiedProperties();
                base.OnClose();
            }
        }
    }
}
