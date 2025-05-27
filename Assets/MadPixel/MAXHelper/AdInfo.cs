using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadPixel {
    public class AdInfo  {
        public string Placement;
        public AdsManager.EAdType AdType;
        public bool HasInternet;
        public string Availability;

        public AdInfo(string a_placement, AdsManager.EAdType a_adType, bool a_hasInternet = true, string a_availability = "available") {
            this.HasInternet = a_hasInternet;
            this.Placement = a_placement;
            this.AdType = a_adType;
            this.Availability = a_availability;
        }
    }
}
