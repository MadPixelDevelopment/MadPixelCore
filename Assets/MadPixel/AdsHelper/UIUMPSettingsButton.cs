using System;
using System.Collections;
using System.Collections.Generic;
using MAXHelper;
using UnityEngine;
using UnityEngine.UI;

namespace MergeFight {
    
    [RequireComponent(typeof(Button))]
    public class UIUMPSettingsButton : MonoBehaviour {


        #region Fields
        private Button m_button;
        #endregion

        #region Unity Event Functions

        private void Awake() {
            m_button = GetComponent<Button>();
        }

        private void OnEnable() {
            bool activeFlag = AdsManager.IsGDPR();
            gameObject.SetActive(activeFlag);
            if (activeFlag) {
                m_button.onClick.AddListener(OnUMPButtonClick);
            }
        }

        private void OnUMPButtonClick() {
            AdsManager.ShowCMPFlow();
        }

        #endregion
        
        
    }
}
