using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace MadPixel.Editor {
    [InitializeOnLoad]
    public class MPCChecker {
        private const int TARGET_SDK = 34;
        static MPCChecker() {


#if UNITY_ANDROID
            int target = (int)PlayerSettings.Android.targetSdkVersion;
            if (target == 0) {
                int highestInstalledVersion = GetHigestInstalledSDK();
                target = highestInstalledVersion;
            }

            if (target < TARGET_SDK || PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24) {
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

        //[MenuItem("Mad Pixel/DeleteKey", priority = 1)]
        public static void DeleteEditorPrefs() {
            EditorPrefs.DeleteKey(Key);
        }

        private static int GetHigestInstalledSDK() {
            string s = Path.Combine(GetHighestInstalledAPI(), "platforms");
            if (Directory.Exists(s)) {
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

            return TARGET_SDK;
        }

        private static string GetHighestInstalledAPI() {
            return EditorPrefs.GetString("AndroidSdkRoot");
        }
#endif

    } 
}
