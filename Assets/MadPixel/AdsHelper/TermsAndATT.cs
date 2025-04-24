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

    public class TermsAndATT : MonoBehaviour {

        private const string TermsAcceptedKey = "UserAcceptTerms";

        #region Fields
        public event UnityAction EventOnTermsAccepted;

        [SerializeField] protected UITermsPanel TermsPanelPrefab;
        [SerializeField] protected Transform PanelParentCanvas;

        private UITermsPanel PanelInstance;
        private bool m_bTermsAccepted;
        #endregion

        #region Public
        public void BeginPlay() {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
#if UNITY_IOS && !UNITY_EDITOR
            ATTIOSDialogHelper attHelperComponent = GetComponent<ATTIOSDialogHelper>();
            if (attHelperComponent){
                attHelperComponent.BeginPlay(OnAuthTrackingStatusChangeCallback);
            } else {
                Debug.LogError($"There is no ATT HELPER present! Please fix it!", this);
            }
#else
            GatherUMPConsent();
#endif
        }
        #endregion

        #region Helpers

#if UNITY_IOS
        private void OnAuthTrackingStatusChangeCallback(ATTrackingStatusBinding.AuthorizationTrackingStatus a_newStatus){
            GatherUMPConsent();
        }
#endif

        private void ShowTermsPanel() {

            Transform PanelParent = PanelParentCanvas;
            if (!PanelParent) {
                Canvas AnyCanvas = FindFirstObjectByType<Canvas>();
                if (AnyCanvas) {
                    PanelParent = AnyCanvas.transform;
                }
            }

            if (PanelParent) {
                if (TermsPanelPrefab) {
                    PanelInstance = Instantiate(TermsPanelPrefab, PanelParent);
                    if (PanelInstance) {
                        PanelInstance.EventOnAcceptClick += PanelInstanceOnEventOnAcceptClick;
                    }
                }
            }
            else {
                Debug.LogError($"MAXHelper: Unable to find proper canvas for Terms panel!", gameObject);
            }

        }

        private void PanelInstanceOnEventOnAcceptClick() {
            PlayerPrefs.SetInt(TermsAcceptedKey, 1);
            bool bHasConsent = true;
#if UNITY_IOS && !UNITY_EDITOR
            ATTrackingStatusBinding.AuthorizationTrackingStatus Status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            bHasConsent = (Status == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED);
            // Debug.LogWarning($"STATUS: {Status}   Has consent: {bHasConsent}");
            
            AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(bHasConsent);
#endif

            //MaxSdk.SetHasUserConsent(bHasConsent);
            IronSource.Agent.setConsent(bHasConsent);


            PanelInstance.EventOnAcceptClick -= PanelInstanceOnEventOnAcceptClick;
            EventOnTermsAccepted?.Invoke();
        }

        private void GatherUMPConsent() {
            Debug.Log($"[MadPixel] gather consent: {ConsentInformation.ConsentStatus}");
            ConsentRequestParameters request = new ConsentRequestParameters();
            ConsentInformation.Update(request, OnConsentInfoUpdated);
        }

        private void OnConsentInfoUpdated(FormError consentError) {
            Debug.Log("[MadPixel] consent response");
            if (consentError != null) {
                // Handle the error.
                UnityEngine.Debug.LogError(consentError);
                TryShowOurInnerPanel();
                return;
            }

            // If the error is null, the consent information state was updated.
            // You are now ready to check if a form is available.
            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) => {
                Debug.Log("[MadPixel] on consent information state updated");
                if (formError != null) {
                    // Consent gathering failed.
                    TryShowOurInnerPanel();
                    UnityEngine.Debug.LogError(consentError);
                    return;
                }

                // Consent has been gathered.


                Debug.Log($"[MadPixel] consent status set: {ConsentInformation.ConsentStatus}");
                TryShowOurInnerPanel();
            });
        }

        private void TryShowOurInnerPanel() {
            int TermsAcceptValue = PlayerPrefs.GetInt(TermsAcceptedKey, 0);
            m_bTermsAccepted = ConsentInformation.ConsentStatus == ConsentStatus.Obtained ||
                               (TermsAcceptValue != 0) && ConsentInformation.ConsentStatus == ConsentStatus.NotRequired;
#if UNITY_EDITOR
            StartCoroutine(ForceWaitAFrame());
            return;
#endif

            if (!m_bTermsAccepted) {
                ShowTermsPanel();
            }
            else {
                EventOnTermsAccepted?.Invoke();
            }
        }

#if UNITY_EDITOR
        private IEnumerator ForceWaitAFrame() {
            yield return new WaitForEndOfFrame();
            if (!m_bTermsAccepted) {
                ShowTermsPanel();
            }
            else {
                EventOnTermsAccepted?.Invoke();
            }
        }
#endif

        #endregion
    }

}


