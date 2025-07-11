#if UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Services.LevelPlay
{
    class EditorBannerAd : IPlatformBannerAd
    {
#pragma warning disable 0618
        GameObject m_AdGameObject;
        BannerPrefab m_AdPrefab;

        bool m_DisplayOnLoad;
        bool m_RespectSafeArea;

        string m_PrefabPath =>
            Directory.Exists("Packages/com.unity.services.levelplay")
            ? "Packages/com.unity.services.levelplay/Runtime/Platforms/Editor/EditorAds/Prefabs/MockBannerEditorAd.prefab"
            : "Assets/LevelPlay/Runtime/Platforms/Editor/EditorAds/Prefabs/MockBannerEditorAd.prefab";


        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdLoaded;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdError> OnAdLoadFailed;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdClicked;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdDisplayed;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdDisplayInfoError> OnAdDisplayFailed;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdExpanded;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdCollapsed;
        public event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdLeftApplication;

        public string AdId => "EditorPrefabMockAdId";
        public string AdUnitId { get; }
        public com.unity3d.mediation.LevelPlayAdSize AdSize { get; }
        public string PlacementName { get; }
        public com.unity3d.mediation.LevelPlayBannerPosition Position { get; }

        public EditorBannerAd(string adUnitId, com.unity3d.mediation.LevelPlayAdSize adSize,
                              com.unity3d.mediation.LevelPlayBannerPosition position, string placementName, bool displayOnLoad,
                              bool respectSafeArea)
        {
            AdUnitId = adUnitId;
            AdSize = adSize;
            Position = position;
            PlacementName = placementName;
            m_DisplayOnLoad = displayOnLoad;
            m_RespectSafeArea = respectSafeArea;

            Setup();
        }

        public EditorBannerAd(string adUnitId, Config config)
        {
            AdUnitId = adUnitId;
            AdSize = config.AdSize;
            Position = config.Position;
            PlacementName = config.PlacementName;
            m_DisplayOnLoad = config.DisplayOnLoad;
            m_RespectSafeArea = config.RespectSafeArea;

            Setup();
        }

        private void Setup()
        {
            var bannerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_PrefabPath);
            m_AdGameObject = Object.Instantiate(bannerPrefab);
            m_AdPrefab = m_AdGameObject.GetComponent<BannerPrefab>();
            Object.DontDestroyOnLoad(m_AdGameObject);

            m_AdPrefab.m_Preview = false;
            m_AdPrefab.m_BannerPosition = Position;
            m_AdPrefab.m_BannerAdSize = AdSize;

            SetupPrefabCallbacks();
        }

        void SetupPrefabCallbacks()
        {
            m_AdPrefab.OnAdLoaded += (sender, args) => OnAdLoaded?.Invoke(this, args);
            m_AdPrefab.OnAdLoadFailed += (sender, args) => OnAdLoadFailed?.Invoke(this, args);
            m_AdPrefab.OnAdClicked += (sender, args) => OnAdClicked?.Invoke(this, args);
            m_AdPrefab.OnAdDisplayed += (sender, args) => OnAdDisplayed?.Invoke(this, args);
            m_AdPrefab.OnAdDisplayFailed += (sender, args) => OnAdDisplayFailed?.Invoke(this, args);
            m_AdPrefab.OnAdExpanded += (sender, args) => OnAdExpanded?.Invoke(this, args);
            m_AdPrefab.OnAdCollapsed += (sender, args) => OnAdCollapsed?.Invoke(this, args);
            m_AdPrefab.OnAdLeftApplication += (sender, args) => OnAdLeftApplication?.Invoke(this, args);
        }

        public void Load()
        {
            m_AdPrefab.LoadAd();
            if (m_DisplayOnLoad)
            {
                m_AdPrefab.ShowAd();
            }
        }

        public void ShowAd()
        {
            m_AdPrefab.ShowAd();
        }

        public void DestroyAd()
        {
            Dispose();
        }

        public void HideAd()
        {
            m_AdPrefab.HideAd();
        }

        public void PauseAutoRefresh()
        {
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log("Pause Auto Refresh has been called in the Editor");
#endif
        }

        public void ResumeAutoRefresh()
        {
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log("Resume Auto Refresh has been called in the Editor");
#endif
        }

        public void Dispose()
        {
            Object.DestroyImmediate(m_AdGameObject);
            m_AdPrefab = null;
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log("Banner Ad object has been disposed in the Editor");
#endif
        }

        internal class Config : IPlatformBannerAd.IConfig
        {
            internal com.unity3d.mediation.LevelPlayAdSize AdSize { get; }
            internal com.unity3d.mediation.LevelPlayBannerPosition Position { get; }
            internal string PlacementName { get; }
            internal bool DisplayOnLoad { get; }
            internal bool RespectSafeArea { get; }

            private Config(
                com.unity3d.mediation.LevelPlayAdSize adSize,
                com.unity3d.mediation.LevelPlayBannerPosition position,
                string placementName,
                bool displayOnLoad,
                bool respectSafeArea)
            {
                AdSize = adSize;
                Position = position;
                PlacementName = placementName;
                DisplayOnLoad = displayOnLoad;
                RespectSafeArea = respectSafeArea;
            }

            internal class Builder : IPlatformBannerAd.IConfigBuilder
            {
                private com.unity3d.mediation.LevelPlayAdSize _adSize;
                private com.unity3d.mediation.LevelPlayBannerPosition _position;
                private string _placementName;
                private bool _displayOnLoad;
                private bool _respectSafeArea;
                private double _bidFloor;

                public void SetBidFloor(double bidFloor)
                {
                    _bidFloor = bidFloor;
                }

                public void SetSize(com.unity3d.mediation.LevelPlayAdSize size)
                {
                    _adSize = size;
                }

                public void SetPosition(com.unity3d.mediation.LevelPlayBannerPosition position)
                {
                    _position = position;
                }

                public void SetPlacementName(string placementName)
                {
                    _placementName = placementName;
                }

                public void SetDisplayOnLoad(bool displayOnLoad)
                {
                    _displayOnLoad = displayOnLoad;
                }

                public void SetRespectSafeArea(bool respectSafeArea)
                {
                    _respectSafeArea = respectSafeArea;
                }

                public IPlatformBannerAd.IConfig Build()
                {
                    return new Config(_adSize, _position, _placementName, _displayOnLoad, _respectSafeArea);
                }
            }
        }
    }
}
#pragma warning restore 0618
#endif
