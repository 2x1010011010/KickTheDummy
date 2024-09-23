using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

public class OutlineHighlighter : MonoBehaviour
{
    private const string OUTLINE_MATERIAL_PATH = "Materials/Outline";

    [BoxGroup("SETUP")]
    [SerializeField] private MeshRenderersContainer _meshRenderersContainer;

    private bool _isHighlighted;

    [BoxGroup("ACTIONS"), Button(ButtonSizes.Large)]
    public void Highlight()
    {
        if (_meshRenderersContainer == null) return;

        if (_isHighlighted) return;

        foreach (var meshRenderer in _meshRenderersContainer.Renderers)
        {
            var materials = meshRenderer.sharedMaterials;
            var newMaterials = new List<Material>();

            newMaterials.AddRange(materials);
            newMaterials.Add(Resources.Load(OUTLINE_MATERIAL_PATH) as Material);

            meshRenderer.sharedMaterials = newMaterials.ToArray();
        }

        _isHighlighted = true;
    }

    [BoxGroup("ACTIONS"), Button(ButtonSizes.Large)]
    public void Deactivate()
    {
        if (_meshRenderersContainer == null) return;

        if (!_isHighlighted) return;

        foreach (var meshRenderer in _meshRenderersContainer.Renderers)
        {
            var materials = meshRenderer.sharedMaterials;
            var newMaterials = new List<Material>();

            newMaterials.AddRange(materials);
            newMaterials.Remove(materials[materials.Length - 1]);

            meshRenderer.sharedMaterials = newMaterials.ToArray();
        }

        _isHighlighted = false;
    }
}
