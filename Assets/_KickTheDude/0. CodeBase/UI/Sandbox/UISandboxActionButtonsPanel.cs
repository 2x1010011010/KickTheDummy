using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UISandboxActionButtonsPanel : UIPanel
{
    [SerializeField] private UIHighlightedButton _timeButton;
    [SerializeField] private UIHighlightedButton _cameraButton;

    private ITimeService _timeService;
    private IPlayer _player;

    [Inject]
    private void Construct(ITimeService timeService, IPlayer player)
    {
        _timeService = timeService;
        _player = player;
    }

    public override void Show()
    {
        _player.CameraModeActiveChanged += CameraModeActiveChanged;
        _cameraButton.ButtonClicked += CameraButtonClicked;
        _timeButton.ButtonClicked += TimeButtonClicked;

        base.Show();
    }

    private void CameraModeActiveChanged(bool active)
    {
        if (active)
            _cameraButton.Highlight();
        else
            _cameraButton.Unhighlight();
    }

    public override void Hide()
    {
        _cameraButton.ButtonClicked -= CameraButtonClicked;
        _timeButton.ButtonClicked -= TimeButtonClicked;

        base.Hide();
    }

    private void CameraButtonClicked(UIButton uiButton)
    {
        if (_player.CameraModeActive)
            _player.SetCameraMode(false);
        else
            _player.SetCameraMode(true);
    }

    private void TimeButtonClicked(UIButton uiButton)
    {
        if(_timeService.CurentTimeValue < 1f)
        {
            _timeButton.Unhighlight();
            _timeService.SetTimeValue(1f);
        }
        else
        {
            _timeButton.Highlight();
            _timeService.SetTimeValue(0f);
        }
    }
}
