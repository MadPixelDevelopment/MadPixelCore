using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Services.LevelPlay;

#pragma warning disable 0618

/// <summary>
/// LevelPlay's API Access point
/// </summary>
public class IronSource : IronSourceIAgent
{
    private IronSourceIAgent _platformAgent;
    private static IronSource _instance;
    [Obsolete("Please use the pluginVersion function", false)]
    public static string UNITY_PLUGIN_VERSION = Constants.AnnotatedPackageVersion;
    private static bool isUnsupportedPlatform;

    private IronSource()
    {
        if (!isUnsupportedPlatform)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            _platformAgent = new UnsupportedPlatformAgent();
#elif (UNITY_IPHONE || UNITY_IOS)
            _platformAgent = new iOSAgent();
#elif UNITY_ANDROID
            _platformAgent = new AndroidAgent();
#endif
        }
        else
        {
            _platformAgent = new UnsupportedPlatformAgent();
        }
        var ironSourceType = typeof(IronSourceEvents);
        var ironSourceRewardedType = typeof(IronSourceRewardedVideoEvents);
        var ironSourceInterstitialType = typeof(IronSourceInterstitialEvents);
        var ironSourceBannerType = typeof(IronSourceBannerEvents);
        var ironSourceEvents = new GameObject("IronSourceEvents", ironSourceType).GetComponent<IronSourceEvents>(); // Creates IronSourceEvents gameObject
        var ironSourceRewardedVideoEvents = new GameObject("IronSourceRewardedVideoEvents", ironSourceRewardedType).GetComponent<IronSourceRewardedVideoEvents>(); // Creates IronSourceRewardedVideoEvents gameObject
        var ironSourceInterstitialEvents = new GameObject("IronSourceInterstitialEvents", ironSourceInterstitialType).GetComponent<IronSourceInterstitialEvents>(); // Creates IronSourceInterstitialEvents gameObject
        var ironSourceBannerEvents = new GameObject("IronSourceBannerEvents", ironSourceBannerType).GetComponent<IronSourceBannerEvents>(); // Creates IronSourceBannerEvents gameObject
    }

    #region IronSourceIAgent implementation
    /// <summary>
    /// Returns a reference to the platform's LevelPlay API.
    /// </summary>
    public static IronSource Agent
    {
        get
        {
            if (_instance == null)
            {
                _instance = new IronSource();
            }
            return _instance;
        }
    }

    [Obsolete("Use LevelPlay.PluginVersion instead.")]
    /// <summary>
    /// LevelPlay plugin version.
    /// </summary>
    /// <returns>LevelPlay's version</returns>
    public static string pluginVersion()
    {
        return UNITY_PLUGIN_VERSION;
    }

    [Obsolete("Use LevelPlay.UnityVersion instead.")]
    /// <summary>
    /// Version of the Unity runtime used to play the content.
    /// </summary>
    /// <returns>Version of the Unity runtime used to play the content</returns>
    public static string unityVersion()
    {
        return Application.unityVersion;
    }

    [Obsolete("This method is deprecated and will be removed in version 9.0.0.")]
    /// <summary>
    /// Flags the current platform as unsupported for LevelPlay.
    /// </summary>
    public static void setUnsupportedPlatform()
    {
        isUnsupportedPlatform = true;
    }

    //******************* Base API *******************//

    /// <summary>
    /// Allows publishers to set configurations for a waterfall of a given ad type.
    /// </summary>
    /// <param name="waterfallConfiguration">The configuration for the given ad types waterfall. </param>
    /// <param name="adFormat">The AdFormat for which to configure the waterfall.</param>
    [Obsolete("This method is obsolete. Use LevelPlayBannerAd, LevelPlayInterstitialAd, or LevelPlayRewardedAd APIs instead. Refer to the documentation for details.")]
    public void SetWaterfallConfiguration(WaterfallConfiguration waterfallConfiguration, AdFormat adFormat)
    {
        _platformAgent.SetWaterfallConfiguration(waterfallConfiguration, adFormat);
    }

    [Obsolete("This method is deprecated and will be removed in version 9.0.0.")]
    /// <summary>
    /// Notifies the LevelPlay SDK that the application has been paused or unpaused.
    /// </summary>
    /// <param name="pause">Is the application paused</param>
    public void onApplicationPause(bool pause)
    {
        _platformAgent.onApplicationPause(pause);
    }

    [Obsolete("This method is deprecated and will be removed in version 9.0.0.")]
    /// <summary>
    /// Returns the advertiser ID.
    /// </summary>
    /// <returns>Advertiser ID</returns>
    public string getAdvertiserId()
    {
        return _platformAgent.getAdvertiserId();
    }

    [Obsolete("Use LevelPlay.ValidateIntegration() instead.")]
    /// <summary>
    /// Runs the integration validation.
    /// </summary>
    public void validateIntegration()
    {
        _platformAgent.validateIntegration();
    }

    [Obsolete("This method is deprecated and will be removed in version 9.0.0.")]
    /// <summary>
    /// Enable or disable tracking of the network state.
    /// By default this is set to false.
    /// </summary>
    /// <param name="track">Should the network state be tracked</param>
    public void shouldTrackNetworkState(bool track)
    {
        _platformAgent.shouldTrackNetworkState(track);
    }

    [Obsolete("Use the LevelPlay.setDynamicUserId(string dynamicUserId) method instead.")]
    /// <summary>
    /// Sets a dynamic user ID that can be changed through the session and will be used in server to server rewarded
    /// ad callbacks.
    /// This parameter helps verify AdRewarded transactions and must be set before calling ShowRewardedVideo.
    /// </summary>
    /// <param name="dynamicUserId">The ID to be set</param>
    /// <returns>Was the dynamic user ID set successfully</returns>
    public bool setDynamicUserId(string dynamicUserId)
    {
        return _platformAgent.setDynamicUserId(dynamicUserId);
    }

    [Obsolete("Use LevelPlay.SetAdaptersDebug(bool enabled) instead.")]
    /// <summary>
    /// Enables or disables adapters debug info.
    /// </summary>
    /// <param name="enabled">Is adapters debug info enabled</param>
    public void setAdaptersDebug(bool enabled)
    {
        _platformAgent.setAdaptersDebug(enabled);
    }

    [Obsolete("Use LevelPlay.SetMetaData(string key, string value) instead.")]
    /// <summary>
    /// Allows setting extra flags, for example "do_not_sell" to allow or disallow selling or sharing personal information.
    /// </summary>
    /// <param name="key">The flag to set</param>
    /// <param name="value">the value for the flag</param>
    public void setMetaData(string key, string value)
    {
        _platformAgent.setMetaData(key, value);
    }

    [Obsolete("Use LevelPlay.SetMetaData(string key, params string[] values) instead.")]
    /// <summary>
    /// Allows setting extra flags, for example "do_not_sell" to allow or disallow selling or sharing personal information.
    /// </summary>
    /// <param name="key">The flag to set</param>
    /// <param name="values">the values for the flag</param>
    public void setMetaData(string key, params string[] values)
    {
        _platformAgent.setMetaData(key, values);
    }

    [Obsolete("This method is deprecated and will be removed in version 9.0.0.")]
    /// <summary>
    /// Returns the conversion value or CV.
    /// </summary>
    /// <returns>Conversion value</returns>
    public int? getConversionValue()
    {
        return _platformAgent.getConversionValue();
    }

    [Obsolete("Use LevelPlayRewardedAd instead.")]
    /// <summary>
    /// Toggles manual loading of rewarded videos.
    /// </summary>
    /// <param name="isOn">Are rewarded videos loaded manually.</param>
    public void setManualLoadRewardedVideo(bool isOn)
    {
        _platformAgent.setManualLoadRewardedVideo(isOn);
    }

    [Obsolete("Use LevelPlay.SetNetworkData(string networkKey, string networkData) instead.")]
    /// <summary>
    /// Set custom network data.
    /// </summary>
    /// <param name="networkKey">The attribute key</param>
    /// <param name="networkData">The attribute value</param>
    public void setNetworkData(string networkKey, string networkData)
    {
        _platformAgent.setNetworkData(networkKey, networkData);
    }

    [Obsolete("Use LevelPlay.SetPauseGame(bool pause) instead.")]
    /// <summary>
    /// When setting your PauseGame status to true, all your Unity 3D game activities will be paused (Except the ad callbacks).
    /// The game activity will be resumed automatically when the ad is closed.
    /// You should call the setPauseGame once in your session, before or after initializing the ironSource SDK,
    /// as it affects all ads (Rewarded Video and Interstitial ads) in the session.
    /// </summary>
    /// <param name="pause">Is the game paused</param>
    public void SetPauseGame(bool pause)
    {
        _platformAgent.SetPauseGame(pause);
    }

    //******************* SDK Init *******************//

    [Obsolete("Use LevelPlay.Init(string appKey, string userId) instead.")]
    /// <summary>
    /// Sets the user's identifier.
    /// If you’re serving the offerwall ad unit or using server-to-server callbacks to reward your users with ironSource
    /// SDK rewarded ad units, you must define a unique identifier for each user using this method.
    /// Set the UserID before the init request, to make sure you avoid any data loses, related to the user.
    /// Use a unique identifier, with up to 64 alphanumeric characters.
    /// </summary>
    /// <param name="userId">User's ID</param>
    public void setUserId(string userId)
    {
        _platformAgent.setUserId(userId);
    }

    [Obsolete("Use the LevelPlay.Init(string appKey, string userId, LevelPlayAdFormat[] adFormats) method instead.")]
    /// <summary>
    /// Initialize the LevelPlay SDK.
    /// </summary>
    /// <param name="appKey">Application key</param>
    public void init(string appKey)
    {
        _platformAgent.init(appKey);
    }

    [Obsolete("Use the LevelPlay.Init(string appKey, string userId, LevelPlayAdFormat[] adFormats) method instead.")]
    /// <summary>
    /// Initialize the LevelPlay SDK for specific ad unit types.
    /// For example IronSourceAdUnits.REWARDED_VIDEO would initialize for rewarded videos.
    /// </summary>
    /// <param name="appKey">Application key</param>
    /// <param name="adUnits">List of ad unit types to initialize</param>
    public void init(string appKey, params string[] adUnits)
    {
        _platformAgent.init(appKey, adUnits);
    }

    //******************* RewardedVideo API *******************//

    [Obsolete("Use LevelPlayRewardedAd.LoadAd() instead.")]
    /// <summary>
    /// Load a Rewarded video.
    /// </summary>
    public void loadRewardedVideo()
    {
        _platformAgent.loadRewardedVideo();
    }

    [Obsolete("Use LevelPlayRewardedAd.ShowAd() instead.")]
    /// <summary>
    /// Show a rewarded video
    /// </summary>
    public void showRewardedVideo()
    {
        _platformAgent.showRewardedVideo();
    }

    [Obsolete("Use LevelPlayRewardedAd.ShowAd(string placementName) instead.")]
    /// <summary>
    /// Show a rewarded video with a specific placement.
    /// </summary>
    /// <param name="placementName">Name of the placement</param>
    public void showRewardedVideo(string placementName)
    {
        _platformAgent.showRewardedVideo(placementName);
    }

    [Obsolete("This method is deprecated and will be removed in version 9.0.0.")]
    /// <summary>
    /// Retrieve a placement by name.
    /// </summary>
    /// <param name="placementName">Placement's name</param>
    /// <returns>An IronSourcePlacement containing the placement infos</returns>
    public IronSourcePlacement getPlacementInfo(string placementName)
    {
        return _platformAgent.getPlacementInfo(placementName);
    }

    [Obsolete("Use LevelPlayRewardedAd.IsAdReady() instead.")]
    /// <summary>
    /// Returns the availability of a rewarded video.
    /// </summary>
    /// <returns>Is a rewarded video available</returns>
    public bool isRewardedVideoAvailable()
    {
        return _platformAgent.isRewardedVideoAvailable();
    }

    [Obsolete("Use LevelPlayRewardedAd.IsPlacementCapped(string placementName) instead.")]
    /// <summary>
    /// Determines if a rewarded video placement is capped.
    /// </summary>
    /// <param name="placementName">Name of the placement</param>
    /// <returns>Is the placement capped</returns>
    public bool isRewardedVideoPlacementCapped(string placementName)
    {
        return _platformAgent.isRewardedVideoPlacementCapped(placementName);
    }

    [Obsolete("This method is deprecated and will be removed in version 9.0.0.")]
    /// <summary>
    /// Set custom parameters for server-to-server completion callbacks.
    /// </summary>
    /// <param name="parameters">Key-Value list of parameters</param>
    public void setRewardedVideoServerParams(Dictionary<string, string> parameters)
    {
        _platformAgent.setRewardedVideoServerParams(parameters);
    }

    [Obsolete("This method is deprecated and will be removed in version 9.0.0.")]
    /// <summary>
    /// Clears custom parameters for server-to-server completion callbacks.
    /// </summary>
    public void clearRewardedVideoServerParams()
    {
        _platformAgent.clearRewardedVideoServerParams();
    }

    //******************* Interstitial API *******************//

    [Obsolete("Use the LevelPlayInterstitialAd.LoadAd() method instead.")]
    /// <summary>
    /// Load an interstitial video
    /// </summary>
    public void loadInterstitial()
    {
        _platformAgent.loadInterstitial();
    }

    [Obsolete("Use the LevelPlayInterstitialAd.ShowAd() method instead.")]
    /// <summary>
    /// Show an interstitial video
    /// </summary>
    public void showInterstitial()
    {
        _platformAgent.showInterstitial();
    }

    [Obsolete("Use the LevelPlayInterstitialAd.ShowAd(string placementName) method instead.")]
    /// <summary>
    /// Show an interstitial video for a specific placement.
    /// </summary>
    /// <param name="placementName">Name of the placement</param>
    public void showInterstitial(string placementName)
    {
        _platformAgent.showInterstitial(placementName);
    }

    [Obsolete("Use the LevelPlayInterstitialAd.IsAdReady() method instead.")]
    /// <summary>
    /// Determines if an interstitial video is ready.
    /// </summary>
    /// <returns>Is an interstitial video ready</returns>
    public bool isInterstitialReady()
    {
        return _platformAgent.isInterstitialReady();
    }

    [Obsolete("Use the LevelPlayInterstitialAd.IsPlacementCapped(string placementName) method instead.")]
    /// <summary>
    /// Determines if an interstitial placement is capped.
    /// </summary>
    /// <param name="placementName">Name of the placement</param>
    /// <returns>Is the placement capped</returns>
    public bool isInterstitialPlacementCapped(string placementName)
    {
        return _platformAgent.isInterstitialPlacementCapped(placementName);
    }

    //******************* Banner API *******************//
    [Obsolete("Use the LevelPlayBannerAd.LoadAd() method instead.")]
    /// <summary>
    /// Load a banner.
    /// </summary>
    /// <param name="size">Standard size of the banner</param>
    /// <param name="position">Anchored position of the banner</param>
    public void loadBanner(IronSourceBannerSize size, IronSourceBannerPosition position)
    {
        _platformAgent.loadBanner(size, position);
    }

    [Obsolete("Use the LevelPlayBannerAd.LoadAd() method instead.")]
    /// <summary>
    /// Load and show a banner for a specific placement.
    /// </summary>
    /// <param name="size">Standard size of the banner</param>
    /// <param name="position">Anchored position of the banner</param>
    /// <param name="placementName">Name of the placement</param>
    public void loadBanner(IronSourceBannerSize size, IronSourceBannerPosition position, string placementName)
    {
        _platformAgent.loadBanner(size, position, placementName);
    }

    [Obsolete("Use the LevelPlayBannerAd.DestroyAd() method instead.")]
    /// <summary>
    /// Destroy a banner.
    /// </summary>
    public void destroyBanner()
    {
        _platformAgent.destroyBanner();
    }

    [Obsolete("Use the LevelPlayBannerAd.ShowAd() method instead.")]
    /// <summary>
    /// Show a banner.
    /// </summary>
    public void displayBanner()
    {
        _platformAgent.displayBanner();
    }

    [Obsolete("Use the LevelPlayBannerAd.HideAd() method instead.")]
    /// <summary>
    /// Hide a banner.
    /// </summary>
    public void hideBanner()
    {
        _platformAgent.hideBanner();
    }

    [Obsolete("This method will be removed in version 9.0.0.")]
    /// <summary>
    /// Determines if a banner placement is capped.
    /// </summary>
    /// <param name="placementName">Name of the placement</param>
    /// <returns>Is the banner placement capped</returns>
    public bool isBannerPlacementCapped(string placementName)
    {
        return _platformAgent.isBannerPlacementCapped(placementName);
    }

    [Obsolete("This method will be removed in version 9.0.0.")]
    /// <summary>
    ///  Get maximal Adaptive Height.
    /// </summary>
    /// <param name="width"> The device width </param>
    public float getMaximalAdaptiveHeight(float width)
    {
        return _platformAgent.getMaximalAdaptiveHeight(width);
    }

    [Obsolete("This method will be removed in version 9.0.0.")]
    /// <summary>
    ///  Get Device Screen Width.
    /// </summary>
    public float getDeviceScreenWidth()
    {
        return _platformAgent.getDeviceScreenWidth();
    }

    [Obsolete("Use LevelPlay.SetSegment(LevelPlaySegment segment) instead.")]
    /// <summary>
    /// Set the segment a user belongs to.
    /// </summary>
    /// <param name="segment">Segment information for the current user</param>
    public void setSegment(IronSourceSegment segment)
    {
        _platformAgent.setSegment(segment);
    }

    [Obsolete("Use LevelPlay.SetConsent(bool consent) instead.")]
    /// <summary>
    /// Set user's GDPR consent
    /// </summary>
    /// <param name="consent">Is the user consenting</param>
    public void setConsent(bool consent)
    {
        _platformAgent.setConsent(consent);
    }

    //******************* ConsentView API *******************//

    [Obsolete("This method is deprecated and will be removed in version 9.0.0.")]
    /// <summary>
    /// Loads the consent view.
    /// </summary>
    /// <param name="consentViewType">View type for the consent popup</param>
    public void loadConsentViewWithType(string consentViewType)
    {
        _platformAgent.loadConsentViewWithType(consentViewType);
    }

    [Obsolete("This method is deprecated and will be removed in version 9.0.0.")]
    /// <summary>
    /// Show the consent view
    /// </summary>
    /// <param name="consentViewType">View type for the consent popup</param>
    public void showConsentViewWithType(string consentViewType)
    {
        _platformAgent.showConsentViewWithType(consentViewType);
    }

    //******************* ILRD API *******************//


    /// <summary>
    /// Set ILRD information
    /// </summary>
    /// <param name="dataSource">Source of the ILRD info</param>
    /// <param name="impressionData">Key-value pair list containing the ILRD data</param>
    public void setAdRevenueData(string dataSource, Dictionary<string, string> impressionData)
    {
        _platformAgent.setAdRevenueData(dataSource, impressionData);
    }

    //******************* TestSuite API *******************//

    [Obsolete("Use LevelPlay.LaunchTestSuite() instead.")]
    public void launchTestSuite()
    {
        _platformAgent.launchTestSuite();
    }

    #endregion
}
