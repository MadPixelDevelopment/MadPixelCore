using System.Collections.Generic;
using Unity.Services.LevelPlay;
using MadPixelAnalytics;
using MAXHelper;
using UnityEngine;
using UnityEngine.Events;
using static MAXHelper.AdsManager;

namespace MadPixel {
    public class LevelPlayComp : MonoBehaviour {
        [SerializeField] private bool bIsDebug;
        [SerializeField] private bool bUseTestSuite;
        private MAXCustomSettings customSettings;
        private bool m_isBannerLoadedOnce = false;

        #region Events Declaration
        public UnityAction<bool> onFinishAdsEvent;
        public UnityAction<LevelPlayAdDisplayInfoError, AdsManager.EAdType> onErrorEvent;
        public UnityAction onInterDismissedEvent;

        public UnityAction<AdsManager.EAdType> onAdLoadedEvent;
        public UnityAction<LevelPlayAdInfo> onBannerAdLoadedEvent;

        private LevelPlayRewardedAd rewardedAd;
        private LevelPlayBannerAd bannerAd;
        private LevelPlayInterstitialAd interstitialAd;

        private bool m_sdkFailedToInitSent = false;
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
        public void Init(MAXCustomSettings a_madPixelSettings) {
            customSettings = a_madPixelSettings;
            if (bIsDebug) {
                if (bUseTestSuite) {
                    IronSource.Agent.setMetaData("is_test_suite", "enable");
                }
            }
            SubscribeAll();
            IronSource.Agent.setAdaptersDebug(bIsDebug);
            TryToInit();

            if (bIsDebug) {
                IronSource.Agent.validateIntegration();
            }
        }

        public bool IsReady(EAdType a_adType) {
#if UNITY_EDITOR
            return true;
#endif
            if (a_adType == EAdType.REWARDED && rewardedAd != null) {
                return rewardedAd.IsAdReady();
            }
            else if (a_adType == EAdType.INTER && interstitialAd != null) {
                return interstitialAd.IsAdReady();

            }

            return false;
        }

        public bool ShowRewarded() {
            if (rewardedAd != null && rewardedAd.IsAdReady()) {
                rewardedAd.ShowAd();
                return true;
            }

            return false;
        }

        public bool ShowInter() {
            if (interstitialAd != null && interstitialAd.IsAdReady()) {
                interstitialAd.ShowAd();
                return true;
            }

            return false;
        }
        #endregion



        #region General Helpers
        private void SubscribeAll() {
            //Add Init Event
            LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
            LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

            //Add ImpressionSuccess Event
            IronSourceEvents.onImpressionDataReadyEvent += ImpressionDataReadyEvent;
        }


        private void UnsubscribeAll() {
            //Add Init Event
            LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
            LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;

            //Add ImpressionSuccess Event
            IronSourceEvents.onImpressionDataReadyEvent -= ImpressionDataReadyEvent;
        }


        private void LoadInter() {
            if (interstitialAd != null) {
                interstitialAd.LoadAd();
            }

            if (bIsDebug) {
                Debug.Log("[MadPixel] I called LoadInter");
            }
        }

        private void LoadRewarded() {
            if (rewardedAd != null) {
                rewardedAd.LoadAd();
            }

            if (bIsDebug) {
                Debug.Log("[MadPixel] I called LoadRewarded");
            }
        }

        #endregion

        #region Rewarded callbacks
        private void Rewarded_OnAdRewarded(LevelPlayAdInfo a_adInfo, LevelPlayReward a_reward) {
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got Rewarded_OnAdRewarded " + a_adInfo);
            }

            onFinishAdsEvent?.Invoke(true);
            //LoadRewarded();
        }

        private void Rewarded_OnAdLoaded(LevelPlayAdInfo a_adInfo) {
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got Rewarded_OnAdLoaded " + a_adInfo);
            }

            onAdLoadedEvent?.Invoke(EAdType.REWARDED);
        }

        private void Rewarded_OnAdLoadFailed(LevelPlayAdError a_error) {
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got Rewarded_OnAdLoadFailed");
            }

            Invoke(nameof(LoadRewarded), 5f);
        }

        private void Rewarded_OnAdDisplayFailed(LevelPlayAdDisplayInfoError a_error) {
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got Rewarded_OnAdDisplayFailed " + a_error.LevelPlayError.ErrorMessage);
            }
            onErrorEvent?.Invoke(a_error, EAdType.REWARDED);
            onFinishAdsEvent?.Invoke(false);

            LoadRewarded();
        }

        private void Rewarded_OnAdClosed(LevelPlayAdInfo a_adInfo) {
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got Rewarded_OnAdClosed " + a_adInfo);
            }
            onFinishAdsEvent?.Invoke(false);
            LoadRewarded();
        }

        private void Rewarded_OnAdDisplayed(LevelPlayAdInfo a_adInfo) {
            Debug.Log(a_adInfo.Revenue);
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got Rewarded_OnAdDisplayed With AdInfo " + a_adInfo);
            }
        }
        #endregion

        #region Inter callbacks
        private void Interstitial_OnAdLoaded(LevelPlayAdInfo a_adInfo) {
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got InterstitialOnAdReadyEvent With AdInfo " + a_adInfo);
            }

            onAdLoadedEvent?.Invoke(EAdType.INTER);
        }
        private void Interstitial_OnAdClosedEvent(LevelPlayAdInfo a_adInfo) {
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got InterstitialOnAdClosedEvent " + a_adInfo);
            }

            onInterDismissedEvent?.Invoke();
            LoadInter();
        }

        private void Interstitial_OnAdDisplayed(LevelPlayAdInfo a_adInfo) {
            onInterDismissedEvent?.Invoke();
        }

        private void Interstitial_OnAdLoadFailed(LevelPlayAdError a_error) {
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got InterstitialOnAdLoadFailed With Error " + a_error);
            }

            Invoke(nameof(LoadInter), 5f);
        }
        private void Interstitial_OnAdShowFailedEvent(LevelPlayAdDisplayInfoError a_error) {
            onErrorEvent?.Invoke(a_error, EAdType.INTER);
            onInterDismissedEvent?.Invoke();
        }

        #endregion



        #region Banner Callbacks

        public void ToggleBanner(bool a_show) {
            if (bannerAd == null) {
                return;
            }
            if (a_show) {
                bannerAd.ShowAd();
            }
            else {
                bannerAd.HideAd();
            }
        }

        void BannerOnAdLoadedEvent(LevelPlayAdInfo a_adInfo) {
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got BannerOnAdLoadedEvent With AdInfo " + a_adInfo);
            }

            m_isBannerLoadedOnce = true;
            onBannerAdLoadedEvent?.Invoke(a_adInfo);
        }

        private void BannerOnAdScreenPresentedEvent(LevelPlayAdInfo a_adInfo) {
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got BannerOnAdScreenPresentedEvent With AdInfo " + a_adInfo);
            }
        }

        private void BannerOnAdLoadFailedEvent(LevelPlayAdError a_adInfo) {
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got BannerOnAdLoadFailedEvent With AdInfo " + a_adInfo);
            }

            if (!m_isBannerLoadedOnce) {
                if (bannerAd != null) {
                    bannerAd.DestroyAd();
                }

                Invoke(nameof(LoadBannerFirstTime), 5f);
                Debug.Log("[MadPixel] I try to load banner again");
            }
        }
        #endregion



        #region General callbacks
        private void SdkInitializationCompletedEvent(LevelPlayConfiguration a_configuration) {
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got SdkInitializationCompletedEvent");
                if (bUseTestSuite) {
                    IronSource.Agent.launchTestSuite();
                }
            }


#if UNITY_ANDROID
            interstitialAd = new LevelPlayInterstitialAd(customSettings.InterstitialID);
            rewardedAd = new LevelPlayRewardedAd(customSettings.RewardedID);
#else
            interstitialAd = new LevelPlayInterstitialAd(customSettings.InterstitialID_IOS);
            rewardedAd = new LevelPlayRewardedAd(customSettings.RewardedID_IOS);
#endif

            rewardedAd.OnAdLoaded += Rewarded_OnAdLoaded;
            rewardedAd.OnAdLoadFailed += Rewarded_OnAdLoadFailed;
            rewardedAd.OnAdDisplayed += Rewarded_OnAdDisplayed;
            rewardedAd.OnAdDisplayFailed += Rewarded_OnAdDisplayFailed;
            rewardedAd.OnAdRewarded += Rewarded_OnAdRewarded;
            rewardedAd.OnAdClosed += Rewarded_OnAdClosed;

            interstitialAd.OnAdLoaded += Interstitial_OnAdLoaded;
            interstitialAd.OnAdLoadFailed += Interstitial_OnAdLoadFailed;
            interstitialAd.OnAdDisplayed += Interstitial_OnAdDisplayed;
            interstitialAd.OnAdDisplayFailed += Interstitial_OnAdShowFailedEvent;
            interstitialAd.OnAdClosed += Interstitial_OnAdClosedEvent;

            LoadInter();
            LoadRewarded();


            if (customSettings.bUseBanners) {
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
            if (bIsDebug) {
                Debug.Log("[MadPixel] Try to init");
            }

#if UNITY_ANDROID
            LevelPlay.Init(customSettings.levelPlayKey);
#else
            LevelPlay.Init(customSettings.levelPlayKey_ios);
#endif
        }

        private void LoadBannerFirstTime() {
            com.unity3d.mediation.LevelPlayAdSize adSize = com.unity3d.mediation.LevelPlayAdSize.CreateAdaptiveAdSize();

#if UNITY_ANDROID
            bannerAd = new LevelPlayBannerAd(customSettings.BannerID, adSize,
                customSettings.useTopBannerPosition ?
                    com.unity3d.mediation.LevelPlayBannerPosition.TopCenter :
                    com.unity3d.mediation.LevelPlayBannerPosition.BottomCenter,
                null, true, customSettings.useTopBannerPosition);
#else
            bannerAd = new LevelPlayBannerAd(customSettings.BannerID_IOS, adSize, 
                customSettings.useTopBannerPosition ? 
                    com.unity3d.mediation.LevelPlayBannerPosition.TopCenter : 
                    com.unity3d.mediation.LevelPlayBannerPosition.BottomCenter,
                null, true, customSettings.useTopBannerPosition);
#endif
            bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
            bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;

            bannerAd.LoadAd();
        }

        void ImpressionDataReadyEvent(IronSourceImpressionData impressionData) {
            if (bIsDebug) {
                Debug.Log("[MadPixel] I got ImpressionDataReadyEvent allData: " + impressionData.allData);
            }
        }
        #endregion
    }
}
