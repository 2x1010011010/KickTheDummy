using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class UIMainMenuWindow : UIWindow
{
    [SerializeField, BoxGroup("BUTTONS")] private UIButton _playButton;
    [SerializeField, BoxGroup("BUTTONS")] private UIButton _settingsButton;
    [SerializeField, BoxGroup("BUTTONS")] private UIButton _exitButtonButton;

    [SerializeField, BoxGroup("PANELS")] private UIPanel _headerPanel;
    [SerializeField, BoxGroup("PANELS")] private UIPanel _versionPanel;
    [SerializeField, BoxGroup("PANELS")] private UIPanel _locationSelectionPanel;
    [SerializeField, BoxGroup("PANELS")] private UIPanel _buttonsPanel;
    

    private IApplicationService _applicationService;
    private IUIService _uiService;
    private ILocationsService _locationsService;

    [Inject]
    private void Construct(IApplicationService applicationService, IUIService uiService, ILocationsService locationsService)
    {
        _uiService = uiService;
        _applicationService = applicationService;
        _locationsService = locationsService;
    }

    public override UniTask Open(params object[] parameters)
    {
        _playButton.ButtonClicked += PlayButtonClicked;
        _exitButtonButton.ButtonClicked += ExitButtonButtonClicked;
        _settingsButton.ButtonClicked += SettingsButtonClicked;

        _headerPanel.Show();
        _versionPanel.Show();
        _buttonsPanel.Show();
        _locationSelectionPanel.Hide();

        return base.Open(parameters);
    }

    private void SettingsButtonClicked(UIButton button)
    {
        
    }

    public override void Close()
    {
        _playButton.ButtonClicked -= PlayButtonClicked;
        _exitButtonButton.ButtonClicked -= ExitButtonButtonClicked;
        _settingsButton.ButtonClicked -= SettingsButtonClicked;

        base.Close();
    }

    private void PlayButtonClicked(UIButton uiButton)
    {
        _uiService.OpenWindow<UILoadingWindow>();

        _headerPanel.Hide();
        _versionPanel.Hide();
        _buttonsPanel.Hide();
        //_locationSelectionPanel.Show();

        _locationsService.LoadLocationByID(LocationID.Box);

        //_locationSelectionPanel.Hided += LocationSelectionPanelHided;
    }

    private void LocationSelectionPanelHided(UIPanel uiPanel)
    {
        //_locationSelectionPanel.Hided -= LocationSelectionPanelHided;

        _headerPanel.Show();
        _versionPanel.Show();
        _buttonsPanel.Show();
        _locationSelectionPanel.Hide();
    }

    private void ExitButtonButtonClicked(UIButton obj)
    {
        _applicationService.QuitGame();
    }
}
