#if UNITY_ANDROID
namespace Unity.Services.LevelPlay
{
    interface IUnityLevelPlayImpressionDataListener
    {
        void onImpressionSuccess(string impressionData);
    }
}
#endif
