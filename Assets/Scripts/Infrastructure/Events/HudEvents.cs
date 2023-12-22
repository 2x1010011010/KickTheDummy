using System;

namespace Infrastructure.Events
{
  public class HudEvents
  {
    public void PressRestartLevelEvent() => OnPressRestartLevel?.Invoke();
    public Action OnPressRestartLevel;
    public void PressFightButtonEvent() => OnPressPlayButton?.Invoke();
    public Action OnPressPlayButton;
        
    public void PressEndUpgradesButtonEvent() => OnPressEndUpgrades?.Invoke();
    public Action OnPressEndUpgrades;
    
    public void ShowChoiceScreenEvent() => 
      OnShowChoiceScreen?.Invoke();
    public Action OnShowChoiceScreen;
  }
}