#if UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Services.LevelPlay
{
    class EditorInterstitialAd : IPlatformInterstitialAd
    {
#pragma warning disable 0618
        string m_PrefabPath =>
            Directory.Exists("Packages/com.unity.services.levelplay")
                ? "Packages/com.unity.services.levelplay/Runtime/Platforms/Editor/EditorAds/Prefabs/MockInterstitialEditorAd.prefab"
                : "Assets/LevelPlay/Runtime/Platforms/Editor/EditorAds/Prefabs/MockInterstitialEditorAd.prefab";

        GameObject m_AdGameObject;
        InterstitialPrefab m_AdPrefab;

        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdLoaded;
        public event Action<com.unity3d.mediation.LevelPlayAdError> OnAdLoadFailed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdDisplayed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClosed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClicked;
        public event Action<com.unity3d.mediation.LevelPlayAdDisplayInfoError> OnAdDisplayFailed;
        public event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdInfoChanged;
        public string AdId => "EditorInterstitialMockAdId";
        public string AdUnitId { get; }

        internal EditorInterstitialAd(string adUnitId)
        {
            AdUnitId = adUnitId;

            var mockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_AdGameObject = UnityEngine.Object.Instantiate(mockPrefab);
            m_AdPrefab = m_AdGameObject.GetComponent<InterstitialPrefab>();
            Object.DontDestroyOnLoad(m_AdGameObject);

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
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log("Interstitial Ad object has been disposed in the Editor");
#endif
        }

        internal static bool IsPlacementCapped(string placementName)
        {
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log("This API is not available on this platform.");
#endif
            return false;
        }
    }
}
#pragma warning restore 0618
#endif
