using System;

namespace Unity.Services.LevelPlay
{
    class UnsupportedRewardedAd : IPlatformRewardedAd
    {
        public void Dispose()
        {
        }

#pragma warning disable 67, 0618
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdLoaded;
        public event Action<com.unity3d.mediation.LevelPlayAdError> OnAdLoadFailed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdDisplayed;
        public event Action<com.unity3d.mediation.LevelPlayAdDisplayInfoError> OnAdDisplayFailed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo, com.unity3d.mediation.LevelPlayReward> OnAdRewarded;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClicked;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClosed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdInfoChanged;
#pragma warning restore 67, 0618

        public string AdId { get; }
        public string AdUnitId { get; }

        public UnsupportedRewardedAd(string adUnitId)
        {
        }

        public void LoadAd()
        {
        }

        public void ShowAd(string placementName)
        {
        }

        public bool IsAdReady()
        {
            return false;
        }

        internal class Config : IPlatformRewardedAd.IConfig
        {
            internal class Builder : IPlatformRewardedAd.IConfigBuilder
            {
                public void SetBidFloor(double bidFloor) {}

                public IPlatformRewardedAd.IConfig Build()
                {
                    return new Config();
                }
            }
        }
    }
}
