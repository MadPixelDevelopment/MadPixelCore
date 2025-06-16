using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MadPixel.Editor {
    public class MPCExport : EditorWindow {
        [MenuItem("Mad Pixel/Export MPC as UnityPackage")]
        public static void ExportMadPixelCoreAsPackage() {
            List<string> exportGUIDs = new List<string>();
            string[] foldersToInclude = new[] {
                "Assets/AppsFlyer",
                "Assets/Editor Default Resources",
                "Assets/Firebase",
                "Assets/MaxSdk",
                "Assets/MadPixel",
                "Assets/Plugins/iOS"
            };

            string[] assetGUIDs = AssetDatabase.FindAssets("", foldersToInclude);
            AddGUIDs(ref exportGUIDs, assetGUIDs);


            string defaultPackageName = $"MPC_{MPCSetupWindow.GetVersion().TrimEnd()}.unitypackage";
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

                // Export the package
                AssetDatabase.ExportPackage(
                    assetPaths.ToArray(),
                    exportPath,
                    ExportPackageOptions.Interactive);

                EditorUtility.RevealInFinder(exportPath);
                Debug.Log("UnityPackage export completed: " + exportPath);
            }
        }

        private static void AddGUIDs(ref List<string> o_exportGUIDs, string[] a_assetGUIDs) {
            foreach (string guid in a_assetGUIDs) {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (path.EndsWith("AppLovinSettings.asset", System.StringComparison.OrdinalIgnoreCase)) {
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