using System;

/// <summary>
/// Interface representing LevelPlay's manual rewarded video events
/// </summary>
[Obsolete("This class will be removed in version 9.0.0.")]
public interface IUnityLevelPlayRewardedVideoManual
{
    /// <summary>
    /// Event triggered when a rewarded video becomes ready
    /// </summary>
    event Action<IronSourceAdInfo> OnAdReady;

    /// <summary>
    /// Event triggered when a rewarded video fails to load
    /// </summary>
    event Action<IronSourceError> OnAdLoadFailed;
}
