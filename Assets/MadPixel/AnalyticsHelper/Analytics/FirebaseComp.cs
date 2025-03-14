using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using MadPixel;
using MAXHelper;
using UnityEngine;

public class FirebaseComp : MonoBehaviour {
    private static bool bInitialized = false;



    #region Unity events
    void Start() {
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
                Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    private void OnDestroy() {
        IronSourceEvents.onImpressionDataReadyEvent -= LogAdPurchase;
    }
    #endregion

    private void InnerInit() {
        bInitialized = true;

        IronSourceEvents.onImpressionDataReadyEvent += LogAdPurchase;
    }

    public void LogAdPurchase(IronSourceImpressionData a_impressionData) {
        if (a_impressionData == null || a_impressionData.revenue == null || a_impressionData.revenue.Value <= 0) { return; }

        double revenue = a_impressionData.revenue.Value;
        if (revenue > 0 && bInitialized) {
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
}