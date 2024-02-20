using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MAXHelper {
    [CreateAssetMenu(fileName = "MAXCustomSettings", menuName = "MAXHelper/Configs/MAXCustomSettings", order = 1)]
    public class MAXCustomSettings : ScriptableObject {
        public bool bUseRewardeds;
        public bool bUseInters;
        public bool bUseBanners;
        public bool bShowMediationDebugger;

        public string SDKKey;

        public string BannerID;
        public string InterstitialID;
        public string RewardedID;

        public string BannerID_IOS;
        public string InterstitialID_IOS;
        public string RewardedID_IOS;

        public Color BannerBackground;
    }
}
