using UnityEngine;

namespace Unity.Services.LevelPlay.Editor
{
    static class GameObjectAnalyticsSender
    {
        static void SendInstantiateBannerEvent()
        {
            EditorServices.Instance.EditorAnalyticsService.SendInstantiateGameObject("Banner");
        }

        static void SendInstantiateInterstitialEvent()
        {
            EditorServices.Instance.EditorAnalyticsService.SendInstantiateGameObject("Interstitial");
        }

        static void SendInstantiateRewardedEvent()
        {
            EditorServices.Instance.EditorAnalyticsService.SendInstantiateGameObject("Rewarded");
        }
    }
}
