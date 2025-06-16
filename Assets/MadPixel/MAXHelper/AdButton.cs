using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MadPixel.Demo {
    public class AdButton : MonoBehaviour {
        #region Fields
        [SerializeField] private string m_placement = "revive_hero";
        private Button m_myButton;
        private UnityAction<bool> m_callback;
        #endregion

        #region Unity Events
        private void Start() {
            m_myButton = GetComponent<Button>();
            if (m_myButton != null) {
                m_myButton.onClick.AddListener(OnAdClick);
            } else {
                Debug.LogError("[MadPixel] Please add a Button component!");
            }
        }
        #endregion


        #region Public
        public void OnAdClick() {
            m_myButton.enabled = false; 
            
            AdsManager.EResultCode result = AdsManager.ShowRewarded(this.gameObject, OnFinishAds, m_placement);
            if (result != AdsManager.EResultCode.OK) {
                Debug.Log("[MadPixel] Ad has not been loaded yet");
                m_myButton.enabled = true;
            }
        }
        
        #endregion

        #region Helpers
        private void OnFinishAds(bool a_success) {
            if (a_success) {
                Debug.Log($"[MadPixel] Give reward to user!");
                
            } else {
                Debug.Log($"[MadPixel] User closed rewarded ad before it was finished");
            }
            m_myButton.enabled = true;
        }
        #endregion
    }
}