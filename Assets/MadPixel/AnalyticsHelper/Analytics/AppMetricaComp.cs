using System.Collections.Generic;
using Io.AppMetrica;
using Io.AppMetrica.Profile;
using MadPixel;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.MiniJSON;

namespace MadPixelAnalytics {
    public class AppMetricaComp : MonoBehaviour {
        [SerializeField] private bool m_debugLogsOnDevice = false;
#if UNITY_EDITOR
        [SerializeField] private bool m_debugLogsInEditor = true;
#endif


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Activate() {
            MadPixelCustomSettings madPixelCustomSettings = AdsManager.LoadMadPixelCustomSettings();

            Io.AppMetrica.AppMetrica.Activate(new AppMetricaConfig(madPixelCustomSettings.appmetricaKey) {
                // copy settings from prefab
                CrashReporting = true, // prefab field 'Exceptions Reporting'
                SessionTimeout = 300, // prefab field 'Session Timeout Sec'
                LocationTracking = false, // prefab field 'Location Tracking'
                Logs = false, // prefab field 'Logs'
                FirstActivationAsUpdate = false, // prefab field 'Handle First Activation As Update'
                DataSendingEnabled = true, // prefab field 'Statistics Sending'
            });
        }



        #region Ads Related
        public void OnAdRevenuePaidEvent(string a_adUnit, MaxSdk.AdInfo a_adInfo) {
            AdRevenue adRevenue = new AdRevenue(a_adInfo.Revenue, "USD");
            Io.AppMetrica.AppMetrica.ReportAdRevenue(adRevenue);
        }

        public void VideoAdWatched(AdInfo AdInfo) {
            SendCustomEvent("video_ads_watch", GetAdAttributes(AdInfo));
        }

        public void VideoAdAvailable(AdInfo AdInfo) {
            SendCustomEvent("video_ads_available", GetAdAttributes(AdInfo));
        }

        public void VideoAdStarted(AdInfo AdInfo) {
            SendCustomEvent("video_ads_started", GetAdAttributes(AdInfo));
        }


        public void VideoAdError(MaxSdkBase.AdInfo adInfo, MaxSdkBase.ErrorInfo EInfo, string placement) {
            Dictionary<string, object> eventAttributes = new Dictionary<string, object>();

            string NetworkName = "unknown";
            if (adInfo != null && !string.IsNullOrEmpty(adInfo.NetworkName)) {
                NetworkName = adInfo.NetworkName;
            }

            string AdLoadFailureInfo = "NULL";
            string Message = "NULL";
            string Code = "NULL";
            if (EInfo != null) {
                if (!string.IsNullOrEmpty(EInfo.Message)) {
                    Message = EInfo.Message;
                }
                if (!string.IsNullOrEmpty(EInfo.AdLoadFailureInfo)) {
                    AdLoadFailureInfo = EInfo.AdLoadFailureInfo;
                }

                Code = EInfo.Code.ToString();
            }

            eventAttributes.Add("network", NetworkName);
            eventAttributes.Add("error_message", Message);
            eventAttributes.Add("error_code", Code);
            eventAttributes.Add("ad_load_failure_info", AdLoadFailureInfo);
            eventAttributes.Add("placement", placement);
            SendCustomEvent("ad_display_error", eventAttributes);
        }

        #endregion


        public void RateUs(int a_rateResult) {
            Dictionary<string, object> eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("rate_result", a_rateResult);
            SendCustomEvent("rate_us", eventAttributes);
        }

        public void ABTestInitMetricaAttributes(string a_value) {
            UserProfile profile = new UserProfile().Apply(Attribute.CustomString("ab_test_group").WithValue(a_value));

            Io.AppMetrica.AppMetrica.ReportUserProfile(profile);
            Io.AppMetrica.AppMetrica.SendEventsBuffer();
        }



        #region Purchases
        public void PurchaseSucceed(MPReceipt Receipt) {
            Dictionary<string, object> eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("inapp_id", Receipt.Product.definition.storeSpecificId);
            eventAttributes.Add("currency", Receipt.Product.metadata.isoCurrencyCode);
            eventAttributes.Add("price", (float)Receipt.Product.metadata.localizedPrice);
            SendCustomEvent("payment_succeed", eventAttributes);

            HandlePurchase(Receipt.Product, Receipt.Data, Receipt.Signature);
        }

        public void HandlePurchase(Product Product, string data, string signature) {
            Revenue Revenue = new Revenue(
                (long)Product.metadata.localizedPrice, Product.metadata.isoCurrencyCode);

            Revenue.Receipt Receipt = new Revenue.Receipt();
            Receipt.Signature = signature;
            Receipt.Data = data;

            Revenue.ReceiptValue = Receipt;
            Revenue.Quantity = 1;
            Revenue.ProductID = Product.definition.storeSpecificId;

#if UNITY_EDITOR
            return;
#else
            AppMetrica.ReportRevenue(Revenue);
#endif

        }
        #endregion


        #region Helpers

        public void SendCustomEvent(string a_eventName, Dictionary<string, object> a_parameters, bool a_sendEventsBuffer = false) {
            if (a_parameters == null) {
                a_parameters = new Dictionary<string, object>();
            }

            bool debugLog = m_debugLogsOnDevice;

#if UNITY_EDITOR
            debugLog = m_debugLogsInEditor;
#else
            AppMetrica.ReportEvent(a_eventName, a_parameters.toJson());
#endif

            if (a_sendEventsBuffer) {
                Io.AppMetrica.AppMetrica.SendEventsBuffer();
            }

            if (debugLog) {
                string eventParams = "";
                foreach (string key in a_parameters.Keys) {
                    var paramValue = a_parameters[key];
                    eventParams = eventParams + "\n" + key + ": " + (paramValue == null ? "null" : paramValue.ToString());
                }

                Debug.Log($"Event: {a_eventName} and params: {eventParams}");
            }
        }


        private Dictionary<string, object> GetAdAttributes(AdInfo a_adInfo) {
            Dictionary<string, object> eventAttributes = new Dictionary<string, object>();
            string adType = "interstitial";
            if (a_adInfo.AdType == AdsManager.EAdType.REWARDED) {
                adType = "rewarded";
            }
            else if (a_adInfo.AdType == AdsManager.EAdType.BANNER) {
                adType = "banner";
            }
            eventAttributes.Add("ad_type", adType);
            eventAttributes.Add("placement", a_adInfo.Placement);
            eventAttributes.Add("connection", a_adInfo.HasInternet ? 1 : 0);
            eventAttributes.Add("result", a_adInfo.Availability);

            return eventAttributes;
        }
        #endregion

    }
}
