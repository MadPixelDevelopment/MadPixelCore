using System;
using System.Collections;
using Unity.Services.LevelPlay;
using GoogleMobileAds.Ump.Api;
using MadPixel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace MAXHelper {

    [RequireComponent(typeof(LevelPlayComp))]
    public class AdsManager : MonoBehaviour {
        private const string version = "1.3.1";
        public enum EResultCode { OK = 0, NOT_LOADED, ADS_FREE, ON_COOLDOWN, ERROR }
        public enum EAdType { REWARDED, INTER, BANNER }

        #region Fields
        [FormerlySerializedAs("bInitializeOnStart")]
        [SerializeField] private bool m_initializeOnStart = true;

        [FormerlySerializedAs("CooldownBetweenInterstitials")]
        [SerializeField] private int m_cooldownBetweenInterstitials = 30;

        [SerializeField] private bool m_debugLogsOn;

        private bool m_canShowBanner = true;
        private bool m_intersOn = true;
        private bool m_hasInternet = true;
        private bool m_ready = false;

        private TermsAndPrivacyPolicyFlow m_termsFlow;
        private MAXCustomSettings m_madPixelSettings;
        private LevelPlayComp m_levelPlayComp;
        private AdInfo m_currentAdInfo;
        private float m_lastInterShown;
        private GameObject m_adsInstigatorObj;
        private UnityAction<bool> m_callbackPending;
        #endregion


        #region Events Declaration (Can be used for Analytics)

        public UnityAction e_onAdsManagerInitialized;

        public UnityAction<EAdType> OnNewAdLoaded;
        public UnityAction<LevelPlayAdDisplayInfoError, EAdType, string> OnAdDisplayError;
        public UnityAction<AdInfo> OnAdShown;
        public UnityAction<AdInfo> OnAdAvailable;
        public UnityAction<AdInfo> OnAdStarted;

        #endregion

        #region Static

        protected static AdsManager _instance;

        public static bool Exist {
            get { return (_instance != null); }
        }

        public static AdsManager Instance {
            get {
                if (_instance == null) {
                    Debug.LogError("[Mad Pixel] AdsManager wasn't created yet!");

                    GameObject go = new GameObject();
                    go.name = "AdsManager";
                    _instance = go.AddComponent(typeof(AdsManager)) as AdsManager;
                }

                return _instance;
            }
        }

        public static bool Ready() {
            if (Exist) {
                return Instance.m_ready;
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


        public static void Destroy(bool immediate = false) {
            if (_instance != null && _instance.gameObject != null) {
                if (immediate) {
                    DestroyImmediate(_instance.gameObject);
                }
                else {
                    GameObject.Destroy(_instance.gameObject);
                }
            }

            _instance = null;
        }

        public static string Version => version;

        #endregion

        #region Event Catchers
        private void OnTermsFlowAcceptedEvent(bool a_hasConsent) {
            if (m_debugLogsOn) {
                Debug.LogWarning($"ON TERMS AND ATT FLOW RESULT: {a_hasConsent}");
            }

            IronSource.Agent.setConsent(a_hasConsent);
#if UNITY_EDITOR
                OnFirebaseInit(a_hasConsent);
#else
                StartCoroutine(WaitForFirebaseInit(a_hasConsent));
#endif
        }

        private IEnumerator WaitForFirebaseInit(bool a_hasConsent){
            yield return (new WaitUntil(FirebaseComp.Initialized));
            OnFirebaseInit(a_hasConsent);
        }

        private void OnFirebaseInit(bool a_hasConsent){
#if !UNITY_EDITOR
            FirebaseComp.SetConsentValues(a_hasConsent);
#endif
            InitInternal();

            if (MadPixelAnalytics.AnalyticsManager.Exist) {
                if (MadPixelAnalytics.AnalyticsManager.Instance.m_useAutoInit) {
                    MadPixelAnalytics.AnalyticsManager.Instance.Init();
                }
            }
            else {
                Debug.LogError($"[MadPixel] Error in initializing Analytics! It doesn't exist on Scene!");
            }
        }

        private void LevelPlay_OnAdLoaded(EAdType a_type) {
            OnNewAdLoaded?.Invoke(a_type);
        }

        private void LevelPlay_OnFinishAds(bool IsFinished) {
            if (m_adsInstigatorObj != null) {
                m_adsInstigatorObj = null;
                m_callbackPending?.Invoke(IsFinished);
                m_callbackPending = null;
            }
            else {
                Debug.LogWarning("[Mad Pixel] Ads Instigator was destroyed or nulled");
            }

            if (m_currentAdInfo == null) {
                // NOTE: after rewarded ad dismissed
                return;
            }

            m_currentAdInfo.Availability = IsFinished ? "watched" : "canceled";
            OnAdShown?.Invoke(m_currentAdInfo);

            RestartInterCooldown();

            m_currentAdInfo = null;
            //NOTE: Temporary disable sounds - off
        }

        private void LevelPlay_OnInterDismissed() {
            if (m_adsInstigatorObj != null) {
                m_adsInstigatorObj = null;
                m_callbackPending?.Invoke(true);
                m_callbackPending = null;
            }
            else {
                Debug.LogWarning("[Mad Pixel] Ads Instigator was destroyed or nulled");
            }

            RestartInterCooldown();

            if (m_currentAdInfo != null) {
                OnAdShown?.Invoke(m_currentAdInfo);
            }

            m_currentAdInfo = null;
            //NOTE: Temporary disable sounds - off
        }

        private void LevelPlay_OnError(LevelPlayAdDisplayInfoError a_error, EAdType a_adType) {
            if (m_currentAdInfo != null) {
                OnAdDisplayError?.Invoke(a_error, a_adType, m_currentAdInfo.Placement);
            }
        }


        private void LevelPlay_OnBannerAdLoaded(LevelPlayAdInfo a_levelPlayAdInfo) {
            AdInfo BannerInfo = new AdInfo("banner", EAdType.BANNER, m_hasInternet); 
            OnAdAvailable?.Invoke(BannerInfo);
            if (m_canShowBanner) {
                OnAdStarted?.Invoke(BannerInfo);
                OnAdShown?.Invoke(BannerInfo);
            }
        }
        #endregion

        #region Unity Events

        private void Awake() {
            if (_instance == null) {
                _instance = this;
                GameObject.DontDestroyOnLoad(this.gameObject);
                m_levelPlayComp = GetComponent<LevelPlayComp>();
            }
            else {
                GameObject.Destroy(gameObject);
                Debug.LogError($"[Mad Pixel] Two AdsManagers at the same time!");
            }
        }

        private void Start(){
            if (m_initializeOnStart) {
                StartTermsAndPrivacyPolicyFlow();
            }
        }

        private void OnDestroy() {
            //if (AppLovin != null) {
            //    AppLovin.onFinishAdsEvent -= AppLovin_OnFinishAds;
            //    AppLovin.onInterDismissedEvent -= AppLovin_OnInterDismissed;
            //    AppLovin.onAdLoadedEvent -= AppLovin_OnAdLoaded;
            //    AppLovin.onErrorEvent -= AppLovin_OnError;

            //    AppLovin.onBannerRevenueEvent -= AppLovin_OnBannerRevenue;
            //    AppLovin.onBannerLoadedEvent -= AppLovin_OnBannerLoaded;
            //}
        }

        #endregion

        #region Public Static
        /// <param name="a_objectRef">Instigator gameobject</param>
        /// <summary>
        /// Shows a Rewarded Ad. Returns OK if the ad is starting to show, NOT_LOADED if Applovin has no loaded ad yet.
        /// </summary>
        public static EResultCode ShowRewarded(GameObject a_objectRef, UnityAction<bool> a_onFinishAds, string a_placement = "none") {
            if (Exist) {
                if (Instance.m_levelPlayComp.IsReady(EAdType.REWARDED)) {
                    Instance.SetCallback(a_onFinishAds, a_objectRef);
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

        public static EResultCode ShowInter(GameObject a_objectRef, UnityAction<bool> a_onAdDismissed, string a_placement = "none") {
#if UNITY_EDITOR
            a_onAdDismissed?.Invoke(true);
            return EResultCode.OK;
#endif
            if (Exist) {
                if (Instance.m_intersOn) {
                    if (Instance.IsCooldownElapsed()) {
                        if (Instance.m_levelPlayComp.IsReady(EAdType.INTER)) {
                            Instance.SetCallback(a_onAdDismissed, a_objectRef);
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
        public static EResultCode ShowInterForced(GameObject a_objectRef, UnityAction<bool> a_onAdDismissed, string a_placement = "none") {
            if (Exist) {
                if (Instance.m_levelPlayComp.IsReady(EAdType.INTER)) {
                    Instance.SetCallback(a_onAdDismissed, a_objectRef);
                    Instance.ShowAdInner(EAdType.INTER, a_placement);
                    return EResultCode.OK;
                }
                else {
                    return EResultCode.NOT_LOADED;
                }
            }
            return EResultCode.ERROR;
        }

        /// <summary>
        /// Returns TRUE if LevelPlay has a loaded ad ready to show
        /// </summary>
        public static bool HasLoadedAd(EAdType a_adType) {
            if (a_adType == EAdType.INTER) {
                return (Instance.m_intersOn && Instance.m_levelPlayComp.IsReady(a_adType) && Instance.IsCooldownElapsed());
            }

            //NOTE: rewarded ads can always be shown
            return Instance.m_levelPlayComp.IsReady(a_adType);
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
                    PlayerPrefs.SetInt("displayBannerOnLoad", 0);
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Ads Manager doesn't exist!");
            }
        }

        public static void ToggleBanner(bool a_show) {
#if UNITY_EDITOR
            return;
#endif
            if (Exist) {
                if (a_show && Instance.m_canShowBanner) {
                    Instance.m_levelPlayComp.ToggleBanner(true);
                }
                else {
                    Instance.m_levelPlayComp.ToggleBanner(false);
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Ads Manager doesn't exist!");
            }
        }


        /// <summary>
        /// Tries to show a Rewarded ad; if a Rewarded ad is not loaded, tries to show an Inter ad instead (ignoring COOLDOWN and ADSFREE conditions)
        /// </summary>
        public static bool ShowRewardedWithSubstitution(GameObject GO, UnityAction<bool> Callback, string Placement) {
            if (GO) {
                EResultCode Result = ShowRewarded(GO, Callback, Placement);
                if (Result == EResultCode.OK) {
                    return (true);
                }

                if (Result == EResultCode.NOT_LOADED) {
                    Result = ShowInterForced(GO, Callback, $"{Placement}_i");
                    if (Result == EResultCode.OK) {
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
        public static bool ShowInterWithSubstitution(GameObject GO, UnityAction<bool> Callback, string Placement) {
#if UNITY_EDITOR
            Callback?.Invoke(true);
            Debug.LogWarning($"[Mad Pixel] Editor dummy: INTER was shown here, placement = {Placement}");
            return true;
#endif
            if (GO) {
                EResultCode Result = ShowInter(GO, Callback, Placement);
                if (Result == EResultCode.OK) {
                    return (true);
                }

                if (Result == EResultCode.NOT_LOADED) {
                    Result = ShowRewarded(GO, Callback, $"{Placement}_r");
                    if (Result == EResultCode.OK) {
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
        private void StartTermsAndPrivacyPolicyFlow() {
            m_termsFlow = GetComponent<TermsAndPrivacyPolicyFlow>();
            m_termsFlow.e_onTermsAccepted += OnTermsFlowAcceptedEvent;
            m_termsFlow.StartFlow();
        }

        private void InitInternal() {
            m_lastInterShown = -m_cooldownBetweenInterstitials;

            m_madPixelSettings = Resources.Load<MAXCustomSettings>("MAXCustomSettings");
            m_levelPlayComp.Init(m_madPixelSettings);

            m_levelPlayComp.e_onFinishAds += LevelPlay_OnFinishAds;
            m_levelPlayComp.m_onAdLoaded += LevelPlay_OnAdLoaded;
            m_levelPlayComp.e_onInterDismissed += LevelPlay_OnInterDismissed;
            m_levelPlayComp.e_onDisplayAdError += LevelPlay_OnError;
            m_levelPlayComp.m_onBannerAdLoaded += LevelPlay_OnBannerAdLoaded;

            m_ready = true;

            e_onAdsManagerInitialized?.Invoke();
        }

        private void SetCallback(UnityAction<bool> a_callback, GameObject a_objectRef) {
            m_adsInstigatorObj = a_objectRef;
            m_callbackPending = a_callback;
        }

        private void ShowAdInner(EAdType a_adType, string a_placement) {
            m_currentAdInfo = new AdInfo(a_placement, a_adType);
            OnAdAvailable?.Invoke(m_currentAdInfo);
            OnAdStarted?.Invoke(m_currentAdInfo);
            // NOTE: Temporary Disable Sounds


#if UNITY_EDITOR
            if (a_adType == EAdType.REWARDED) {
                LevelPlay_OnFinishAds(true);
            }
            else if (a_adType == EAdType.INTER) {
                LevelPlay_OnInterDismissed();
            }

            return;
#endif

            if (a_adType == EAdType.REWARDED) {
                m_levelPlayComp.ShowRewarded();
            }
            else if (a_adType == EAdType.INTER) {
                m_levelPlayComp.ShowInter();
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

        private void OnPingComplete(bool bHasInternet) {
            if (m_currentAdInfo != null) {
                m_currentAdInfo.Availability = "not_available";
                m_currentAdInfo.HasInternet = bHasInternet;
                OnAdAvailable?.Invoke(m_currentAdInfo);
            }

            this.m_hasInternet = bHasInternet;
        }

        #endregion

        #region CMP (Google UMP) flow

        public static void ShowCMPFlow() {
            if (Ready()) {
                ConsentForm.ShowPrivacyOptionsForm((FormError formError) => {
                    Debug.Log($"Error: {formError}");
                });
            }
        }

        public static bool IsGDPR() {
            if (Ready()) {
                Debug.LogWarning($"[MadPixel] My status: {ConsentInformation.PrivacyOptionsRequirementStatus}");
                return ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
            }

            return false;
        }

        #endregion
    }
}
