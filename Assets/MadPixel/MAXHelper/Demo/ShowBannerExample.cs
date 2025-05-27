using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadPixel.Examples {
    public class ShowBannerExample : MonoBehaviour {
        private bool bBannerIsShown;

        public void OnBannerButtonClick() {
            if (AdsManager.Exist) {
                bBannerIsShown = !bBannerIsShown;
                AdsManager.ToggleBanner(bBannerIsShown);
            }
            else {
                Debug.Log("AdsManager does not exist!");
            }
        }
    } 
}
