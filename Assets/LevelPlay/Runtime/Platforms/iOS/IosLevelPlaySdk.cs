#if UNITY_IOS
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace com.unity3d.mediation
{
    [Obsolete("This class will be made private in version 9.0.0.")]
    public class IosLevelPlaySdk : MonoBehaviour
    {
        public static event Action<LevelPlayConfiguration> OnInitSuccess;
        public static event Action<LevelPlayInitError> OnInitFailed;
        public static event Action<LevelPlayImpressionData> OnImpressionDataReady;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        static IosLevelPlaySdk()
        {
        }

        public static void Initialize(string appKey, string userId, LevelPlayAdFormat[] adFormats)
        {
            setPluginData("Unity", IronSource.pluginVersion(), IronSource.unityVersion());
            new GameObject("IosLevelPlaySdk", typeof(IosLevelPlaySdk)).GetComponent<IosLevelPlaySdk>();
            LPMInitialize(appKey, userId, GetAdFormatArray(adFormats));
        }

        public static void SetPauseGame(bool pause)
        {
            LPMSetPauseGame(pause);
        }

        internal static bool SetDynamicUserId(string dynamicUserId)
        {
            return LPMSetDynamicUserId(dynamicUserId);
        }

        internal static void ValidateIntegration()
        {
            LPMValidateIntegration();
        }
        internal static void LaunchTestSuite()
        {
            LPMLaunchTestSuite();
        }

        internal static void SetAdaptersDebug(bool enabled) {
            LPMSetAdaptersDebug(enabled);
        }

        internal static void SetNetworkData(string networkKey, string networkData) {
            LPMSetNetworkData(networkKey, networkData);
        }

        internal static void SetMetaData(string key, string value)
        {
            LPMSetMetaData(key, value);
        }

        internal static void SetMetaData(string key, params string[] values)
        {
            LPMSetMetaDataWithValues(key, values);
        }

        internal static void SetConsent(bool consent)
        {
            LPMSetConsent(consent);
        }

        internal static void SetSegment(LevelPlaySegment segment)
        {
            var dict = segment.GetSegmentAsDictionary();
            var json = IronSourceJSON.Json.Serialize(dict);
            LPMSetSegment(json);
        }

        private static string[] GetAdFormatArray(LevelPlayAdFormat[] adFormats)
        {
            if (adFormats == null)
            {
                return null;
            }
            var adFormatsArray = new string[adFormats.Length];
            for (var i = 0; i < adFormats.Length; i++)
            {
                var adFormat = adFormats[i];
                var adFormatString = adFormat switch
                {
                    LevelPlayAdFormat.BANNER => "banner",
                    LevelPlayAdFormat.INTERSTITIAL => "interstitial",
                    LevelPlayAdFormat.REWARDED => "rewardedvideo",
                    _ => ""
                };
                adFormatsArray[i] = adFormatString;
            }
            return adFormatsArray;
        }

        [DllImport("__Internal")]
        private static extern void LPMInitialize(string appKey, string userId, string[] adFormats);

        [DllImport("__Internal")]
        private static extern void setPluginData(string pluginType, string pluginVersion, string pluginFrameworkVersion);

        [DllImport("__Internal")]
        private static extern void LPMSetPauseGame(bool pause);

        public void OnInitializationSuccess(string configuration)
        {
            OnInitSuccess?.Invoke(new LevelPlayConfiguration(configuration));
        }

        public void OnInitializationFailed(string error)
        {
            OnInitFailed?.Invoke(new LevelPlayInitError(error));
        }

        public void onImpressionSuccess(string impressionData)
        {
            OnImpressionDataReady?.Invoke(new LevelPlayImpressionData(impressionData));
        }

        [DllImport("__Internal")]
        private static extern bool LPMSetDynamicUserId(string dynamicUserId);

        [DllImport("__Internal")]
        private static extern void LPMValidateIntegration();

        [DllImport("__Internal")]
        private static extern void LPMLaunchTestSuite();

        [DllImport("__Internal")]
        private static extern void LPMSetAdaptersDebug(bool enabled);

        [DllImport("__Internal")]
        private static extern void LPMSetNetworkData(string networkKey, string networkData);

        [DllImport("__Internal")]
        private static extern void LPMSetMetaData(string key, string value);

        [DllImport("__Internal")]
        private static extern void LPMSetMetaDataWithValues(string key, params string[] values);

        [DllImport("__Internal")]
        private static extern void LPMSetConsent(bool consent);

        [DllImport("__Internal")]
        private static extern void LPMSetSegment(string json);
    }

}
#endif
