using System.Collections.Generic;
using Unity.Services.LevelPlay;
using MadPixel;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Serialization;

namespace MadPixelAnalytics {

    public class AnalyticsManager : MonoBehaviour {
        #region Fields
        public const string VERSION = "1.0.9";

        [FormerlySerializedAs("bUseAutoInit")]
        public bool m_useAutoInit = true;
        [FormerlySerializedAs("bSubscribeOnStart")]
        public bool m_subscribeToAdsOnStart = true;

        private AppMetricaComp m_appMetricaComp;
        private AppsFlyerComp m_appsFlyerComp;

        private bool m_initialized = false;

        #endregion


        #region Static

        protected static AnalyticsManager m_instance;

        public static bool Exist {
            get { return (m_instance != null); }
        }

        public static AnalyticsManager Instance {
            get {
                if (m_instance == null) {
                    Debug.LogError("AnalyticsManager wasn't created yet!");

                    GameObject go = new GameObject();
                    go.name = "AnalyticsManager";
                    m_instance = go.AddComponent(typeof(AnalyticsManager)) as AnalyticsManager;
                }

                return m_instance;
            }
        }

        public static void Destroy(bool immediate = false) {
            if (m_instance != null && m_instance.gameObject != null) {
                if (immediate) {
                    DestroyImmediate(m_instance.gameObject);
                }
                else {
                    GameObject.Destroy(m_instance.gameObject);
                }
            }

            m_instance = null;
        }

        #endregion


        #region UnityEvents

        private void Awake() {
            if (m_instance == null) {
                m_instance = this;
                GameObject.DontDestroyOnLoad(this.gameObject);
            }
            else {
                GameObject.Destroy(gameObject);
                Debug.LogError($"Already have Analytics on scene!");
            }
        }

        private void Start() {
            if (m_subscribeToAdsOnStart) {
                SubscribeToAdsManager();
            }
        }

        private void OnDestroy() {
            if (AdsManager.Exist) {
                AdsManager.Instance.e_onAdAvailable -= OnAdAvailable;
                AdsManager.Instance.e_onAdShown -= OnAdWatched;
                AdsManager.Instance.e_onAdDisplayError -= OnAdError;
                AdsManager.Instance.e_onAdStarted -= OnAdStarted;
            }
        }

        #endregion


        #region Helpers

        public void SubscribeToAdsManager() {
            AdsManager Ads = FindFirstObjectByType<AdsManager>();
            if (Ads != null) {
                Ads.e_onAdAvailable += OnAdAvailable;
                Ads.e_onAdShown += OnAdWatched;
                Ads.e_onAdDisplayError += OnAdError;
                Ads.e_onAdStarted += OnAdStarted;
            }
        }

        public void Init() {
            if (m_initialized) {
                Debug.LogError($"[MadPixel] Analytics is trying to initialize for the second time. Check if there is a logic error!");
                return;
            }


            m_appMetricaComp = this.GetComponent<AppMetricaComp>();
            if (m_appMetricaComp) {
                Debug.Log("[MadPixel] AppMetrica is INITIALIZED!");
            }
            else {
                Debug.LogError("[MadPixel] AppMetrica is NOT INITIALIZED!");
            }

            m_appsFlyerComp = this.GetComponent<AppsFlyerComp>();
            if (m_appsFlyerComp) {
                m_appsFlyerComp.Init();
                Debug.Log("[MadPixel] AppsFlyer is INITIALIZED!");
            }
            else {
                Debug.LogError("[MadPixel] AppsFlyer is NOT INITIALIZED!");
            }

            m_initialized = true;
        }

        #endregion



        #region Events

        #region Ads Related

        private static void OnAdStarted(AdInfo AdInfo) {
            if (Exist) {
                if (Instance.m_appMetricaComp != null) {
                    Instance.m_appMetricaComp.VideoAdStarted(AdInfo);
                }
                else {
                    Debug.LogError("[Mad Pixel] AppMetrica was not initialized!");
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Analytics Manager doesn't exist!");
            }
        }

        private static void OnAdError(LevelPlayAdDisplayInfoError a_error, AdsManager.EAdType a_adType, string a_placement) {
            if (Exist) {
                if (Instance.m_appMetricaComp != null) {
                    Instance.m_appMetricaComp.VideoAdError(a_error, a_adType, a_placement);
                }
                else {
                    Debug.LogError("[Mad Pixel] AppMetrica was not initialized!");
                }
            }
            else {
                Debug.LogError("[Mad Pixel] Analytics Manager doesn't exist!");
            }
        }

        private static void OnAdWatched(AdInfo AdInfo) {
            if (Exist) {
                if (Instance.m_appMetricaComp != null) {
                    Instance.m_appMetricaComp.VideoAdWatched(AdInfo);
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
                if (Instance.m_appMetricaComp != null) {
                    Instance.m_appMetricaComp.VideoAdAvailable(AdInfo);
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

        public static void PaymentSucceed(Product a_product) {
            if (Exist) {
                if (Instance.m_appMetricaComp != null && Instance.m_appsFlyerComp != null) {
                    MPReceipt receipt = ExtensionMethods.GetReceipt(a_product);


                    if (Instance.m_appMetricaComp != null) {
                        Instance.m_appMetricaComp.PurchaseSucceed(receipt);
                    }
                    else {
                        Debug.LogError("[Mad Pixel] AppMetrica was not initialized!");
                    }


                    if (Instance.m_appsFlyerComp != null) {
                        if (PlayerPrefs.GetInt("FirstPurchaseWas", 0) == 0) {
                            Instance.m_appsFlyerComp.OnFirstInApp();
                            PlayerPrefs.SetInt("FirstPurchaseWas", 1);
                        }

                        if (!Instance.m_appsFlyerComp.UseInappConnector) {
                            Instance.m_appsFlyerComp.VerificateAndSendPurchase(receipt);
                        }
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
                if (Instance.m_appMetricaComp != null) {
                    Instance.m_appMetricaComp.RateUs(rateResult);
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
                if (Instance.m_appMetricaComp != null) {
                    Instance.m_appMetricaComp.SendCustomEvent(eventName, parameters, bSendEventsBuffer);
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