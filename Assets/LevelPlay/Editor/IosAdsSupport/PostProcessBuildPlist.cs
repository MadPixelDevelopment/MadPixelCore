#if UNITY_2018_1_OR_NEWER  && UNITY_IOS
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.iOS.Xcode;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Reporting;

namespace Unity.Services.LevelPlay.Editor
{
    internal class PostProcessBuildPlist : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        private const string k_SkAdNetworkIdentifier = "SKAdNetworkIdentifier";
        private const string k_SkAdNetworkItems = "SKAdNetworkItems";
        static ILevelPlayLogger m_Logger;
        ILevelPlayNetworkManager m_LevelPlayNetworkManager;
        INetworkManagerSettingsService m_NetworkManagerSettingsService;
        IEditorAnalyticsService m_AnalyticsService;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
            {
                return;
            }

            m_NetworkManagerSettingsService = new NetworkManagerSettingsService();

            if (!m_NetworkManagerSettingsService.Settings.AddNetworksSkadnetworkID)
            {
                return;
            }

            m_Logger = new LevelPlayLogger();
            m_LevelPlayNetworkManager = EditorServices.Instance.LevelPlayNetworkManager;
            m_LevelPlayNetworkManager.LoadVersionsFromJson();
            m_AnalyticsService = EditorServices.Instance.EditorAnalyticsService;
            UpdateInfoPlistWithSkAdNetworkIds(report.summary.outputPath);
        }

        internal void UpdateInfoPlistWithSkAdNetworkIds(string pathToPlistFile)
        {
            var ids = new HashSet<string>();
            foreach (var adapter in m_LevelPlayNetworkManager.Adapters.Values.Where(adapter =>
                !string.IsNullOrEmpty(m_LevelPlayNetworkManager.InstalledAdapterVersionString(adapter))))
            {
                try
                {
                    ids.UnionWith(SkAdNetworkXmlParser.ParseSource(
                        new SkAdNetworkRemoteSource(adapter.SKAdNetworkIdXmlURL)));
                    m_Logger.Log($"Added SKAdNetwork Id for: {adapter.DisplayName}");
                }
                catch (Exception e)
                {
                    m_AnalyticsService.SendFailedToAddSkAdNetworkId(adapter.DisplayName);
                    m_Logger.LogError($"Failed to parse SKAdNetwork for {adapter.DisplayName} files due to following reason: {e.Message}. You still can add them manually or please contact us for assistance.");
                }

                try
                {
                    WriteSkAdNetworkIdsToInfoPlist(ids, pathToPlistFile);
                }
                catch (Exception e)
                {
                    m_AnalyticsService.SendFailedToAddSkAdNetworkId(adapter.DisplayName);
                    m_Logger.LogError($"Failed to update info.plist file due to following reason: {e.Message}. You still can add them manually or please contact us for assistance.");
                }
            }
        }

        /// <summary>
        /// Write all plistValues to an existing Info.plist file
        /// </summary>
        internal static void WriteSkAdNetworkIdsToInfoPlist(HashSet<string> skAdNetworkIds, string outputPath)
        {
            var infoPlistPath = Path.Combine(outputPath + "/Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromString(EditorServices.Instance.FileService.ReadAllText(infoPlistPath));
            var root = plist.root;

            if (root == null)
            {
                m_Logger.LogWarning("[Unity SKAdNetwork Parser] Unable to parse info.plist.  Unable to add SkAdNetwork Identifiers. You still can add them manually or please contact us for assistance.");
                return;
            }

            if (!root.values?.ContainsKey(k_SkAdNetworkItems) ?? false)
            {
                root.CreateArray(k_SkAdNetworkItems);
            }

            var adNetworkItems = root[k_SkAdNetworkItems].AsArray();

            if (adNetworkItems == null)
            {
                m_Logger.LogWarning("[Unity SKAdNetwork Parser] Unable to modify existing info.plist.  Unable to add SkAdNetwork Identifiers. You still can add them manually or please contact us for assistance.");
                return;
            }

            foreach (var adNetworkId in skAdNetworkIds)
            {
                if (!PlistContainsAdNetworkId(adNetworkItems, adNetworkId))
                {
                    adNetworkItems.AddDict().SetString(k_SkAdNetworkIdentifier, adNetworkId);
                }
            }

            EditorServices.Instance.FileService.WriteAllText(infoPlistPath, plist.WriteToString());
        }

        /// <summary>
        /// Check if the value is already contained in the plist
        /// </summary>
        internal static bool PlistContainsAdNetworkId(PlistElementArray adNetworkItems, string adNetworkId)
        {
            foreach (var adNetworkItem in adNetworkItems.values)
            {
                var item = adNetworkItem.AsDict();
                if (item.values.TryGetValue(k_SkAdNetworkIdentifier, out var value))
                {
                    if (value.AsString() == adNetworkId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
#endif //UNITY_2018_1_OR_NEWER
