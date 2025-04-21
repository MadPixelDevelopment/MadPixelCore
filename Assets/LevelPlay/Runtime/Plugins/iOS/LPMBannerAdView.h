// LPMBannerAdView.h

#import <Foundation/Foundation.h>
#import <IronSource/IronSource.h>

#ifdef __cplusplus
extern "C" {
#endif

void *LPMBannerAdViewCreate(const char *adUnitId, const char *placementName,
                            LPMAdSize *adSize);
void LPMBannerAdViewSetDelegate(void *bannerAdViewRef, void *delegateRef);

void LPMBannerAdViewLoadAd(void *bannerAdViewRef);
void LPMBannerAdViewDestroy(void *bannerAdViewRef);
void LPMBannerAdViewSetPosition(void *bannerAdViewRef, const char *positionDescription, float x, float y);
void LPMBannerAdViewShow(void *bannerAdViewRef);
void LPMBannerAdViewHide(void *bannerAdViewRef);

void LPMBannerAdViewPauseAutoRefresh(void *bannerAdViewRef);
void LPMBannerAdViewResumeAutoRefresh(void *bannerAdViewRef);

const char *LPMBannerAdViewAdId(void *bannerAdViewRef);

#ifdef __cplusplus
}
#endif
