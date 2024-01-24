using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MAXHelper {
    public class InterstitialButton : MonoBehaviour {
        #region Fields
        private Button MyButton;
        #endregion

        #region Unity Events

        private void Start() {
            MyButton = GetComponent<Button>();
            if (MyButton != null) {
                MyButton.onClick.AddListener(OnAdClick);
            }
            else {
                Debug.LogError("[Mad Pixel] Please add a Button component!");
            }
        }

        #endregion

        public void OnAdClick() {
            MyButton.enabled = false;

            // NOTE: Switch is implemented to show you how to work with Result codes.
            AdsManager.EResultCode Result = AdsManager.ShowInter(this.gameObject, OnInterDismissed, "inter_placement");
            switch (Result) {
                case AdsManager.EResultCode.ADS_FREE:
                    Debug.Log("[Mad Pixel] User bought adsfree and has no inters");
                    MyButton.enabled = true;
                    break;

                case AdsManager.EResultCode.NOT_LOADED:
                    Debug.Log("[Mad Pixel] Ad has not been loaded yet");
                    MyButton.enabled = true;
                    break;

                case AdsManager.EResultCode.ON_COOLDOWN:
                    float Seconds = AdsManager.CooldownLeft;
                    Debug.Log($"[Mad Pixel] Cooldown for ad has not finished! Can show inter in {Seconds} seconds"); 
                    MyButton.enabled = true;
                    break;

                case AdsManager.EResultCode.OK:
                    Debug.Log("[Mad Pixel] Inter was shown");
                    break;
            }
        }


        private void OnInterDismissed(bool bSuccess) {
            Debug.Log($"[Mad Pixel] User dismissed the interstitial ad");

            MyButton.enabled = true;
        }

    }
}