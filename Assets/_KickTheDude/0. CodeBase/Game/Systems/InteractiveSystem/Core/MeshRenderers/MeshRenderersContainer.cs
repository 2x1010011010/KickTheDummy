using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

public class MeshRenderersContainer : MonoBehaviour
{
    [SerializeField, BoxGroup("SETUP")] private Transform _renderersRoot;
    [SerializeField, BoxGroup("SETUP")] private List<Renderer> _renderers = new List<Renderer>();

    public Transform RenderersRoot => _renderersRoot;
    public IEnumerable<Renderer> Renderers => _renderers;

    private void OnValidate()
    {
        if (_renderersRoot == null) _renderersRoot = transform;
    }

    [Button(ButtonSizes.Large), BoxGroup("SETUP")]
    public void TryFillContainer()
    {
        ClearContainer();

        foreach (Renderer renderer in _renderersRoot.GetComponentsInChildren<Renderer>(false))
        {
            _renderers.Add(renderer);
        }
    }

    [Button(ButtonSizes.Large), BoxGroup("SETUP")]
    public void ClearContainer()
    {
        _renderers.Clear();
    }

    [Button(ButtonSizes.Large), BoxGroup("ACTIONS")]
    public void Show()
    {
        foreach (var renderer in _renderers)
            renderer.enabled = true;
    }

    [Button(ButtonSizes.Large), BoxGroup("ACTIONS")]
    public void Hide()
    {
        foreach (var renderer in _renderers)
            renderer.enabled = false;
    }

    public void ApplyMaterial(Material material)
    {
        foreach (var meshRenderer in _renderers)
        {
            var materials = meshRenderer.sharedMaterials;
            var newMaterials = new List<Material>();

            newMaterials.AddRange(materials);
            newMaterials.Add(material);

            meshRenderer.sharedMaterials = newMaterials.ToArray();
        }
    }

    public void RemoveMaterial(Material material)
    {
        foreach (var meshRenderer in _renderers)
        {
            var materials = meshRenderer.sharedMaterials;
            var newMaterials = new List<Material>();

            newMaterials.AddRange(materials);
            newMaterials.Remove(material);

            meshRenderer.sharedMaterials = newMaterials.ToArray();
        }
    }
}
