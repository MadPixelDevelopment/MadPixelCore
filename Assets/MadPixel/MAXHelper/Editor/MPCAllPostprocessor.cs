using MadPixel;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MadPixel.Editor {
    public class MPCAllPostprocessor : AssetPostprocessor {
        private const string OLD_MAX_CONFIGS_PATH = "Assets/MadPixel/MAXHelper/Configs/MAXCustomSettings.asset";
        private const string OLD_MAX_RESOURCES_CONFIGS_PATH = "Assets/Resources/MAXCustomSettings.asset";
        private const string NEW_CONFIGS_RESOURCES_PATH = "Assets/Resources/MadPixelCustomSettings.asset";

        private const string APPMETRICA_FOLDER = "Assets/AppMetrica";
        private const string EDM4U_FOLDER = "Assets/ExternalDependencyManager";
        private const string APPSFLYER_MAIN_SCRIPT = "Assets/AppsFlyer/AppsFlyer.cs";


        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload) {
            CheckPackagesExistence();
            CheckNewResourcesFile();

            if (importedAssets.Length > 0 || deletedAssets.Length > 0 || movedAssets.Length > 0) {
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }

        #region Appmetrica and EDM as packages
        private static void CheckPackagesExistence() {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();
            bool hasDuplicatedAppmetrica = false;
            bool hasDuplicatedAppsFlyer = false;
            bool hasDuplicatedEDM = false;
            int amount = 0;

            foreach (var package in packageInfo) {
                if (package.name.Equals("com.google.external-dependency-manager")) {
                    amount++;
                    if (CheckExistence(EDM4U_FOLDER)) {
                        hasDuplicatedEDM = true;
                    }
                }
                else if (package.name.Equals("io.appmetrica.analytics")) {
                    amount++;
                    if (CheckExistence(APPMETRICA_FOLDER)) {
                        hasDuplicatedAppmetrica = true;
                    }
                }
                else if (package.name.Equals("appsflyer-unity-plugin")) {
                    amount++;
                    if (CheckExistence(APPSFLYER_MAIN_SCRIPT)) {
                        hasDuplicatedAppsFlyer = true;
                    }
                }

                if (amount >= 3) {
                    break;
                }
            }

            if (hasDuplicatedAppmetrica || hasDuplicatedEDM || hasDuplicatedAppsFlyer) {
                MPCDeleteFoldersWindow.ShowWindow(hasDuplicatedAppmetrica, hasDuplicatedEDM, hasDuplicatedAppsFlyer);
            }
        }

        public static void DeleteOldPackages(bool a_deleteOldPackages) {
            if (a_deleteOldPackages) {
                if (CheckExistence(APPMETRICA_FOLDER)) {
                    FileUtil.DeleteFileOrDirectory(APPMETRICA_FOLDER);

                    string meta = APPMETRICA_FOLDER + ".meta";
                    if (CheckExistence(meta)) {
                        FileUtil.DeleteFileOrDirectory(meta);
                    }
                }

                if (CheckExistence(APPSFLYER_MAIN_SCRIPT)) {
                    FileUtil.DeleteFileOrDirectory(APPSFLYER_MAIN_SCRIPT);

                    string meta = APPSFLYER_MAIN_SCRIPT + ".meta";
                    if (CheckExistence(meta)) {
                        FileUtil.DeleteFileOrDirectory(meta);
                    }
                }

                if (CheckExistence(EDM4U_FOLDER)) {
                    FileUtil.DeleteFileOrDirectory(EDM4U_FOLDER);

                    string meta = EDM4U_FOLDER + ".meta";
                    if (CheckExistence(meta)) {
                        FileUtil.DeleteFileOrDirectory(meta);
                    }
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        #endregion

        private static void CheckNewResourcesFile() {
            var oldConfig = AssetDatabase.LoadAssetAtPath(OLD_MAX_CONFIGS_PATH, typeof(MadPixelCustomSettings));
            if (oldConfig != null) {
                var resObj = AssetDatabase.LoadAssetAtPath(OLD_MAX_RESOURCES_CONFIGS_PATH, typeof(MadPixelCustomSettings));
                if (resObj == null) {
                    Debug.Log("MadPixelCustomSettings file doesn't exist, creating a new one...");
                    ScriptableObject so = MadPixelCustomSettings.CreateInstance(AdsManager.SETTINGS_FILE_NAME);
                    AssetDatabase.CreateAsset(so, NEW_CONFIGS_RESOURCES_PATH);
                    resObj = so;
                }

                var newCustomSettings = (MadPixelCustomSettings)resObj;
                newCustomSettings.Set((MadPixelCustomSettings)oldConfig);

                FileUtil.DeleteFileOrDirectory(OLD_MAX_CONFIGS_PATH);
                EditorUtility.SetDirty(newCustomSettings);
                AssetDatabase.SaveAssets();

                Debug.Log("MadPixelCustomSettings migrated");
            }
            else {
                oldConfig = AssetDatabase.LoadAssetAtPath(OLD_MAX_RESOURCES_CONFIGS_PATH, typeof(MadPixelCustomSettings));
                if (oldConfig != null) {
                    string result = AssetDatabase.RenameAsset(OLD_MAX_RESOURCES_CONFIGS_PATH, $"{AdsManager.SETTINGS_FILE_NAME}.asset");
                    if (!string.IsNullOrEmpty(result)) {
                        Debug.Log($"[Mad Pixel] {result}");
                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }


        private static bool CheckExistence(string a_location) {
            return File.Exists(a_location) ||
                   Directory.Exists(a_location) ||
                   (a_location.EndsWith("/*") && Directory.Exists(Path.GetDirectoryName(a_location)));
        }

    }

}