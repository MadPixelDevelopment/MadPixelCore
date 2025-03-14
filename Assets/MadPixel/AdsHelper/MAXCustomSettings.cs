using System.Collections;
using System.Collections.Generic;
using MadPixel;
using UnityEngine;

namespace MAXHelper {
    [CreateAssetMenu(fileName = "MAXCustomSettings", menuName = "MAXHelper/Configs/MAXCustomSettings", order = 1)]
    public class MAXCustomSettings : ScriptableObject {
        public bool bUseRewardeds;
        public bool bUseInters;
        public bool bUseBanners;
        public bool bShowMediationDebugger;

        public string BannerID;
        public string InterstitialID;
        public string RewardedID;

        public string BannerID_IOS;
        public string InterstitialID_IOS;
        public string RewardedID_IOS;

        public Color BannerBackground;

        public string appmetricaKey;
        public string appsFlyerID_ios;

        public string levelPlayKey;
        public string levelPlayKey_ios;
        public bool useBanner = true;
        public bool useTopBannerPosition = false;

        public const string APPSFLYER_SDK_KEY = "bAfXoQibEMwiDKEGT6UHTG";


        public void Set(MAXCustomSettings other) {
            bUseRewardeds = other.bUseRewardeds;
            bUseInters = other.bUseInters;
            bUseBanners = other.bUseBanners;

            bShowMediationDebugger = other.bShowMediationDebugger;

            BannerID = other.BannerID;
            BannerID_IOS = other.BannerID_IOS;
            InterstitialID = other.InterstitialID;
            InterstitialID_IOS = other.InterstitialID_IOS;
            RewardedID = other.RewardedID;
            RewardedID_IOS = other.RewardedID_IOS;

            BannerBackground = other.BannerBackground;
            appmetricaKey = other.appmetricaKey;
            appsFlyerID_ios = other.appsFlyerID_ios;

            levelPlayKey = other.levelPlayKey;
            levelPlayKey_ios = other.levelPlayKey_ios;
            useBanner = other.useBanner;
            useTopBannerPosition = other.useTopBannerPosition;


        }
    }
}
