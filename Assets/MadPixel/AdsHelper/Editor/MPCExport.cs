using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MadPixel.Editor {
    public class MPCExport : EditorWindow {
        [MenuItem("Mad Pixel/Export MPC as UnityPackage")]
        public static void ExportMadPixelCoreAsPackage() {
            List<string> exportGUIDs = new List<string>();
            string[] foldersToInclude = new [] {
                "Assets/AppsFlyer",
                "Assets/Editor Default Resources",
                "Assets/Firebase",
                "Assets/GoogleMobileAds",
                "Assets/LevelPlay",
                "Assets/MadPixel",
                "Assets/Plugins/iOS"
            };

            string[] assetGUIDs = AssetDatabase.FindAssets("", foldersToInclude);
            AddGUIDs(ref exportGUIDs, assetGUIDs);


            string defaultPackageName = $"MPC_levelPlay_{MPCSetupWindow.GetMPCLevelPlayVersion().TrimEnd()}.unitypackage";
            string exportPath = EditorUtility.SaveFilePanel(
                "Export MPC_levelPlay Folder as UnityPackage",
                "",
                defaultPackageName,
                "unitypackage");


            if (!string.IsNullOrEmpty(exportPath)) {
                // Convert GUIDs to asset paths
                List<string> assetPaths = new List<string>();
                for (int i = 0; i < exportGUIDs.Count; i++) {
                    assetPaths.Add(AssetDatabase.GUIDToAssetPath(exportGUIDs[i]));
                }

                assetPaths.Add("Assets/Plugins/Android/GoogleMobileAdsPlugin.androidlib");
                assetPaths.Add("Assets/Plugins/Android/googlemobileads-unity.aar");

                // Export the package
                AssetDatabase.ExportPackage(
                    assetPaths.ToArray(),
                    exportPath,
                    ExportPackageOptions.Recurse |
                    ExportPackageOptions.Interactive);

                EditorUtility.RevealInFinder(exportPath);
                Debug.Log("UnityPackage export completed: " + exportPath);
            }
        }

        private static void AddGUIDs(ref List<string> o_exportGUIDs, string[] a_assetGUIDs) {
            foreach (string guid in a_assetGUIDs) {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (path.EndsWith("IronSourceMediatedNetworkSettings.asset", System.StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }
                if (path.EndsWith("IronSourceMediationSettings.asset", System.StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }
                if (path.EndsWith("GoogleMobileAdsSettings.asset", System.StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }
                if (path.EndsWith("MPCExport.cs", System.StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                o_exportGUIDs.Add(guid);
            }
        }
    }
}