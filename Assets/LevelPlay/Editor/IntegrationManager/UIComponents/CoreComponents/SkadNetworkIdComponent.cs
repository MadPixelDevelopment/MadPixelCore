using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Services.LevelPlay.Editor.IntegrationManager.UIComponents
{
    class SkadNetworkIdComponent : IDrawable
    {
        internal static bool m_AddSkAdNetworkIds;

        const string k_SKAdNetworkDocsLink = "https://developers.is.com/ironsource-mobile/unity/managing-skadnetwork-ids/";
        const string k_SKAdNetworkToggleText = "Automatically add installed networks' SKAdNetwork IDs to info.plist file. If unchecked, you must do this manually.";
        const string k_LearnMoreText = "<a>Learn more</a>";
        const string k_SkadNetworkLabelText = "SKAdNetwork IDs";

        INetworkManagerSettingsService m_NetworkManagerSettingsService;
        IEditorAnalyticsService m_AnalyticsService;

        internal SkadNetworkIdComponent()
        {
            m_NetworkManagerSettingsService = new NetworkManagerSettingsService();
            m_AddSkAdNetworkIds = m_NetworkManagerSettingsService.Settings.AddNetworksSkadnetworkID;

            m_AnalyticsService = EditorServices.Instance.EditorAnalyticsService;
        }

        public void Draw()
        {
            var titleStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                fixedHeight = 20,
                stretchWidth = true,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
            };
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Label(k_SkadNetworkLabelText, titleStyle);

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();

            var toggleStyle = new GUIStyle(EditorStyles.toggle)
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(20, 0, 0, 0),
                stretchWidth = false
            };
            m_AddSkAdNetworkIds = GUILayout.Toggle(m_AddSkAdNetworkIds, k_SKAdNetworkToggleText, toggleStyle);

            if (EditorGUI.EndChangeCheck())
            {
                m_NetworkManagerSettingsService.Settings.AddNetworksSkadnetworkID = m_AddSkAdNetworkIds;
                if (m_NetworkManagerSettingsService.ServiceFileExists || m_AddSkAdNetworkIds)
                {
                    m_NetworkManagerSettingsService.SaveSettingsToFile();
                }
                m_AnalyticsService.SendInteractWithSkanIdCheckBox(m_AddSkAdNetworkIds);
            }

            var linkStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                richText = true,
                alignment = TextAnchor.MiddleLeft,
                stretchHeight = false,
                fixedHeight = 12f,
                stretchWidth = false
            };

            if (GUILayout.Button(k_LearnMoreText, linkStyle))
            {
                Application.OpenURL(k_SKAdNetworkDocsLink);
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}
