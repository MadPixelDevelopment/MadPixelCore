using System;

namespace com.unity3d.mediation
{
    /// <summary>
    /// Implements ILevelPlayBannerAd to provide functionality for managing banner ads.
    /// </summary>
    [Obsolete("The namespace com.unity3d.mediation is deprecated. Use LevelPlayBannerAd under the new namespace Unity.Services.LevelPlay.")]
    public sealed class LevelPlayBannerAd : Unity.Services.LevelPlay.LevelPlayBannerAd
    {
        public LevelPlayBannerAd(string adUnitId, LevelPlayAdSize size = null, LevelPlayBannerPosition position = null, string placementName = null, bool displayOnLoad = true, bool respectSafeArea = false) : base(adUnitId, size, position, placementName, displayOnLoad, respectSafeArea) {}
    }
}


namespace Unity.Services.LevelPlay
{
    /// <summary>
    /// Implements ILevelPlayBannerAd to provide functionality for managing banner ads.
    /// </summary>
    public class LevelPlayBannerAd : ILevelPlayBannerAd
    {
#pragma warning disable 0618
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdLoaded;
        public event Action<com.unity3d.mediation.LevelPlayAdError> OnAdLoadFailed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClicked;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdDisplayed;
        public event Action<com.unity3d.mediation.LevelPlayAdDisplayInfoError> OnAdDisplayFailed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdExpanded;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdCollapsed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdLeftApplication;
#pragma warning disable 0618

        bool _autoRefresh;
        readonly IPlatformBannerAd _bannerAd;

        /// <summary>
        /// Initializes a new instance of the LevelPlayBannerAd with a config.
        /// </summary>
        /// <param name="adUnitId">The unique ID for the ad unit.</param>
        /// <param name="config">The ad unit configuration.</param>
        public LevelPlayBannerAd(string adUnitId, Config config)
        {
#if !UNITY_ANDROID && !UNITY_IOS
            _bannerAd = new UnsupportedBannerAd(adUnitId, (UnsupportedBannerAd.Config)config.PlatformConfig);
#elif UNITY_EDITOR
            _bannerAd = new EditorBannerAd(adUnitId, (EditorBannerAd.Config)config.PlatformConfig);
#elif UNITY_ANDROID
            _bannerAd = new AndroidBannerAd(adUnitId, (AndroidBannerAd.Config)config.PlatformConfig);
#elif UNITY_IOS
            _bannerAd = new iOSBannerAd(adUnitId, (iOSBannerAd.Config)config.PlatformConfig);
#endif

            SetupCallbacks();
        }

        /// <summary>
        /// Initializes a new instance of the LevelPlayBannerAd with specified ad properties.
        /// </summary>
        /// <param name="adUnitId">The unique ID for the ad unit.</param>
        /// <param name="size">Size of the banner ad.
        /// Defaults to <see cref="LevelPlayAdSize.BANNER"/> if null.</param>
        /// <param name="position">Position on the screen where the ad will be displayed.
        /// Defaults to <see cref="LevelPlayBannerPosition.BottomCenter"/> if not specified.</param>
        /// <param name="placementName">Optional name used for reporting and targeting. This parameter is optional and can be null.</param>
        /// <param name="displayOnLoad">Determines whether the ad should be displayed immediately after loading.</param>
        /// <param name="respectSafeArea">Determines whether the ad should be displayed within the safe area of the screen, where no notch, status bar or camera is present.
        /// Defaults to false.</param>
        public LevelPlayBannerAd(
            string adUnitId,
            com.unity3d.mediation.LevelPlayAdSize size = null,
            com.unity3d.mediation.LevelPlayBannerPosition position = null,
            string placementName = null,
            bool displayOnLoad = true,
            bool respectSafeArea = false)
        {
            if (size == null)
            {
                size = com.unity3d.mediation.LevelPlayAdSize.BANNER;
            }

            position = position ?? com.unity3d.mediation.LevelPlayBannerPosition.BottomCenter;

#if !UNITY_IOS && !UNITY_ANDROID
            _bannerAd = new UnsupportedBannerAd(adUnitId, size, position, placementName);
#elif UNITY_EDITOR
            _bannerAd = new EditorBannerAd(adUnitId, size, position, placementName, displayOnLoad, respectSafeArea);
#elif UNITY_ANDROID
            _bannerAd = new AndroidBannerAd(adUnitId, size, position, placementName, displayOnLoad, respectSafeArea);
#elif UNITY_IOS
            _bannerAd = new iOSBannerAd(adUnitId, size, position, placementName, displayOnLoad);
#endif

            SetupCallbacks();
        }

        /// <summary>
        /// Loads the banner ad.
        /// </summary>
        public void LoadAd()
        {
            _bannerAd.Load();
        }

        /// <summary>
        /// Destroys the banner ad and releases resources.
        /// </summary>
        public void DestroyAd()
        {
            _bannerAd.DestroyAd();
        }

        /// <summary>
        /// Displays the banner ad to the user.
        /// </summary>
        public void ShowAd()
        {
            _bannerAd.ShowAd();
        }

        /// <summary>
        /// Hides the banner ad from the user.
        /// </summary>
        public void HideAd()
        {
            _bannerAd.HideAd();
        }

        /// <summary>
        /// Gets the ad ID associated with this ad.
        /// </summary>
        /// <returns>The ID of the ad.</returns>
        public string GetAdId()
        {
            return _bannerAd.AdId;
        }

        /// <summary>
        /// Gets the ad unit ID associated with this ad.
        /// </summary>
        /// <returns>The ID of the ad unit.</returns>
        public string GetAdUnitId()
        {
            return _bannerAd.AdUnitId;
        }

        /// <summary>
        /// Retrieves the size of the ad.
        /// </summary>
        /// <returns>The size of the ad.</returns>
        public com.unity3d.mediation.LevelPlayAdSize GetAdSize()
        {
            return _bannerAd.AdSize;
        }

        /// <summary>
        /// Retrieves the position of the banner ad.
        /// </summary>
        /// <returns>The position of the ad.</returns>
        public com.unity3d.mediation.LevelPlayBannerPosition GetPosition()
        {
            return _bannerAd.Position;
        }

        /// <summary>
        /// Retrieves the placement name associated with this ad.
        /// </summary>
        /// <returns>The placement name of the ad.</returns>
        public string GetPlacementName()
        {
            return _bannerAd.PlacementName;
        }

        /// <summary>
        /// Pauses the auto-refreshing of the banner ad.
        /// </summary>
        public void PauseAutoRefresh()
        {
            _bannerAd.PauseAutoRefresh();
        }

        /// <summary>
        /// Resumes the auto-refreshing of the banner ad that was previously paused.
        /// </summary>
        public void ResumeAutoRefresh()
        {
            _bannerAd.ResumeAutoRefresh();
        }

        void SetupCallbacks()
        {
            _bannerAd.OnAdLoaded += (sender, args) => OnAdLoaded?.Invoke(args);
            _bannerAd.OnAdLoadFailed += (sender, args) => OnAdLoadFailed?.Invoke(args);
            _bannerAd.OnAdClicked += (sender, args) => OnAdClicked?.Invoke(args);
            _bannerAd.OnAdDisplayed += (sender, args) => OnAdDisplayed?.Invoke(args);
            _bannerAd.OnAdDisplayFailed += (sender, args) => OnAdDisplayFailed?.Invoke(args);
            _bannerAd.OnAdExpanded += (sender, args) => OnAdExpanded?.Invoke(args);
            _bannerAd.OnAdCollapsed += (sender, args) => OnAdCollapsed?.Invoke(args);
            _bannerAd.OnAdLeftApplication += (sender, args) => OnAdLeftApplication?.Invoke(args);
        }

        public void Dispose()
        {
            _bannerAd?.DestroyAd();
        }

        public class Config
        {
            internal IPlatformBannerAd.IConfig PlatformConfig { get; }

            private Config(IPlatformBannerAd.IConfig platformConfig)
            {
                PlatformConfig = platformConfig;
            }

            public class Builder
            {
                private readonly IPlatformBannerAd.IConfigBuilder m_Builder;

                public Builder()
                {
#if !UNITY_ANDROID && !UNITY_IOS
                    m_Builder = new UnsupportedBannerAd.Config.Builder();
#elif UNITY_EDITOR
                    m_Builder = new EditorBannerAd.Config.Builder();
#elif UNITY_ANDROID
                    m_Builder = new AndroidBannerAd.Config.Builder();
#elif UNITY_IOS
                    m_Builder = new iOSBannerAd.Config.Builder();
#endif
                }

                /// <summary>
                /// Set a Bid floor
                /// </summary>
                /// <param name="bidFloor">Bid floor in USD</param>
                /// <returns>this builder</returns>
                public Builder SetBidFloor(double bidFloor)
                {
                    m_Builder.SetBidFloor(bidFloor);
                    return this;
                }

                /// <summary>
                /// Set a size
                /// </summary>
                /// <param name="size">Size of the banner ad. Defaults to <see cref="LevelPlayAdSize.BANNER"/>.</param>
                /// <returns>this builder</returns>
                public Builder SetSize(com.unity3d.mediation.LevelPlayAdSize size)
                {
                    m_Builder.SetSize(size);
                    return this;
                }

                /// <summary>
                /// Set a position
                /// </summary>
                /// <param name="position">Position on the screen where the ad will be displayed.
                /// Defaults to <see cref="LevelPlayBannerPosition.BottomCenter"/>.</param>
                /// <returns>this builder</returns>
                public Builder SetPosition(com.unity3d.mediation.LevelPlayBannerPosition position)
                {
                    m_Builder.SetPosition(position);
                    return this;
                }

                /// <summary>
                /// Set a placement name, ignores `null`
                /// </summary>
                /// <param name="placementName">Name used for reporting and targeting</param>
                /// <returns>this builder</returns>
                public Builder SetPlacementName(string placementName)
                {
                    if (placementName != null)
                    {
                        m_Builder.SetPlacementName(placementName);
                    }
                    return this;
                }

                /// <summary>
                /// Set the "displayOnLoad" flag
                /// </summary>
                /// <param name="displayOnLoad">Determines whether the ad should be displayed immediately after loading.
                /// Defaults to true.</param>
                /// <returns>this builder</returns>
                public Builder SetDisplayOnLoad(bool displayOnLoad)
                {
                    m_Builder.SetDisplayOnLoad(displayOnLoad);
                    return this;
                }

                /// <summary>
                /// Set the "respectSafeArea" flag
                /// </summary>
                /// <param name="respectSafeArea">Determine whether the ad should be displayed within the safe area of the screen,
                /// where no notch, status bar or camera is present. Defaults to false</param>
                /// <returns>this builder</returns>
                public Builder SetRespectSafeArea(bool respectSafeArea)
                {
                    m_Builder.SetRespectSafeArea(respectSafeArea);
                    return this;
                }

                /// <summary>
                /// Build a new config object
                /// </summary>
                public Config Build()
                {
                    var platformConfig = m_Builder.Build();
                    return new Config(platformConfig);
                }
            }
        }
    }
}
