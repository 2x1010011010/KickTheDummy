using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class UISandboxHUDWindow : UIWindow
{
    [SerializeField, BoxGroup("BUTTONS")] private UIButton _restartButton;
    [SerializeField, BoxGroup("BUTTONS")] private UIButton _exitButton;
    [SerializeField, BoxGroup("BUTTONS")] private UIButton _settingsButton;

    [SerializeField, BoxGroup("PANELS")] private UIPanel _toolsCategoriesPanel;
    [SerializeField, BoxGroup("PANELS")] private UIPanel _mainButtonsPanel;
    [SerializeField, BoxGroup("PANELS")] private UIPanel _actionButtonsPanel;

    private IApplicationService _applicationService;
    private IUIService _uiService;
    private ILocationsService _locationsService;

    [Inject]
    private void Construct(IApplicationService applicationService, IUIService uiService, ILocationsService locationsService)
    {
        _applicationService = applicationService;
        _uiService = uiService;
        _locationsService = locationsService;
    }

    public override UniTask Open(params object[] parameters)
    {
        _toolsCategoriesPanel.Show();

        _actionButtonsPanel.Show();

        _restartButton.ButtonClicked += RestartButtonClicked;
        _exitButton.ButtonClicked += ExitButtonClicked;

        return base.Open(parameters);
    }

    public override void Close()
    {
        _toolsCategoriesPanel.Hide();

        _actionButtonsPanel.Hide();

        _restartButton.ButtonClicked -= RestartButtonClicked;
        _exitButton.ButtonClicked -= ExitButtonClicked;

        base.Close();
    }

    private void RestartButtonClicked(UIButton uiButton)
    {
        _locationsService.ReloadLocation();
    }

    private async void ExitButtonClicked(UIButton uiButton)
    {
        await _uiService.OpenWindow<UILoadingWindow>();

        _exitButton.ButtonClicked -= ExitButtonClicked;

        _locationsService.LoadLocationByID(LocationID.MainMenu);
    }
}
