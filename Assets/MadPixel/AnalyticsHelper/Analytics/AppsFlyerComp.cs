using AppsFlyerSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using MadPixel;
using System.Globalization;
using AppsFlyerConnector;

namespace MadPixelAnalytics {
    public class AppsFlyerComp : MonoBehaviour {
        #region Fields
        [SerializeField] private bool m_usePurchaseConnector;
        [SerializeField] private string monetizaionPubKey;
        [Space]
        [Header("Turn Debug OFF for production builds")]
        [SerializeField] private bool m_debugMode;
        #endregion

        #region Properties
        public bool UseInappConnector => m_usePurchaseConnector;
        #endregion


        #region Init

        public void Init() {
            AppsFlyer.setIsDebug(m_debugMode);

#if UNITY_ANDROID
            AppsFlyer.initSDK(MadPixelCustomSettings.APPSFLYER_SDK_KEY, null, this);
#else
            MadPixelCustomSettings customSettings = AdsManager.LoadMadPixelCustomSettings();
            if (customSettings != null && !string.IsNullOrEmpty(customSettings.appsFlyerID_ios)) {
                AppsFlyer.initSDK(MadPixelCustomSettings.APPSFLYER_SDK_KEY, customSettings.appsFlyerID_ios, this);
            }
            else {
                Debug.LogError($"Can not find IOS APP ID for appsflyer ios!");
            }
#endif
            AppsFlyer.enableTCFDataCollection(true);


            // Purchase connector implementation 
            if (m_usePurchaseConnector) {
                AppsFlyerPurchaseConnector.init(this, AppsFlyerConnector.Store.GOOGLE);
                AppsFlyerPurchaseConnector.setIsSandbox(false);
                AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(
                    AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions,
                    AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases);
                AppsFlyerPurchaseConnector.build();

                AppsFlyerPurchaseConnector.startObservingTransactions();
            }

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

        public void VerificateAndSendPurchase(MPReceipt a_receipt) {
            string currency = a_receipt.product.metadata.isoCurrencyCode;
            float revenue = (float)a_receipt.product.metadata.localizedPrice;
            string revenueString = revenue.ToString(CultureInfo.InvariantCulture);

#if UNITY_ANDROID
            if (string.IsNullOrEmpty(monetizaionPubKey)) {
                return;
            }

            AppsFlyer.validateAndSendInAppPurchase(monetizaionPubKey,
                a_receipt.signature, a_receipt.data, revenueString, currency, null, this);
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
        #endregion



        #region AdRevenue

        public static void SetAdRevenue(string a_adUnit, MaxSdkBase.AdInfo a_adInfo) {
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add("custom_AdUnitIdentifier", a_adInfo.AdUnitIdentifier);
            additionalParams.Add(AdRevenueScheme.AD_TYPE, a_adInfo.AdFormat);

            var logRevenue = new AFAdRevenueData(a_adInfo.NetworkName, MediationNetwork.ApplovinMax, "USD", a_adInfo.Revenue);
            AppsFlyer.logAdRevenue(logRevenue, additionalParams);
        }
        #endregion

    }
}
