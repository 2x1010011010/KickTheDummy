using System;

public interface IAdvertisingService
{
    event Action<int> InterstitialTimerCounterChanged;
    event Action<int> EnableAdsTimerCounterChanged;

    bool IsInitialized { get; }
    bool AdsDisabled { get; }

    int AdsDisabledTimer { get; }
    int InterstitialTimer { get; }
    bool ReadyToShowInter { get; }

    void ShowRewardAds(Action afterRewardAction = null, Action afterFailedAction = null);
    void ShowInterstitialAds(Action afterInterstitialAction = null);
    void ShowBannerAds();
    void HideBannerAds();
    void DisableAds(bool forever);

    void DropInterCooldown();
    void DropDisableAdsCooldown();
}
