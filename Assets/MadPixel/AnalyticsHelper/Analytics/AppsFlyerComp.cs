using AppsFlyerSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using MadPixel;
using MAXHelper;
using System.Globalization;

namespace MadPixelAnalytics {
    public class AppsFlyerComp : MonoBehaviour {
        [SerializeField] private bool bIsDebug;
        [SerializeField] private string monetizaionPubKey;

        #region Init

        public void Init() {
            if (bIsDebug) {
                AppsFlyer.setIsDebug(true);
            }

#if UNITY_ANDROID
            AppsFlyer.initSDK(MAXCustomSettings.APPSFLYER_SDK_KEY, null, this);
#else
            MAXCustomSettings customSettings = Resources.Load<MAXCustomSettings>("MAXCustomSettings");
            if (customSettings != null && !string.IsNullOrEmpty(customSettings.appsFlyerID_ios)) {
                AppsFlyer.initSDK(MAXCustomSettings.APPSFLYER_SDK_KEY, customSettings.appsFlyerID_ios, this);
            }
            else {
                Debug.LogError($"Can not find IOS APP ID for appsflyer ios!");
            }
#endif
            AppsFlyer.enableTCFDataCollection(true);

            AppsFlyer.startSDK();

            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += SetAdRevenue;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += SetAdRevenue;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += SetAdRevenue;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += SetAdRevenue;
        }

        private void OnDestroy() {
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= SetAdRevenue;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= SetAdRevenue;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= SetAdRevenue;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent -= SetAdRevenue;
        }

        #endregion

        #region AppsFlyer's Inner Stuff

        public void didFinishValidateReceipt(string result) {
            Debug.Log($"Purchase {result}");
        }

        public void didFinishValidateReceiptWithError(string error) {
            Debug.Log($"Purchase {error}");
        }

        public void onConversionDataSuccess(string conversionData) {
            AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
            // add deferred deeplink logic here
        }

        public void onConversionDataFail(string error) {
            AppsFlyer.AFLog("onConversionDataFail", error);
        }

        public void onAppOpenAttribution(string attributionData) {
            AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            Dictionary<string, object> attributionDataDictionary =
                AppsFlyer.CallbackStringToDictionary(attributionData);
            // add direct deeplink logic here
        }

        public void onAppOpenAttributionFailure(string error) {
            AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
        }

        #endregion

        #region Events

        public void VerificateAndSendPurchase(MPReceipt receipt) {
            string currency = receipt.Product.metadata.isoCurrencyCode;
            float revenue = (float)receipt.Product.metadata.localizedPrice;
            string revenueString = revenue.ToString(CultureInfo.InvariantCulture);

#if UNITY_ANDROID
            if (string.IsNullOrEmpty(monetizaionPubKey)) {
                return;
            }

            AppsFlyer.validateAndSendInAppPurchase(monetizaionPubKey,
                receipt.Signature, receipt.Data, revenueString, currency, null, this);
#endif

#if UNITY_IOS
            AppsFlyer.validateAndSendInAppPurchase(receipt.SKU, revenueString,  currency,  receipt.Product.transactionID,  null,  this);
#endif
        }

        public void OnFirstInApp() {
            AppsFlyer.sendEvent("Unique_PU", null);
        }

        public void OnRewardedShown(string Placement) {
            Dictionary<string, string> rvfinishEvent = new Dictionary<string, string>();
            rvfinishEvent.Add("Placement", Placement);
            AppsFlyer.sendEvent("RV_finish", rvfinishEvent);
        }

        public void OnInterShown() {
            AppsFlyer.sendEvent("IT_finish", null);
        }


        public void GameEnd(int Place, int Kills) {
#if UNITY_EDITOR
            Debug.Log($"Combat End {Place} {Kills}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("Place", Place.ToString());
            Event.Add("Kills", Kills.ToString());
            AppsFlyer.sendEvent("CombatEnd", Event);
        }

        public void GameStart() {
#if UNITY_EDITOR
            Debug.Log("Combat Start");
#endif
            AppsFlyer.sendEvent("CombatStart", null);
        }

        #endregion



        #region AdRevenue

        public static void SetAdRevenue(string adUnit, MaxSdkBase.AdInfo adInfo) {
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add("custom_AdUnitIdentifier", adInfo.AdUnitIdentifier);
            additionalParams.Add(AdRevenueScheme.AD_TYPE, adInfo.AdFormat);

            var logRevenue = new AFAdRevenueData(adInfo.NetworkName, MediationNetwork.ApplovinMax, "USD", adInfo.Revenue);
            AppsFlyer.logAdRevenue(logRevenue, additionalParams);
        }
        #endregion

    }
}
