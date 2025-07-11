using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Unity.Services.LevelPlay;
#if UNITY_IOS
using Unity.Advertisement.IosSupport; 
#endif
using UnityEngine;

namespace MadPixel {
    public class FirebaseComp : MonoBehaviour {
        #region Fields
        [SerializeField] private bool m_debugLogsOn;
        private static bool m_initialized = false;
        #endregion


        #region Public Static
        public static bool Initialized() {
            return (m_initialized);
        }

        public static void SetConsentValues(bool a_hasConsent) {
#if UNITY_EDITOR
            return;
#endif
            if (m_initialized) {
#if UNITY_IOS
                ATTrackingStatusBinding.AuthorizationTrackingStatus status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                if (status != ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED) { // NOTE: if ATT is Denied, consent is always False
                    ApplyConsentValues(false);
                    return;
                }
#endif

                if (AdsManager.IsGDPR()) {
                    // NOTE: we do not override UMP consent status
                }
                else {
                    ApplyConsentValues(a_hasConsent);
                }
            }
            else {
                Debug.LogError($"[Mad Pixel] Trying to set consent status but Firebase isn't initialized! Please fix it!");
            }
        }

        /// <summary>
        /// Sets True consent if it's non-GDPR region. Or False Consent when ATT is denied
        /// </summary>
        private static void ApplyConsentValues(bool a_hasConsent) {
            ConsentStatus statusValue = a_hasConsent ? ConsentStatus.Granted : ConsentStatus.Denied;
            var consentMap = new Dictionary<ConsentType, ConsentStatus> {
                { ConsentType.AdStorage, statusValue },
                //{ ConsentType.AnalyticsStorage, statusValue },
                { ConsentType.AdPersonalization, statusValue },
                { ConsentType.AdUserData, statusValue },
            };

            FirebaseAnalytics.SetConsent(consentMap);
        }

        #endregion


        #region Unity events
        void Start() {
#if UNITY_EDITOR
            m_initialized = true;
            return;
#endif
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available) {
                    FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    InnerInit();
                    if (m_debugLogsOn) {
                        Debug.Log("[Mad Pixel] Firebase init successful. Don't forget to check DebugView");
                    }
                }
                else {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    // Firebase Unity SDK is not safe to use here.
                }
            });
        }

        private void OnDestroy() {
            LevelPlay.OnImpressionDataReady -= LogAdPurchase;
        }
        #endregion



        #region Helpers
        private void InnerInit() {
            m_initialized = true;
            LevelPlay.OnImpressionDataReady += LogAdPurchase;
        }

        private void LogAdPurchase(LevelPlayImpressionData a_impressionData) {
            if (a_impressionData == null || a_impressionData.Revenue == null || a_impressionData.Revenue.Value <= 0) {
                return;
            }

            double revenue = a_impressionData.Revenue.Value;
            if (revenue > 0) {
                var impressionParameters = new[] {
                    new Firebase.Analytics.Parameter("ad_platform", "IronSource"),
                    new Firebase.Analytics.Parameter("ad_source", a_impressionData.AdNetwork),
                    new Firebase.Analytics.Parameter("ad_unit_name", a_impressionData.MediationAdUnitName),
                    new Firebase.Analytics.Parameter("ad_format", a_impressionData.AdFormat),
                    new Firebase.Analytics.Parameter("value", revenue),
                    new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
                };
                Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);

                if (m_debugLogsOn) {
                    Debug.Log($"[Mad Pixel] Firebase Revenue logged {revenue}");
                }
            }
        }

        #endregion
    } 
}