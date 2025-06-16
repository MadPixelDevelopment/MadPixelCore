using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MadPixel.Demo {
    public class InterstitialButton : MonoBehaviour {
        #region Fields
        private Button m_myButton;
        #endregion

        #region Unity Events

        private void Start() {
            m_myButton = GetComponent<Button>();
            if (m_myButton != null) {
                m_myButton.onClick.AddListener(OnAdClick);
            }
            else {
                Debug.LogError("[MadPixel] Please add a Button component!");
            }
        }

        #endregion

        public void OnAdClick() {
            m_myButton.enabled = false;

            // NOTE: Switch is implemented to show you how to work with Result codes.
            AdsManager.EResultCode result = AdsManager.ShowInter(this.gameObject, OnInterDismissed, "inter_placement");
            switch (result) {
                case AdsManager.EResultCode.ADS_FREE:
                    Debug.Log("[MadPixel] User bought adsfree and has no inters");
                    m_myButton.enabled = true;
                    break;

                case AdsManager.EResultCode.NOT_LOADED:
                    Debug.Log("[MadPixel] Ad has not been loaded yet");
                    m_myButton.enabled = true;
                    break;

                case AdsManager.EResultCode.ON_COOLDOWN:
                    float seconds = AdsManager.CooldownLeft;
                    Debug.Log($"[MadPixel] Cooldown for ad has not finished! Can show inter in {seconds} seconds"); 
                    m_myButton.enabled = true;
                    break;

                case AdsManager.EResultCode.OK:
                    Debug.Log("[MadPixel] Inter was shown");
                    break;
            }
        }


        private void OnInterDismissed(bool bSuccess) {
            Debug.Log($"[MadPixel] User dismissed the interstitial ad");

            m_myButton.enabled = true;
        }

    }
}