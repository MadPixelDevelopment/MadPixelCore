﻿//
//  MaxInitialization.cs
//  AppLovin MAX Unity Plugin
//
//  Created by Thomas So on 5/24/19.
//  Copyright © 2019 AppLovin. All rights reserved.
//

using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace AppLovinMax.Scripts.IntegrationManager.Editor
{
    [InitializeOnLoad]
    public class AppLovinInitialize
    {
        private static readonly List<string> ObsoleteNetworks = new List<string>
        {
            "AdColony",
            "Criteo",
            "Nend",
            "Snap",
            "Tapjoy",
            "VerizonAds",
            "VoodooAds"
        };

        private static readonly List<string> ObsoleteFileExportPathsToDelete = new List<string>
        {
            // The `MaxSdk/Scripts/Editor` folder contents have been moved into `MaxSdk/Scripts/IntegrationManager/Editor`.
            "MaxSdk/Scripts/Editor",
            "MaxSdk/Scripts/Editor.meta",

            // The `EventSystemChecker` has been renamed to `MaxEventSystemChecker`.
            "MaxSdk/Scripts/EventSystemChecker.cs",
            "MaxSdk/Scripts/EventSystemChecker.cs.meta",

            // Google AdMob adapter pre/post process scripts. The logic has been migrated to the main plugin.
            "MaxSdk/Mediation/Google/Editor/MaxGoogleInitialize.cs",
            "MaxSdk/Mediation/Google/Editor/MaxGoogleInitialize.cs.meta",
            "MaxSdk/Mediation/Google/Editor/MaxMediationGoogleUtils.cs",
            "MaxSdk/Mediation/Google/Editor/MaxMediationGoogleUtils.cs.meta",
            "MaxSdk/Mediation/Google/Editor/PostProcessor.cs",
            "MaxSdk/Mediation/Google/Editor/PostProcessor.cs.meta",
            "MaxSdk/Mediation/Google/Editor/PreProcessor.cs",
            "MaxSdk/Mediation/Google/Editor/PreProcessor.cs.meta",
            "MaxSdk/Mediation/Google/Editor/MaxSdk.Mediation.Google.Editor.asmdef",
            "MaxSdk/Mediation/Google/MaxSdk.Mediation.Google.Editor.asmdef.meta",
            "Plugins/Android/MaxMediationGoogle.androidlib",
            "Plugins/Android/MaxMediationGoogle.androidlib.meta",

            // Google Ad Manager adapter pre/post process scripts. The logic has been migrated to the main plugin.
            "MaxSdk/Mediation/GoogleAdManager/Editor/MaxGoogleAdManagerInitialize.cs",
            "MaxSdk/Mediation/GoogleAdManager/Editor/MaxGoogleAdManagerInitialize.cs.meta",
            "MaxSdk/Mediation/GoogleAdManager/Editor/PostProcessor.cs",
            "MaxSdk/Mediation/GoogleAdManager/Editor/PostProcessor.cs.meta",
            "MaxSdk/Mediation/GoogleAdManager/Editor/MaxSdk.Mediation.GoogleAdManager.Editor.asmdef",
            "MaxSdk/Mediation/GoogleAdManager/Editor/MaxSdk.Mediation.GoogleAdManager.Editor.asmdef.meta",
            "Plugins/Android/MaxMediationGoogleAdManager.androidlib",
            "Plugins/Android/MaxMediationGoogleAdManager.androidlib.meta",

            // The `VariableService` has been removed.
            "MaxSdk/Scripts/MaxVariableServiceAndroid.cs",
            "MaxSdk/Scripts/MaxVariableServiceAndroid.cs.meta",
            "MaxSdk/Scripts/MaxVariableServiceiOS.cs",
            "MaxSdk/Scripts/MaxVariableServiceiOS.cs.meta",
            "MaxSdk/Scripts/MaxVariableServiceUnityEditor.cs",
            "MaxSdk/Scripts/MaxVariableServiceUnityEditor.cs.meta",

            // The `MaxSdk/Scripts/Editor` folder contents have been moved into `MaxSdk/Scripts/IntegrationManager/Editor`.
            "MaxSdk/Version.md",
            "MaxSdk/Version.md.meta",

            // TODO: Add MaxTargetingData and MaxUserSegment when the plugin has enough traction.
        };

        static AppLovinInitialize()
        {
            // Don't run obsolete file cleanup logic when entering play mode.
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

#if UNITY_IOS
            // Check that the publisher is targeting iOS 9.0+
            if (!PlayerSettings.iOS.targetOSVersionString.StartsWith("9.") && !PlayerSettings.iOS.targetOSVersionString.StartsWith("1"))
            {
                MaxSdkLogger.UserError("Detected iOS project version less than iOS 9 - The AppLovin MAX SDK WILL NOT WORK ON < iOS9!!!");
            }
#endif

            var isPluginInPackageManager = AppLovinIntegrationManager.IsPluginInPackageManager;
            if (!isPluginInPackageManager)
            {
                var changesMade = false;
                foreach (var obsoleteFileExportPathToDelete in ObsoleteFileExportPathsToDelete)
                {
                    var pathToDelete = MaxSdkUtils.GetAssetPathForExportPath(obsoleteFileExportPathToDelete);
                    if (CheckExistence(pathToDelete))
                    {
                        MaxSdkLogger.UserDebug("Deleting obsolete file '" + pathToDelete + "' that is no longer needed.");
                        FileUtil.DeleteFileOrDirectory(pathToDelete);
                        changesMade = true;
                    }
                }

                var pluginParentDir = AppLovinIntegrationManager.PluginParentDirectory;
                // Check if any obsolete networks are installed
                foreach (var obsoleteNetwork in ObsoleteNetworks)
                {
                    var networkDir = Path.Combine(pluginParentDir, "MaxSdk/Mediation/" + obsoleteNetwork);
                    if (CheckExistence(networkDir))
                    {
                        MaxSdkLogger.UserDebug("Deleting obsolete network " + obsoleteNetwork + " from path " + networkDir + "...");
                        FileUtil.DeleteFileOrDirectory(networkDir);
                        FileUtil.DeleteFileOrDirectory(networkDir + ".meta");
                        changesMade = true;
                    }
                }

                // Refresh UI
                if (changesMade)
                {
                    AssetDatabase.Refresh();
                    MaxSdkLogger.UserDebug("Obsolete networks and files removed.");
                }
            }

            AppLovinAutoUpdater.Update();
        }

        private static bool CheckExistence(string location)
        {
            return File.Exists(location) ||
                   Directory.Exists(location) ||
                   (location.EndsWith("/*") && Directory.Exists(Path.GetDirectoryName(location)));
        }
    }
}
