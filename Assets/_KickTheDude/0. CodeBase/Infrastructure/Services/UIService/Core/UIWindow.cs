using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

public abstract class UIWindow : SerializedMonoBehaviour
{
    public virtual event Action<UIWindow> WindowOpen;
    public virtual event Action<UIWindow> WindowClosed;

    [SerializeField, ReadOnly, BoxGroup("DEBUG")] public bool IsOpen { get; private set; }

    [Button("OPEN", ButtonSizes.Large), BoxGroup("ACTIONS")]
    public async virtual UniTask Open(params object[] parameters)
    {
        if (IsOpen) return; else IsOpen = true;

        gameObject.SetActive(true);

        IsOpen = true;

        WindowOpen?.Invoke(this);
    }

    [Button("CLOSE", ButtonSizes.Large), BoxGroup("ACTIONS")]
    public virtual void Close()
    {
        IsOpen = false;

        gameObject.SetActive(false);
        WindowClosed?.Invoke(this);
    }
}
