using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MAXHelper {
    public class AdButton : MonoBehaviour {
        #region Fields
        [SerializeField] private string Placement = "revive_hero";
        private Button MyButton;
        private UnityAction<bool> Callback;
        #endregion

        #region Unity Events
        private void Start() {
            MyButton = GetComponent<Button>();
            if (MyButton != null) {
                MyButton.onClick.AddListener(OnAdClick);
            } else {
                Debug.LogError("[Mad Pixel] Please add a Button component!");
            }
        }
        #endregion


        #region Public
        public void OnAdClick() {
            MyButton.enabled = false; 
            
            AdsManager.EResultCode Result = AdsManager.ShowRewarded(this.gameObject, OnFinishAds, Placement);
            if (Result != AdsManager.EResultCode.OK) {
                Debug.Log("[Mad Pixel] Ad has not been loaded yet");
                MyButton.enabled = true;
            }
        }
        
        #endregion

        #region Helpers
        private void OnFinishAds(bool Success) {
            if (Success) {
                Debug.Log($"[Mad Pixel] Give reward to user!");
                
            } else {
                Debug.Log($"[Mad Pixel] User closed rewarded ad before it was finished");
            }
            MyButton.enabled = true;
        }
        #endregion
    }
}