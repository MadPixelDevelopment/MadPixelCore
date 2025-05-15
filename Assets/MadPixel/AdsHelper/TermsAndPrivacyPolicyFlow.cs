using System;
using System.Collections;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif


namespace MAXHelper {

    public class TermsAndPrivacyPolicyFlow : MonoBehaviour {

        private const string TermsAcceptedKey = "UserAcceptTerms";

        #region Fields
        public event UnityAction<bool> e_onTermsAccepted;

        [SerializeField] protected UITermsPanel TermsPanelPrefab;
        [SerializeField] protected Transform PanelParentCanvas;

        private UITermsPanel m_madPixelTermsPanel;

        private bool MadPixelTermsAcceptedFlag{
            get{ return(PlayerPrefs.GetInt(TermsAcceptedKey, 0) != 0); }
            set { PlayerPrefs.SetInt(TermsAcceptedKey, value ? 1 : 0); }
        }
        #endregion

        #region Public
        public void StartFlow() {
            MobileAds.RaiseAdEventsOnUnityMainThread = true; // NOTE: This is mandatory for Google UMP
            GatherUMPConsent(OnUMPConsentUpdated);
        }
        #endregion

        #region Helpers

#if UNITY_IOS

        private void StartIOSSpecificATTFlow(){
            ATTIOSDialogHelper attHelperComponent = GetComponent<ATTIOSDialogHelper>();
            if (attHelperComponent){
                attHelperComponent.BeginPlay(OnAuthTrackingStatusChangeCallback);
            } else {
                Debug.LogError($"There is no ATT HELPER present! Please fix it!", this);
            }
        }
        
        private void OnAuthTrackingStatusChangeCallback(ATTrackingStatusBinding.AuthorizationTrackingStatus a_newStatus){
            bool hasConsent = false;
#if !UNITY_EDITOR
            hasConsent = (a_newStatus == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED);
            Debug.LogWarning($"STATUS: {a_newStatus}   Has consent: {hasConsent}");
            AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(hasConsent);
#endif
            SendFlowResultEvent(hasConsent);
        }
#endif

        private void MadPixelTermsOnAcceptResultEvent() {
            ToggleSubsToMadPixelTermsPanelResult(false);
            MadPixelTermsAcceptedFlag = true;
            ConsentAcceptRoutine(MadPixelTermsAcceptedFlag);
        }

        private void GatherUMPConsent(Action<FormError> a_callback) {
            Debug.Log($"[MadPixel] gather consent: {ConsentInformation.ConsentStatus}");
            ConsentRequestParameters request = new ConsentRequestParameters();
            ConsentInformation.Update(request, a_callback);
        }

        private void OnUMPConsentUpdated(FormError a_error) {
            Debug.Log("[MadPixel] consent response");
            if (a_error == null) { // NOTE: If there is no error
                ConsentForm.LoadAndShowConsentFormIfRequired(OnUMPConsentFormResult);
            } else {
                OnUMPConsentFailed(a_error);
            }
        }

        private void OnUMPConsentFormResult(FormError a_error){
            Debug.Log("[MadPixel] on consent information state updated");
            if (a_error == null){ // NOTE: If there is no error
                if (ConsentInformation.ConsentStatus == ConsentStatus.NotRequired){
                    MadPixelTermsAndPrivacyPolicyFlow();
                } else { 
                    ConsentAcceptRoutine(true);
                }
            } else{
                OnUMPConsentFailed(a_error);
            }
        }

        private void OnUMPConsentFailed(FormError a_error){
            Debug.LogError(a_error.Message);
            MadPixelTermsAndPrivacyPolicyFlow();
        }

        private void MadPixelTermsAndPrivacyPolicyFlow(){
            
            Debug.LogWarning($"UMP CONSENT STATUS: {ConsentInformation.ConsentStatus}");
            if (ConsentInformation.ConsentStatus == ConsentStatus.Obtained){
                MadPixelTermsAcceptedFlag = true; // NOTE: Means we are in NOT GDPR region or already gathered consent
            }
            if (MadPixelTermsAcceptedFlag) {
                ConsentAcceptRoutine(MadPixelTermsAcceptedFlag);
            } else{
                ShowMadPixelTermsPanel();
            }
        }

        private void ShowMadPixelTermsPanel(){
            Transform panelParent = PanelParentCanvas;
            if (!panelParent) {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas) {
                    panelParent = canvas.transform;
                }
            }
            m_madPixelTermsPanel = Instantiate(TermsPanelPrefab, panelParent);
            
            ToggleSubsToMadPixelTermsPanelResult(true);
            
        }

        private void ToggleSubsToMadPixelTermsPanelResult(bool a_on){
            if (m_madPixelTermsPanel){
                if (a_on){
                    m_madPixelTermsPanel.e_onAcceptResult += MadPixelTermsOnAcceptResultEvent;
                } else {
                    m_madPixelTermsPanel.e_onAcceptResult -= MadPixelTermsOnAcceptResultEvent;
                }
            }
        }

        private void ConsentAcceptRoutine(bool a_hasConsent){
            Debug.LogWarning($"UMP CONSENT STATUS: {ConsentInformation.ConsentStatus}");
#if UNITY_IOS && !UNITY_EDITOR
            StartIOSSpecificATTFlow();
#else
            SendFlowResultEvent(a_hasConsent);
#endif
        }

        private void SendFlowResultEvent(bool a_hasConsent){
            e_onTermsAccepted?.Invoke(a_hasConsent);
        }

        #endregion
    }

}


