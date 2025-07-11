//
//  LPMRewardedAd.h
//  iOSBridge
//

#import <Foundation/Foundation.h>
#import <IronSource/IronSource.h>

#ifdef __cplusplus
extern "C" {
#endif

void *LPMRewardedAdCreate(const char *adUnitId);
void *LPMRewardedAdCreateWithConfig(const char *adUnitId, void *configRef);
void LPMRewardedAdSetDelegate(void *rewardedAdRef, void *delegateRef);

void LPMRewardedAdLoadAd(void *rewardedAdRef);
void LPMRewardedAdShowAd(void *rewardedAdRef, const char *placementName);
bool LPMRewardedAdIsAdReady(void *rewardedAdRef);

bool LPMRewardedAdIsPlacementCapped(const char *placementName);

const char *LPMRewardedAdAdId(void *rewardedAdRef);

// Config
void *LPMRewardedAdCreateConfigBuilder();
void LPMRewardedAdConfigBuilderSetBidFloor(void *builderRef, double bidFloor);
void *LPMRewardedAdConfigBuilderBuild(void *builderRef);

#ifdef __cplusplus
}
#endif
