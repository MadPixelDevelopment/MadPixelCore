using System.Collections.Generic;
using MadPixel;
using MAXHelper;
using UnityEngine;
using UnityEngine.Purchasing;

namespace MadPixelAnalytics {

    public class AnalyticsManager : MonoBehaviour {
        #region Fields
        public const string VERSION = "1.0.6";

        public bool bUseAutoInit = true;
        public bool bSubscribeOnStart = true;
        private AppMetricaComp AppMetricaComp;
        private AppsFlyerComp AppsFlyerComp;

        private bool bInitialized = false;

        #endregion


        #region Static

        protected static AnalyticsManager _instance;

        public static bool Exist {
            get { return (_instance != null); }
        }

        public static AnalyticsManager Instance {
            get {
                if (_instance == null) {
                    Debug.LogError("AnalyticsManager wasn't created yet!");

                    GameObject go = new GameObject();
                    go.name = "AnalyticsManager";
                    _instance = go.AddComponent(typeof(AnalyticsManager)) as AnalyticsManager;
                }

                return _instance;
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

        #endregion


        #region UnityEvents

        private void Awake() {
            if (_instance == null) {
                _instance = this;
                GameObject.DontDestroyOnLoad(this.gameObject);
            }
            else {
                GameObject.Destroy(gameObject);
                Debug.LogError($"Already have Analytics on scene!");
            }
        }

        private void Start() {
            if (bSubscribeOnStart) {
                SubscribeToAdsManager();
            }
        }

        private void OnDestroy() {
            if (AdsManager.Exist) {
                AdsManager.Instance.OnAdAvailable -= OnAdAvailable;
                AdsManager.Instance.OnAdShown -= OnAdWatched;
                AdsManager.Instance.OnAdDisplayError -= OnAdError;
                AdsManager.Instance.OnAdStarted -= OnAdStarted;
            }
        }

        #endregion


        #region Helpers

        public void SubscribeToAdsManager() {
            AdsManager Ads = FindObjectOfType<AdsManager>();
            if (Ads != null) {
                Ads.OnAdAvailable += OnAdAvailable;
                Ads.OnAdShown += OnAdWatched;
                Ads.OnAdDisplayError += OnAdError;
                Ads.OnAdStarted += OnAdStarted;
            }
        }

        public void Init() {
            if (bInitialized) {
                Debug.LogError($"[MadPixel] Analytics is trying to initialize for the second time. Check if there is a logic error!");
                return;
            }


            AppMetricaComp = this.GetComponent<AppMetricaComp>();
            if (AppMetricaComp) {
                AppMetricaComp.Init();
                Debug.Log("[MadPixel] AppMetrica is INITIALIZED!");
            }
            else {
                Debug.LogError("[MadPixel] AppMetrica is NOT INITIALIZED!");
            }

            AppsFlyerComp = this.GetComponent<AppsFlyerComp>();
            if (AppsFlyerComp) {
                AppsFlyerComp.Init();
                Debug.Log("[MadPixel] AppsFlyer is INITIALIZED!");
            }
            else {
                Debug.LogError("[MadPixel] AppsFlyer is NOT INITIALIZED!");
            }

            bInitialized = true;
        }

        #endregion



        #region Events

        #region Ads Related

        private static void OnAdStarted(AdInfo AdInfo) {
            if (Exist) {
                if (Instance.AppMetricaComp != null) {
                    Instance.AppMetricaComp.VideoAdStarted(AdInfo);
                }
                else {
                    Debug.LogError("[Mad Pixel] AppMetrica was not initialized!");
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Analytics Manager doesn't exist!");
            }
        }

        private static void OnAdError(MaxSdkBase.AdInfo MAXAdInfo, MaxSdkBase.ErrorInfo ErrorInfo, AdInfo AdInfo) {
            if (Exist) {
                if (Instance.AppMetricaComp != null) {
                    Instance.AppMetricaComp.VideoAdError(MAXAdInfo, ErrorInfo, AdInfo.Placement);
                } else {
                    Debug.LogError("[Mad Pixel] AppMetrica was not initialized!");
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Analytics Manager doesn't exist!");
            }
        }

        private static void OnAdWatched(AdInfo AdInfo) {
            if (Exist) {
                if (Instance.AppMetricaComp != null) {
                    Instance.AppMetricaComp.VideoAdWatched(AdInfo);
                } else {
                    Debug.LogError("[Mad Pixel] AppMetrica was not initialized!");
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Analytics Manager doesn't exist!");
            }
        }

        private static void OnAdAvailable(AdInfo AdInfo) {
            if (Exist) {
                if (Instance.AppMetricaComp != null) {
                    Instance.AppMetricaComp.VideoAdAvailable(AdInfo);
                } else {
                    Debug.LogError("[Mad Pixel] AppMetrica was not initialized!");
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Analytics Manager doesn't exist!");
            }
        }

        #endregion




        #region Purchase

        public static void PaymentSucceed(Product Product) {
            if (Exist) {
                if (Instance.AppMetricaComp != null && Instance.AppsFlyerComp != null) {
                    MPReceipt Receipt = ExtensionMethods.GetReceipt(Product);


                    if (Instance.AppMetricaComp != null) {
                        Instance.AppMetricaComp.PurchaseSucceed(Receipt);
                    }
                    else {
                        Debug.LogError("[Mad Pixel] AppMetrica was not initialized!");
                    }


                    if (Instance.AppsFlyerComp != null) {
                        if (PlayerPrefs.GetInt("FirstPurchaseWas", 0) == 0) {
                            Instance.AppsFlyerComp.OnFirstInApp();
                            PlayerPrefs.SetInt("FirstPurchaseWas", 1);
                        }

                        Instance.AppsFlyerComp.VerificateAndSendPurchase(Receipt);
                    }
                    else {
                        Debug.LogError("[Mad Pixel] AppsFlyer was not initialized!");
                    }

                }
                else {
                    Debug.LogError("[Mad Pixel] AppMetrica/AppsFlyer was not initialized!");
                }

            }
            else {
                Debug.LogError("[Mad Pixel] Analytics Manager doesn't exist!");
            }
        }

        #endregion


        #region Other Events
        public static void RateUs(int rateResult) {
            if (Exist) {
                if (Instance.AppMetricaComp != null) {
                    Instance.AppMetricaComp.RateUs(rateResult);
                } else {
                    Debug.LogError("[Mad Pixel] AppMetrica was not initialized!");
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Analytics Manager doesn't exist!");
            }
        }
        public static void ABTestGroup(string GroupID) {
            if (Exist) {
                if (Instance.AppMetricaComp != null) {
                    Instance.AppMetricaComp.ABTestInitMetricaAttributes(GroupID);
                } else {
                    Debug.LogError("[Mad Pixel] AppMetrica was not initialized!");
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Analytics Manager doesn't exist!");
            }
        }
        
        public static void CustomEvent(string eventName, Dictionary<string, object> parameters, bool bSendEventsBuffer = false) {
            if (Exist) {
                if (Instance.AppMetricaComp != null) {
                    Instance.AppMetricaComp.SendCustomEvent(eventName, parameters, bSendEventsBuffer);
                } else {
                    Debug.LogError("[Mad Pixel] AppMetrica was not initialized!");
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Analytics Manager doesn't exist!");
            }
        }
        #endregion

        #endregion
    }
}