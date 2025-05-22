using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MadPixel {
    
    public class UITermsPanel : MonoBehaviour {
        
        public event UnityAction e_onAcceptResult;

        #region Fields
        [SerializeField] protected Button AcceptButton;
        [SerializeField] protected Button PrivacyButton;
        [SerializeField] protected Button TermsButton;
        [SerializeField] protected string PrivacyLink;
        #endregion

        #region Unity Event Functions

        private void Awake() {
            AcceptButton.onClick.AddListener(OnAcceptClick);
            PrivacyButton.onClick.AddListener(OnPrivacyClick);
            if (TermsButton != null) {
                TermsButton.onClick.AddListener(OnPrivacyClick);
            }
        }

        private void Start() {
            transform.SetAsLastSibling();
        }

        private void OnDestroy() {
            AcceptButton.onClick.RemoveAllListeners();
            PrivacyButton.onClick.RemoveAllListeners();
            if (TermsButton != null) {
                TermsButton.onClick.RemoveAllListeners();
            }
        }
        #endregion

        #region Helpers
        protected virtual void OnAcceptClick() {
            e_onAcceptResult?.Invoke();
            Hide();
        }

        protected virtual void OnPrivacyClick() {
            if (!string.IsNullOrEmpty(PrivacyLink)) {
                Application.OpenURL(PrivacyLink);
            }
        }

        protected virtual void Hide() {
            Destroy(gameObject);
        }
        #endregion

    }
    
}


