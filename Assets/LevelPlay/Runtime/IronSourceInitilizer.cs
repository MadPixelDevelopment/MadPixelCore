using System;
using Unity.Services.LevelPlay;
using UnityEngine;
using LevelPlayAdFormat = com.unity3d.mediation.LevelPlayAdFormat;

[Obsolete("This class will be made private in a future version.", false)]
public class IronSourceInitilizer
{
#if UNITY_IOS || UNITY_ANDROID
    [RuntimeInitializeOnLoadMethod]
    static void Initilize()
    {
        var developerSettings = Resources.Load<IronSourceMediationSettings>(IronSourceConstants.IRONSOURCE_MEDIATION_SETTING_NAME);
        if (developerSettings != null)
        {
#if UNITY_ANDROID
            string appKey = developerSettings.AndroidAppKey;
#elif UNITY_IOS
            string appKey = developerSettings.IOSAppKey;
#endif
            if (developerSettings.EnableIronsourceSDKInitAPI == true)
            {
                if (appKey.Equals(string.Empty))
                {
                    LevelPlayLogger.LogWarning("LevelPlay SDK cannot initialize without AppKey");
                }
                else
                {
                    IronSource.UNITY_PLUGIN_VERSION += IronSource.UNITY_PLUGIN_VERSION.Contains("-r") ? "i" : "-i";

                    LevelPlayAdFormat[] adFormats = { LevelPlayAdFormat.REWARDED, LevelPlayAdFormat.INTERSTITIAL, LevelPlayAdFormat.BANNER};
                    LevelPlay.Init(appKey:appKey, adFormats:adFormats);
                }
            }

            if (developerSettings.EnableAdapterDebug)
            {
                IronSource.Agent.setAdaptersDebug(true);
            }

            if (developerSettings.EnableIntegrationHelper)
            {
                IronSource.Agent.validateIntegration();
            }
        }
    }

#endif
}
