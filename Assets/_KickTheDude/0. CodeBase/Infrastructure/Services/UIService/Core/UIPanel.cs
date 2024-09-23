using UnityEngine;
using UnityEditor;
using System;
using Sirenix.OdinInspector;

public class UIPanel : SerializedMonoBehaviour
{
    public virtual event Action<UIPanel> Showed;
    public virtual event Action<UIPanel> Hided;

    [SerializeField, ReadOnly, BoxGroup("DEBUG")] public bool IsShowed { get; protected set; }

    [Button("SHOW", ButtonSizes.Large), BoxGroup("ACTIONS")]
    public virtual void Show()
    {
        gameObject.SetActive(true);

        IsShowed = true;

        Showed?.Invoke(this);
    }

    [Button("HIDE", ButtonSizes.Large), BoxGroup("ACTIONS")]
    public virtual void Hide()
    {
        gameObject.SetActive(false);

        IsShowed = false;

        Hided?.Invoke(this);
    }
}