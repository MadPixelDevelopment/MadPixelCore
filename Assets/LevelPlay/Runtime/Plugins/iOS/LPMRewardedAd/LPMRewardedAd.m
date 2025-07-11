//
//  LPMRewardedAd.m
//  iOSBridge
//

#import "LPMRewardedAd.h"
#import "LPMRewardedAdCallbacksWrapper.h"
#import "LPMUtilities.h"
#import <IronSource/LPMRewardedAd.h>
#import <IronSource/LPMRewardedAdConfig.h>
#import <IronSource/LPMRewardedAdConfigBuilder.h>
#import <UIKit/UIKit.h>

#ifdef __cplusplus
extern "C" {
#endif

    void *LPMRewardedAdCreate(const char *adUnitId) {
        LPMRewardedAd *rewardedAd = [[LPMRewardedAd alloc] initWithAdUnitId:[LPMUtilities getStringFromCString:adUnitId]];

        return (__bridge_retained void *)rewardedAd;
    }

    void *LPMRewardedAdCreateWithConfig(const char *adUnitId, void *configRef) {
        NSString *adUnitIdStr = [LPMUtilities getStringFromCString:adUnitId];
        LPMRewardedAdConfig *config = (__bridge LPMRewardedAdConfig *)configRef;
        LPMRewardedAd *rewardedAd = [[LPMRewardedAd alloc] initWithAdUnitId:adUnitIdStr config:config];

        return (__bridge_retained void *)rewardedAd;
    }

    void LPMRewardedAdSetDelegate(void *rewardedAdRef, void *delegateRef) {
        LPMRewardedAd *rewardedAd = (__bridge LPMRewardedAd *)rewardedAdRef;
        LPMRewardedAdCallbacksWrapper *delegate = (__bridge LPMRewardedAdCallbacksWrapper *)delegateRef;
        [rewardedAd setDelegate:delegate];
    }

    void LPMRewardedAdLoadAd(void *rewardedAdRef) {
        LPMRewardedAd *rewardedAd = (__bridge LPMRewardedAd *)rewardedAdRef;
        [rewardedAd loadAd];
    }

    void LPMRewardedAdShowAd(void *rewardedAdRef, const char *placementName) {
        LPMRewardedAd *rewardedAd = (__bridge LPMRewardedAd *)rewardedAdRef;
        NSString *placementNameString = placementName ? [LPMUtilities getStringFromCString:placementName] : nil;
        [rewardedAd showAdWithViewController:[UIApplication sharedApplication].keyWindow.rootViewController placementName:placementNameString];
    }

    bool LPMRewardedAdIsAdReady(void *rewardedAdRef) {
        LPMRewardedAd *rewardedAd = (__bridge LPMRewardedAd *)rewardedAdRef;
        return [rewardedAd isAdReady];
    }

    bool LPMRewardedAdIsPlacementCapped(const char *placementName) {
        return [LPMRewardedAd isPlacementCapped:[LPMUtilities getStringFromCString:placementName]];
    }

    const char *LPMRewardedAdAdId(void *rewardedAdRef) {
        LPMRewardedAd *rewardedAd = (__bridge LPMRewardedAd *)rewardedAdRef;
        return strdup([[rewardedAd adId] UTF8String]);
    }

    // config
    void *LPMRewardedAdCreateConfigBuilder() {
        LPMRewardedAdConfigBuilder *builder = [[LPMRewardedAdConfigBuilder alloc] init];

        return (__bridge_retained void *)builder;
    }

    void LPMRewardedAdConfigBuilderSetBidFloor(void *builderRef, double bidFloor) {
        LPMRewardedAdConfigBuilder *builder = (__bridge LPMRewardedAdConfigBuilder *)builderRef;
        NSNumber *bidFloorNum = [NSNumber numberWithDouble:bidFloor];
        [builder setWithBidFloor:bidFloorNum];
    }

    void *LPMRewardedAdConfigBuilderBuild(void *builderRef) {
        LPMRewardedAdConfigBuilder *builder = (__bridge LPMRewardedAdConfigBuilder *)builderRef;
        LPMRewardedAdConfig *config = [builder build];

        return (__bridge_retained void *)config;
    }

#ifdef __cplusplus
}
#endif
