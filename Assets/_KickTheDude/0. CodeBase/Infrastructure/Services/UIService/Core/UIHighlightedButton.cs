using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHighlightedButton : UIButton
{
    [SerializeField, BoxGroup("PARAMETERS")] private bool _highlightByColor;

    [SerializeField, BoxGroup("SETUP"), ShowIf("_highlightByColor")] private Image[] _images;
    [SerializeField, BoxGroup("SETUP"), ShowIf("_highlightByColor")] private Color _standartColor = Color.white;
    [SerializeField, BoxGroup("SETUP"), ShowIf("_highlightByColor")] private Color _highlightColor = Color.yellow;

    [SerializeField, BoxGroup("SETUP"), HideIf("_highlightByColor")] private List<GameObject> _activateObjects;
    [SerializeField, BoxGroup("SETUP"), HideIf("_highlightByColor")] private List<GameObject> _deactivateObjects;

    public bool IsHighlighted { get; private set; }

    [Button("HIGHLIGHT", ButtonSizes.Large), BoxGroup("ACTIONS")]
    public virtual void Highlight()
    {
        if (_highlightByColor)
        {
            SetImageColor(_highlightColor);
        }
        else
        {
            foreach (var activateObject in _activateObjects)
                activateObject.SetActive(true);

            foreach (var deactivateObject in _deactivateObjects)
                deactivateObject.SetActive(false);
        }

        IsHighlighted = true;
    }

    [Button("UNHIGHLIGHT", ButtonSizes.Large), BoxGroup("ACTIONS")]
    public virtual void Unhighlight()
    {
        if (_highlightByColor)
        {
            SetImageColor(_standartColor);
        }
        else
        {
            foreach (var activateObject in _activateObjects)
                activateObject.SetActive(false);

            foreach (var deactivateObject in _deactivateObjects)
                deactivateObject.SetActive(true);
        }

        IsHighlighted = false;
    }

    private void SetImageColor(Color color)
    {
        foreach (var image in _images)
            image.color = color;
    }
}
