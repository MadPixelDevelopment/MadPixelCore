#if UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Services.LevelPlay
{
    class EditorRewardedAd : IPlatformRewardedAd
    {
#pragma warning disable 0618
        string m_PrefabPath =>
            Directory.Exists("Packages/com.unity.services.levelplay")
                ? "Packages/com.unity.services.levelplay/Runtime/Platforms/Editor/EditorAds/Prefabs/MockRewardedEditorAd.prefab"
                : "Assets/LevelPlay/Runtime/Platforms/Editor/EditorAds/Prefabs/MockRewardedEditorAd.prefab";

        static GameObject m_AdGameObject;
        static RewardedPrefab m_AdPrefab;

        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdLoaded;
        public event Action<com.unity3d.mediation.LevelPlayAdError> OnAdLoadFailed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdDisplayed;
        public event Action<com.unity3d.mediation.LevelPlayAdDisplayInfoError> OnAdDisplayFailed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo, com.unity3d.mediation.LevelPlayReward> OnAdRewarded;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClicked;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClosed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdInfoChanged;
        public string AdId => "EditorRewardedMockAdId";
        public string AdUnitId { get; }

        internal EditorRewardedAd(string adUnitId)
        {
            AdUnitId = adUnitId;

            var mockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_AdGameObject = UnityEngine.Object.Instantiate(mockPrefab);
            m_AdPrefab = m_AdGameObject.GetComponent<RewardedPrefab>();
            UnityEngine.Object.DontDestroyOnLoad(m_AdGameObject);
            m_AdPrefab.m_Preview = false;

            SetupPrefabCallbacks();
        }

        void SetupPrefabCallbacks()
        {
            m_AdPrefab.OnAdLoaded += (args) => OnAdLoaded?.Invoke(args);
            m_AdPrefab.OnAdLoadFailed += (error) => OnAdLoadFailed?.Invoke(error);
            m_AdPrefab.OnAdDisplayed += (args) => OnAdDisplayed?.Invoke(args);
            m_AdPrefab.OnAdClosed += (args) => OnAdClosed?.Invoke(args);
            m_AdPrefab.OnAdClicked += (args) => OnAdClicked?.Invoke(args);
            m_AdPrefab.OnAdDisplayFailed += (infoError) => OnAdDisplayFailed?.Invoke(infoError);
            m_AdPrefab.OnAdInfoChanged += (args) => OnAdInfoChanged?.Invoke(args);
            m_AdPrefab.OnAdRewarded += (adInfo, adReward) => OnAdRewarded?.Invoke(adInfo, adReward);
        }

        public void LoadAd()
        {
            m_AdPrefab.LoadAd();
        }

        public void ShowAd(string placementName)
        {
            m_AdPrefab.ShowAd(placementName);
        }

        public bool IsAdReady()
        {
            return m_AdPrefab.IsAdReady();
        }

        public void Dispose()
        {
            Object.DestroyImmediate(m_AdGameObject);
            m_AdPrefab = null;
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log("Rewarded Ad object has been disposed in the Editor");
#endif
        }

        internal static bool IsPlacementCapped(string placementName)
        {
            LevelPlayLogger.Log("This API is not available on this platform.");
            return false;
        }
    }
}
#pragma warning restore 0618
#endif
