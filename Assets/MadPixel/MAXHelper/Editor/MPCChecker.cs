using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using MAXHelper;

[InitializeOnLoad]
public class MPCChecker {
    private static readonly List<string> ObsoleteDirectoriesToDelete = new List<string> {
        "Assets/Amazon",
    };    
    
    private static readonly List<string> ObsoleteFilesToDelete = new List<string> {
        "Assets/MadPixel/MAXHelper/Configs/Amazon_APS.unitypackage",
        "Assets/MadPixel/MAXHelper/Configs/Amazon_APS.unitypackage.meta",
        "Assets/Amazon.meta",
    };
    private const string OLD_CONFIGS_PATH = "Assets/MadPixel/MAXHelper/Configs/MAXCustomSettings.asset";
    private const string NEW_CONFIGS_PATH = "Assets/Resources/MAXCustomSettings.asset";

    private const string APPMETRICA_FOLDER = "Assets/AppMetrica";
    private const string EDM4U_FOLDER = "Assets/ExternalDependencyManager";

    static MPCChecker() {
        CheckPackagesExistence();
        CheckObsoleteFiles();

#if UNITY_ANDROID
        int target = (int)PlayerSettings.Android.targetSdkVersion;
        if (target == 0) {
            int highestInstalledVersion = GetHigestInstalledSDK();
            target = highestInstalledVersion;
        }

        if (target < 33 || PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24) {
            if (EditorPrefs.HasKey(Key)) {
                string lastMPCVersionChecked = EditorPrefs.GetString(Key);
                string currVersion = MAXHelper.MAXHelperInitWindow.GetVersion();
                if (lastMPCVersionChecked != currVersion) {
                    ShowSwitchTargetWindow(target);
                }
            }
            else {
                ShowSwitchTargetWindow(target);
            }
        }
        SaveKey();
#endif
    }


#if UNITY_ANDROID
    private static string appKey = null;
    private static string Key {
        get {
            if (string.IsNullOrEmpty(appKey)) {
                appKey = GetMd5Hash(Application.dataPath) + "MPCv";
            }

            return appKey;
        }
    }

    private static void ShowSwitchTargetWindow(int target) {
        MPCTargetCheckerWindow.ShowWindow(target, (int)PlayerSettings.Android.targetSdkVersion);

        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
    }


    private static string GetMd5Hash(string input) {
        MD5 md5 = MD5.Create();
        byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < data.Length; i++) {
            sb.Append(data[i].ToString("x2"));
        }

        return sb.ToString();
    }

    public static void SaveKey() {
        EditorPrefs.SetString(Key, MAXHelper.MAXHelperInitWindow.GetVersion());
    }

    //[MenuItem("Mad Pixel/DeleteKey", priority = 1)]
    public static void DeleteEditorPrefs() {
        EditorPrefs.DeleteKey(Key);
    }

    private static int GetHigestInstalledSDK() {
        string s = Path.Combine(GetHighestInstalledAPI(), "platforms");
        string[] directories = Directory.GetDirectories(s);
        int maxV = 0;
        foreach (string directory in directories) {
            string version = directory.Substring(directory.Length - 2, 2);
            int.TryParse(version, out int v);
            if (v > 0) {
                maxV = Mathf.Max(v, maxV);
            }
        }
        return maxV;
    }

    private static string GetHighestInstalledAPI() {
        return EditorPrefs.GetString("AndroidSdkRoot");
    }
#endif


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
            AssetDatabase.Refresh();
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
            AssetDatabase.SaveAssets();

            Debug.Log("MAXCustomSettings migrated");
        }
    }


    private static bool CheckExistence(string location) {
        return File.Exists(location) ||
               Directory.Exists(location) ||
               (location.EndsWith("/*") && Directory.Exists(Path.GetDirectoryName(location)));
    }

    #region Appmetrica and EDM as packages
    private static void CheckPackagesExistence() {
        var packageInfo = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();
        bool hasDuplicatedAppmetrica = false;
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

            if (amount >= 2) {
                break;
            }
        }

        if (hasDuplicatedAppmetrica || hasDuplicatedEDM) {
            MPCDeleteFoldersWindow.ShowWindow(hasDuplicatedAppmetrica, hasDuplicatedEDM);
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
}
