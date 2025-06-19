using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace MadPixel {
    
    [RequireComponent(typeof(AppLovinComp))]
    public class AdsManager : MonoBehaviour {
        public const string VERSION = "1.3.1";
        public enum EResultCode {OK = 0, NOT_LOADED, ADS_FREE, ON_COOLDOWN, ERROR}
        public enum EAdType {REWARDED, INTER, BANNER}

        #region Fields
        [FormerlySerializedAs("bInitializeOnStart")]
        [SerializeField] private bool m_initializeOnStart = true;

        [FormerlySerializedAs("CooldownBetweenInterstitials")]
        [SerializeField] private int m_cooldownBetweenInterstitials = 30;

        private bool m_canShowBanner = true;
        private bool m_intersOn = true;
        private bool m_hasInternet = true;
        private bool m_ready = false;

        private MadPixelCustomSettings m_madPixelSettings;
        private AppLovinComp m_appLovinComp;
        private AdInfo m_currentAdInfo;
        private float m_lastInterShown;
        private GameObject m_adsInstigatorObj;
        private UnityAction<bool> m_callbackPending;

        #endregion

        #region Events Declaration

        public UnityAction e_onAdsManagerInitialized;

        public UnityAction e_onNewRewardedLoaded;
        public UnityAction<MaxSdkBase.AdInfo, MaxSdkBase.ErrorInfo, AdInfo> e_onAdDisplayError;
        public UnityAction<AdInfo> e_onAdShown;
        public UnityAction<AdInfo> e_onAdAvailable;
        public UnityAction<AdInfo> e_onAdStarted;

        #endregion

        #region Static

        protected static AdsManager m_instance;

        public static bool Exist {
            get { return (m_instance != null); }
        }

        public static AdsManager Instance {
            get {
                if (m_instance == null) {
                    Debug.LogError("[Mad Pixel] AdsManager wasn't created yet!");

                    GameObject go = new GameObject();
                    go.name = "AdsManager";
                    m_instance = go.AddComponent(typeof(AdsManager)) as AdsManager;
                }

                return m_instance;
            }
        }

        public static bool Ready() {
            if (Exist) {
                return (Instance.m_ready && Instance.m_appLovinComp != null && Instance.m_appLovinComp.IsInitialized);
            }
            return (false);
        }

        public static float CooldownLeft {
            get {
                if (Exist) {
                    return Instance.m_lastInterShown + Instance.m_cooldownBetweenInterstitials - Time.time;
                }

                return -1f;
            }
        }


        public static void Destroy(bool a_immediate = false) {
            if (m_instance != null && m_instance.gameObject != null) {
                if (a_immediate) {
                    DestroyImmediate(m_instance.gameObject);
                }
                else {
                    GameObject.Destroy(m_instance.gameObject);
                }
            }

            m_instance = null;
        }

        public static string SETTINGS_FILE_NAME = "MadPixelCustomSettings";
        #endregion



        #region Init

        public void StartInitializationFlow() {
            InitApplovinInternal();
        }
        #endregion

        #region Event Catchers
        private void AppLovin_OnAdLoadedEvent(bool a_isRewarded) {
            if (a_isRewarded) {
                e_onNewRewardedLoaded?.Invoke();
            }
        }

        private void AppLovin_OnFinishAdsEvent(bool a_isFinished) {
            if (m_adsInstigatorObj != null) {
                m_adsInstigatorObj = null;
                m_callbackPending?.Invoke(a_isFinished);
                m_callbackPending = null;
            }
            else {
                Debug.LogError("[Mad Pixel] Ads Instigator was destroyed or nulled");
            }

            if (m_currentAdInfo == null) {
                // some AdDisplayFailed error happened before this was invoked
                return;
            }

            m_currentAdInfo.availability = a_isFinished ? "watched" : "canceled";
            e_onAdShown?.Invoke(m_currentAdInfo);

            RestartInterCooldown();

            m_currentAdInfo = null;
            //NOTE: Temporary disable sounds - off
        }

        private void AppLovin_OnInterDismissedEvent() {
            if (m_adsInstigatorObj != null) {
                m_adsInstigatorObj = null;
                m_callbackPending?.Invoke(true);
                m_callbackPending = null;
            } else {
                //Debug.LogError("[Mad Pixel] Ads Instigator was destroyed or nulled");
            }

            RestartInterCooldown();

            if (m_currentAdInfo != null) {
                e_onAdShown?.Invoke(m_currentAdInfo);
            }

            m_currentAdInfo = null;
            //NOTE: Temporary disable sounds - off
        }

        private void AppLovin_OnAdFailedToDisplayEvent(MaxSdkBase.AdInfo a_adInfo, MaxSdkBase.ErrorInfo a_errorInfo, EAdType a_adType) {
            if (m_currentAdInfo != null) {
                e_onAdDisplayError?.Invoke(a_adInfo, a_errorInfo, m_currentAdInfo);
            }
            else {
                e_onAdDisplayError?.Invoke(a_adInfo, a_errorInfo, new AdInfo("unknown", a_adType));
            }


#if UNITY_ANDROID
            bool cancelRetry = a_errorInfo.Code == MaxSdkBase.ErrorCode.DontKeepActivitiesEnabled       // NOTE: User won't see any ads in this session anyway (Droid)
                              || a_errorInfo.Code == MaxSdkBase.ErrorCode.FullscreenAdAlreadyShowing;    // NOTE: Can't show ad if it's already showing
#else
            bool cancelRetry = a_errorInfo.Code == MaxSdkBase.ErrorCode.FullscreenAdAlreadyShowing;     // NOTE: Can't show ad if it's already showing
#endif

            if (a_adType == EAdType.REWARDED) {
                ProccessRewardError(!cancelRetry);
            } else {
                ProccessInterError(!cancelRetry);
            }
        }

        
        private void AppLovin_OnBannerRevenueEvent(MaxSdkBase.AdInfo a_adInfo) {
            AdInfo bannerInfo = new AdInfo("banner", EAdType.BANNER, m_hasInternet);
            e_onAdShown?.Invoke(bannerInfo);
        }

        private void AppLovin_OnBannerLoadedEvent(MaxSdkBase.AdInfo a_adInfo, MaxSdkBase.ErrorInfo a_errorInfo) {
            AdInfo bannerInfo = new AdInfo("banner", EAdType.BANNER, m_hasInternet, a_errorInfo == null ? "available" : "not_available");
            e_onAdAvailable?.Invoke(bannerInfo);
            if (a_errorInfo == null && m_canShowBanner) {
                e_onAdStarted?.Invoke(bannerInfo);
            }
        }


        private void ProccessRewardError(bool a_retry) {
            if (a_retry && m_appLovinComp.IsReady(true) && m_currentAdInfo != null && m_callbackPending != null) {
                m_currentAdInfo.availability = "waited";
                e_onAdAvailable?.Invoke(m_currentAdInfo);
                m_appLovinComp.ShowRewarded();
            }
            else {
                m_appLovinComp.CancelRewardedAd();
            }
        }

        private void ProccessInterError(bool a_retry) {
            if (a_retry && m_appLovinComp.IsReady(false) && m_currentAdInfo != null) {
                m_currentAdInfo.availability = "waited";
                e_onAdAvailable?.Invoke(m_currentAdInfo);
                m_appLovinComp.ShowInterstitial();
            }
            else {
                m_appLovinComp.CancelInterAd();
            }
        }

        #endregion

        #region Unity Events

        private void Awake() {
            if (m_instance == null) {
                m_instance = this;
                GameObject.DontDestroyOnLoad(this.gameObject);

                m_appLovinComp = GetComponent<AppLovinComp>();
            }
            else {
                GameObject.Destroy(gameObject);
                Debug.LogError($"[MadPixel] Two AdsManagers at the same time!");
            }
        }

        private void Start() {
            if (m_initializeOnStart) {
                StartInitializationFlow();
            }
        }

        private void OnDestroy() {
            if (m_appLovinComp != null) {
                m_appLovinComp.e_onFinishAds -= AppLovin_OnFinishAdsEvent;
                m_appLovinComp.e_onInterDismissed -= AppLovin_OnInterDismissedEvent;
                m_appLovinComp.e_onAdLoaded -= AppLovin_OnAdLoadedEvent;
                m_appLovinComp.e_onAdFailedToDisplay -= AppLovin_OnAdFailedToDisplayEvent;

                m_appLovinComp.e_onBannerRevenuePaid -= AppLovin_OnBannerRevenueEvent;
                m_appLovinComp.e_onBannerLoaded -= AppLovin_OnBannerLoadedEvent;
            }
        }

        #endregion

        #region Public Static
        public static MadPixelCustomSettings LoadMadPixelCustomSettings() {
            return Resources.Load<MadPixelCustomSettings>(SETTINGS_FILE_NAME);
        }

        /// <param name="a_gameObjectRef">Instigator gameobject</param>
        /// <summary>
        /// Shows a Rewarded As. Returns OK if the ad is starting to show, NOT_LOADED if Applovin has no loaded ad yet.
        /// </summary>
        public static EResultCode ShowRewarded(GameObject a_gameObjectRef, UnityAction<bool> a_onFinishAds, string a_placement = "none") {
            if (Exist) {
                if (Instance.m_appLovinComp.IsReady(true)) {
                    Instance.SetCallback(a_onFinishAds, a_gameObjectRef);
                    Instance.ShowAdInner(EAdType.REWARDED, a_placement);
                    return EResultCode.OK;
                }
                else {
                    Instance.StartCoroutine(Instance.Ping());
                    return EResultCode.NOT_LOADED;
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Ads Manager doesn't exist!");
            }

            return EResultCode.ERROR;
        }

        public static EResultCode ShowInter(string a_placement = "none") {
            return ShowInter(null, null, a_placement);
        }

        public static EResultCode ShowInter(GameObject a_gameObjectRef, UnityAction<bool> a_onAdDismissed, string a_placement = "none") {
            if (Exist) {
                if (Instance.m_intersOn) {
                    if (Instance.IsCooldownElapsed()) {
                        if (Instance.m_appLovinComp.IsReady(false)) {
                            Instance.SetCallback(a_onAdDismissed, a_gameObjectRef);
                            Instance.ShowAdInner(EAdType.INTER, a_placement);
                            return EResultCode.OK;
                        }
                        else {
                            return EResultCode.NOT_LOADED;
                        }
                    }
                    else {
                        return EResultCode.ON_COOLDOWN;
                    }
                }
                else {
                    return EResultCode.ADS_FREE;
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Ads Manager doesn't exist!");
            }
            
            return EResultCode.ERROR;
        }

        /// <summary>
        /// Ignores ADS FREE and COOLDOWN conditions for interstitials
        /// </summary>
        public static EResultCode ShowInterForced(GameObject a_gameObjectRef, UnityAction<bool> a_onAdDismissed, string a_placement = "none") {
            if (Exist) {
                if (Instance.m_appLovinComp.IsReady(false)) {
                    Instance.SetCallback(a_onAdDismissed, a_gameObjectRef);
                    Instance.ShowAdInner(EAdType.INTER, a_placement);
                    return EResultCode.OK;
                } else {
                    return EResultCode.NOT_LOADED;
                }
            }
            return EResultCode.ERROR;
        }

        /// <summary>
        /// Returns TRUE if Applovin has a loaded ad ready to show
        /// </summary>
        public static bool HasLoadedAd(EAdType a_adType) {
            if (Exist) {
                if (a_adType == EAdType.REWARDED) {
                    return Instance.m_appLovinComp.IsReady(true);
                }
                else if (a_adType == EAdType.INTER) {
                    return (Instance.m_intersOn && Instance.m_appLovinComp.IsReady(false) && Instance.IsCooldownElapsed());
                }
                else {
                    Debug.LogError("[Mad Pixel] Can't use this for banners!");
                } 
            }

            return false;
        }


        /// <summary>
        /// Turns banners and inters off and prevents them from showing (this session only)
        /// Call this on AdsFree bought or on AdsFree checked at game start
        /// </summary>
        public static void CancelAllAds(bool a_disableInters = true, bool a_disableBanners = true) {
            if (Exist) {
                if (a_disableInters) {
                    Instance.m_intersOn = false;
                }
                if (a_disableBanners) {
                    Instance.m_canShowBanner = false;
                    ToggleBanner(false);
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Ads Manager doesn't exist!");
            }
        }

        public static void ToggleBanner(bool a_show, MaxSdkBase.AdViewPosition a_newPosition = MaxSdkBase.AdViewPosition.BottomCenter) {
            if (Exist) {
                if (a_show && Instance.m_canShowBanner) {
                    Instance.m_appLovinComp?.ShowBanner(true, a_newPosition);
                }
                else {
                    Instance.m_appLovinComp?.ShowBanner(false);
                }
            } else {
                Debug.LogError("[Mad Pixel] Ads Manager doesn't exist!");
            }
        }


        /// <summary>
        /// Tries to show a Rewarded ad; if a Rewarded ad is not loaded, tries to show an Inter ad instead (ignoring COOLDOWN and ADSFREE conditions)
        /// </summary>
        public static bool ShowRewardedWithSubstitution(GameObject a_gameObjectRef, UnityAction<bool> a_callback, string a_placement) {
            if (a_gameObjectRef) {
                EResultCode result = ShowRewarded(a_gameObjectRef, a_callback, a_placement);
                if (result == EResultCode.OK) {
                    return (true);
                }

                if (result == EResultCode.NOT_LOADED) {
                    result = ShowInterForced(a_gameObjectRef, a_callback, $"{a_placement}_i");
                    if (result == EResultCode.OK) {
                        return (true);
                    }
                }

                return (false);
            }
            return (false);
        }

        /// <summary>
        /// Tries to show an Inter ad; if an Inter ad is not loaded by Applovin, tries to show a Rewarded ad instead
        /// </summary>
        public static bool ShowInterWithSubstitution(GameObject a_gameObjectRef, UnityAction<bool> a_callback, string a_placement) {
            if (a_gameObjectRef) {
                EResultCode result = ShowInter(a_gameObjectRef, a_callback, a_placement);
                if (result == EResultCode.OK) {
                    return (true);
                }

                if (result == EResultCode.NOT_LOADED) {
                    result = ShowRewarded(a_gameObjectRef, a_callback, $"{a_placement}_r");
                    if (result == EResultCode.OK) {
                        return (true);
                    }
                }

                return (false);
            }
            return (false);
        }

        /// <summary>
        /// Returns mandatory Cooldown between interstitials, if set
        /// </summary>
        public static int GetCooldownBetweenInters() {
            if (Exist) {
                return Instance.m_cooldownBetweenInterstitials;
            }

            return 0;
        }

        /// <summary>
        /// Restarts interstitial cooldown (it already restarts automatically after an ad is watched)
        /// </summary>
        public static void RestartInterstitialCooldown() {
            if (Exist) {
                Instance.RestartInterCooldown();
            }
        }
        #endregion

        #region Helpers
        private void InitApplovinInternal() {
            m_lastInterShown = -m_cooldownBetweenInterstitials;

            m_madPixelSettings = LoadMadPixelCustomSettings();
            m_appLovinComp.Init(m_madPixelSettings);

            m_appLovinComp.e_onFinishAds += AppLovin_OnFinishAdsEvent;
            m_appLovinComp.e_onAdLoaded += AppLovin_OnAdLoadedEvent;
            m_appLovinComp.e_onInterDismissed += AppLovin_OnInterDismissedEvent;
            m_appLovinComp.e_onAdFailedToDisplay += AppLovin_OnAdFailedToDisplayEvent;
            m_appLovinComp.e_onBannerRevenuePaid += AppLovin_OnBannerRevenueEvent;
            m_appLovinComp.e_onBannerLoaded += AppLovin_OnBannerLoadedEvent;
            
            m_ready = true;

            e_onAdsManagerInitialized?.Invoke();
        }

        private void SetCallback(UnityAction<bool> a_callback, GameObject a_gameObjectRef) {
            m_adsInstigatorObj = a_gameObjectRef;
            m_callbackPending = a_callback;
        }

        private void ShowAdInner(EAdType a_adType, string a_placement) {
            m_currentAdInfo = new AdInfo(a_placement, a_adType);
            e_onAdAvailable?.Invoke(m_currentAdInfo);
            e_onAdStarted?.Invoke(m_currentAdInfo);
            // NOTE: Temporary Disable Sounds

            if (a_adType == EAdType.REWARDED) {
                m_appLovinComp.ShowRewarded();
            }
            else if (a_adType == EAdType.INTER) {
                m_appLovinComp.ShowInterstitial();
            }
        }

        private bool IsCooldownElapsed() {
            return (Time.time - m_lastInterShown > m_cooldownBetweenInterstitials);
        }

        private void RestartInterCooldown() {
            if (m_cooldownBetweenInterstitials > 0) {
                m_lastInterShown = Time.time;
            }
        }

        private IEnumerator Ping() {
            bool result;
            using (UnityWebRequest request = UnityWebRequest.Head("https://www.google.com/")) {
                request.timeout = 3;
                yield return request.SendWebRequest();
                result = request.result != UnityWebRequest.Result.ProtocolError && request.result != UnityWebRequest.Result.ConnectionError;
            }

            if (!result) {
                Debug.LogWarning("[Mad Pixel] Some problem with connection.");
            }

            OnPingComplete(result);
        }

        private void OnPingComplete(bool a_hasInternet) {
            if (m_currentAdInfo != null) {
                m_currentAdInfo.availability = "not_available";
                m_currentAdInfo.hasInternet = a_hasInternet;
                e_onAdAvailable?.Invoke(m_currentAdInfo);
            }

            this.m_hasInternet = a_hasInternet;
        }

        #endregion

        #region CMP (Google UMP) flow

        public static void ShowCMPFlow() {
            if (Ready()) {
                var cmpService = MaxSdk.CmpService;
                cmpService.ShowCmpForExistingUser(error => {
                    if (null == error) {
                        // The CMP alert was shown successfully.
                    }
                    else {
                        Debug.LogError(error);
                    }
                });
            }
        }

        public static bool IsGDPR() {
            if (Ready()) {
                return MaxSdk.GetSdkConfiguration().ConsentFlowUserGeography == MaxSdkBase.ConsentFlowUserGeography.Gdpr;
            }

            return false;
        }

        #endregion
    }
}
