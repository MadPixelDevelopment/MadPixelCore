using System.Collections.Generic;
using Unity.Services.LevelPlay;
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
        public void VideoAdWatched(AdInfo a_adInfo) {
            SendCustomEvent("video_ads_watch", GetAdAttributes(a_adInfo));
        }

        public void VideoAdAvailable(AdInfo a_adInfo) {
            SendCustomEvent("video_ads_available", GetAdAttributes(a_adInfo));
        }

        public void VideoAdStarted(AdInfo a_adInfo) {
            SendCustomEvent("video_ads_started", GetAdAttributes(a_adInfo));
        }


        public void VideoAdError(LevelPlayAdDisplayInfoError a_error, AdsManager.EAdType a_adType, string a_placement) {
            Dictionary<string, object> eventAttributes = new Dictionary<string, object>();

            string NetworkName = "unknown";
            if (a_error != null && a_error.DisplayLevelPlayAdInfo != null && !string.IsNullOrEmpty(a_error.DisplayLevelPlayAdInfo.AdNetwork)) {
                NetworkName = a_error.DisplayLevelPlayAdInfo.AdNetwork;
            }

            string Message = "NULL";
            string Code = "NULL";
            if (a_error != null) {
                Message = a_error.LevelPlayError.ErrorMessage;
                if (string.IsNullOrEmpty(Message)) {
                    Message = "NULL";
                }
                Code = a_error.LevelPlayError.ErrorCode.ToString();
            }

            eventAttributes.Add("network", NetworkName);
            eventAttributes.Add("error_message", Message);
            eventAttributes.Add("error_code", Code);
            eventAttributes.Add("placement", a_placement);
            SendCustomEvent("ad_display_error", eventAttributes);
        }


        #endregion


        public void RateUs(int rateResult) {
            Dictionary<string, object> eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("rate_result", rateResult);
            SendCustomEvent("rate_us", eventAttributes);
        }

        public void ABTestInitMetricaAttributes(string value) {
            UserProfile profile = new UserProfile().Apply(Attribute.CustomString("ab_test_group").WithValue(value));

            Io.AppMetrica.AppMetrica.ReportUserProfile(profile);
            Io.AppMetrica.AppMetrica.SendEventsBuffer();
        }



        #region Purchases
        public void PurchaseSucceed(MPReceipt a_receipt) {
            Dictionary<string, object> eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("inapp_id", a_receipt.Product.definition.storeSpecificId);
            eventAttributes.Add("currency", a_receipt.Product.metadata.isoCurrencyCode);
            eventAttributes.Add("price", (float)a_receipt.Product.metadata.localizedPrice);
            SendCustomEvent("payment_succeed", eventAttributes);

            HandlePurchase(a_receipt.Product, a_receipt.Data, a_receipt.Signature);
        }

        public void HandlePurchase(Product a_product, string a_data, string a_signature) {
            Revenue revenue = new Revenue(
                (long)a_product.metadata.localizedPrice, a_product.metadata.isoCurrencyCode);

            Revenue.Receipt Receipt = new Revenue.Receipt();
            Receipt.Signature = a_signature;
            Receipt.Data = a_data;

            revenue.ReceiptValue = Receipt;
            revenue.Quantity = 1;
            revenue.ProductID = a_product.definition.storeSpecificId;

#if UNITY_EDITOR
            return;
#else
            AppMetrica.ReportRevenue(revenue);
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
