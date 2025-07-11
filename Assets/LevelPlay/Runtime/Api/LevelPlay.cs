using System;
using System.Linq;
using com.unity3d.mediation;
using UnityEngine;

namespace com.unity3d.mediation
{
    /// <summary>
    /// Manages initialization and basic operations of the LevelPlay SDK.
    /// This class provides methods to initialize the SDK and handles global events for initialization success and failure.
    /// </summary>
    [Obsolete(
        "The namespace com.unity3d.mediation is deprecated. Use LevelPlay under the new namespace Unity.Services.LevelPlay.")]
    public class LevelPlay : Unity.Services.LevelPlay.LevelPlay
    {
    }
}

namespace Unity.Services.LevelPlay
{
    /// <summary>
    /// Manages initialization and basic operations of the LevelPlay SDK.
    /// This class provides methods to initialize the SDK and handles global events for initialization success and failure.
    /// </summary>
    public class LevelPlay
    {
        /// <summary>
        /// Returns the Unity Editor version.
        /// </summary>
        public static string UnityVersion => Application.unityVersion;

        /// <summary>
        /// Returns the LevelPlay Package version.
        /// </summary>
        public static string PluginVersion => Constants.AnnotatedPackageVersion;

#pragma warning disable 0618
        static event Action<com.unity3d.mediation.LevelPlayConfiguration> OnInitSuccessReceived;
        static event Action<com.unity3d.mediation.LevelPlayInitError> OnInitFailedReceived;
        static event Action<LevelPlayImpressionData> OnImpressionDataReadyReceived;

        /// <summary>
        /// Adds or removes event handlers for the SDK initialization success event.
        /// Ensures that the same handler cannot be added multiple times.
        /// </summary>
        public static event Action<com.unity3d.mediation.LevelPlayConfiguration> OnInitSuccess
        {
            add
            {
                if (OnInitSuccessReceived == null || !OnInitSuccessReceived.GetInvocationList().Contains(value))
                {
                    OnInitSuccessReceived += value;
                }
            }

            remove
            {
                if (OnInitSuccessReceived != null && OnInitSuccessReceived.GetInvocationList().Contains(value))
                {
                    OnInitSuccessReceived -= value;
                }
            }
        }

        /// <summary>
        /// Adds or removes event handlers for the SDK initialization failure event.
        /// Ensures that the same handler cannot be added multiple times.
        /// </summary>
        public static event Action<com.unity3d.mediation.LevelPlayInitError> OnInitFailed
        {
            add
            {
                if (OnInitFailedReceived == null || !OnInitFailedReceived.GetInvocationList().Contains(value))
                {
                    OnInitFailedReceived += value;
                }
            }

            remove
            {
                if (OnInitFailedReceived != null && OnInitFailedReceived.GetInvocationList().Contains(value))
                {
                    OnInitFailedReceived -= value;
                }
            }
        }

        /// <summary>
        /// Event triggered when an impression event occurs.
        /// This event is triggered on a background thread, not the Unity main thread.
        /// </summary>
        public static event Action<LevelPlayImpressionData> OnImpressionDataReady
        {
            add
            {
                if (OnImpressionDataReadyReceived == null || !OnImpressionDataReadyReceived.GetInvocationList().Contains(value))
                {
                    OnImpressionDataReadyReceived += value;
                }
            }

            remove
            {
                if (OnImpressionDataReadyReceived != null && OnImpressionDataReadyReceived.GetInvocationList().Contains(value))
                {
                    OnImpressionDataReadyReceived -= value;
                }
            }
        }

        /// <summary>
        /// Static constructor to hook up platform-specific initialization callbacks.
        /// </summary>
        static LevelPlay()
        {
#if !UNITY_IOS && !UNITY_ANDROID
            return;
#elif UNITY_EDITOR
            EditorLevelPlaySdk.OnInitSuccess += (configuration) =>
            {
                OnInitSuccessReceived?.Invoke(configuration);
            };
            EditorLevelPlaySdk.OnInitFailed += (error) =>
            {
                OnInitFailedReceived?.Invoke(error);
            };
            EditorLevelPlaySdk.OnImpressionDataReady += (impressionData) =>
            {
                OnImpressionDataReadyReceived?.Invoke(impressionData);
            };
#elif UNITY_ANDROID
            AndroidLevelPlaySdk.OnInitSuccess += (configuration) =>
            {
                OnInitSuccessReceived?.Invoke(configuration);
            };
            AndroidLevelPlaySdk.OnInitFailed += (error) =>
            {
                OnInitFailedReceived?.Invoke(error);
            };
            AndroidLevelPlaySdk.OnImpressionDataReady += (impressionData) =>
            {
                OnImpressionDataReadyReceived?.Invoke(impressionData);
            };
#elif UNITY_IOS
            IosLevelPlaySdk.OnInitSuccess += (configuration) =>
            {
                OnInitSuccessReceived?.Invoke(configuration);
            };
            IosLevelPlaySdk.OnInitFailed += (error) =>
            {
                OnInitFailedReceived?.Invoke(error);
            };
            IosLevelPlaySdk.OnImpressionDataReady += (impressionData) =>
            {
                OnImpressionDataReadyReceived?.Invoke(impressionData);
            };
#endif
        }

        /// <summary>
        /// Initializes the LevelPlay SDK with the specified app key and optional user ID and ad format list.
        /// </summary>
        /// <param name="appKey">The application key for the SDK.</param>
        /// <param name="userId">Optional user identifier for use within the SDK.</param>
        /// <param name="adFormats">Optional array of ad formats to initialize.</param>
        public static void Init(string appKey, string userId = null,
            com.unity3d.mediation.LevelPlayAdFormat[] adFormats = null)
        {
#if !UNITY_IOS && !UNITY_ANDROID
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log("LevelPlay is unsupported in this platform");
#endif
            return;
#elif UNITY_EDITOR
            EditorLevelPlaySdk.Initialize();
#elif UNITY_ANDROID
            AndroidLevelPlaySdk.Initialize(appKey, userId, adFormats);
#elif UNITY_IOS
            IosLevelPlaySdk.Initialize(appKey, userId, adFormats);
#endif
        }

        /// <summary>
        /// When setting your PauseGame status to true, all your Unity 3D game activities will be paused (Except the ad callbacks).
        /// The game activity will be resumed automatically when the ad is closed.
        /// You should call the setPauseGame once in your session, before or after initializing the ironSource SDK,
        /// as it affects all ads (Rewarded Video and Interstitial ads) in the session.
        /// </summary>
        /// <param name="pause">Is the game paused</param>
        public static void SetPauseGame(bool pause)
        {
#if UNITY_IOS && !UNITY_EDITOR
            IosLevelPlaySdk.SetPauseGame(pause);
#endif
        }

        /// <summary>
        /// Sets a dynamic user ID that can be changed through the session and will be used in server to server rewarded
        /// ad callbacks.
        /// This parameter helps verify AdRewarded transactions and must be set before calling ShowRewardedVideo.
        /// </summary>
        /// <param name="dynamicUserId">The ID to be set</param>
        /// <returns>Was the dynamic user ID set successfully</returns>
        public static bool SetDynamicUserId(string dynamicUserId)
        {
#if UNITY_EDITOR
            return false;
#elif UNITY_ANDROID
            return AndroidLevelPlaySdk.SetDynamicUserId(dynamicUserId);
#elif UNITY_IOS
            return IosLevelPlaySdk.SetDynamicUserId(dynamicUserId);
#else
            return false;
#endif
        }

        /// <summary>
        /// Runs the integration validation.
        /// </summary>
        public static void ValidateIntegration()
        {
#if UNITY_EDITOR
            return;
#elif UNITY_ANDROID
            AndroidLevelPlaySdk.ValidateIntegration();
#elif UNITY_IOS
            IosLevelPlaySdk.ValidateIntegration();
#endif
        }

        /// <summary>
        /// Launches the Test Suite. Mediation SDK must be initialized before calling this method.
        /// </summary>
        public static void LaunchTestSuite()
        {
#if UNITY_EDITOR
            return;
#elif UNITY_ANDROID
            AndroidLevelPlaySdk.LaunchTestSuite();
#elif UNITY_IOS
            IosLevelPlaySdk.LaunchTestSuite();
#endif
        }

        /// <summary>
        /// Enables or disables adapters debug info.
        /// </summary>
        /// <param name="enabled">Is adapters debug info enabled</param>
        public static void SetAdaptersDebug(bool enabled)
        {
#if UNITY_EDITOR
            return;
#elif UNITY_ANDROID
            AndroidLevelPlaySdk.SetAdaptersDebug(enabled);
#elif UNITY_IOS
            IosLevelPlaySdk.SetAdaptersDebug(enabled);
#endif
        }

        /// <summary>
        /// Set custom network data.
        /// </summary>
        /// <param name="networkKey">The attribute key</param>
        /// <param name="networkData">The attribute value</param>
        public static void SetNetworkData(string networkKey, string networkData)
        {
#if UNITY_EDITOR
            return;
#elif UNITY_ANDROID
            AndroidLevelPlaySdk.SetNetworkData(networkKey, networkData);
#elif UNITY_IOS
            IosLevelPlaySdk.SetNetworkData(networkKey, networkData);
#endif
        }

        /// <summary>
        /// Allows setting extra flags, for example "do_not_sell" to allow or disallow selling or sharing personal information.
        /// </summary>
        /// <param name="key">The flag to set</param>
        /// <param name="value">The value for the flag</param>
        public static void SetMetaData(string key, string value)
        {
#if UNITY_EDITOR
            return;
#elif UNITY_ANDROID
            AndroidLevelPlaySdk.SetMetaData(key, value);
#elif UNITY_IOS
            IosLevelPlaySdk.SetMetaData(key, value);
#endif
        }

        /// <summary>
        /// Allows setting extra flags, for example "do_not_sell" to allow or disallow selling or sharing personal information.
        /// </summary>
        /// <param name="key">The flag to set</param>
        /// <param name="values">The values for the flag</param>
        public static void SetMetaData(string key, params string[] values)
        {
#if UNITY_EDITOR
            return;
#elif UNITY_ANDROID
            AndroidLevelPlaySdk.SetMetaData(key, values);
#elif UNITY_IOS
            IosLevelPlaySdk.SetMetaData(key, values);
#endif
        }

        /// <summary>
        /// Set user's GDPR consent.
        /// </summary>
        /// <param name="consent">Whether the user has granted consent</param>
        public static void SetConsent(bool consent)
        {
#if UNITY_EDITOR
            return;
#elif UNITY_ANDROID
            AndroidLevelPlaySdk.SetConsent(consent);
#elif UNITY_IOS
            IosLevelPlaySdk.SetConsent(consent);
#endif
        }

        /// <summary>
        /// Set the segment a user belongs to.
        /// </summary>
        /// <param name="segment">Segment information for the current user</param>
        public static void SetSegment(LevelPlaySegment segment)
        {
#if UNITY_EDITOR
            return;
#elif UNITY_ANDROID
            AndroidLevelPlaySdk.SetSegment(segment);
#elif UNITY_IOS
            IosLevelPlaySdk.SetSegment(segment);
#endif
        }
    }
#pragma warning restore 0618
}
