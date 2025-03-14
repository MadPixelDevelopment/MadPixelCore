using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


[InitializeOnLoad]
public class MAXHelperUpdater {
    private const string ADDRESS = ""; 
    private const string MAXHelperUpdatePath = "Assets/Resources/MAXHelperUpdate.package";
    private const string MAXHelperUpdateFolder = "Assets/Resources/";
    private const string KeyLastUpdateCheckTime = "maxhelperlastupdatetime";
    private const string Address = "https://drive.google.com/file/d/14G8Dg6Tpfs68mWO7PVmf2swdNV1M-rks/";
    private static readonly DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    static MAXHelperUpdater() {
        //CheckForUpdates();
    }


    private static void CheckForUpdates() {
        var now = (int)(DateTime.UtcNow - EpochTime).TotalSeconds;
        if (EditorPrefs.HasKey(KeyLastUpdateCheckTime)) {
            var elapsedTime = now - EditorPrefs.GetInt(KeyLastUpdateCheckTime);
            if (elapsedTime < 86400) {
                //return;
            }
        }

        EditorPrefs.SetInt(KeyLastUpdateCheckTime, now);

        LoadAndUpdateFromGoogle();
    }

    public static void LoadAndUpdateFromGoogle() {
        Debug.Log("Loading...");
        UnityWebRequest DownloadWebRequest = UnityWebRequest.Get(Address);
        DownloadWebRequest.downloadHandler = new DownloadHandlerFile(Application.temporaryCachePath + "/UpdatedMAXHelper.unitypackage");
        AsyncOperation AsynOpHandle = DownloadWebRequest.SendWebRequest();
        AsynOpHandle.completed += AsynOpHandleOncompleted;
    }

    private static void AsynOpHandleOncompleted(AsyncOperation obj) {
        UnityWebRequestAsyncOperation Result = (obj as UnityWebRequestAsyncOperation);
        if (Result != null) {
            if (Result.webRequest.result == UnityWebRequest.Result.Success) {
                AssetDatabase.ImportPackage(Application.temporaryCachePath + "/UpdatedMAXHelper.unitypackage", true);
            }
        }
        
    }
}
