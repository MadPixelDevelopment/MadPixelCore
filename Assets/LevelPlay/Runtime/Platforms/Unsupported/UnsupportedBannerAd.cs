using System;
using Unity.Services.LevelPlay;

namespace com.unity3d.mediation
{
    [Obsolete("This class will be made private in version 9.0.0.")]
    public class UnsupportedBannerAd : Unity.Services.LevelPlay.UnsupportedBannerAd
    {
        public UnsupportedBannerAd(string adUnitId, com.unity3d.mediation.LevelPlayAdSize size, com.unity3d.mediation.LevelPlayBannerPosition position, string placementId) : base(adUnitId, size, position, placementId) {}
    }
}

namespace Unity.Services.LevelPlay
{
#pragma warning disable 67, 0618
    public class UnsupportedBannerAd : IPlatformBannerAd
    {
        public UnsupportedBannerAd(string adUnitId, com.unity3d.mediation.LevelPlayAdSize size, com.unity3d.mediation.LevelPlayBannerPosition position, string placementId)
        {
        }

        internal UnsupportedBannerAd(string adUnitId, Config config)
        {
        }

        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdLoaded;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdError> OnAdLoadFailed;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdClicked;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdDisplayed;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdDisplayInfoError> OnAdDisplayFailed;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdExpanded;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdCollapsed;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdLeftApplication;

        public com.unity3d.mediation.LevelPlayBannerPosition Position { get; }

        public void Load()
        {
        }

        public void DestroyAd()
        {
        }

        public void ShowAd()
        {
        }

        public void HideAd()
        {
        }

        public void PauseAutoRefresh()
        {
        }

        public void ResumeAutoRefresh()
        {
        }

        public void SetAutoRefresh(bool flag)
        {
        }

        public void Dispose()
        {
        }

        public string AdId { get; }
        public string AdUnitId { get; }
        public com.unity3d.mediation.LevelPlayAdSize AdSize { get; }
        public LevelPlayAdSize Size { get; }
        public string PlacementName { get; }

        internal class Config : IPlatformBannerAd.IConfig
        {
            internal class Builder : IPlatformBannerAd.IConfigBuilder
            {
                public void SetBidFloor(double bidFloor) {}

                public void SetSize(com.unity3d.mediation.LevelPlayAdSize size) {}

                public void SetPosition(com.unity3d.mediation.LevelPlayBannerPosition position) {}

                public void SetPlacementName(string placementName) {}

                public void SetDisplayOnLoad(bool displayOnLoad) {}

                public void SetRespectSafeArea(bool respectSafeArea) {}

                public IPlatformBannerAd.IConfig Build()
                {
                    return new Config();
                }
            }
        }
    }
}
