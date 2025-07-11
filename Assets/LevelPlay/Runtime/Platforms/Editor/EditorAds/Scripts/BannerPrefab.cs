using System;
using UnityEngine;

namespace Unity.Services.LevelPlay
{
    [ExecuteInEditMode]
    class BannerPrefab : AdPrefab
    {
#pragma warning disable 0618
#if UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
#pragma warning disable CS0067
        internal event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdLoaded;
        internal event EventHandler<com.unity3d.mediation.LevelPlayAdError> OnAdLoadFailed;
        internal event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdClicked;
        internal event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdDisplayed;
        internal event EventHandler<com.unity3d.mediation.LevelPlayAdDisplayInfoError> OnAdDisplayFailed;
        internal event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdExpanded;
        internal event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdCollapsed;
        internal event EventHandler<com.unity3d.mediation.LevelPlayAdInfo> OnAdLeftApplication;
#pragma warning restore CS0067

        static float m_BannerTextWidth = 534f;
        static float m_BannerTextHeight = 96f;

        internal com.unity3d.mediation.LevelPlayBannerPosition m_BannerPosition;
        internal com.unity3d.mediation.LevelPlayAdSize m_BannerAdSize = com.unity3d.mediation.LevelPlayAdSize.BANNER;

        [Tooltip("Mock Ad coordinates in pixels (only for preview purpose)")] [SerializeField]
        Vector2 m_BannerCoordinates = new Vector2(0, 0);

        [Tooltip("Position of Mock Banner Ad")] [SerializeField]
        Preset Position = Preset.BottomCenter;

        [Tooltip("Size of Mock Banner Ad")] [SerializeField]
        AdSize m_AdSize = AdSize.BANNER;

        [Tooltip("Mock Ad size in pixels (only for preview purpose)")] [SerializeField]
        Vector2 m_BannerSize;

        void Start()
        {
            SendInstantiateEvent("Banner");
        }

        void OnGUI()
        {
            GUI.depth = 0;
            if (!m_Preview)
            {
                return;
            }

            if (m_BackgroundTexture)
            {
                var bannerRect = GetBannerRect(m_BannerPosition);
                var textRect = new Rect(
                    bannerRect.x + bannerRect.width / 2 - m_BannerTextWidth / 2,
                    bannerRect.y + bannerRect.height / 2 - m_BannerTextHeight / 2,
                    m_BannerTextWidth,
                    m_BannerTextHeight);
                GUI.DrawTexture(bannerRect, m_BackgroundTexture, ScaleMode.StretchToFill);
                GUI.DrawTexture(textRect, m_LevelPlayLogo, ScaleMode.StretchToFill);
                if (GUI.Button(bannerRect, string.Empty, GUIStyle.none))
                {
                    OnAdClicked?.Invoke(this, m_MockAdInfo);
                }
            }
        }

        void OnValidate()
        {
            switch (Position)
            {
                case Preset.TopLeft:
                    m_BannerPosition = com.unity3d.mediation.LevelPlayBannerPosition.TopLeft;
                    break;
                case Preset.TopCenter:
                    m_BannerPosition = com.unity3d.mediation.LevelPlayBannerPosition.TopCenter;
                    break;
                case Preset.TopRight:
                    m_BannerPosition = com.unity3d.mediation.LevelPlayBannerPosition.TopRight;
                    break;
                case Preset.CenterLeft:
                    m_BannerPosition = com.unity3d.mediation.LevelPlayBannerPosition.CenterLeft;
                    break;
                case Preset.Center:
                    m_BannerPosition = com.unity3d.mediation.LevelPlayBannerPosition.Center;
                    break;
                case Preset.CenterRight:
                    m_BannerPosition = com.unity3d.mediation.LevelPlayBannerPosition.CenterRight;
                    break;
                case Preset.BottomLeft:
                    m_BannerPosition = com.unity3d.mediation.LevelPlayBannerPosition.BottomLeft;
                    break;
                case Preset.BottomCenter:
                    m_BannerPosition = com.unity3d.mediation.LevelPlayBannerPosition.BottomCenter;
                    break;
                case Preset.BottomRight:
                    m_BannerPosition = com.unity3d.mediation.LevelPlayBannerPosition.BottomRight;
                    break;
                case Preset.Custom:
                    m_BannerPosition = new com.unity3d.mediation.LevelPlayBannerPosition(m_BannerCoordinates);
                    break;
            }

            switch (m_AdSize)
            {
                case AdSize.LARGE:
                    m_BannerAdSize = com.unity3d.mediation.LevelPlayAdSize.LARGE;
                    break;
                case AdSize.BANNER:
                    m_BannerAdSize = com.unity3d.mediation.LevelPlayAdSize.BANNER;
                    break;
                case AdSize.ADAPTIVE:
                    m_BannerAdSize = com.unity3d.mediation.LevelPlayAdSize.CreateAdaptiveAdSize(Screen.width);
                    break;
                case AdSize.LEADERBOARD:
                    m_BannerAdSize = com.unity3d.mediation.LevelPlayAdSize.LEADERBOARD;
                    break;
                case AdSize.MEDIUM_RECTANGLE:
                    m_BannerAdSize = com.unity3d.mediation.LevelPlayAdSize.MEDIUM_RECTANGLE;
                    break;
                case AdSize.CUSTOM:
                    m_BannerAdSize =
                        com.unity3d.mediation.LevelPlayAdSize.CreateCustomBannerSize((int)m_BannerSize.x,
                            (int)m_BannerSize.y);
                    break;
            }
        }

        Rect GetBannerRect(com.unity3d.mediation.LevelPlayBannerPosition position)
        {
            switch (position.Description)
            {
                case "TopLeft":
                    m_BannerCoordinates = new Vector2(0, 0);
                    break;
                case "TopCenter":
                    m_BannerCoordinates = new Vector2(Screen.width / 2 - m_BannerSize.x / 2, 0);
                    break;
                case "TopRight":
                    m_BannerCoordinates = new Vector2(Screen.width - m_BannerSize.x, 0);
                    break;
                case "CenterLeft":
                    m_BannerCoordinates = new Vector2(0, Screen.height / 2 - m_BannerSize.y / 2);
                    break;
                case "Center":
                    m_BannerCoordinates = new Vector2(Screen.width / 2 - m_BannerSize.x / 2,
                        Screen.height / 2 - m_BannerSize.y / 2);
                    break;
                case "CenterRight":
                    m_BannerCoordinates =
                        new Vector2(Screen.width - m_BannerSize.x, Screen.height / 2 - m_BannerSize.y / 2);
                    break;
                case "BottomLeft":
                    m_BannerCoordinates = new Vector2(0, Screen.height - m_BannerSize.y);
                    break;
                case "BottomCenter":
                    m_BannerCoordinates =
                        new Vector2(Screen.width / 2 - m_BannerSize.x / 2, Screen.height - m_BannerSize.y);
                    break;
                case "BottomRight":
                    m_BannerCoordinates = new Vector2(Screen.width - m_BannerSize.x, Screen.height - m_BannerSize.y);
                    break;
                case "Custom":
                    m_BannerCoordinates = position.Position;
                    break;
            }

            m_BannerSize = new Vector2((Screen.dpi / 160) * m_BannerAdSize.Width,
                (Screen.dpi / 160) * m_BannerAdSize.Height);

            return new Rect(m_BannerCoordinates.x, m_BannerCoordinates.y, m_BannerSize.x,
                m_BannerSize.y);
        }

        enum Preset
        {
            TopLeft,
            TopCenter,
            TopRight,
            CenterLeft,
            Center,
            CenterRight,
            BottomLeft,
            BottomCenter,
            BottomRight,
            Custom
        }

        enum AdSize
        {
            BANNER,
            LARGE,
            MEDIUM_RECTANGLE,
            LEADERBOARD,
            ADAPTIVE,
            CUSTOM
        }

        internal void LoadAd()
        {
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            EditorLevelPlaySdk.CheckMockIsInitializedAndWarn();
            LevelPlayLogger.Log("Load Banner Ad has been called in the Editor");
#endif
            OnAdLoaded?.Invoke(this, m_MockAdInfo);
        }

        internal void ShowAd()
        {
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log("Show Banner Ad has been called in the Editor");
            EditorLevelPlaySdk.CheckMockIsInitializedAndWarn();
#endif
            m_Preview = true;
            OnAdDisplayed?.Invoke(this, m_MockAdInfo);
        }

        internal void HideAd()
        {
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log("Hide Banner Ad has been called in the Editor");
            EditorLevelPlaySdk.CheckMockIsInitializedAndWarn();
#endif
            m_Preview = false;
        }

#endif
#pragma warning restore 0618
    }
}
