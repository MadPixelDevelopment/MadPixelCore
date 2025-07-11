using System.Reflection;
using UnityEngine;

namespace Unity.Services.LevelPlay
{
    class AdPrefab : MonoBehaviour
    {
#if UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        internal int m_LevelPlayLogoWidth = 534;
        internal int m_LevelPlayLogoHeight = 96;

        internal bool m_IsAdReady;

        [Tooltip("Preview Mock Ad In the Editor")] [SerializeField]
        internal bool m_Preview = true;

        [Tooltip("Preview Mock Ad In the Editor")] [SerializeField]
        internal Texture2D m_BackgroundTexture;

        [Tooltip("Font of the Mock Ad")] [SerializeField]
        internal Font m_Font;

        [Tooltip("Image Texture of the Mock Ad")] [SerializeField]
        internal Texture2D m_LevelPlayLogo;

        const string m_AdInfoJson = @"
        {
            ""adId"": ""editor_mock_ad_id"",
            ""adUnitId"": ""editor_mock_ad_unit_id"",
            ""adUnitName"": ""editor_mock_name"",
            ""adSize"": {
                ""description"": ""editor_mock_description"",
                ""width"": 1,
                ""height"": 1
            },
            ""adFormat"": ""editor_mock_format"",
            ""placementName"": ""editor_mock_placement"",
            ""auctionId"": ""editor_mock_auction"",
            ""country"": ""editor_mock_country"",
            ""ab"": ""editor_mock_ab"",
            ""segmentName"": ""editor_mock_segment"",
            ""adNetwork"": ""editor_mock_ad_network"",
            ""instanceName"": ""editor_mock_instance"",
            ""instanceId"": ""editor_mock_instance_id"",
            ""revenue"": 1,
            ""precision"": ""editor_mock_precision"",
            ""encryptedCPM"": ""editor_mock_cpm""
        }";

#pragma warning disable 0618
        internal com.unity3d.mediation.LevelPlayAdInfo m_MockAdInfo =
            new com.unity3d.mediation.LevelPlayAdInfo(m_AdInfoJson);
#pragma warning restore 0618

        internal Texture2D CreateTextureFromColor(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        internal void SendInstantiateEvent(string adFormat)
        {
            var editorAssembly = Assembly.Load("Unity.LevelPlay.Editor");
            var editorType = editorAssembly.GetType("Unity.Services.LevelPlay.Editor.GameObjectAnalyticsSender");
            var methodInfo =
                editorType.GetMethod($"SendInstantiate{adFormat}Event", BindingFlags.Static | BindingFlags.NonPublic);
            methodInfo?.Invoke(null, null);
        }
#endif
    }
}
