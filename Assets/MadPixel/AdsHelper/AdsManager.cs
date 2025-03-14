using System.Collections;
using com.unity3d.mediation;
using GoogleMobileAds.Ump.Api;
using MadPixel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;


namespace MAXHelper {

    [RequireComponent(typeof(LevelPlayComp))]
    public class AdsManager : MonoBehaviour {
        private const string version = "1.2.9";
        public enum EResultCode { OK = 0, NOT_LOADED, ADS_FREE, ON_COOLDOWN, ERROR }
        public enum EAdType { REWARDED, INTER, BANNER }

        #region Fields

        [SerializeField] private bool bInitializeOnStart = true;
        [SerializeField] private int CooldownBetweenInterstitials = 30;

        private bool bCanShowBanner = true;
        private bool bIntersOn = true;
        private bool bHasInternet = true;

        private TermsAndATT terms;
        private MAXCustomSettings madPixelSettings;
        private LevelPlayComp LPComp;
        private AdInfo CurrentAdInfo;
        private float LastInterShown;
        private GameObject AdsInstigatorObj;
        private UnityAction<bool> CallbackPending;

        #endregion

        #region Events Declaration (Can be used for Analytics)

        public UnityAction OnAdsManagerInitialized;

        public bool bReady { get; private set; }

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
                return Instance.bReady;
            }
            return (false);
        }

        public static float CooldownLeft {
            get {
                if (Exist) {
                    return Instance.LastInterShown + Instance.CooldownBetweenInterstitials - Time.time;
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
        private void TermsOnEventOnTermsAccepted() {
            InitInternal();

            if (MadPixelAnalytics.AnalyticsManager.Exist) {
                if (MadPixelAnalytics.AnalyticsManager.Instance.bUseAutoInit) {
                    MadPixelAnalytics.AnalyticsManager.Instance.Init();
                }
            }
            else {
                Debug.LogError($"[MadPixel] Error in initializing Analytics! It doesn't exist on Scene!");
            }
        }

        private void AppLovin_OnAdLoaded(EAdType a_type) {
            OnNewAdLoaded?.Invoke(a_type);
        }

        private void AppLovin_OnFinishAds(bool IsFinished) {
            if (AdsInstigatorObj != null) {
                AdsInstigatorObj = null;
                CallbackPending?.Invoke(IsFinished);
                CallbackPending = null;
            }
            else {
                Debug.LogWarning("[Mad Pixel] Ads Instigator was destroyed or nulled");
            }

            if (CurrentAdInfo == null) {
                //Debug.LogWarning("[Mad Pixel] Current AdInfo was nulled before");
                // after rewarded ad dismissed
                return;
            }

            CurrentAdInfo.Availability = IsFinished ? "watched" : "canceled";
            OnAdShown?.Invoke(CurrentAdInfo);

            RestartInterCooldown();

            CurrentAdInfo = null;
            //Debug.LogWarning("[Mad Pixel] Current AdInfo set as null");
            //NOTE: Temporary disable sounds - off
        }

        private void AppLovin_OnInterDismissed() {
            if (AdsInstigatorObj != null) {
                AdsInstigatorObj = null;
                CallbackPending?.Invoke(true);
                CallbackPending = null;
            }
            else {
                //Debug.LogError("[Mad Pixel] Ads Instigator was destroyed or nulled");
            }

            RestartInterCooldown();

            if (CurrentAdInfo != null) {
                OnAdShown?.Invoke(CurrentAdInfo);
            }

            CurrentAdInfo = null;
            //NOTE: Temporary disable sounds - off
        }

        private void LevelPlay_OnError(LevelPlayAdDisplayInfoError a_error, EAdType a_adType) {
            if (CurrentAdInfo != null) {
                OnAdDisplayError?.Invoke(a_error, a_adType, CurrentAdInfo.Placement);
            }
        }


        private void LevelPlay_onBannerAdLoaded(LevelPlayAdInfo a_ironSourceAdInfo) {
            AdInfo BannerInfo = new AdInfo("banner", EAdType.BANNER, bHasInternet); 
            OnAdAvailable?.Invoke(BannerInfo);
            if (bCanShowBanner) {
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

                LPComp = GetComponent<LevelPlayComp>();

                if (bInitializeOnStart) {
                    TermsAndATTRoutine();
                }
            }
            else {
                GameObject.Destroy(gameObject);
                Debug.LogError($"[Mad Pixel] Two AdsManagers at the same time!");
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
        /// <param name="ObjectRef">Instigator gameobject</param>
        /// <summary>
        /// Shows a Rewarded As. Returns OK if the ad is starting to show, NOT_LOADED if Applovin has no loaded ad yet.
        /// </summary>
        public static EResultCode ShowRewarded(GameObject ObjectRef, UnityAction<bool> OnFinishAds, string Placement = "none") {
#if UNITY_EDITOR
            OnFinishAds?.Invoke(true);
            return EResultCode.OK;
#endif
            if (Exist) {
                if (Instance.LPComp.IsReady(EAdType.REWARDED)) {
                    Instance.SetCallback(OnFinishAds, ObjectRef);
                    Instance.ShowAdInner(EAdType.REWARDED, Placement);
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

        public static EResultCode ShowInter(string Placement = "none") {
            return ShowInter(null, null, Placement);
        }

        public static EResultCode ShowInter(GameObject ObjectRef, UnityAction<bool> OnAdDismissed, string Placement = "none") {
#if UNITY_EDITOR
            OnAdDismissed?.Invoke(true);
            return EResultCode.OK;
#endif
            if (Exist) {
                if (Instance.bIntersOn) {
                    if (Instance.IsCooldownElapsed()) {
                        if (Instance.LPComp.IsReady(EAdType.INTER)) {
                            Instance.SetCallback(OnAdDismissed, ObjectRef);
                            Instance.ShowAdInner(EAdType.INTER, Placement);
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
        public static EResultCode ShowInterForced(GameObject ObjectRef, UnityAction<bool> OnAdDismissed, string Placement = "none") {
            if (Exist) {
                if (Instance.LPComp.IsReady(EAdType.INTER)) {
                    Instance.SetCallback(OnAdDismissed, ObjectRef);
                    Instance.ShowAdInner(EAdType.INTER, Placement);
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
        public static bool HasLoadedAd(EAdType AdType) {
#if UNITY_EDITOR
            return true;
#endif
            return Instance.LPComp.IsReady(AdType);
        }


        /// <summary>
        /// Turns banners and inters off and prevents them from showing (this session only)
        /// Call this on AdsFree bought or on AdsFree checked at game start
        /// </summary>
        public static void CancelAllAds(bool bDisableInters = true, bool bDisableBanners = true) {
            if (Exist) {
                if (bDisableInters) {
                    Instance.bIntersOn = false;
                }
                if (bDisableBanners) {
                    Instance.bCanShowBanner = false;
                    ToggleBanner(false);
                    PlayerPrefs.SetInt("displayBannerOnLoad", 0);
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Ads Manager doesn't exist!");
            }
        }

        public static void ToggleBanner(bool bShow) {
#if UNITY_EDITOR
            return;
#endif
            if (Exist) {
                if (bShow && Instance.bCanShowBanner) {
                    Instance.LPComp.ToggleBanner(true);
                }
                else {
                    Instance.LPComp.ToggleBanner(false);
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
#if UNITY_EDITOR
            Callback?.Invoke(true);
            return true;
#endif
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
                return Instance.CooldownBetweenInterstitials;
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
        private void TermsAndATTRoutine() {
            terms = GetComponent<TermsAndATT>();
            terms.EventOnTermsAccepted += TermsOnEventOnTermsAccepted;
            terms.BeginPlay();
        }

        private void InitInternal() {
            LastInterShown = -CooldownBetweenInterstitials;

            madPixelSettings = Resources.Load<MAXCustomSettings>("MAXCustomSettings");
            LPComp.Init(madPixelSettings);
            LPComp.onFinishAdsEvent += AppLovin_OnFinishAds;
            LPComp.onAdLoadedEvent += AppLovin_OnAdLoaded;
            LPComp.onInterDismissedEvent += AppLovin_OnInterDismissed;
            LPComp.onErrorEvent += LevelPlay_OnError;

            LPComp.onBannerAdLoadedEvent += LevelPlay_onBannerAdLoaded;

            bReady = true;

            OnAdsManagerInitialized?.Invoke();
        }

        private void SetCallback(UnityAction<bool> Callback, GameObject objectRef) {
            AdsInstigatorObj = objectRef;
            CallbackPending = Callback;
        }

        private void ShowAdInner(EAdType AdType, string Placement) {
            CurrentAdInfo = new AdInfo(Placement, AdType);
            OnAdAvailable?.Invoke(CurrentAdInfo);
            OnAdStarted?.Invoke(CurrentAdInfo);
            // NOTE: Temporary Disable Sounds


            if (AdType == EAdType.REWARDED) {
                LPComp.ShowRewarded();
            }
            else if (AdType == EAdType.INTER) {
                LPComp.ShowInter();
            }
        }

        private bool IsCooldownElapsed() {
            return (Time.time - LastInterShown > CooldownBetweenInterstitials);
        }

        private void RestartInterCooldown() {
            if (CooldownBetweenInterstitials > 0) {
                LastInterShown = Time.time;
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
            if (CurrentAdInfo != null) {
                CurrentAdInfo.Availability = "not_available";
                CurrentAdInfo.HasInternet = bHasInternet;
                OnAdAvailable?.Invoke(CurrentAdInfo);
            }

            this.bHasInternet = bHasInternet;
        }

        #endregion

        #region CMP (Google UMP) flow

        public static void ShowCMPFlow() {
            if (Ready()) {
                //var cmpService = MaxSdk.CmpService;
                //cmpService.ShowCmpForExistingUser(error => {
                //    if (null == error) {
                //        // The CMP alert was shown successfully.
                //    }
                //    else {
                //        Debug.LogError(error);
                //    }
                //});
                ConsentForm.ShowPrivacyOptionsForm((FormError formError) => {
                    Debug.Log($"Error: {formError}");
                });
            }
        }

        public static bool IsGDPR() {
            if (Ready()) {
                //return MaxSdk.GetSdkConfiguration().ConsentFlowUserGeography == MaxSdkBase.ConsentFlowUserGeography.Gdpr;
                Debug.LogWarning($"[MadPixel] My status: {ConsentInformation.PrivacyOptionsRequirementStatus}");
                return ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
            }

            return false;
        }

        #endregion
    }
}
