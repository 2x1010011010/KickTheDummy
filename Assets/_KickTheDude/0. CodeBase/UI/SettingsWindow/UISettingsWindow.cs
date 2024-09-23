using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KickTheDude._0._CodeBase.UI.SettingsWindow
{
  public class UISettingsWindow : UIWindow
  {
    [SerializeField, BoxGroup("BUTTONS")] private UIButton _exitButton; 
    [SerializeField, BoxGroup("PANELS")] private UIPanel _screenSettingsPanel;

    public override UniTask Open(params object[] parameters)
    {
      _exitButton.ButtonClicked += ExitButtonClicked;
      _screenSettingsPanel.Show();
      return base.Open(parameters);
    }

    public override void Close()
    {
      _exitButton.ButtonClicked -= ExitButtonClicked;
      _screenSettingsPanel.Hide();
      
      base.Close();
    }

    private void ExitButtonClicked(UIButton obj) => Close();
  }
}