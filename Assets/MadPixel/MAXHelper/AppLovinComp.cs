using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if MADPIXEL_AMAZON_DROID
using AmazonAds;
#endif
#if UNITY_IOS
using Unity.Advertisement.IosSupport; // NOTE: Import "com.unity.ads.ios-support" from Package Manager, if it's missing
#endif

namespace MAXHelper {
    public class AppLovinComp : MonoBehaviour {
        #region Fields
        private MaxSdkBase.AdInfo ShowedInfo;
        private MAXCustomSettings Settings;
        private string RewardedID = "empty";
        private string BannerID = "empty";
        private string InterstitialID = "empty";
        public bool bInitialized { get; private set; }
        [SerializeField] private bool bShowDebug;
        private Dictionary<string, string> Keywords = new Dictionary<string, string>();
        #endregion


        #region Events Declaration
        public UnityAction<bool> onFinishAdsEvent;
        public UnityAction<MaxSdkBase.AdInfo, MaxSdkBase.ErrorInfo, AdsManager.EAdType> onErrorEvent;
        public UnityAction onInterDismissedEvent;
        public UnityAction OnBannerInitialized;
        public UnityAction<bool> onAdLoadedEvent; // true = rewarded 

        public UnityAction<string, MaxSdkBase.AdInfo, MaxSdkBase.ErrorInfo> onBannerLoadedEvent;
        public UnityAction<string, MaxSdkBase.AdInfo> onBannerRevenueEvent;
        #endregion


        #region Initialization
        public void Init(MAXCustomSettings CustomSettings) {
            Settings = CustomSettings;
            if (string.IsNullOrEmpty(Settings.SDKKey)) {
                Debug.LogError("[MadPixel] Cant init SDK with a null SDK key!");
            }
            else {
                MaxSdkCallbacks.OnSdkInitializedEvent += OnAppLovinInitialized;
                InitSDK();

                TryAddKeyword("tier", GetUpperLowerTierKeyword());
                TryAddKeyword("app_version", Application.version.Replace(".", string.Empty));
                if (PlayerPrefs.GetInt("FirstPurchaseWas", 0) == 1) { // NOTE: was set for appsflyer in AnalyticsManager
                    TryAddKeyword("purchase", "purchase");
                }

                SetKeywordsToMax();
            }
        }

        private void InitSDK() {
            MaxSdk.SetSdkKey(Settings.SDKKey);
            MaxSdk.InitializeSdk();
            MaxSdk.SetVerboseLogging(bShowDebug);
        }


        private void OnAppLovinInitialized(MaxSdkBase.SdkConfiguration sdkConfiguration) {
            if (Settings.bShowMediationDebugger) {
                MaxSdk.ShowMediationDebugger();
            }

            if (Settings.bUseBanners) {
                InitializeBannerAds();
            }

            if (Settings.bUseRewardeds) {
                InitializeRewardedAds();
            }

            if (Settings.bUseInters) {
                InitializeInterstitialAds();
            }

            Debug.Log("[MadPixel] AppLovin is initialized");

            if (MadPixelAnalytics.AnalyticsManager.Exist) {
                if (MadPixelAnalytics.AnalyticsManager.Instance.bUseAutoInit) {
                    MadPixelAnalytics.AnalyticsManager.Instance.Init();
                }
            }
            else {
                Debug.LogError($"[MadPixel] Error in initializing Analytics! It doesn't exist on Scene!");
            }


            bInitialized = true;
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
            if (!string.IsNullOrEmpty(Settings.BannerID)) {
                BannerID = Settings.BannerID;
            } else {
                Debug.LogError("[MadPixel] Banner ID in Settings is Empty!");
            }
            string amazonID = Settings.AmazonBannerID;
#else
            if (!string.IsNullOrEmpty(Settings.BannerID_IOS)) {
                BannerID = Settings.BannerID_IOS;
            } else {
                Debug.LogError("Banner ID in Settings is Empty!");
            }
            string amazonID = Settings.AmazonBannerID_IOS;
#endif

#if MADPIXEL_AMAZON_DROID && !UNITY_EDITOR
            int width = 320;
            int height = 50;

            if (!string.IsNullOrEmpty(amazonID)) {
                var apsBanner = new APSBannerAdRequest(width, height, amazonID);
                apsBanner.onSuccess += (adResponse) => {
                    Debug.LogWarning($"[MadPixel] Banners. Amazon.onSuccess!");
                    MaxSdk.SetBannerLocalExtraParameter(BannerID, "amazon_ad_response", adResponse.GetResponse());
                    MaxSdk.CreateBanner(BannerID, MaxSdkBase.BannerPosition.BottomCenter);
                    MaxSdk.SetBannerBackgroundColor(BannerID, Settings.BannerBackground);
                    OnBannerInitialized?.Invoke();
                };
                apsBanner.onFailedWithError += (adError) => {
                    Debug.LogWarning($"[MadPixel] Banners. Amazon.onFailedWithError!");
                    MaxSdk.SetBannerLocalExtraParameter(BannerID, "amazon_ad_error", adError.GetAdError());
                    MaxSdk.CreateBanner(BannerID, MaxSdkBase.BannerPosition.BottomCenter);
                    MaxSdk.SetBannerBackgroundColor(BannerID, Settings.BannerBackground);
                    OnBannerInitialized?.Invoke();
                };

                apsBanner.LoadAd();
                return;
            }
#endif
            MaxSdk.CreateBanner(BannerID, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerBackgroundColor(BannerID, Settings.BannerBackground);
            OnBannerInitialized?.Invoke();
        }

        private void OnBannerAdLoadedEvent(string type, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log($"OnBannerAdLoadedEvent invoked. {type}, {adInfo}");
            }
            onBannerLoadedEvent?.Invoke(type, adInfo, null);
        }

        private void OnBannerAdLoadFailedEvent(string type, MaxSdkBase.ErrorInfo errorInfo) {
            if (bShowDebug) {
                Debug.Log($"OnBannerAdLoadFailedEvent invoked. {type}, {errorInfo}");
            }
            onBannerLoadedEvent?.Invoke(type, null, errorInfo);
        }

        private void OnBannerAdRevenuePaidEvent(string type, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log($"OnBannerAdRevenuePaidEvent invoked. {type}, {adInfo}");
            }
            onBannerRevenueEvent?.Invoke(type, adInfo);
        }

        public void ShowBanner(bool bShow, MaxSdkBase.BannerPosition NewPosition = MaxSdkBase.BannerPosition.BottomCenter) {
            if (bInitialized) {
                if (bShow) {
                    MaxSdk.UpdateBannerPosition(BannerID, NewPosition);
                    MaxSdk.ShowBanner(BannerID);
                }
                else {
                    MaxSdk.HideBanner(BannerID);
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
            onInterDismissedEvent?.Invoke();
        }

        private bool isFirstLoad_inter = true;
        private void LoadInterstitial() {
            if (isFirstLoad_inter) {
                isFirstLoad_inter = false;
#if UNITY_ANDROID
                if (!string.IsNullOrEmpty(Settings.InterstitialID)) {
                    InterstitialID = Settings.InterstitialID;
                }
                else {
                    Debug.LogError("[MadPixel] Interstitial ID in Settings is Empty!");
                }
                string amazonID = Settings.AmazonInterstitialID;
#else
                if (!string.IsNullOrEmpty(Settings.InterstitialID_IOS)) {
                    InterstitialID = Settings.InterstitialID_IOS;
                } else {
                    Debug.LogError("Interstitial ID in Settings is Empty!");
                }
                string amazonID = Settings.AmazonInterstitialID_IOS;
#endif

#if MADPIXEL_AMAZON_DROID && !UNITY_EDITOR
                // APS LoadAd only needs to be called once.
                if (!string.IsNullOrEmpty(amazonID)) {
                    var interstitialVideoAdRequest = new APSVideoAdRequest(320, 480, amazonID);
                    interstitialVideoAdRequest.onSuccess += (adResponse) => {
                        Debug.LogWarning($"[MadPixel] Inters. Amazon.onSuccess!");
                        MaxSdk.SetInterstitialLocalExtraParameter(InterstitialID, "amazon_ad_response", adResponse.GetResponse());
                        MaxSdk.LoadInterstitial(InterstitialID);
                    };
                    interstitialVideoAdRequest.onFailedWithError += (adError) => {
                        Debug.LogWarning($"[MadPixel] Inters. Amazon.onFailedWithError!");
                        MaxSdk.SetInterstitialLocalExtraParameter(InterstitialID, "amazon_ad_error", adError.GetAdError());
                        MaxSdk.LoadInterstitial(InterstitialID);
                    };
                    interstitialVideoAdRequest.LoadAd();
                    return;
                }
#endif
            }
            MaxSdk.LoadInterstitial(InterstitialID);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            onAdLoadedEvent?.Invoke(false);
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) {
            // Interstitial ad failed to load. We recommend re-trying in 3 seconds.
            Invoke("LoadInterstitial", 3);
            Debug.LogWarning("OnInterstitialFailedEvent");
        }

        private void OnInterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log("OnInterstitialFailedToDisplayEvent invoked");
            }
            LoadInterstitial();

            onErrorEvent?.Invoke(adInfo, errorInfo, AdsManager.EAdType.INTER);

            Debug.LogWarning("InterstitialFailedToDisplayEvent");
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log("OnInterstitialDismissedEvent invoked");
            }

            LoadInterstitial();
            onInterDismissedEvent?.Invoke();
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
            onFinishAdsEvent?.Invoke(false);
            ShowedInfo = null;
        }

        private bool isFirstLoad_rew = true;
        private void LoadRewardedAd() {
            if (isFirstLoad_rew) {
                isFirstLoad_rew = false;
#if UNITY_ANDROID
                if (!string.IsNullOrEmpty(Settings.RewardedID)) {
                    RewardedID = Settings.RewardedID;
                }
                else {
                    Debug.LogError("[MadPixel] Rewarded ID in Settings is Empty!");
                }
                string amazonID = Settings.AmazonRewardedID;
#else
                if (!string.IsNullOrEmpty(Settings.RewardedID_IOS)) {
                    RewardedID = Settings.RewardedID_IOS;
                } else {
                    Debug.LogError("Rewarded ID in Settings is Empty!");
                }
                string amazonID = Settings.AmazonRewardedID_IOS;
#endif

#if MADPIXEL_AMAZON_DROID && !UNITY_EDITOR
                // APS LoadAd only needs to be called once.
                if (!string.IsNullOrEmpty(amazonID)) {
                    var rewardedVideoAdRequest = new APSVideoAdRequest(320, 480, amazonID);
                    rewardedVideoAdRequest.onSuccess += (adResponse) => {
                        Debug.LogWarning($"[MadPixel] Rewardeds. Amazon.onSuccess!");
                        MaxSdk.SetRewardedAdLocalExtraParameter(RewardedID, "amazon_ad_response",
                            adResponse.GetResponse());
                        MaxSdk.LoadRewardedAd(RewardedID);
                    };
                    rewardedVideoAdRequest.onFailedWithError += (adError) => {
                        Debug.LogWarning($"[MadPixel] Rewardeds. Amazon.onFailedWithError!");
                        MaxSdk.SetRewardedAdLocalExtraParameter(RewardedID, "amazon_ad_error", adError.GetAdError());
                        MaxSdk.LoadRewardedAd(RewardedID);
                    };
                    rewardedVideoAdRequest.LoadAd();
                    return;
                }
#endif
            }
            MaxSdk.LoadRewardedAd(RewardedID);
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log("OnRewardedAdDisplayedEvent invoked");
            }
            ShowedInfo = adInfo;
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            onAdLoadedEvent?.Invoke(true);
            ShowedInfo = adInfo; 
            if (bShowDebug) {
                Debug.Log("OnRewardedAdLoadedEvent invoked");
            }
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) {
            // Rewarded ad failed to load. We recommend re-trying in 3 seconds.
            Invoke("LoadRewardedAd", 3); 
            if (bShowDebug) {
                Debug.Log("OnRewardedAdLoadFailedEvent invoked");
            }
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) {
            // Rewarded ad failed to display. We recommend loading the next ad

            if (bShowDebug) {
                Debug.Log("OnRewardedAdFailedToDisplayEvent invoked");
            }

            OnError(adInfo, errorInfo);
            LoadRewardedAd();
        }

        private void OnError(MaxSdkBase.AdInfo adInfo, MaxSdkBase.ErrorInfo EInfo) {
            onErrorEvent?.Invoke(adInfo, EInfo, AdsManager.EAdType.REWARDED);
            ShowedInfo = null;
        }

        private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log("OnRewardedAdDismissedEvent invoked");
            }

            if (ShowedInfo != null) {
                onFinishAdsEvent?.Invoke(false);
            }
            
            ShowedInfo = null;
            LoadRewardedAd();
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo) {
            if (bShowDebug) {
                Debug.Log("OnRewardedAdReceivedRewardEvent invoked");
            }

            onFinishAdsEvent?.Invoke(ShowedInfo != null);
            ShowedInfo = null;
        }

        #endregion

        #region Show Ads

        public bool ShowInterstitial() {
            if (bInitialized && MaxSdk.IsInterstitialReady(InterstitialID)) {
                MaxSdk.ShowInterstitial(InterstitialID);
                return true;
            }

            return false;
        }

        public void ShowRewarded() {
            if (bInitialized && MaxSdk.IsRewardedAdReady(RewardedID)) {
                MaxSdk.ShowRewardedAd(RewardedID);
            }
        }

        public bool IsReady(bool bIsRewarded) {
            if (bInitialized) {
                if (bIsRewarded) {
                    return MaxSdk.IsRewardedAdReady(RewardedID);
                }
                else {
                    return MaxSdk.IsInterstitialReady(InterstitialID);
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
            if (bInitialized) {
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


        #region Keywords
        private string GetUpperLowerTierKeyword() {
            bool bUpper = true;
            if (SystemInfo.deviceType == DeviceType.Handheld) {
                bUpper = SystemInfo.systemMemorySize > 5000 &&
                         SystemInfo.graphicsMemorySize > 2000 &&
                         SystemInfo.processorCount >= 8 &&
                         Screen.currentResolution.height >= 1920 &&
                         Screen.currentResolution.width >= 1080;

            }

            return bUpper ? "upper" : "lower";
        }


        public bool TryAddKeyword(string keyword, string newValue, bool a_forceSetToMax = false) {
            Keywords.TryGetValue(keyword, out string v);
            if (string.IsNullOrEmpty(v)) {
                Keywords.Add(keyword, newValue);
                if (a_forceSetToMax) {
                    SetKeywordsToMax();
                }
                return true;
            }
            else if (v != newValue) {
                Keywords[keyword] = newValue;
                if (a_forceSetToMax) {
                    SetKeywordsToMax();
                }
                return true;
            }

            return false;
        }

        private void SetKeywordsToMax() {
            MaxSdk.TargetingData.Keywords = null;

            List<string> list = new List<string>();
            foreach (KeyValuePair<string, string> kvp in Keywords) {
                list.Add($"{kvp.Key}:{kvp.Value}");
            }
            MaxSdk.TargetingData.Keywords = list.ToArray();

            if (bShowDebug) {
                foreach (string key in list) {
                    Debug.Log($"Keyword recorded: {key}");
                }
            }
        }
        #endregion
    }
}