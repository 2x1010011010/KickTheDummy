using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FluidFlow
{
    public class EditorZoomPanArea
    {
        private const float kEditorWindowTabHeight = 21.0f;
        private const float kZoomSensitivity = .01f;
        private const float panMargin = 3;
        public Rect CanvasRect { get; private set; }
        private Vector2 zoomRange;
        private float zoom = 1;
        private Vector2 pan = Vector2.zero;
        public float Zoom => zoom;

        public EditorZoomPanArea(Vector2 zoomMinMax)
        {
            zoomRange = zoomMinMax;
        }

        public void Draw(Rect screenRect, Vector2 canvasSize, Action drawInternals, bool editorWindow = true)
        {
            var oldMatrix = GUI.matrix;
            if (Event.current.type == EventType.Repaint)    // limit pan, so part of the canvas is always visible
                pan = new Vector2(Mathf.Clamp(pan.x, Mathf.Min(0, -screenRect.width + panMargin * zoom), Mathf.Max(0, (canvasSize.x - panMargin) * zoom)),
                                  Mathf.Clamp(pan.y, Mathf.Min(0, -screenRect.height + panMargin * zoom), Mathf.Max(0, (canvasSize.y - panMargin) * zoom)));
            CanvasRect = new Rect((-pan) + screenRect.min, canvasSize);

            if (editorWindow) {
                screenRect.y += kEditorWindowTabHeight;
                GUI.EndGroup(); // end default window group from unity
            }
            HandleEvents(screenRect);

            GUI.matrix = Matrix4x4.Scale(new Vector3(zoom, zoom, 1));
            var zoomInv = 1f / zoom;
            using (new GUI.GroupScope(new Rect(screenRect.min * zoomInv, screenRect.size * zoomInv))) { // zoom group
                using (new GUI.GroupScope(new Rect((-pan) * zoomInv, canvasSize))) { // pan group
                    drawInternals.Invoke();
                }
            }
            GUI.matrix = oldMatrix; // restore
            if (editorWindow)
                GUI.BeginGroup(new Rect(0.0f, kEditorWindowTabHeight, Screen.width, Screen.height));
        }

        private void HandleEvents(Rect rect)
        {
            var screenCoordsMousePos = Event.current.mousePosition;
            if (!rect.Contains(screenCoordsMousePos))
                return;
            if (Event.current.type == EventType.ScrollWheel) {  // zooming
                var mouseBefore = ScreenToCanvas(screenCoordsMousePos);
                zoom += Mathf.Pow(zoom - Event.current.delta.y * kZoomSensitivity, 2) - Mathf.Pow(zoom, 2); // 'quadratic' zoom feels more linear
                zoom = Mathf.Clamp(zoom, zoomRange.x, zoomRange.y);
                var mouseAfter = ScreenToCanvas(screenCoordsMousePos);
                pan -= (mouseAfter - mouseBefore) * CanvasRect.size * zoom;
                Event.current.Use();
            }
            if (Event.current.type == EventType.MouseDrag && Event.current.button == 2) {   // scrollwheel drag
                pan -= Event.current.delta;
                Event.current.Use();
            }
        }

        public Vector2 ScreenToCanvas(Vector2 mousePos) => (mousePos - CanvasRect.min) / (CanvasRect.size * zoom);

        public void DrawViewControls(Vector2 viewSize)
        {
            var newZoom = Mathf.Pow(EditorGUILayout.Slider("Zoom", Mathf.Sqrt(zoom), Mathf.Sqrt(zoomRange.x), Mathf.Sqrt(zoomRange.y)), 2);
            if (newZoom != zoom) {
                var centerBefore = (viewSize * .5f - CanvasRect.min) / (CanvasRect.size * zoom);
                var centerAfter = (viewSize * .5f - CanvasRect.min) / (CanvasRect.size * newZoom);
                pan -= (centerAfter - centerBefore) * CanvasRect.size * newZoom;
                zoom = newZoom;
            }
            pan = EditorGUILayout.Vector2Field("Pan", pan);
        }
    }

    public class FFCanvasLayoutEditor : EditorWindow
    {
        [MenuItem("Window/FFCanvas Layout Editor")]
        private static void Init()
        {
            Init(null);
        }

        public static void Init(FFCanvas target)
        {
            var window = GetWindow<FFCanvasLayoutEditor>(false, "FFCanvas Layout");
            window.canvas = target;
            window.Show();
            FocusWindowIfItsOpen<FFCanvasLayoutEditor>();
        }

        private GUIStyle settingsWindowStyleCache = null;
        private GUIStyle SettingsWindowStyle {
            get {
                if (settingsWindowStyleCache == null) {
                    settingsWindowStyleCache = new GUIStyle(GUI.skin.window);
                    settingsWindowStyleCache.onNormal = settingsWindowStyleCache.normal;
                }
                return settingsWindowStyleCache;
            }
        }
        private ReorderableList surfacesList;
        private const string helpBoxText = "Mouse-wheel: zoom&pan\nLeft-click: select\nDrag-Right: move\nCtrl+Drag-Right: scale\n"
                                        + "Red areas mark overlapping UVs. Painting of the FFCanvas will not work properly with overlapping UVs! "
                                        + "Resolve this via a custom UV unwrap, switching to lightmapping UVs, or splitting submeshes into separate render targets.";

        private const string SurfaceDescrsName = "SurfaceDescriptors";
        private const string SubmeshDescrsName = "SubmeshDescriptors";
        private const string UVOffsetName = "UVOffset";
        private const string UVScaleName = "UVScale";
        private EditorZoomPanArea zoomArea;
        private FFCanvas canvas, lastCanvas;
        private readonly List<Vector2Int> selectedIndices = new List<Vector2Int>();
        private readonly List<string> selectedNames = new List<string>();
        private bool CanvasResizeModeExtend = true;
        private bool LockedAspectRatio = true;
        private float AspectRatio;

        private void OnEnable()
        {
            zoomArea = new EditorZoomPanArea(new Vector2(.1f, 1000f));
            SurfaceDescriptorDrawer.Lists.Clear();
        }

        private void PreviewUVLayout(Rect rect)
        {
            var onSelect = Event.current.type == EventType.MouseDown && Event.current.button == 0;
            var combineSelection = Event.current.shift;
            if (!canvas)
                return;
            var screenCoordsMousePos = Event.current.mousePosition; // relative to current GUI matrix
            var zoomCoord = (screenCoordsMousePos - rect.min) / rect.size;
            zoomCoord.y = 1 - zoomCoord.y;

            var resolution = canvas.Resolution;
            var aspectRatio = canvas.TextureChannelAspectRatio;
            var descriptor = new RenderTextureDescriptor(resolution.x, resolution.y, RenderTextureFormat.ARGB32);
            using (var overlap = new TmpRenderTexture(descriptor)) {
                using (var uv = new TmpRenderTexture(descriptor)) {
                    using (RestoreRenderTarget.RestoreActive()) {
                        Graphics.SetRenderTarget(uv);
                        GL.Clear(false, true, Color.clear);

                        for (var s = 0; s < canvas.SurfaceDescriptors.Count; s++) {
                            var surfaceDescr = canvas.SurfaceDescriptors[s];
                            var mesh = surfaceDescr.Renderer.GetMesh();
                            if (mesh) {
                                if (surfaceDescr.UVSet == UVSet.UV1 && !mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord1)) {
                                    if (surfaceDescr.Cache) { // try to get secondary uv mesh from cache
                                        if (surfaceDescr.Cache.TryGetSecondaryUVMesh(mesh, out var cache))
                                            mesh = cache;
                                    }
                                    if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord1)) {
                                        Debug.LogWarningFormat("Failed drawing preview for {0}, as it has no UV1", mesh);
                                        continue;
                                    }
                                }
                                for (var i = 0; i < surfaceDescr.SubmeshDescriptors.Count; i++) {
                                    var submeshDescr = surfaceDescr.SubmeshDescriptors[i];
                                    var hover = FFCanvasLayoutEditorUtil.AtlasRect(mesh, surfaceDescr.UVSet, submeshDescr, aspectRatio).Contains(zoomCoord);
                                    if (hover && onSelect) {
                                        ChangeSelection(new Vector2Int(s, i), combineSelection);
                                    }
                                    var active = selectedIndices.Contains(new Vector2Int(s, i));

                                    var submeshMask = mesh.ValidateSubmeshMask(submeshDescr.SubmeshMask);
                                    Shader.SetGlobalVector(DrawExtensions.AtlasTransformPropertyID, submeshDescr.AtlasTransform(aspectRatio));
                                    Shader.SetGlobalFloat(InternalShaders.DataPropertyID, (active ? .45f : 0) + (hover ? .15f : 0));
                                    FFCanvasLayoutEditorUtil.UVOverlap.Get(Utility.SetBit(0, surfaceDescr.UVSet == UVSet.UV1)).SetPass(0);
                                    foreach (var submeshIndex in submeshMask.EnumerateSetBits())
                                        Graphics.DrawMeshNow(mesh, Matrix4x4.identity, submeshIndex);
                                }
                            }
                        }
                        Graphics.Blit(uv, overlap, FFCanvasLayoutEditorUtil.MarkOverlap);
                    }
                }
                GUI.DrawTexture(rect, overlap, ScaleMode.ScaleToFit);
            }
        }


        private Rect windowRect = new Rect(Vector2.zero, Vector2.one * 500);
        private bool resizing = false;
        private Vector2 zoomStartPos;
        private Vector2 settingScrollPos;
        private Vector2 CanvasMousePosition;

        private void ResizeRect(ref Rect rect)
        {
            var mousePosition = Event.current.mousePosition;
            var border = new RectOffset(2, 2, 2, 2);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !resizing) {
                if (border.Add(rect).Contains(mousePosition) && !border.Remove(rect).Contains(mousePosition)) {
                    resizing = true;
                    Repaint();
                }
            }
            if (Event.current.type == EventType.MouseDrag && Event.current.button == 0 && resizing) {
                var delta = Event.current.delta;
                var lastMousePosition = mousePosition - delta;
                if (lastMousePosition.x < rect.xMin + 2)
                    rect.xMin = Mathf.Min(rect.xMin + delta.x, rect.xMax - 100);
                if (lastMousePosition.x > rect.xMax - 2)
                    rect.xMax = Mathf.Max(rect.xMax + delta.x, rect.xMin + 100);
                if (lastMousePosition.y < rect.yMin + 2)
                    rect.yMin = Mathf.Min(rect.yMin + delta.y, rect.yMax - 100);
                if (lastMousePosition.y > rect.yMax - 2)
                    rect.yMax = Mathf.Max(rect.yMax + delta.y, rect.yMin + 100);
                Repaint();
            }
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && resizing) {
                resizing = false;
            }
        }

        private void DrawNonZoomArea(Rect rect)
        {
            ResizeRect(ref windowRect);
            windowRect.position = new Vector2(Mathf.Clamp(windowRect.position.x, 0, rect.width - windowRect.width),
                                              Mathf.Clamp(windowRect.position.y, 0, rect.height - windowRect.height));
            windowRect.width = Mathf.Min(windowRect.width, position.width);
            windowRect.height = Mathf.Min(windowRect.height, position.height);

            BeginWindows();
            windowRect = GUI.Window(1, windowRect, DoSettingsWindow, "Settings", SettingsWindowStyle);
            EndWindows();
        }

        private void DoSettingsWindow(int _)
        {
            using (var scroll = new GUILayout.ScrollViewScope(settingScrollPos)) {
                settingScrollPos = scroll.scrollPosition;
                EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
                canvas = EditorGUILayout.ObjectField("Canvas", canvas, typeof(FFCanvas), true) as FFCanvas;
                EditorGUILayout.Space();
                if (canvas) {
                    if (canvas != lastCanvas || surfacesList == null) {
                        ResetSelection();
                        lastCanvas = canvas;
                        var canvasSO = new SerializedObject(canvas);
                        surfacesList = SurfaceDescriptorDrawer.CreateSurfaceDescriptorsListEdit(canvasSO);
                        var surfacesProp = canvasSO.FindProperty(SurfaceDescrsName);
                        void updateSelectionCallback(ReorderableList _)
                        {
                            for (var i = 0; i < surfacesProp.arraySize; i++) {
                                var list = SurfaceDescriptorDrawer.GetOrCreateSurfaceDescriptorEditList(surfacesProp.GetArrayElementAtIndex(i));
                                var local = i;
                                list.onSelectCallback = (_) => { ChangeSelection(new Vector2Int(local, list.index), Event.current.shift); };
                            }
                        }
                        updateSelectionCallback(surfacesList);
                        surfacesList.onChangedCallback = updateSelectionCallback;
                        AspectRatio = canvas.TextureChannelAspectRatio;
                    }

                    using (new EditorGUI.IndentLevelScope()) {
                        var so = new SerializedObject(canvas);
                        var resolutionProp = so.FindProperty("Resolution");
                        var oldRes = resolutionProp.vector2IntValue;
                        var newRes = EditorGUILayout.Vector2IntField("Resolution", oldRes);
                        {
                            var locked = EditorGUILayout.Toggle("Aspect-Ratio Locked", LockedAspectRatio);
                            if (!LockedAspectRatio && locked)
                                AspectRatio = canvas.TextureChannelAspectRatio;
                            LockedAspectRatio = locked;
                        }
                        CanvasResizeModeExtend = EditorGUILayout.Toggle("Resize Mode Extend", CanvasResizeModeExtend);
                        if (newRes != oldRes) {
                            if (newRes.y != oldRes.y && LockedAspectRatio)
                                newRes.x = (int)(newRes.y * AspectRatio);
                            if (newRes.x != oldRes.x && LockedAspectRatio)
                                newRes.y = (int)(newRes.x / AspectRatio);
                            newRes = Vector2Int.Min(Vector2Int.Max(newRes, Vector2Int.one), Vector2Int.one * SystemInfo.maxTextureSize);

                            resolutionProp.vector2IntValue = newRes;

                            if (CanvasResizeModeExtend) {
                                var surfaceDescrProp = so.FindProperty(SurfaceDescrsName);
                                for (var s = 0; s < surfaceDescrProp.arraySize; s++) {
                                    var sub = surfaceDescrProp.GetArrayElementAtIndex(s).FindPropertyRelative(SubmeshDescrsName);
                                    for (var i = 0; i < sub.arraySize; i++) {
                                        var descrProp = sub.GetArrayElementAtIndex(i);
                                        var offsetProp = descrProp.FindPropertyRelative(UVOffsetName);
                                        var scaleProp = descrProp.FindPropertyRelative(UVScaleName);
                                        offsetProp.vector2Value = (offsetProp.vector2Value * oldRes) / newRes;
                                        scaleProp.floatValue = (scaleProp.floatValue * oldRes.y) / newRes.y;
                                    }
                                }
                            }
                        }

                        EditorGUILayout.Space();
                        surfacesList.DoLayoutListIndented();
                        so.ApplyModifiedProperties();
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Layout Tools", EditorStyles.boldLabel);

                    using (new EditorGUI.IndentLevelScope()) {
                        Selection();
                        EditorGUILayout.Space();

                        var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.miniButton);
                        rect = EditorGUI.IndentedRect(rect);

                        ValidateSelection();
                        using (new GUIEnableScope(selectedIndices.Count > 0)) {
                            if (GUI.Button(new Rect(rect.min, new Vector2(rect.width * .5f, rect.height)), "Normalize Selected", EditorStyles.miniButton)) {
                                if (EditorUtility.DisplayDialog("Normalize Selected Descriptor?", "This will overwrite the existing layout!", "Yes", "No")) {
                                    Undo.RecordObject(canvas, "FFCanvas Normalize Selected");
                                    foreach (var selection in selectedIndices)
                                        canvas.SurfaceDescriptors[selection.x].Normalize(selection.y, canvas.TextureChannelAspectRatio);
                                }
                            }
                        }
                        if (GUI.Button(new Rect(rect.min + Vector2.right * rect.width * .5f, new Vector2(rect.width / 2, rect.height)), "Auto Layout Grid", EditorStyles.miniButton)) {
                            if (EditorUtility.DisplayDialog("Auto Layout Surfaces?", "This will overwrite the existing layout!", "Yes", "No")) {
                                Undo.RecordObject(canvas, "FFCanvas Auto Layout Grid");
                                canvas.GridPack();
                                AspectRatio = canvas.TextureChannelAspectRatio; // update aspect ratio
                            }
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("View", EditorStyles.boldLabel);
                    using (new EditorGUI.IndentLevelScope()) {
                        zoomArea.DrawViewControls(position.size);
                        EditorGUILayout.Space();
                        if (canvas) {
                            var resolution = canvas.Resolution;
                            EditorGUILayout.LabelField("Pixel:", string.Format("({0}, {1})",
                                Mathf.FloorToInt(CanvasMousePosition.x * resolution.x) + 1,
                                Mathf.FloorToInt(CanvasMousePosition.y * resolution.y) + 1), EditorStyles.miniLabel);
                            EditorGUILayout.LabelField("UV:", string.Format("({0}, {1})", CanvasMousePosition.x, CanvasMousePosition.y), EditorStyles.miniLabel);
                        }
                    }
                }
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(helpBoxText, MessageType.Info);
            }

            GUI.DragWindow(new Rect(Vector2.one * 2, windowRect.size - Vector2.one * 4));
        }

        private void Selection()
        {
            using (new GUILayout.HorizontalScope()) {

                EditorGUILayout.LabelField("Selected:", EditorStyles.boldLabel);
                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.MaxWidth(75))) {
                    GUI.FocusControl(null);
                    var popup = new GenericMenu();
                    void callback(object data)
                    {
                        var selection = (Vector2Int)data;
                        if (selection == -Vector2Int.one) {
                            ResetSelection();
                        } else if (selection == -Vector2Int.one * 2) {
                            ResetSelection();
                            for (var s = 0; s < canvas.SurfaceDescriptors.Count; s++) {
                                for (var i = 0; i < canvas.SurfaceDescriptors[s].SubmeshDescriptors.Count; i++) {
                                    ChangeSelection(new Vector2Int(s, i), true);
                                }
                            }
                        } else {
                            ChangeSelection(selection, true);
                        }
                    }
                    popup.AddItem(new GUIContent("None"), false, callback, -Vector2Int.one);
                    popup.AddItem(new GUIContent("All"), false, callback, -Vector2Int.one * 2);
                    for (var s = 0; s < canvas.SurfaceDescriptors.Count; s++) {
                        var surf = canvas.SurfaceDescriptors[s];
                        if (surf.Renderer == null)
                            continue;
                        var mesh = surf.Renderer.GetMesh();
                        if (mesh == null)
                            continue;
                        for (var i = 0; i < surf.SubmeshDescriptors.Count; i++) {
                            var selection = new Vector2Int(s, i);
                            popup.AddItem(new GUIContent(SelectionToName(selection)), selectedIndices.Contains(selection), callback, selection);
                        }
                    }
                    popup.ShowAsContext();
                }
            }

            using (new EditorGUI.IndentLevelScope()) {
                foreach (var selectedName in selectedNames) {
                    EditorGUILayout.LabelField(selectedName, EditorStyles.boldLabel);
                }
            }
        }

        private bool ValidateSelection()
        {
            if (!canvas)
                return false;
            for (var i = selectedIndices.Count - 1; i >= 0; i--) { // backwards, so no special handling for deletion is needed
                var selection = selectedIndices[i];
                var selectionValid = true;
                for (var n = i - 1; n >= 0; n--) {
                    if (selectedIndices[n] == selection)   // duplicate selection
                        selectionValid = false;
                }
                selectionValid &= selection.x >= 0 && selection.y >= 0 && selection.x < canvas.SurfaceDescriptors.Count && selection.y < canvas.SurfaceDescriptors[selection.x].SubmeshDescriptors.Count;   // out of range
                if (!selectionValid) {
                    selectedIndices.RemoveAt(i);
                }
            }
            return selectedIndices.Count > 0;
        }

        private string SelectionToName(Vector2Int selection)
        {
            var surf = canvas.SurfaceDescriptors[selection.x];
            var rendererName = "Unset";
            var submeshMaskName = "";
            if (surf.Renderer) {
                rendererName = surf.Renderer.name;
                var mesh = surf.Renderer.GetMesh();
                if (mesh) {
                    submeshMaskName = EditorUtil.SubmeshMaskToString(surf.SubmeshDescriptors[selection.y].SubmeshMask, mesh.subMeshCount);
                }
            }
            return $"{selection.x}.{selection.y}: {rendererName} ({submeshMaskName})";
        }

        private void ChangeSelection(Vector2Int newSelection, bool combine)
        {
            if (!combine)
                selectedIndices.Clear();
            if (selectedIndices.Contains(newSelection)) {
                selectedIndices.Remove(newSelection);
            } else {
                selectedIndices.Add(newSelection);
            }
            if (!ValidateSelection()) {
                ResetSelection();
            }
            if (selectedIndices.Contains(newSelection) && surfacesList != null) {
                surfacesList.index = newSelection.x;
                SurfaceDescriptorDrawer.GetOrCreateSurfaceDescriptorEditList(surfacesList.serializedProperty.GetArrayElementAtIndex(newSelection.x)).index = newSelection.y;
            }
            selectedNames.Clear();
            foreach (var selection in selectedIndices) {
                selectedNames.Add(SelectionToName(selection));
            }
        }

        private void ResetSelection()
        {
            selectedIndices.Clear();
            selectedNames.Clear();
            if (surfacesList != null)
                surfacesList.index = -1;
        }

        public void OnGUI()
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1) {
                zoomStartPos = CanvasMousePosition;
                zoomStartPos.y = 1 - zoomStartPos.y;
            }
            if (Event.current.type == EventType.MouseDrag && Event.current.button == 1) {
                ValidateSelection();
                if (selectedIndices.Count == 0)
                    return;

                var delta = new Vector2(Event.current.delta.x, -Event.current.delta.y);
                var move = !(Event.current.control || Event.current.shift);
                if (canvas && delta.sqrMagnitude > 0) {
                    var so = new SerializedObject(canvas);
                    foreach (var selection in selectedIndices) {
                        var descrProp = so.FindProperty(SurfaceDescrsName).GetArrayElementAtIndex(selection.x)
                                          .FindPropertyRelative(SubmeshDescrsName).GetArrayElementAtIndex(selection.y);
                        var offsetProp = descrProp.FindPropertyRelative(UVOffsetName);
                        var scaleProp = descrProp.FindPropertyRelative(UVScaleName);
                        if (move) {
                            offsetProp.vector2Value += delta / (zoomArea.CanvasRect.size * zoomArea.Zoom);
                        } else {
                            var amount = delta.y / (zoomArea.CanvasRect.height * zoomArea.Zoom);
                            var distBefore = Vector2.Distance(offsetProp.vector2Value, zoomStartPos);
                            offsetProp.vector2Value += (offsetProp.vector2Value - zoomStartPos) * amount;
                            var distAfter = Vector2.Distance(offsetProp.vector2Value, zoomStartPos);
                            scaleProp.floatValue *= distAfter / distBefore;
                        }
                    }
                    so.ApplyModifiedProperties();
                }
            }

            var rect = new Rect(0, 0, position.width, position.height);
            var minSize = Mathf.Min(rect.width, rect.height);
            if (canvas) {
                var res = canvas.Resolution;
                zoomArea.Draw(rect, res, () => { PreviewUVLayout(new Rect(0, 0, res.x, res.y)); });
                CanvasMousePosition = zoomArea.ScreenToCanvas(Event.current.mousePosition);
            }
            DrawNonZoomArea(rect);

            wantsMouseMove = true;
            if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown)
                Repaint();
        }
    }

    public static class FFCanvasLayoutEditorUtil
    {
        public static readonly MaterialCache UVOverlap = new MaterialCache(InternalShaders.RootPath + "/UVOverlap", InternalShaders.SetSecondaryUV);
        public static readonly MaterialCache MarkOverlap = new MaterialCache(InternalShaders.RootPath + "/MarkOverlap");

        private static Dictionary<(Mesh, UVSet), Rect[]> UVBoundsCache = new Dictionary<(Mesh, UVSet), Rect[]>();

        private static Rect UVBounds(Mesh mesh, UVSet uvSet, int submeshMask)
        {
            if (!UVBoundsCache.TryGetValue((mesh, uvSet), out var bounds)) {
                bounds = SurfaceExtensions.UVBounds(mesh, uvSet);
                UVBoundsCache.Add((mesh, uvSet), bounds);
            }
            return bounds.Combine(mesh.ValidateSubmeshMask(submeshMask));
        }

        public static Rect AtlasRect(Mesh mesh, UVSet uvSet, SurfaceDescriptor.SubmeshDescriptor descriptor, float textureAspectRatio)
        {
            var uvBounds = UVBounds(mesh, uvSet, descriptor.SubmeshMask);
            var aspect = descriptor.UVAspect / textureAspectRatio;
            return new Rect(uvBounds.x * descriptor.UVScale * aspect + descriptor.UVOffset.x,
                            uvBounds.y * descriptor.UVScale + descriptor.UVOffset.y,
                            uvBounds.width * descriptor.UVScale * aspect,
                            uvBounds.height * descriptor.UVScale);
        }

    }
}