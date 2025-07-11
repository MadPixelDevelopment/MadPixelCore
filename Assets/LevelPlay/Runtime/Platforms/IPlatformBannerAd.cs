using System;

namespace Unity.Services.LevelPlay
{
    /// <summary>
    /// Interface of a Banner Ad.
    /// </summary>
    interface IPlatformBannerAd : IDisposable
    {
#pragma warning disable 0618
        event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdLoaded;
        event EventHandler<com.unity3d.mediation.LevelPlayAdError> OnAdLoadFailed;
        event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdClicked;
        event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdDisplayed;
        event EventHandler<com.unity3d.mediation.LevelPlayAdDisplayInfoError> OnAdDisplayFailed;
        event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdExpanded;
        event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdCollapsed;
        event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdLeftApplication;
#pragma warning restore 0618

        string AdId { get; }
        string AdUnitId { get; }

#pragma warning disable 0618
        com.unity3d.mediation.LevelPlayAdSize AdSize { get; }
#pragma warning restore 0618

        string PlacementName { get; }

#pragma warning disable 0618
        com.unity3d.mediation.LevelPlayBannerPosition Position { get; }
#pragma warning restore 0618

        void Load();
        void DestroyAd();
        void ShowAd();
        void HideAd();
        void PauseAutoRefresh();
        void ResumeAutoRefresh();

        internal interface IConfig
        {
        }

        internal interface IConfigBuilder
        {
            void SetBidFloor(double bidFloor);

#pragma warning disable 0618
            void SetSize(com.unity3d.mediation.LevelPlayAdSize size);

            void SetPosition(com.unity3d.mediation.LevelPlayBannerPosition position);
#pragma warning restore 0618

            void SetPlacementName(string placementName);

            void SetDisplayOnLoad(bool displayOnLoad);

            void SetRespectSafeArea(bool respectSafeArea);

            IConfig Build();
        }
    }
}
