using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Services.LevelPlay.Editor
{
    class NetworkManagerSettingsService : INetworkManagerSettingsService
    {
        public NetworkManagerSettings Settings { get; }

        public bool ServiceFileExists { get; private set; }

        private string m_SettingsPath => FilePaths.NetworkManagerSettingsFilePath;

        internal NetworkManagerSettingsService()
        {
            ServiceFileExists = false;
            var networkManagerSettings = LoadSettingsFile();
            if (networkManagerSettings != null)
            {
                Settings = networkManagerSettings;
                ServiceFileExists = true;
            }
            else
            {
                Settings = ScriptableObject.CreateInstance<NetworkManagerSettings>();
            }
        }

        public void CreateSettingsFile()
        {
            AssetDatabase.CreateAsset(Settings, m_SettingsPath);
            ServiceFileExists = true;
        }

        NetworkManagerSettings LoadSettingsFile()
        {
            var networkManagerSettings =
                AssetDatabase.LoadAssetAtPath<NetworkManagerSettings>(m_SettingsPath);
            return networkManagerSettings;
        }

        public void SaveSettingsToFile()
        {
            if (!ServiceFileExists)
            {
                CreateSettingsFile();
            }
            var networkManagerSettings = LoadSettingsFile();
            networkManagerSettings.AddNetworksSkadnetworkID = Settings.AddNetworksSkadnetworkID;
            EditorUtility.SetDirty(networkManagerSettings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
