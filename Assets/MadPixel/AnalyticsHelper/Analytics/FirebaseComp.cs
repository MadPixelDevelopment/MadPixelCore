using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
#if UNITY_IOS
using Unity.Advertisement.IosSupport; 
#endif
using UnityEngine;

namespace MadPixel {
    public class FirebaseComp : MonoBehaviour {
        #region Fields
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
                Debug.LogError($"Trying to set consent status but Firebase isn't initialized! Please fix it!");
            }
        }

        /// <summary>
        /// Sets True consent if it's non-GDPR region. Or False Consent when ATT is denied
        /// </summary>
        private static void ApplyConsentValues(bool a_hasConsent) {
            ConsentStatus statusValue = a_hasConsent ? ConsentStatus.Granted : ConsentStatus.Denied;
            var consentMap = new Dictionary<ConsentType, ConsentStatus> {
            { ConsentType.AdStorage, statusValue },
            { ConsentType.AnalyticsStorage, statusValue },
            { ConsentType.AdPersonalization, statusValue },
            { ConsentType.AdUserData, statusValue },
        };
            // Set the consent status
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
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    InnerInit();

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                }
                else {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    // Firebase Unity SDK is not safe to use here.
                }
            });
        }

        private void OnDestroy() {
            IronSourceEvents.onImpressionDataReadyEvent -= LogAdPurchase;
        }
        #endregion



        #region Helpers
        private void InnerInit() {
            m_initialized = true;
            IronSourceEvents.onImpressionDataReadyEvent += LogAdPurchase;
        }

        private void LogAdPurchase(IronSourceImpressionData a_impressionData) {
            if (a_impressionData == null || a_impressionData.revenue == null || a_impressionData.revenue.Value <= 0) { return; }

            double revenue = a_impressionData.revenue.Value;
            if (revenue > 0) {
                var impressionParameters = new[] {
                new Firebase.Analytics.Parameter("ad_platform", "IronSource"),
                new Firebase.Analytics.Parameter("ad_source", a_impressionData.adNetwork),
                new Firebase.Analytics.Parameter("ad_unit_name", a_impressionData.mediationAdUnitName),
                new Firebase.Analytics.Parameter("ad_format", a_impressionData.adFormat),
                new Firebase.Analytics.Parameter("value", revenue),
                new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
            };

                Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);

                //Debug.Log($"[MadPixel] Firebase Revenue logged {revenue}");
            }
        }
        #endregion
    } 
}