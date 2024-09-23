using System.Collections.Generic;
using UnityEngine;

namespace FluidFlow
{
    public class RPMaterialSwitcher : MonoBehaviour
    {
        public enum RenderPipeline
        {
            DefaultRP,
            URP,
            HDRP
        }

        public RPMaterialStash MaterialStash;
        [SerializeField, HideInInspector] private RenderPipeline lastRP = RenderPipeline.DefaultRP;
        [SerializeField, Header("Select RP")] private RenderPipeline TargetRP = RenderPipeline.DefaultRP;

        private RenderPipeline CurrentRP()
        {
            var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            if (rp) {
                // we can not directly check for the RP-asset type, as these might not be defined in the users project.. so just check the name and pray
                // UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset
                // UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset
                var rpName = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.GetType().Name.ToLower();
                if (rpName.Contains("universal") || rpName.Contains("ur"))
                    return RenderPipeline.URP;
                if (rpName.Contains("high") || rpName.Contains("hd"))
                    return RenderPipeline.HDRP;
            }
            return RenderPipeline.DefaultRP;
        }

        private void OnEnable()
        {
            var rp = CurrentRP();
            if (rp != TargetRP) {
                Debug.LogWarningFormat(
                    "FluidFlow: It seems like you are using '{0}', while the demo scene is set to '{1}'. Try to switch the target RP in the '{2}'.",
                    rp,
                    TargetRP,
                    name);
                UpdateScene(rp);
            }
        }

        public void UpdateScene(RenderPipeline targetRP)
        {
            TargetRP = targetRP;
            Debug.LogFormat("FluidFlow: Switching materials to '{0}'.", targetRP);
            var renderers = FindObjectsOfType<Renderer>(true);
            var materials = new List<Material>();
            foreach (var rd in renderers) {
                rd.GetSharedMaterials(materials);
                var modified = false;
                for (var i = materials.Count - 1; i >= 0; i--) {
                    if (MaterialStash.TryFind(materials[i], out var rpMaterial)) {
                        var target = targetRP switch {
                            RenderPipeline.URP => rpMaterial.URP,
                            RenderPipeline.HDRP => rpMaterial.HDRP,
                            _ => rpMaterial.Default,
                        };
                        if (materials[i] != target) {
                            Debug.LogFormat("FluidFlow: Switching '{0}' to '{1}'.", materials[i], target);
                            materials[i] = target;
                            modified = true;
                        }
                    }
                }
                if (modified)
                    rd.sharedMaterials = materials.ToArray();
            }

            var lights = FindObjectsOfType<Light>(true);
            foreach (var light in lights) {
                light.intensity = 1.5f;    // switching from HDRP seems to mess with light intensity
            }
        }

        private void OnValidate()
        {
            if (lastRP != TargetRP) {
                lastRP = TargetRP;
                UpdateScene(TargetRP);
            }
        }

        [ContextMenu("Switch To Default")] public void SwitchToDefault() => UpdateScene(RenderPipeline.DefaultRP);
        [ContextMenu("Switch To URP")] public void SwitchToURP() => UpdateScene(RenderPipeline.URP);
        [ContextMenu("Switch To HDRP")] public void SwitchToHDRP() => UpdateScene(RenderPipeline.HDRP);

#if UNITY_EDITOR
        [ContextMenu("Create RPMaterialStash")]
        public void CreateMaterialStashSO()
        {
            var asset = ScriptableObject.CreateInstance<RPMaterialStash>();
            UnityEditor.AssetDatabase.CreateAsset(asset, "Assets/RPMaterialStash.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.EditorUtility.FocusProjectWindow();
            UnityEditor.Selection.activeObject = asset;
        }
#endif
    }
}
