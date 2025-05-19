using UnityEngine;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
using System.Runtime.InteropServices;
using UnityEngine.Events;
#endif

#if UNITY_IOS && !UNITY_EDITOR

namespace AudienceNetwork
{
    public static class AdSettings
    {
        [DllImport("__Internal")] 
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
        {
            FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
        }
    }
}

#endif


namespace MadPixel{
    
    // NOTE: Disable component by default
    public class ATTIOSDialogHelper : MonoBehaviour {
#if UNITY_IOS
        #region Fields
        private UnityAction<ATTrackingStatusBinding.AuthorizationTrackingStatus> m_onChangeCallback;

        private bool m_waitForResponse = false;
        #endregion
        

        #region Unity Event Functions
        private void Update(){
#if UNITY_IOS
            if (m_waitForResponse){
                ATTrackingStatusBinding.AuthorizationTrackingStatus status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                if (status != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED){
                    m_onChangeCallback?.Invoke(status);
                    m_waitForResponse = false;
                    enabled = false;
                }
            }
#endif
        }
        #endregion

        #region Public
        public void BeginPlay(UnityAction<ATTrackingStatusBinding.AuthorizationTrackingStatus> a_onAuthTrackingStatusChangeCallback){
            m_onChangeCallback = a_onAuthTrackingStatusChangeCallback;
            ATTrackingStatusBinding.AuthorizationTrackingStatus status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED) {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
                enabled = true;
                m_waitForResponse = true;
            } else {
                m_onChangeCallback?.Invoke(status);
            }
        }
        #endregion
#endif
    }
}