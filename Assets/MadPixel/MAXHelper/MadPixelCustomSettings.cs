using UnityEngine;

namespace MadPixel {
    [CreateAssetMenu(fileName = "MadPixelCustomSettings", menuName = "MadPixel/Configs/MadPixelCustomSettings", order = 1)]
    public class MadPixelCustomSettings : ScriptableObject {
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

        public const string APPLOVIN_SDK_KEY = "R5ZeDg0t8rV5BQ4h_72SUwzDKUOipd1Ju_H3yph9eKZV6NZBDqI_rLKZmyFWiyFWdOn4ITSHwMdob2TtWHuzio";
        public const string APPSFLYER_SDK_KEY = "bAfXoQibEMwiDKEGT6UHTG";


        public void Set(MadPixelCustomSettings other) {
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
        }
    }
}
