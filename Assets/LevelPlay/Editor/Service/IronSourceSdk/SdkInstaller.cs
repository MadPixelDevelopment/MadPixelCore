using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;

namespace Unity.Services.LevelPlay.Editor
{
    class SdkInstaller
    {
        readonly ILevelPlayLogger m_Logger;
        readonly ILevelPlayNetworkManager m_LevelPlayNetworkManager;

        internal SdkInstaller(ILevelPlayLogger logger, ILevelPlayNetworkManager levelPlayNetworkManager)
        {
            m_Logger = logger;
            m_LevelPlayNetworkManager = levelPlayNetworkManager;
        }

        internal async Task PreInstallAsync()
        {
            try
            {
                m_LevelPlayNetworkManager.LoadVersionsFromJson();
            }
            catch (Exception e)
            {
                m_Logger.LogError($"Failed to load versions json : {e}");
            }
            try
            {
                await m_LevelPlayNetworkManager.GetVersionsWebRequest();
            }
            catch (Exception e)
            {
                m_Logger.LogError($"Failed to fetch versions json : {e}");
            }
            try
            {
                m_LevelPlayNetworkManager.LoadVersionsFromJson();
            }
            catch (Exception e)
            {
                m_Logger.LogError($"Failed to load versions json after fetching from remote : {e}");
            }
        }

        internal async Task InstallUnityAdsAdapterAsync()
        {
            var unityAdsAdapter = m_LevelPlayNetworkManager.Adapters.Values.FirstOrDefault(adapter => adapter.KeyName == EditorConstants.k_UnityAdapterName);
            if (unityAdsAdapter == null)
                return;

            if (m_LevelPlayNetworkManager.ShouldSkipAutoInstall(unityAdsAdapter))
                return;

            try
            {
                var version = m_LevelPlayNetworkManager.CompatibleAdapterVersions(unityAdsAdapter).FirstOrDefault();
                if (version != null)
                {
                    await m_LevelPlayNetworkManager.Install(unityAdsAdapter, version);
                    AssetDatabase.Refresh();
                    m_LevelPlayNetworkManager.UiUpdate();
                }
            }
            catch (Exception e)
            {
                m_Logger.LogError($"Failed to automatically install Unity Ads adapter: {e.Message}");
            }
        }

        internal async Task InstallLatestIronSourceSdkAsync()
        {
            try
            {
                var latestIronSourceSdkVersion = m_LevelPlayNetworkManager.CompatibleIronSourceSdkVersions().FirstOrDefault();
                if (latestIronSourceSdkVersion != null)
                {
                    await m_LevelPlayNetworkManager.Install(latestIronSourceSdkVersion);
                    AssetDatabase.Refresh();
                    m_LevelPlayNetworkManager.UiUpdate();
                }
            }
            catch (Exception e)
            {
                m_Logger.LogError($"Failed to install IronSource SDK with exception : {e.ToString()}");
            }
        }
    }
}
