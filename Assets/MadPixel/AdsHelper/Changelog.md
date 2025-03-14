# Version 1.2.9 - February 05, 2024
* Removed Amazon

# Version 1.2.8 - November 28, 2023
* Analytics is initialized on Applovin initialized
* UMP implemented (Applovin 12.1.0)

# Version 1.2.7 - October 09, 2023
* Amazon iOS included
* Target SDK and min SDK checker (Android)
* No more PangleChina/Google AdManager
* Keywords refactored

# Version 1.2.6 - September 04, 2023
* Amazon included

# Version 1.2.5 - August 24, 2023
* AdColony and Tapjoy removed from maxpackage
* Version added to MadPixel/SetupAds window

# Version 1.2.4 - August 21, 2023
* Keywords updated
* Advertising Attribution end point
* Terms Panel v2
* Set consent to FB Audience network

# Version 1.2.3 - April 05, 2023
* Keywords
* No networkconfig file

# Version 1.2.2 - March 21, 2023
* Applovin.bInitialized flag added
* AdsManager waits for Applovin.bInitialized
* MaxSdk.SetVerboseLogging(bShowDebug) added

# Version 1.2.1 - February 13, 2023
* New privacy policy consent flow
* UITermsPanel

# Version 1.2.0 - February 8, 2023
* Cancel All Ads can cancel inters, banners or both

# Version 1.1.9 - December 12, 2022
* Yandex/MyTarget added to 
* AppLovin SDK updated to 5.6.1

# Version 1.1.5 - November 2, 2022
* Tracking for banners events (OnAdAvailable, OnAdStarted, OnAdShown) added
* AppLovin SDK updated to 11.5.4 for Android and 11.5.4 for iOS

# Version 1.1.4 - October 4, 2022
* `ShowInterWithSubstitution` added
* `ShowRewardedWithSubstitution` added
* A few summaries for methods added

# Version 1.1.3 - September 16, 2022
* `ShowInterForced` added
* InterDismissed always returns true as bSuccess

# Version 1.1.1 - September 8, 2022
* `EAdType` added. `AdInfo` class is changed a bit
* `EResultCode` for `AdsManager.ShowInter` and `AdsManager.ShowRewarded` added
* `AdsManager.CooldownLeft` added
* Yandex and MyTarget are a part of MaximumPack now
* AppLovin SDK updated to 11.4.6 for Android and 11.4.4 for iOS

# Version 1.1.0 - August 11, 2022
* No more Subscriptions and Unsubscriptions for AdsManager
* Static methods for `AdsManager.ShowInter` and `AdsManager.ShowRewarded` added
* Static `AdsManager.ToggleBanner` method added
* Logs are unified

