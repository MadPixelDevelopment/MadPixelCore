using System;

namespace Unity.Services.LevelPlay.Editor
{
    internal interface IEditorAnalyticsService
    {
        void Initialize();
        void SendEventEditorClick(string component, string action);
        void SendInstallAdapterEvent(string adapterName, string newVersion, string currentVersion);
        void SendInstallLPSDKEvent(string newVersion);
        void SendUpdateLPSDKEvent(string newVersion, string currentVersion);
        void SendUpdateAdapterEvent(string adapterName, string newVersion, string currentVersion);
        void SendNewSession(string packageType);
        void SendInstallPackage(string component);
        void SendInteractWithSkanIdCheckBox(bool action);
        void SendFailedToAddSkAdNetworkId(string adapterName);
        void SendInstantiateGameObject(string adFormat);
        void SendMdrEvent(string action);
    }
}
