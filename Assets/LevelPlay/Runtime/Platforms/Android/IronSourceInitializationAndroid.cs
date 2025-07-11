#if UNITY_ANDROID
using System;
using Unity.Services.LevelPlay;
using UnityEngine;

[Obsolete("This class will be made private in version 9.0.0.")]
public class IronSourceInitializationAndroid : AndroidJavaProxy, IUnityInitialization
{
    public event Action OnSdkInitializationCompletedEvent = delegate {};

    public IronSourceInitializationAndroid() : base(IronSourceConstants.initializeBridgeListenerClass)
    {
        try
        {
            using (var pluginClass = new AndroidJavaClass(IronSourceConstants.bridgeClass))
            {
                var bridgeInstance = pluginClass.CallStatic<AndroidJavaObject>("getInstance");
                bridgeInstance.Call("setUnityInitializationListener", this);
            }
        }
        catch (Exception e)
        {
            LevelPlayLogger.LogError("setUnityInitializationListener method doesn't exist, error: " + e.Message);
        }
    }

    void onSdkInitializationCompleted()
    {
        if (this.OnSdkInitializationCompletedEvent != null)
        {
            this.OnSdkInitializationCompletedEvent();
        }
    }
}

#endif
