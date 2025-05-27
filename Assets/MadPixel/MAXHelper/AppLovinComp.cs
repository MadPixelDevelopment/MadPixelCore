using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_IOS
using Unity.Advertisement.IosSupport; // NOTE: Import "com.unity.ads.ios-support" from Package Manager, if it's missing
#endif

namespace MadPixel {
    public class AppLovinComp : MonoBehaviour {
        #region Fields
        [SerializeField] private bool m_showDebugLogs;

        private MaxSdkBase.AdInfo m_showedInfo;
        private MadPixelCustomSettings m_settings;
        private string m_rewardedID = "empty";
        private string m_bannerID = "empty";
        private string m_interstitialID = "empty";

        private bool m_isFirstLoadForInter = true;

        public bool IsInitialized { get; private set; }
        #endregion


        #region Events Declaration
        public UnityAction<bool> e_onFinishAds;
        public UnityAction<MaxSdkBase.AdInfo, MaxSdkBase.ErrorInfo, AdsManager.EAdType> e_onAdFailedToDisplay;
        public UnityAction e_onInterDismissed;
        public UnityAction e_onBannerInitialized;
        public UnityAction<bool> e_onAdLoaded; // true = rewarded 

        public UnityAction<MaxSdkBase.AdInfo, MaxSdkBase.ErrorInfo> e_onBannerLoaded;
        public UnityAction<MaxSdkBase.AdInfo> e_onBannerRevenuePaid;
        #endregion


        #region Initialization
        public void Init(MadPixelCustomSettings a_customSettings) {
            m_settings = a_customSettings;
            if (string.IsNullOrEmpty(MadPixelCustomSettings.APPLOVIN_SDK_KEY)) {
                Debug.LogError("[MadPixel] Cant init SDK with a null SDK key!");
            }
            else {
                MaxSdkCallbacks.OnSdkInitializedEvent += OnAppLovinInitialized;
                InitSDK();
            }
        }

        private void InitSDK() {
            MaxSdk.InitializeSdk();
            MaxSdk.SetVerboseLogging(m_showDebugLogs);
        }


        private void OnAppLovinInitialized(MaxSdkBase.SdkConfiguration a_sdkConfiguration) {
            if (m_settings.bShowMediationDebugger) {
                MaxSdk.ShowMediationDebugger();
            }

            if (m_settings.bUseBanners) {
                InitializeBannerAds();
            }

            if (m_settings.bUseRewardeds) {
                InitializeRewardedAds();
            }

            if (m_settings.bUseInters) {
                InitializeInterstitialAds();
            }

            Debug.Log("[MadPixel] AppLovin is initialized");

            if (MadPixelAnalytics.AnalyticsManager.Exist) {
                if (MadPixelAnalytics.AnalyticsManager.Instance.useAutoInit) {
                    MadPixelAnalytics.AnalyticsManager.Instance.Init();
                }
            }
            else {
                Debug.LogError($"[MadPixel] Error in initializing Analytics! It doesn't exist on Scene!");
            }


            IsInitialized = true;
        }

        #endregion

        #region Banners
        public void InitializeBannerAds() {
            // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
            // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            

#if UNITY_ANDROID
            if (!string.IsNullOrEmpty(m_settings.BannerID)) {
                m_bannerID = m_settings.BannerID;
            } else {
                Debug.LogError("[MadPixel] Banner ID in Settings is Empty!");
            }
#else
            if (!string.IsNullOrEmpty(m_settings.BannerID_IOS)) {
                m_bannerID = m_settings.BannerID_IOS;
            } else {
                Debug.LogError("Banner ID in Settings is Empty!");
            }
#endif

            var adViewConfiguration = new MaxSdkBase.AdViewConfiguration(MaxSdkBase.AdViewPosition.BottomCenter);
            MaxSdk.CreateBanner(m_bannerID, adViewConfiguration);
            MaxSdk.SetBannerBackgroundColor(m_bannerID, m_settings.BannerBackground);
            e_onBannerInitialized?.Invoke();
        }

        private void OnBannerAdLoadedEvent(string a_adType, MaxSdkBase.AdInfo a_adInfo) {
            if (m_showDebugLogs) {
                Debug.Log($"OnBannerAdLoadedEvent invoked. {a_adType}, {a_adInfo}");
            }
            e_onBannerLoaded?.Invoke(a_adInfo, null);
        }

        private void OnBannerAdLoadFailedEvent(string a_adType, MaxSdkBase.ErrorInfo a_errorInfo) {
            if (m_showDebugLogs) {
                Debug.Log($"OnBannerAdLoadFailedEvent invoked. {a_adType}, {a_errorInfo}");
            }
            e_onBannerLoaded?.Invoke(null, a_errorInfo);
        }

        private void OnBannerAdRevenuePaidEvent(string a_adType, MaxSdkBase.AdInfo a_adInfo) {
            if (m_showDebugLogs) {
                Debug.Log($"OnBannerAdRevenuePaidEvent invoked. {a_adType}, {a_adInfo}");
            }
            e_onBannerRevenuePaid?.Invoke(a_adInfo);
        }

        public void ShowBanner(bool a_show, MaxSdkBase.AdViewPosition a_newPosition = MaxSdkBase.AdViewPosition.BottomCenter) {
            if (IsInitialized) {
                if (a_show) {
                    MaxSdk.UpdateBannerPosition(m_bannerID, a_newPosition);
                    MaxSdk.ShowBanner(m_bannerID);
                }
                else {
                    MaxSdk.HideBanner(m_bannerID);
                }
            }
        }
        #endregion

        #region Interstitials
        public void InitializeInterstitialAds() {
            // Attach callback
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialFailedToDisplayEvent;

            // Load the first interstitial
            LoadInterstitial();
        }

        public void CancelInterAd() {
            e_onInterDismissed?.Invoke();
        }

        private void LoadInterstitial() {
            if (m_isFirstLoadForInter) {
                m_isFirstLoadForInter = false;
#if UNITY_ANDROID
                if (!string.IsNullOrEmpty(m_settings.InterstitialID)) {
                    m_interstitialID = m_settings.InterstitialID;
                }
                else {
                    Debug.LogError("[MadPixel] Interstitial ID in Settings is Empty!");
                }
#else
                if (!string.IsNullOrEmpty(m_settings.InterstitialID_IOS)) {
                    m_interstitialID = m_settings.InterstitialID_IOS;
                } else {
                    Debug.LogError("Interstitial ID in Settings is Empty!");
                }
#endif
            }
            MaxSdk.LoadInterstitial(m_interstitialID);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            e_onAdLoaded?.Invoke(false);
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) {
            // Interstitial ad failed to load. We recommend re-trying in 3 seconds.
            Invoke("LoadInterstitial", 3);
            Debug.LogWarning("OnInterstitialFailedEvent");
        }

        private void OnInterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) {
            if (m_showDebugLogs) {
                Debug.Log("OnInterstitialFailedToDisplayEvent invoked");
            }
            LoadInterstitial();

            e_onAdFailedToDisplay?.Invoke(adInfo, errorInfo, AdsManager.EAdType.INTER);

            Debug.LogWarning("InterstitialFailedToDisplayEvent");
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            if (m_showDebugLogs) {
                Debug.Log("OnInterstitialDismissedEvent invoked");
            }

            LoadInterstitial();
            e_onInterDismissed?.Invoke();
        }
        #endregion

        #region Rewarded
        public void InitializeRewardedAds() {
            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

            // Load the first RewardedAd
            LoadRewardedAd();
        }

        public void CancelRewardedAd() {
            e_onFinishAds?.Invoke(false);
            m_showedInfo = null;
        }

        private bool isFirstLoad_rew = true;
        private void LoadRewardedAd() {
            if (isFirstLoad_rew) {
                isFirstLoad_rew = false;
#if UNITY_ANDROID
                if (!string.IsNullOrEmpty(m_settings.RewardedID)) {
                    m_rewardedID = m_settings.RewardedID;
                }
                else {
                    Debug.LogError("[MadPixel] Rewarded ID in Settings is Empty!");
                }
#else
                if (!string.IsNullOrEmpty(m_settings.RewardedID_IOS)) {
                    m_rewardedID = m_settings.RewardedID_IOS;
                } else {
                    Debug.LogError("Rewarded ID in Settings is Empty!");
                }
#endif
            }
            MaxSdk.LoadRewardedAd(m_rewardedID);
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            if (m_showDebugLogs) {
                Debug.Log("OnRewardedAdDisplayedEvent invoked");
            }
            m_showedInfo = adInfo;
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            e_onAdLoaded?.Invoke(true);
            m_showedInfo = adInfo; 
            if (m_showDebugLogs) {
                Debug.Log("OnRewardedAdLoadedEvent invoked");
            }
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) {
            // Rewarded ad failed to load. We recommend re-trying in 3 seconds.
            Invoke("LoadRewardedAd", 3); 
            if (m_showDebugLogs) {
                Debug.Log("OnRewardedAdLoadFailedEvent invoked");
            }
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) {
            // Rewarded ad failed to display. We recommend loading the next ad

            if (m_showDebugLogs) {
                Debug.Log("OnRewardedAdFailedToDisplayEvent invoked");
            }

            OnError(adInfo, errorInfo);
            LoadRewardedAd();
        }

        private void OnError(MaxSdkBase.AdInfo a_adInfo, MaxSdkBase.ErrorInfo a_errorInfo) {
            e_onAdFailedToDisplay?.Invoke(a_adInfo, a_errorInfo, AdsManager.EAdType.REWARDED);
            m_showedInfo = null;
        }

        private void OnRewardedAdDismissedEvent(string a_adUnitId, MaxSdkBase.AdInfo a_adInfo) {
            if (m_showDebugLogs) {
                Debug.Log("OnRewardedAdDismissedEvent invoked");
            }

            if (m_showedInfo != null) {
                e_onFinishAds?.Invoke(false);
            }
            
            m_showedInfo = null;
            LoadRewardedAd();
        }

        private void OnRewardedAdReceivedRewardEvent(string a_adUnitId, MaxSdk.Reward a_maxReward, MaxSdkBase.AdInfo a_adInfo) {
            if (m_showDebugLogs) {
                Debug.Log("OnRewardedAdReceivedRewardEvent invoked");
            }

            e_onFinishAds?.Invoke(m_showedInfo != null);
            m_showedInfo = null;
        }

        #endregion

        #region Show Ads

        public bool ShowInterstitial() {
            if (IsInitialized && MaxSdk.IsInterstitialReady(m_interstitialID)) {
                MaxSdk.ShowInterstitial(m_interstitialID);
                return true;
            }

            return false;
        }

        public void ShowRewarded() {
            if (IsInitialized && MaxSdk.IsRewardedAdReady(m_rewardedID)) {
                MaxSdk.ShowRewardedAd(m_rewardedID);
            }
        }

        public bool IsReady(bool bIsRewarded) {
            if (IsInitialized) {
                if (bIsRewarded) {
                    return MaxSdk.IsRewardedAdReady(m_rewardedID);
                }
                else {
                    return MaxSdk.IsInterstitialReady(m_interstitialID);
                }
            }
            return false;
        }
        #endregion

        #region Unsubscribers

        void OnDestroy() {
            UnsubscribeAll();
        }
        public void UnsubscribeAll() {
            if (IsInitialized) {
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterstitialLoadedEvent;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnInterstitialFailedEvent;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnInterstitialDismissedEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnInterstitialFailedToDisplayEvent;

                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnRewardedAdLoadedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnRewardedAdLoadFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= OnRewardedAdDisplayedEvent;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnRewardedAdDismissedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnRewardedAdFailedToDisplayEvent;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnRewardedAdReceivedRewardEvent;
            }
        }

        #endregion
    }
}