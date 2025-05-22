using UnityEngine;
using UnityEngine.UI;

namespace MadPixel {
    
    [RequireComponent(typeof(Button))]
    public class UIUMPSettingsButton : MonoBehaviour {


        #region Fields
        private Button m_button;
        #endregion

        #region Unity Event Functions

        private void Awake() {
            m_button = GetComponent<Button>();
            m_button.onClick.AddListener(OnUMPButtonClick);
        }

        private void OnEnable() {
            bool activeFlag = AdsManager.IsGDPR();
            gameObject.SetActive(activeFlag);
        }

        private void OnUMPButtonClick() {
            AdsManager.ShowCMPFlow();
        }

        #endregion
        
        
    }
}
