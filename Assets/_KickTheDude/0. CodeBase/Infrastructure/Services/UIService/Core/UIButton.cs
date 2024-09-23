using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class UIButton : SerializedMonoBehaviour, IActivatiable
{
    public event Action<UIButton> ButtonClicked;

    [SerializeField, BoxGroup("SETUP")] protected Button _button;
    [SerializeField, BoxGroup("PARAMETERS")] protected bool _scaleWhenPressed;
    [SerializeField, BoxGroup("PARAMETERS"), ShowIf("_scaleWhenPressed")] protected float _pressedScale = 0.1f;
    [SerializeField, BoxGroup("PARAMETERS"), ShowIf("_scaleWhenPressed")] protected float _pressedRotation = 0f;

    [SerializeField, BoxGroup("UNITY EVENTS")] private UnityEvent ButtonClickAction;

    public bool IsActive { get; private set; }

    private void OnValidate()
    {
        if (_button == null) _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(Press);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveAllListeners();
    }

    public void Activate()
    {
        IsActive = true;

        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        IsActive = false;

        gameObject.SetActive(false);
    }

    [Button("PRESS", ButtonSizes.Large), BoxGroup("ACTIONS")]
    public virtual void Press()
    {
        if (_scaleWhenPressed)
        {
            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * _pressedScale, 0.2f, 0, 0);
            transform.DOPunchRotation(new Vector3(0, 0, _pressedRotation), 0.2f, 0, 0);
        }

        ButtonClickAction?.Invoke();

        ButtonClicked?.Invoke(this);
    }
}
