using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using System.Text;
using static UnityEngine.GraphicsBuffer;
using System.IO;

[InitializeOnLoad]
public class MPCChecker {
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
    static MPCChecker() {
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
    }

    private static void ShowSwitchTargetWindow(int target) {
        MPCTargetCheckerWindow.ShowWindow(target, (int)PlayerSettings.Android.targetSdkVersion);

        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)33;
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
}
