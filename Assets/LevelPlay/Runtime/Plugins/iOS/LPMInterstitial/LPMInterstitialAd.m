//
//  LPMInterstitialAd.m
//  iOSBridge
//

#import "LPMInterstitialAd.h"
#import "LPMInterstitialAdCallbacksWrapper.h"
#import "LPMUtilities.h"
#import <IronSource/LPMInterstitialAd.h>
#import <IronSource/LPMInterstitialAdConfig.h>
#import <IronSource/LPMInterstitialAdConfigBuilder.h>
#import <UIKit/UIKit.h>

#ifdef __cplusplus
extern "C" {
#endif

    void *LPMInterstitialAdCreate(const char *adUnitId) {
        LPMInterstitialAd *interstitialAd = [[LPMInterstitialAd alloc] initWithAdUnitId:[LPMUtilities getStringFromCString:adUnitId]];

        return (__bridge_retained void *)interstitialAd;
    }

    void *LPMInterstitialAdCreateWithConfig(const char *adUnitId, void *configRef) {
        NSString *adUnitIdStr = [LPMUtilities getStringFromCString:adUnitId];
        LPMInterstitialAdConfig *config = (__bridge LPMInterstitialAdConfig *)configRef;
        LPMInterstitialAd *interstitialAd = [[LPMInterstitialAd alloc] initWithAdUnitId:adUnitIdStr config:config];

        return (__bridge_retained void *)interstitialAd;
    }

    void LPMInterstitialAdSetDelegate(void *interstitialAdRef, void *delegateRef) {
        LPMInterstitialAd *interstitialAd = (__bridge LPMInterstitialAd *)interstitialAdRef;
        LPMInterstitialAdCallbacksWrapper *delegate = (__bridge LPMInterstitialAdCallbacksWrapper *)delegateRef;
        [interstitialAd setDelegate:delegate];
    }

    void LPMInterstitialAdLoadAd(void *interstitialAdRef) {
        LPMInterstitialAd *interstitialAd = (__bridge LPMInterstitialAd *)interstitialAdRef;
        [interstitialAd loadAd];
    }

    void LPMInterstitialAdShowAd(void *interstitialAdRef, const char *placementName) {
        LPMInterstitialAd *interstitialAd = (__bridge LPMInterstitialAd *)interstitialAdRef;
        NSString *placementNameString = placementName ? [LPMUtilities getStringFromCString:placementName] : nil;
        [interstitialAd showAdWithViewController:[UIApplication sharedApplication].keyWindow.rootViewController placementName:placementNameString];
    }

    bool LPMInterstitialAdIsAdReady(void *interstitialAdRef) {
        LPMInterstitialAd *interstitialAd = (__bridge LPMInterstitialAd *)interstitialAdRef;
        return [interstitialAd isAdReady];
    }

    bool LPMInterstitialAdIsPlacementCapped(const char *placementName) {
        return [LPMInterstitialAd isPlacementCapped:[LPMUtilities getStringFromCString:placementName]];
    }

    const char *LPMInterstitialAdAdId(void *interstitialAdRef) {
        LPMInterstitialAd *interstitialAd = (__bridge LPMInterstitialAd *)interstitialAdRef;
        return strdup([[interstitialAd adId] UTF8String]);
    }

    // config
    void *LPMInterstitialAdCreateConfigBuilder() {
        LPMInterstitialAdConfigBuilder *builder = [[LPMInterstitialAdConfigBuilder alloc] init];

        return (__bridge_retained void *)builder;
    }

    void LPMInterstitialAdConfigBuilderSetBidFloor(void *builderRef, double bidFloor) {
        LPMInterstitialAdConfigBuilder *builder = (__bridge LPMInterstitialAdConfigBuilder *)builderRef;
        NSNumber *bidFloorNum = [NSNumber numberWithDouble:bidFloor];
        [builder setWithBidFloor:bidFloorNum];
    }

    void *LPMInterstitialAdConfigBuilderBuild(void *builderRef) {
        LPMInterstitialAdConfigBuilder *builder = (__bridge LPMInterstitialAdConfigBuilder *)builderRef;
        LPMInterstitialAdConfig *config = [builder build];

        return (__bridge_retained void *)config;
    }

#ifdef __cplusplus
}
#endif
