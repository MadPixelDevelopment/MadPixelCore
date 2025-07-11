using System;

/// <summary>
/// Interface to access initialization events
/// </summary>
[Obsolete("This class will be removed in version 9.0.0.")]
public interface IUnityInitialization
{
    /// <summary>
    /// Event triggered when the SDK initialization completes successfully
    /// </summary>
    event Action OnSdkInitializationCompletedEvent;
}
