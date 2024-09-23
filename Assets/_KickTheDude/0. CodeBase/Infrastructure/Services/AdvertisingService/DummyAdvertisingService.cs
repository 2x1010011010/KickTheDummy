using System;

public class DummyAdvertisingService : IAdvertisingService
{
    public event Action<int> InterstitialTimerCounterChanged;
    public event Action<int> EnableAdsTimerCounterChanged;

    public bool IsInitialized { get { return true; } }
    public bool AdsDisabled { get { return false; } }
    public int AdsDisabledTimer { get { return 1; } }
    public int InterstitialTimer { get { return 1; } }

    public bool ReadyToShowInter { get { return AdsDisabledTimer == 0; } }

    public void DisableAds(bool forever)
    {
        
    }

    public void HideBannerAds()
    {
        
    }

    public void ShowBannerAds()
    {
        
    }

    public void ShowInterstitialAds(Action afterInterstitialAction = null)
    {
        //afterInterstitialAction?.Invoke();
    }

    public void ShowRewardAds(Action afterRewardAction = null, Action afterFailedAction = null)
    {
        afterRewardAction?.Invoke();
    }

    public void DropInterCooldown()
    {
        
    }

    public void DropDisableAdsCooldown()
    {
        
    }
}
