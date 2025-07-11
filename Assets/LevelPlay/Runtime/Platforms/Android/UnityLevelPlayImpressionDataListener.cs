#if UNITY_ANDROID
using UnityEngine;

namespace Unity.Services.LevelPlay
{
    class UnityLevelPlayImpressionDataListener : AndroidJavaProxy, IUnityLevelPlayImpressionDataListener
    {
        const string k_ILevelPlayImpressionDataListenerName = "com.ironsource.unity.androidbridge.UnityImpressionDataListener";
        IUnityLevelPlayImpressionDataListener m_Listener;
        public UnityLevelPlayImpressionDataListener(IUnityLevelPlayImpressionDataListener listener) : base(k_ILevelPlayImpressionDataListenerName)
        {
            m_Listener = listener;
        }

        public void onImpressionSuccess(string impressionData)
        {
            ThreadUtil.Post(state => m_Listener.onImpressionSuccess(impressionData));
        }
    }
}
#endif
