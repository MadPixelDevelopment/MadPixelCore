using System;
using JetBrains.Annotations;
using Unity.Services.LevelPlay;

namespace com.unity3d.mediation
{
    [Obsolete(
        "The namespace com.unity3d.mediation is deprecated. Use LevelPlayInterstitialAd under the new namespace Unity.Services.LevelPlay.")]
    public class LevelPlayInterstitialAd : Unity.Services.LevelPlay.LevelPlayInterstitialAd
    {
        public LevelPlayInterstitialAd(string adUnitId) : base(adUnitId) {}

        internal LevelPlayInterstitialAd(IPlatformInterstitialAd platformInterstitialAd) : base(platformInterstitialAd)
        {
        }
    }
}

namespace Unity.Services.LevelPlay
{
    public class LevelPlayInterstitialAd : ILevelPlayInterstitialAd
    {
#pragma warning disable 0618
        /// <summary>
        /// Invoked when the interstitial ad is loaded.
        /// </summary>
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdLoaded;

        /// <summary>
        /// Invoked when the interstitial ad fails to load.
        /// </summary>
        public event Action<com.unity3d.mediation.LevelPlayAdError> OnAdLoadFailed;

        /// <summary>
        /// Invoked when the interstitial ad is displayed.
        /// </summary>
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdDisplayed;

        /// <summary>
        /// Invoked when the interstitial ad is closed.
        /// </summary>
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClosed;

        /// <summary>
        /// Invoked when the user clicks on the interstitial ad.
        /// </summary>
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClicked;

        /// <summary>
        /// Invoked when the interstitial ad fails to display.
        /// </summary>
        public event Action<com.unity3d.mediation.LevelPlayAdDisplayInfoError> OnAdDisplayFailed;

        /// <summary>
        /// Invoked when the interstitial ad info is changed.
        /// </summary>
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdInfoChanged;
#pragma warning restore 0618

        readonly IPlatformInterstitialAd m_InterstitialAd;

        /// <summary>
        /// Gets the ad unit id of the ad.
        /// </summary>
        public string AdUnitId => m_InterstitialAd.AdUnitId;

        /// <summary>
        /// Creates and Initializes a new instance of the LevelPlay Interstitial Ad.
        /// </summary>
        /// <param name="adUnitId">The unique ID for the ad unit.</param>
        public LevelPlayInterstitialAd(string adUnitId)
        {
#if !UNITY_IOS && !UNITY_ANDROID
            m_InterstitialAd = new UnsupportedInterstitialAd(adUnitId);
#elif UNITY_EDITOR
            m_InterstitialAd = new EditorInterstitialAd(adUnitId);
#elif UNITY_ANDROID
            m_InterstitialAd = new AndroidInterstitialAd(adUnitId);
#elif UNITY_IOS
            m_InterstitialAd = new IosInterstitialAd(adUnitId);
#endif

            SetupEvents();
        }

        /// <summary>
        /// Creates and Initializes a new instance of the LevelPlay Interstitial Ad with a config.
        /// </summary>
        /// <param name="adUnitId">The unique ID for the ad unit.</param>
        /// <param name="config">The ad unit configuration.</param>
        public LevelPlayInterstitialAd(string adUnitId, Config config)
        {
#if UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            m_InterstitialAd = new EditorInterstitialAd(adUnitId);
#elif UNITY_ANDROID
            m_InterstitialAd = new AndroidInterstitialAd(adUnitId, (AndroidInterstitialAd.Config)config.PlatformConfig);
#elif UNITY_IOS
            m_InterstitialAd = new IosInterstitialAd(adUnitId, (IosInterstitialAd.Config)config.PlatformConfig);
#else
            m_InterstitialAd = new UnsupportedInterstitialAd(adUnitId);
#endif

            SetupEvents();
        }

        private void SetupEvents()
        {
            m_InterstitialAd.OnAdLoaded += (info) => OnAdLoaded?.Invoke(info);
            m_InterstitialAd.OnAdLoadFailed += (error) => OnAdLoadFailed?.Invoke(error);
            m_InterstitialAd.OnAdDisplayed += (info) => OnAdDisplayed?.Invoke(info);
            m_InterstitialAd.OnAdClosed += (info) => OnAdClosed?.Invoke(info);
            m_InterstitialAd.OnAdClicked += (info) => OnAdClicked?.Invoke(info);
            m_InterstitialAd.OnAdDisplayFailed += (infoError) => OnAdDisplayFailed?.Invoke(infoError);
            m_InterstitialAd.OnAdInfoChanged += (info) => OnAdInfoChanged?.Invoke(info);
        }

        internal LevelPlayInterstitialAd(IPlatformInterstitialAd platformInterstitialAd)
        {
            m_InterstitialAd = platformInterstitialAd;
        }

        /// <summary>
        /// Loads the Interstitial Ad.
        /// </summary>
        public void LoadAd()
        {
            m_InterstitialAd.LoadAd();
        }

        /// <summary>
        /// Destroys the Interstitial Ad.
        /// </summary>
        public void DestroyAd()
        {
            Dispose();
        }

        /// <summary>
        /// Shows the Interstitial Ad.
        /// </summary>
        /// <param name="placementName"><i><b>(Optional)</b></i>Placement Name for the Interstitial Ad.</param>
        public void ShowAd(string placementName = null)
        {
            m_InterstitialAd.ShowAd(placementName);
        }

        /// <summary>
        /// Checks if the interstitial ad is ready
        /// </summary>
        /// <returns>Returns true if the interstitial ad is ready, returns false if not.</returns>
        public bool IsAdReady()
        {
            return m_InterstitialAd.IsAdReady();
        }

        /// <summary>
        /// Checks if a given Placement Name is capped.
        /// </summary>
        /// <param name="placementName">Placement Name for the Interstitial Ad.</param>
        /// <returns>Returns true if placement is capped, returns false if not.</returns>
        public static bool IsPlacementCapped(string placementName)
        {
#if !UNITY_IOS && !UNITY_ANDROID
            return false;
#elif UNITY_EDITOR
            return EditorInterstitialAd.IsPlacementCapped(placementName);
#elif UNITY_ANDROID
            return AndroidInterstitialAd.IsPlacementCapped(placementName);
#elif UNITY_IOS
            return IosInterstitialAd.IsPlacementCapped(placementName);
#else
            return false;
#endif
        }

        /// <summary>
        /// Dispose the interstitial ad
        /// </summary>
        public void Dispose()
        {
            m_InterstitialAd.Dispose();
        }

        /// <summary>
        /// Gets the ad ID associated with this ad.
        /// </summary>
        /// <returns>The ID of the ad.</returns>
        public string GetAdId()
        {
            return m_InterstitialAd.AdId;
        }

        /// <summary>
        /// Interstitial ad configuration, use a <see cref="LevelPlayInterstitialAd.Config.Builder"/> to initialize
        ///<br/>
        /// </summary>
        public class Config
        {
            [CanBeNull] internal IPlatformInterstitialAd.IConfig PlatformConfig { get; }

            private Config([CanBeNull] IPlatformInterstitialAd.IConfig platformConfig)
            {
                PlatformConfig = platformConfig;
            }

            /// <summary>
            /// A config builder
            /// </summary>
            public class Builder
            {
                private readonly IPlatformInterstitialAd.IConfigBuilder m_Builder;

                /// <summary>
                /// Initialize a new config builder
                /// </summary>
                public Builder()
                {
                    #if UNITY_ANDROID && !UNITY_EDITOR
                    m_Builder = new AndroidInterstitialAd.Config.Builder();
                    #elif UNITY_IOS && !UNITY_EDITOR
                    m_Builder = new IosInterstitialAd.Config.Builder();
                    #else
                    m_Builder = new UnsupportedInterstitialAd.Config.Builder();
                    #endif
                }

                /// <summary>
                /// Set a bid floor
                /// <param name="bidFloor">bid floor in USD</param>
                /// </summary>
                public Builder SetBidFloor(double bidFloor)
                {
                    m_Builder.SetBidFloor(bidFloor);
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
