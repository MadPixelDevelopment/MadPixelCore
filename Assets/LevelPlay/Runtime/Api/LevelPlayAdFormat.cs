using System;

namespace com.unity3d.mediation
{
    /// <summary>
    /// Defines the types of advertisement formats available in the LevelPlay SDK.
    /// </summary>
    [Obsolete("com.unity3d.mediation.LevelPlayAdFormat is deprecated and will be removed from the LevelPlay.Init method in version 9.0.0.")]
    public enum LevelPlayAdFormat
    {
        BANNER,
        INTERSTITIAL,
        REWARDED
    }
}

namespace Unity.Services.LevelPlay
{
    /// <summary>
    /// Defines the types of advertisement formats available in the LevelPlay SDK.
    /// </summary>
    public enum LevelPlayAdFormat
    {
        BANNER,
        INTERSTITIAL,
        REWARDED
    }
}
