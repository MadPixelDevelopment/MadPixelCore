using AppsFlyerSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using MadPixel;
using MAXHelper;

namespace MadPixelAnalytics {
    public class AppsFlyerComp : MonoBehaviour {
        [SerializeField] private string AppsFlyerDevKey;
        [SerializeField] private string AppID_IOs;
        [SerializeField] private bool bIsDebug;
        [SerializeField] private string monetizaionPubKey;

        #region Init

        public void Init() {
            if (bIsDebug) {
                AppsFlyer.setIsDebug(true);
            }


#if UNITY_ANDROID
            AppsFlyer.initSDK(AppsFlyerDevKey, null, this);
#else
            AppsFlyer.initSDK(AppsFlyerDevKey, AppID_IOs, this);
#endif
            AppsFlyer.startSDK();

            AppsFlyerAdRevenue.start();
            AppsFlyerAdRevenue.setIsDebug(bIsDebug);
            

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
            Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
            conversionDataDictionary.TryGetValue("media_source", out object MediaSource);
            if (MediaSource != null) {
                AdsManager.AddMediaSource((string)MediaSource);
            }
            else {
                AdsManager.AddMediaSource("Organic");
                //Debug.Log("MediaSource is null");
            }
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
            if (string.IsNullOrEmpty(monetizaionPubKey)) {
                return;
            }

            string currency = receipt.Product.metadata.isoCurrencyCode;
            float revenue = (float)receipt.Product.metadata.localizedPrice;

#if UNITY_ANDROID
            AppsFlyer.validateAndSendInAppPurchase(monetizaionPubKey,
                receipt.Signature, receipt.Data, revenue.ToString(), currency, null, this);
#endif

#if UNITY_IOS
            AppsFlyer.validateAndSendInAppPurchase(receipt.SKU,  revenue.ToString(),  currency,  receipt.Product.transactionID,  null,  this);
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
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("custom_AdUnitIdentifier", adInfo.AdUnitIdentifier);

            AppsFlyerAdRevenue.logAdRevenue(adInfo.NetworkName,
                AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax,
                adInfo.Revenue,
                "USD",
                dic);
        }
        #endregion

    }
}
