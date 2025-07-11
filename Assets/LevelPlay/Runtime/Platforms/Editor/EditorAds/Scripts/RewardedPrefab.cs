using System;
using System.Collections;
using UnityEngine;

namespace Unity.Services.LevelPlay
{
    [ExecuteInEditMode]
    class RewardedPrefab : AdPrefab
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

        internal event Action<com.unity3d.mediation.LevelPlayAdInfo, com.unity3d.mediation.LevelPlayReward>
            OnAdRewarded;

        public float m_CountdownTime = 5f;
        bool m_StartCoroutine;

        void Start()
        {
            SendInstantiateEvent("Rewarded");
        }


        void OnGUI()
        {
            GUI.depth = 0;
            if (!m_Preview)
            {
                return;
            }

            if (m_StartCoroutine)
            {
                StartCoroutine(StartCountdown());
                m_StartCoroutine = false;
            }

            var buttonRectStyle = new GUIStyle(GUI.skin.button)
            {
                font = m_Font,
                wordWrap = true,
                fontSize = 56,
                normal =
                {
                    textColor = Color.white,
                    background = CreateTextureFromColor(new Color(48f / 255f, 164f / 255f, 108f / 255f))
                },
                hover = {background = CreateTextureFromColor(new Color(51f / 255f, 176f / 255f, 116f / 255f))}
            };

            var countDownRectStyle = new GUIStyle(GUI.skin.label)
            {
                font = m_Font, wordWrap = true, alignment = TextAnchor.MiddleCenter, fontSize = 50
            };
            var adNameRectStyle = new GUIStyle(GUI.skin.label)
            {
                font = m_Font, wordWrap = true, alignment = TextAnchor.MiddleCenter, fontSize = 75
            };

            if (m_BackgroundTexture)
            {
                var rewardedRect = new Rect(0, 0, Screen.width,
                    Screen.height);
                var buttonRect = new Rect(Screen.width * 0.75f, Screen.height * 0.05f, Screen.width * 0.2f,
                    Screen.height * 0.05f);
                var textRect = new Rect(
                    Screen.width / 2 - m_LevelPlayLogoWidth / 2,
                    Screen.height / 2 - m_LevelPlayLogoHeight / 2,
                    m_LevelPlayLogoWidth,
                    m_LevelPlayLogoHeight);
                var adNameRect = new Rect(
                    0,
                    Screen.height / 2 + m_LevelPlayLogoHeight / 2 + 50,
                    Screen.width,
                    m_LevelPlayLogoHeight);
                var countDownRect = new Rect(
                    0,
                    Screen.height / 2 + 2 * m_LevelPlayLogoHeight + 50,
                    Screen.width,
                    m_LevelPlayLogoHeight);

                GUI.DrawTexture(rewardedRect, m_BackgroundTexture, ScaleMode.StretchToFill);
                if (GUI.Button(buttonRect, "Close", buttonRectStyle))
                {
                    HideAd();
                }

                GUI.DrawTexture(textRect, m_LevelPlayLogo, ScaleMode.StretchToFill);
                GUI.Label(adNameRect, "Rewarded Ad", adNameRectStyle);
                GUI.Label(countDownRect, $"Ad will be closed in {m_CountdownTime} seconds", countDownRectStyle);
                if (GUI.Button(rewardedRect, string.Empty, GUIStyle.none))
                {
                    OnAdClicked?.Invoke(m_MockAdInfo);
                }
            }
        }

        IEnumerator StartCountdown()
        {
            while (m_CountdownTime > 0)
            {
                yield return new WaitForSeconds(1f);
                m_CountdownTime--;
            }

            HideAd();
        }

        internal void LoadAd()
        {
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log("Load Rewarded Ad has been called in the Editor");
            EditorLevelPlaySdk.CheckMockIsInitializedAndWarn();
#endif
            m_IsAdReady = true;
            OnAdLoaded?.Invoke(m_MockAdInfo);
        }

        internal void ShowAd(string placementName)
        {
#if ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING
            LevelPlayLogger.Log(string.IsNullOrEmpty(placementName)
                ? "Show Rewarded Ad has been called in the Editor"
                : $"Show Rewarded Ad has been called in the Editor with Placement Name: {placementName}");
#endif
            if (!m_IsAdReady)
            {
                return;
            }

            m_Preview = true;
            m_StartCoroutine = true;
            OnAdDisplayed?.Invoke(m_MockAdInfo);
            m_IsAdReady = false;
        }

        void HideAd()
        {
            StopAllCoroutines();
            m_Preview = false;
            m_CountdownTime = 5;
            OnAdClosed?.Invoke(m_MockAdInfo);
            OnAdRewarded?.Invoke(m_MockAdInfo,
                new com.unity3d.mediation.LevelPlayReward("editor_reward", 20));
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
