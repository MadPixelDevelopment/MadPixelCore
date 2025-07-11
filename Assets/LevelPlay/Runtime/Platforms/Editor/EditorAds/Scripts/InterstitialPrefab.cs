using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Unity.Services.LevelPlay
{
    [ExecuteInEditMode]
    class InterstitialPrefab : AdPrefab
    {
#pragma warning disable 0618
#if UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
#pragma warning disable CS0067
        internal event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdLoaded;
        internal event Action<com.unity3d.mediation.LevelPlayAdError> OnAdLoadFailed;
        internal event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdDisplayed;
        internal event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClosed;
        internal event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdClicked;
        internal event Action<com.unity3d.mediation.LevelPlayAdDisplayInfoError> OnAdDisplayFailed;
        internal event Action<com.unity3d.mediation.LevelPlayAdInfo> OnAdInfoChanged;
#pragma warning restore CS0067

        void Start()
        {
            SendInstantiateEvent("Interstitial");
        }

        void OnGUI()
        {
            GUI.depth = 0;
            if (!m_Preview)
            {
                return;
            }

            var buttonRectStyle = new GUIStyle(GUI.skin.button)
            {
                font = m_Font,
                wordWrap = true,
                normal =
                {
                    textColor = Color.white,
                    background = CreateTextureFromColor(new Color(0.039f, 0.431f, 0.925f, 1.0f))
                },
                hover = {background = CreateTextureFromColor(new Color(0.075f, 0.38f, 0.678f, 1.0f))}
            };

            var textRect = new Rect(
                Screen.width / 2 - m_LevelPlayLogoWidth / 2,
                Screen.height / 2 - m_LevelPlayLogoHeight / 2,
                m_LevelPlayLogoWidth,
                m_LevelPlayLogoHeight);

            var adNameRectStyle = new GUIStyle(GUI.skin.label)
            {
                font = m_Font, wordWrap = true, alignment = TextAnchor.MiddleCenter, fontSize = 75
            };

            if (m_BackgroundTexture)
            {
                var interstitialRect = new Rect(0, 0, Screen.width,
                    Screen.height);
                var buttonRect = new Rect(Screen.width * 0.75f, Screen.height * 0.05f, Screen.width * 0.2f,
                    Screen.height * 0.05f);
                var adNameRect = new Rect(
                    0,
                    Screen.height / 2 + m_LevelPlayLogoHeight / 2 + 50,
                    Screen.width,
                    m_LevelPlayLogoHeight);

                GUI.DrawTexture(interstitialRect, m_BackgroundTexture, ScaleMode.StretchToFill);
                if (GUI.Button(buttonRect, "Close", buttonRectStyle))
                {
                    HideAd();
                }

                GUI.DrawTexture(textRect, m_LevelPlayLogo, ScaleMode.StretchToFill);
                GUI.Label(adNameRect, "Interstitial Ad", adNameRectStyle);
                if (GUI.Button(interstitialRect, string.Empty, GUIStyle.none))
                {
                    OnAdClicked?.Invoke(m_MockAdInfo);
                }
            }
        }


        internal void LoadAd()
        {
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log("Load Interstitial Ad has been called in the Editor");
            EditorLevelPlaySdk.CheckMockIsInitializedAndWarn();
#endif
            m_IsAdReady = true;
            OnAdLoaded?.Invoke(m_MockAdInfo);
        }

        internal void ShowAd(string placementName)
        {
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log(string.IsNullOrEmpty(placementName)
                ? "Show Interstitial Ad has been called in the Editor"
                : $"Show Interstitial Ad has been called in the Editor with Placement Name: {placementName}");
            EditorLevelPlaySdk.CheckMockIsInitializedAndWarn();
#endif
            if (!m_IsAdReady)
            {
                return;
            }

            m_Preview = true;
            OnAdDisplayed?.Invoke(m_MockAdInfo);
            m_IsAdReady = false;
        }

        void HideAd()
        {
            m_Preview = false;
            OnAdClosed?.Invoke(m_MockAdInfo);
        }

        internal bool IsAdReady()
        {
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log("Is Ad Ready for Interstitial Ad has been called in the Editor");
            EditorLevelPlaySdk.CheckMockIsInitializedAndWarn();
#endif
            return m_IsAdReady;
        }
#endif
#pragma warning restore 0618
    }
}
