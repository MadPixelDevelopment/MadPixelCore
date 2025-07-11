#if UNITY_ANDROID
using System;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace com.unity3d.mediation
{
    [Obsolete("This class will be made private in version 9.0.0.")]
    public class AndroidLevelPlaySdk : Unity.Services.LevelPlay.IUnityLevelPlayInitListener, IUnityLevelPlayImpressionDataListener
    {
        private static AndroidJavaObject _levelPlayBridge;
        static readonly string LevelPlayBridge = "com.ironsource.unity.androidbridge.LevelPlayBridge";

        public static event Action<LevelPlayConfiguration> OnInitSuccess;
        public static event Action<LevelPlayInitError> OnInitFailed;
        internal static event Action<LevelPlayImpressionData> OnImpressionDataReady;

        private static Unity.Services.LevelPlay.IUnityLevelPlayInitListener _listener;
        private static Unity.Services.LevelPlay.IUnityLevelPlayImpressionDataListener _impressionListener;

        public void onInitSuccess(string configuration)
        {
            OnInitSuccess?.Invoke(new LevelPlayConfiguration(configuration));
        }

        public void onInitFailed(string error)
        {
            OnInitFailed?.Invoke(new LevelPlayInitError(error));
        }

        public void onImpressionSuccess(string impressionData)
        {
            OnImpressionDataReady?.Invoke(new LevelPlayImpressionData(impressionData));
        }

        private AndroidLevelPlaySdk()
        {
            _listener = new UnityLevelPlayInitListener(this);
        }

        static AndroidLevelPlaySdk()
        {
            IronSourceEventsDispatcher.initialize();
        }

        private static AndroidJavaObject GetBridge()
        {
            if (_levelPlayBridge == null)
                using (var pluginClass = new AndroidJavaClass(LevelPlayBridge))
                    _levelPlayBridge = pluginClass.CallStatic<AndroidJavaObject>("getInstance");

            return _levelPlayBridge;
        }

        public static void Initialize(string appKey, string userId, LevelPlayAdFormat[] adFormats)
        {
            GetBridge().Call("setPluginData", "Unity", IronSource.pluginVersion(), IronSource.unityVersion());
            if (_listener == null)
            {
                _listener = new UnityLevelPlayInitListener(new AndroidLevelPlaySdk());
            }
            if (_impressionListener == null)
            {
                _impressionListener = new UnityLevelPlayImpressionDataListener(new AndroidLevelPlaySdk());
                GetBridge().Call("setUnityImpressionDataListener", _impressionListener);
            }
            GetBridge().Call("initialize", appKey, userId, GetAdFormatArray(adFormats), _listener);
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
                adFormatsArray[i] = adFormats[i].ToString();
            }
            return adFormatsArray;
        }

        internal static bool SetDynamicUserId(string dynamicUserId)
        {
            return GetBridge().Call<bool>("setDynamicUserId", dynamicUserId);
        }

        internal static void LaunchTestSuite()
        {
            GetBridge().Call("launchTestSuite");
        }

        internal static void SetAdaptersDebug(bool enabled)
        {
            GetBridge().Call("setAdaptersDebug", enabled);
        }

        internal static void ValidateIntegration()
        {
            GetBridge().Call("validateIntegration");
        }

        internal static void SetNetworkData(string networkKey, string networkData)
        {
            GetBridge().Call("setNetworkData", networkKey, networkData);
        }

        internal static void SetMetaData(string key, string value)
        {
            GetBridge().Call("setMetaData", key, value);
        }

        internal static void SetMetaData(string key, params string[] values)
        {
            GetBridge().Call("setMetaData", key, values);
        }

        internal static void SetConsent(bool consent)
        {
            GetBridge().Call("setConsent", consent);
        }

        internal static void SetSegment(LevelPlaySegment segment)
        {
            var dict = segment.GetSegmentAsDictionary();
            var json = IronSourceJSON.Json.Serialize(dict);
            GetBridge().Call("setSegment", json);
        }
    }
}
#endif
