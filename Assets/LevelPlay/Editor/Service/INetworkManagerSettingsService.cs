namespace Unity.Services.LevelPlay.Editor
{
    interface INetworkManagerSettingsService
    {
        NetworkManagerSettings Settings { get; }
        bool ServiceFileExists { get; }
        void SaveSettingsToFile();
    }
}
