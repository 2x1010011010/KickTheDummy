using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public enum ToolButtonState
{
    Unlocked,
    Locked
}

public class UISandboxToolButton : UIButton
{
    [SerializeField, BoxGroup("SETUP")] private Image _toolImage;
    [SerializeField, BoxGroup("SETUP")] private Outline _toolImageOutline;
    [SerializeField, BoxGroup("SETUP")] private Shadow _toolImageShadow;

    [SerializeField, BoxGroup("DEBUG"), ReadOnly] public ToolEntity Tool { get; private set; }

    private IUIService _uiService;

    public void Construct(ToolEntity tool, IUIService uiService)
    {
        Tool = tool;
        _uiService = uiService;

        _toolImage.sprite = Tool.ToolIcon;
        _toolImage.rectTransform.localEulerAngles = new Vector3(0, 0, tool.RotateAngle);

        _toolImage.rectTransform.offsetMin = new Vector2(tool.BorderOffset, tool.BorderOffset);
        _toolImage.rectTransform.offsetMax = new Vector2(-tool.BorderOffset, -tool.BorderOffset);

        //_toolImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, tool.BorderOffset);
    }

    public void SetState(ToolButtonState toolButtonState)
    {
        switch (toolButtonState)
        {
            case ToolButtonState.Locked:
                Lock();
                break;
            case ToolButtonState.Unlocked:
                Unlock();
                break;
        }
    }

    public override void Press()
    {
        if(Tool.SelectClip != null)
            _uiService.PlayUISound(Tool.SelectClip);

        base.Press();
    }

    [Button("UNLOCK", ButtonSizes.Large), BoxGroup("ACTIONS", true, false, 2)]
    private void Unlock()
    {
        _toolImage.color = Color.white;
        _toolImageOutline.effectColor = new Color(0, 0, 0, 0);
        _toolImageShadow.effectColor = Color.black;
    }

    [Button("LOCK", ButtonSizes.Large), BoxGroup("ACTIONS", true, false, 2)]
    private void Lock()
    {
        _toolImage.color = Color.black;
        _toolImageOutline.effectColor = Color.white;
        _toolImageShadow.effectColor = new Color(0, 0, 0, 0);
    }
}
