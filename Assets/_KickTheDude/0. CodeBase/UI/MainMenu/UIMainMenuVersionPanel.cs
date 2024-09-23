using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIMainMenuVersionPanel : UIPanel
{
    [SerializeField] private TextMeshProUGUI _versionLabel;

    private IApplicationService _applicationService;

    [Inject]
    private void Construct(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    public override void Show()
    {
        _versionLabel.text = "VERSION " + _applicationService.ApplicationVersion.ToUpper();

        base.Show();
    }
}
