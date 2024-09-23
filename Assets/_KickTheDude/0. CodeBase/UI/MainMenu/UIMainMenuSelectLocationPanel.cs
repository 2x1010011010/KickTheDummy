using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class UIMainMenuSelectLocationPanel : UIPanel
{
    [SerializeField, BoxGroup("SETUP")] private UIButton _closeButton;

    [SerializeField, BoxGroup("LOCATIONS")] private UIButton _boxLocationButton;
    [SerializeField, BoxGroup("LOCATIONS")] private UIButton _box2LocationButton;

    private ILocationsService _locationsService;

    [Inject]
    private void Construct(ILocationsService locationsService)
    {
        _locationsService = locationsService;
    }

    public override void Show()
    {
        _closeButton.ButtonClicked += CloseButtonClicked;

        _boxLocationButton.ButtonClicked += BoxLocationButtonClicked;
        _box2LocationButton.ButtonClicked += Box2LocationButtonClicked;

        base.Show();
    }

    public override void Hide()
    {
        _closeButton.ButtonClicked -= CloseButtonClicked;

        _boxLocationButton.ButtonClicked -= BoxLocationButtonClicked;
        _box2LocationButton.ButtonClicked -= Box2LocationButtonClicked;

        base.Hide();
    }

    private void BoxLocationButtonClicked(UIButton uiButton)
    {
        _locationsService.LoadLocationByID(LocationID.Box);
    }

    private void Box2LocationButtonClicked(UIButton uiButton)
    {
        _locationsService.LoadLocationByID(LocationID.Stairs);
    }

    private void CloseButtonClicked(UIButton uiButton)
    {
        Hide();
    }
}
