using System.Collections.Generic;
using Unity.Services.LevelPlay;
using MadPixelAnalytics;
using UnityEngine;
using UnityEngine.Events;
using static MadPixel.AdsManager;

namespace MadPixel {
    public class LevelPlayComp : MonoBehaviour {
        #region Fields
        [Header("Turn both Debugs OFF for production builds")]
        [SerializeField] private bool m_debugLogsOn;
        [SerializeField] private bool m_debugTestSuiteOn;

        private MadPixelCustomSettings m_customSettings;
        private bool m_isBannerLoadedOnce = false;
        private bool m_sdkFailedToInitSent = false;

        private LevelPlayRewardedAd m_rewardedAd;
        private LevelPlayBannerAd m_bannerAd;
        private LevelPlayInterstitialAd m_interstitialAd;

        #endregion



        #region Events Declaration
        public UnityAction<bool> e_onFinishAds;
        public UnityAction<LevelPlayAdDisplayInfoError, AdsManager.EAdType> e_onDisplayAdError;
        public UnityAction e_onInterDismissed;
        public UnityAction<AdsManager.EAdType> m_onAdLoaded;
        public UnityAction<LevelPlayAdInfo> m_onBannerAdLoaded;

        #endregion



        #region UnityEvents
        void OnApplicationPause(bool isPaused) {
            IronSource.Agent.onApplicationPause(isPaused);
        }

        void OnDestroy() {
            UnsubscribeAll();
        }
        #endregion



        #region Public
        public void Init(MadPixelCustomSettings a_madPixelSettings) {
            m_customSettings = a_madPixelSettings;
            if (m_debugLogsOn) {
                if (m_debugTestSuiteOn) {
                    IronSource.Agent.setMetaData("is_test_suite", "enable");
                }
            }
            SubscribeAll();
            IronSource.Agent.setAdaptersDebug(m_debugLogsOn);
            TryToInit();

            if (m_debugLogsOn) {
                IronSource.Agent.validateIntegration();
            }
        }

        public bool IsReady(EAdType a_adType) {
#if UNITY_EDITOR
            return true;
#endif
            if (a_adType == EAdType.REWARDED && m_rewardedAd != null) {
                return m_rewardedAd.IsAdReady();
            }
            else if (a_adType == EAdType.INTER && m_interstitialAd != null) {
                return m_interstitialAd.IsAdReady();

            }

            return false;
        }

        public void ShowRewarded() {
            if (m_rewardedAd != null && m_rewardedAd.IsAdReady()) {
                m_rewardedAd.ShowAd();
            }
        }

        public void ShowInter() {
            if (m_interstitialAd != null && m_interstitialAd.IsAdReady()) {
                m_interstitialAd.ShowAd();
            }
        }
        #endregion



        #region General Helpers
        private void SubscribeAll() {
            LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
            LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

            IronSourceEvents.onImpressionDataReadyEvent += ImpressionDataReadyEvent;
        }


        private void UnsubscribeAll() {
            LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
            LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;

            IronSourceEvents.onImpressionDataReadyEvent -= ImpressionDataReadyEvent;
        }


        private void LoadInter() {
            if (m_interstitialAd != null) {
                m_interstitialAd.LoadAd();
            }

            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I called LoadInter");
            }
        }

        private void LoadRewarded() {
            if (m_rewardedAd != null) {
                m_rewardedAd.LoadAd();
            }

            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I called LoadRewarded");
            }
        }

        #endregion

        #region Rewarded callbacks
        private void Rewarded_OnAdRewarded(LevelPlayAdInfo a_adInfo, LevelPlayReward a_reward) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got Rewarded_OnAdRewarded " + a_adInfo);
            }

            e_onFinishAds?.Invoke(true);
        }

        private void Rewarded_OnAdLoaded(LevelPlayAdInfo a_adInfo) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got Rewarded_OnAdLoaded " + a_adInfo);
            }

            m_onAdLoaded?.Invoke(EAdType.REWARDED);
        }

        private void Rewarded_OnAdLoadFailed(LevelPlayAdError a_error) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got Rewarded_OnAdLoadFailed");
            }

            Invoke(nameof(LoadRewarded), 5f);
        }

        private void Rewarded_OnAdDisplayFailed(LevelPlayAdDisplayInfoError a_error) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got Rewarded_OnAdDisplayFailed " + a_error.LevelPlayError.ErrorMessage);
            }
            e_onDisplayAdError?.Invoke(a_error, EAdType.REWARDED);
            e_onFinishAds?.Invoke(false);

            LoadRewarded();
        }

        private void Rewarded_OnAdClosed(LevelPlayAdInfo a_adInfo) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got Rewarded_OnAdClosed " + a_adInfo);
            }
            e_onFinishAds?.Invoke(false);
            LoadRewarded();
        }

        private void Rewarded_OnAdDisplayed(LevelPlayAdInfo a_adInfo) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got Rewarded_OnAdDisplayed With AdInfo " + a_adInfo);
            }
        }
        #endregion

        #region Inter callbacks
        private void Interstitial_OnAdLoaded(LevelPlayAdInfo a_adInfo) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got Interstitial_OnAdReadyEvent With AdInfo " + a_adInfo);
            }

            m_onAdLoaded?.Invoke(EAdType.INTER);
        }
        private void Interstitial_OnAdClosedEvent(LevelPlayAdInfo a_adInfo) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got Interstitial_OnAdClosedEvent " + a_adInfo);
            }

            e_onInterDismissed?.Invoke();
            LoadInter();
        }

        private void Interstitial_OnAdDisplayed(LevelPlayAdInfo a_adInfo) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got Interstitial_OnAdDisplayed " + a_adInfo);
            }
        }

        private void Interstitial_OnAdLoadFailed(LevelPlayAdError a_error) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got Interstitial_OnAdLoadFailed With Error " + a_error);
            }

            Invoke(nameof(LoadInter), 5f);
        }
        private void Interstitial_OnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError a_error) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got Interstitial_OnAdDisplayFailedEvent With Error " + a_error);
            }
            e_onDisplayAdError?.Invoke(a_error, EAdType.INTER);
            e_onInterDismissed?.Invoke();
        }

        #endregion



        #region Banner Callbacks

        public void ToggleBanner(bool a_show) {
            if (m_bannerAd == null) {
                return;
            }

            if (a_show) {
                m_bannerAd.ShowAd();
            }
            else {
                m_bannerAd.HideAd();
            }
        }

        private void BannerOnAdLoadedEvent(LevelPlayAdInfo a_adInfo) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got BannerOnAdLoadedEvent With AdInfo " + a_adInfo);
            }

            m_isBannerLoadedOnce = true;
            m_onBannerAdLoaded?.Invoke(a_adInfo);
        }

        private void BannerOnAdScreenPresentedEvent(LevelPlayAdInfo a_adInfo) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got BannerOnAdScreenPresentedEvent With AdInfo " + a_adInfo);
            }
        }

        private void BannerOnAdLoadFailedEvent(LevelPlayAdError a_adInfo) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got BannerOnAdLoadFailedEvent With AdInfo " + a_adInfo);
            }

            if (!m_isBannerLoadedOnce) {
                if (m_bannerAd != null) {
                    m_bannerAd.DestroyAd();
                }

                Invoke(nameof(LoadBannerFirstTime), 5f);
                Debug.Log("[MadPixel] I try to load banner again");
            }
        }
        #endregion



        #region General callbacks
        private void SdkInitializationCompletedEvent(LevelPlayConfiguration a_configuration) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got SdkInitializationCompletedEvent");
                if (m_debugTestSuiteOn) {
                    IronSource.Agent.launchTestSuite();
                }
            }


#if UNITY_ANDROID
            m_interstitialAd = new LevelPlayInterstitialAd(m_customSettings.InterstitialID);
            m_rewardedAd = new LevelPlayRewardedAd(m_customSettings.RewardedID);
#else
            m_interstitialAd = new LevelPlayInterstitialAd(m_customSettings.InterstitialID_IOS);
            m_rewardedAd = new LevelPlayRewardedAd(m_customSettings.RewardedID_IOS);
#endif

            m_rewardedAd.OnAdLoaded += Rewarded_OnAdLoaded;
            m_rewardedAd.OnAdLoadFailed += Rewarded_OnAdLoadFailed;
            m_rewardedAd.OnAdDisplayed += Rewarded_OnAdDisplayed;
            m_rewardedAd.OnAdDisplayFailed += Rewarded_OnAdDisplayFailed;
            m_rewardedAd.OnAdRewarded += Rewarded_OnAdRewarded;
            m_rewardedAd.OnAdClosed += Rewarded_OnAdClosed;

            m_interstitialAd.OnAdLoaded += Interstitial_OnAdLoaded;
            m_interstitialAd.OnAdLoadFailed += Interstitial_OnAdLoadFailed;
            m_interstitialAd.OnAdDisplayed += Interstitial_OnAdDisplayed;
            m_interstitialAd.OnAdDisplayFailed += Interstitial_OnAdDisplayFailedEvent;
            m_interstitialAd.OnAdClosed += Interstitial_OnAdClosedEvent;

            LoadInter();
            LoadRewarded();


            if (m_customSettings.bUseBanners) {
                LoadBannerFirstTime();
            }

            AnalyticsManager.CustomEvent("LevelPlayInit", new Dictionary<string, object>() {
                {"init_success", null}
            });
        }

        private void SdkInitializationFailedEvent(LevelPlayInitError a_error) {
            Debug.LogError($"[MadPixel] I got SdkInitializationFailedEvent {a_error.ErrorCode}: {a_error.ErrorMessage}");
            if (!m_sdkFailedToInitSent) {
                m_sdkFailedToInitSent = true;
                AnalyticsManager.CustomEvent("LevelPlayInit", new Dictionary<string, object>() {
                    {"init_failed", a_error.ErrorMessage}
                });
            }

            Invoke(nameof(TryToInit), 10f);
        }

        private void TryToInit() {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] Try to init");
            }

#if UNITY_ANDROID
            LevelPlay.Init(m_customSettings.levelPlayKey);
#else
            LevelPlay.Init(m_customSettings.levelPlayKey_ios);
#endif
        }

        private void LoadBannerFirstTime() {
            com.unity3d.mediation.LevelPlayAdSize adSize = com.unity3d.mediation.LevelPlayAdSize.CreateAdaptiveAdSize();

#if UNITY_ANDROID
            m_bannerAd = new LevelPlayBannerAd(m_customSettings.BannerID, adSize,
                m_customSettings.useTopBannerPosition ?
                    com.unity3d.mediation.LevelPlayBannerPosition.TopCenter :
                    com.unity3d.mediation.LevelPlayBannerPosition.BottomCenter,
                null, true, m_customSettings.useTopBannerPosition);
#else
            m_bannerAd = new LevelPlayBannerAd(m_customSettings.BannerID_IOS, adSize, 
                m_customSettings.useTopBannerPosition ? 
                    com.unity3d.mediation.LevelPlayBannerPosition.TopCenter : 
                    com.unity3d.mediation.LevelPlayBannerPosition.BottomCenter,
                null, true, m_customSettings.useTopBannerPosition);
#endif
            m_bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
            m_bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;

            m_bannerAd.LoadAd();
        }

        void ImpressionDataReadyEvent(IronSourceImpressionData a_impressionData) {
            if (m_debugLogsOn) {
                Debug.Log("[MadPixel] I got ImpressionDataReadyEvent allData: " + a_impressionData.allData);
            }
        }
        #endregion
    }
}
