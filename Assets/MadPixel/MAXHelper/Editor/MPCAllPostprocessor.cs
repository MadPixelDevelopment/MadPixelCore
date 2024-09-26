using MAXHelper;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MPCAllPostprocessor : AssetPostprocessor {
    private const string OLD_CONFIGS_PATH = "Assets/MadPixel/MAXHelper/Configs/MAXCustomSettings.asset";
    private const string NEW_CONFIGS_PATH = "Assets/Resources/MAXCustomSettings.asset";

    private const string APPMETRICA_FOLDER = "Assets/AppMetrica";
    private const string EDM4U_FOLDER = "Assets/ExternalDependencyManager";
    private const string APPSFLYER_FOLDER = "Assets/AppsFlyer";

    private static readonly List<string> ObsoleteDirectoriesToDelete = new List<string> {
        "Assets/Amazon",
    };

    private static readonly List<string> ObsoleteFilesToDelete = new List<string> {
        "Assets/MadPixel/MAXHelper/Configs/Amazon_APS.unitypackage",
        "Assets/MadPixel/MAXHelper/Configs/Amazon_APS.unitypackage.meta",
        "Assets/Amazon.meta",
    };


    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload) {
        CheckPackagesExistence();
        CheckObsoleteFiles();

        foreach (string str in importedAssets) {
            //Debug.Log("[MadPixel] Reimported Asset: " + str);
        }
        foreach (string str in deletedAssets) {
            //Debug.Log("[MadPixel] Deleted Asset: " + str);
        }

        for (int i = 0; i < movedAssets.Length; i++) {
            //Debug.Log("[MadPixel] Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
        }

        if (didDomainReload) {
            //Debug.Log("[MadPixel] Domain has been reloaded");
        }

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
                if (CheckExistence(APPSFLYER_FOLDER)) {
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
            
            if (CheckExistence(APPSFLYER_FOLDER)) {
                FileUtil.DeleteFileOrDirectory(APPSFLYER_FOLDER);

                string meta = APPSFLYER_FOLDER + ".meta";
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


    private static void CheckObsoleteFiles() {
        bool changesMade = false;
        foreach (var pathToDelete in ObsoleteFilesToDelete) {
            if (CheckExistence(pathToDelete)) {
                FileUtil.DeleteFileOrDirectory(pathToDelete);
                changesMade = true;
            }
        }

        foreach (string directory in ObsoleteDirectoriesToDelete) {
            if (CheckExistence(directory)) {
                FileUtil.DeleteFileOrDirectory(directory);
                changesMade = true;
            }
        }

        CheckNewResourcesFile();

        MAXHelperDefineSymbols.DefineSymbols(false);

        if (changesMade) {
            //AssetDatabase.Refresh();
            Debug.LogWarning("ATTENTION: Amazon removed from this project");
        }
    }

    private static void CheckNewResourcesFile() {
        var oldConfig = AssetDatabase.LoadAssetAtPath(OLD_CONFIGS_PATH, typeof(MAXCustomSettings));
        if (oldConfig != null) {
            var resObj = AssetDatabase.LoadAssetAtPath(NEW_CONFIGS_PATH, typeof(MAXCustomSettings));
            if (resObj == null) {
                Debug.Log("MAXCustomSettings file doesn't exist, creating a new one...");
                ScriptableObject so = MAXCustomSettings.CreateInstance("MAXCustomSettings");
                AssetDatabase.CreateAsset(so, NEW_CONFIGS_PATH);
                resObj = so;
            }

            var newCustomSettings = (MAXCustomSettings)resObj;
            newCustomSettings.Set((MAXCustomSettings)oldConfig);

            FileUtil.DeleteFileOrDirectory(OLD_CONFIGS_PATH);
            EditorUtility.SetDirty(newCustomSettings);

            Debug.Log("MAXCustomSettings migrated");
        }
    }


    private static bool CheckExistence(string location) {
        return File.Exists(location) ||
               Directory.Exists(location) ||
               (location.EndsWith("/*") && Directory.Exists(Path.GetDirectoryName(location)));
    }

}
