using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadPixel {
    public class AdInfo  {
        public string placement;
        public AdsManager.EAdType adType;
        public bool hasInternet;
        public string availability;

        public AdInfo(string a_placement, AdsManager.EAdType a_adType, bool a_hasInternet = true, string a_availability = "available") {
            this.hasInternet = a_hasInternet;
            this.placement = a_placement;
            this.adType = a_adType;
            this.availability = a_availability;
        }
    }
}
