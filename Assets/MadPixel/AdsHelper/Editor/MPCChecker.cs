using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace MadPixel.Editor {
    [InitializeOnLoad]
    public class MPCChecker {
        #region Fields
        private const string OLD_MAX_CONFIGS_PATH = "Assets/MadPixel/MAXHelper/Configs/MAXCustomSettings.asset";
        private const string OLD_MAX_RESOURCES_CONFIGS_PATH = "Assets/Resources/MAXCustomSettings.asset";
        private const string NEW_CONFIGS_RESOURCES_PATH = "Assets/Resources/MadPixelCustomSettings.asset";

        private const string APPMETRICA_FOLDER = "Assets/AppMetrica";
        private const string EDM4U_FOLDER = "Assets/ExternalDependencyManager";
        private const string APPSFLYER_MAIN_SCRIPT = "Assets/AppsFlyer/AppsFlyer.cs";
        #endregion

//        static MPCChecker() {
//            CheckPackagesExistence();
//            CheckNewResourcesFile();
//#if UNITY_ANDROID
//            CheckTargetAPI();
//#endif
//        }

        #region Android Target API 

#if UNITY_ANDROID
        private static string m_appKey = null;
        private static string Key {
            get {
                if (string.IsNullOrEmpty(m_appKey)) {
                    m_appKey = GetMd5Hash(Application.dataPath) + "MPCv";
                }

                return m_appKey;
            }
        }

        private static void CheckTargetAPI() {
            int target = (int)PlayerSettings.Android.targetSdkVersion;
            if (target == 0) {
                int highestInstalledVersion = GetHigestInstalledSDK();
                target = highestInstalledVersion;
            }

            if (target < 33 || PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24) {
                if (EditorPrefs.HasKey(Key)) {
                    string lastMPCVersionChecked = EditorPrefs.GetString(Key);
                    string currVersion = MPCSetupWindow.GetVersion();
                    if (lastMPCVersionChecked != currVersion) {
                        ShowSwitchTargetWindow(target);
                    }
                }
                else {
                    ShowSwitchTargetWindow(target);
                }
            }
            SaveKey();
        }

        private static void ShowSwitchTargetWindow(int a_target) {
            MPCTargetCheckerWindow.ShowWindow(a_target, (int)PlayerSettings.Android.targetSdkVersion);

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
        }


        private static string GetMd5Hash(string a_input) {
            MD5 md5 = MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(a_input));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++) {
                sb.Append(data[i].ToString("x2"));
            }

            return sb.ToString();
        }

        public static void SaveKey() {
            EditorPrefs.SetString(Key, MPCSetupWindow.GetVersion());
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

        #endregion


        #region Static Helpers
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


        private static bool CheckExistence(string location) {
            return File.Exists(location) ||
                   Directory.Exists(location) ||
                   (location.EndsWith("/*") && Directory.Exists(Path.GetDirectoryName(location)));
        }
        #endregion

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

                if (CheckExistence(EDM4U_FOLDER)) {
                    FileUtil.DeleteFileOrDirectory(EDM4U_FOLDER);

                    string meta = EDM4U_FOLDER + ".meta";
                    if (CheckExistence(meta)) {
                        FileUtil.DeleteFileOrDirectory(meta);
                    }
                }
            }
        }
        #endregion
    }

}