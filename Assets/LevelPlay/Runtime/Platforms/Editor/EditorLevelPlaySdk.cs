#if UNITY_EDITOR
using System;

namespace Unity.Services.LevelPlay
{
    class EditorLevelPlaySdk
    {
#pragma warning disable CS0067
        static bool m_IsInitialized = false;
#pragma warning disable 0618
        internal static event Action<com.unity3d.mediation.LevelPlayConfiguration> OnInitSuccess;
        internal static event Action<com.unity3d.mediation.LevelPlayInitError> OnInitFailed;
        internal static event Action<LevelPlayImpressionData> OnImpressionDataReady;
#pragma warning restore CS0067

        internal static void Initialize()
        {
            OnInitSuccess?.Invoke(new com.unity3d.mediation.LevelPlayConfiguration(string.Empty));
#pragma warning restore 0618
            m_IsInitialized = true;
        }

        internal static void CheckMockIsInitializedAndWarn()
        {
            if (m_IsInitialized)
            {
                return;
            }

            LevelPlayLogger.LogWarning("LevelPlay SDK is not Initialized");
        }
    }
}
#endif
