#import "LPMBannerAdView.h"
#import "LPMBannerAdViewCallbacksWrapper.h"
#import "LPMUtilities.h"
#import <IronSource/LPMBannerAdView.h>
#import <UIKit/UIKit.h>
#import "LPMBannerPosition.h"

#ifdef __cplusplus
extern "C" {
#endif
    static UIWindow *getActiveWindow(void) {
        for (UIWindowScene *scene in [UIApplication sharedApplication].connectedScenes) {
            if ([scene isKindOfClass:[UIWindowScene class]] && scene.activationState == UISceneActivationStateForegroundActive) {
                return scene.windows.firstObject;
            }
        }
        return nil;
    }

    void *LPMCreateAdaptiveAdSize() {
        return (__bridge_retained void *)[LPMAdSize createAdaptiveAdSize];
    }

    void *LPMCreateAdaptiveAdSizeWithWidth(int width) {
        return (__bridge_retained void *)[LPMAdSize createAdaptiveAdSizeWithWidth:width];
    }

    void *LPMCreateAdSizeWithType(int adSizeType, int width, int height) {
        LPMAdSize *adSize = nil;
        switch (adSizeType) {
            case 1:
                adSize = [LPMAdSize bannerSize];
                break;
            case 2:
                adSize = [LPMAdSize largeSize];
                break;
            case 3:
                adSize = [LPMAdSize mediumRectangleSize];
                break;
            case 4:
                adSize = [LPMAdSize customSizeWithWidth:width height:height];
                break;
            case 5:
                adSize = [LPMAdSize leaderBoardSize];
                break;
            default:
                adSize = [LPMAdSize bannerSize];
                break;
        }
        return (__bridge_retained void *)adSize;
    }

    NSInteger LPMGetAdSizeWidth(void *adSizeRef) {
        LPMAdSize *adSize = (__bridge LPMAdSize *)adSizeRef;
        return [adSize width];
    }

    NSInteger LPMGetAdSizeHeight(void *adSizeRef) {
        LPMAdSize *adSize = (__bridge LPMAdSize *)adSizeRef;
        return [adSize height];
    }

    void *LPMBannerAdViewCreate(const char *adUnitId, const char *placementName,
                                LPMAdSize *adSize) {
        LPMBannerAdView *bannerAdView =
        [[LPMBannerAdView alloc] initWithAdUnitId:[LPMUtilities getStringFromCString:adUnitId]];

        [bannerAdView setPlacementName:[LPMUtilities getStringFromCString:placementName]];
        [bannerAdView setAdSize:adSize];

        bannerAdView.frame = CGRectMake(0, 0, (CGFloat)adSize.width, (CGFloat)adSize.height);

        return (__bridge_retained void *)bannerAdView;
    }

    void LPMBannerAdViewSetDelegate(void *bannerAdViewRef, void *delegateRef) {
        LPMBannerAdView *bannerAdView = (__bridge LPMBannerAdView *)bannerAdViewRef;
        LPMBannerAdViewCallbacksWrapper *delegate = (__bridge LPMBannerAdViewCallbacksWrapper *)delegateRef;
        [bannerAdView setDelegate:delegate];
    }

    void LPMBannerAdViewLoadAd(void *bannerAdViewRef) {
        LPMBannerAdView *bannerAdView = (__bridge LPMBannerAdView *)bannerAdViewRef;
        UIWindow *window = getActiveWindow();
        if (window && window.rootViewController) {
            [bannerAdView loadAdWithViewController:window.rootViewController];
        }
    }

    void LPMBannerAdViewDestroy(void *bannerAdViewRef) {
        LPMBannerAdView *bannerAdView =
        (__bridge_transfer LPMBannerAdView *)bannerAdViewRef;
        [bannerAdView destroy];
    }

    void LPMBannerAdViewPauseAutoRefresh(void *bannerAdViewRef) {
        LPMBannerAdView *bannerAdView = (__bridge LPMBannerAdView *)bannerAdViewRef;
        [bannerAdView pauseAutoRefresh];
    }

    void LPMBannerAdViewResumeAutoRefresh(void *bannerAdViewRef) {
        LPMBannerAdView *bannerAdView = (__bridge LPMBannerAdView *)bannerAdViewRef;
        [bannerAdView resumeAutoRefresh];
    }

    void LPMBannerAdViewSetPosition(
        void *bannerAdViewRef,
        const char *positionDescription,
        float xOffset,
        float yOffset
    ) {
        LPMBannerAdView *bannerAdView = (__bridge LPMBannerAdView *)bannerAdViewRef;
        NSString *position = [NSString stringWithUTF8String:positionDescription];

        dispatch_async(dispatch_get_main_queue(), ^{
            UIWindow *window = getActiveWindow();
            if (!window) return;

            UIView *rootView = window.rootViewController.view;
            if (!rootView) return;

            bannerAdView.translatesAutoresizingMaskIntoConstraints = NO;
            [bannerAdView removeFromSuperview];
            [rootView addSubview:bannerAdView];

            NSMutableArray<NSLayoutConstraint *> *constraints = [[NSMutableArray alloc] init];
            NSLayoutAttribute horizontalAttribute = NSLayoutAttributeCenterX;
            NSLayoutAttribute verticalAttribute = NSLayoutAttributeBottom;

            LPMBannerPosition bannerPosition = LPMBannerPositionFromString(position);
            switch (bannerPosition) {
                case LPMBannerPositionTopLeft:
                case LPMBannerPositionCustom:
                    horizontalAttribute = NSLayoutAttributeLeading;
                    verticalAttribute = NSLayoutAttributeTop;
                    break;
                case LPMBannerPositionTopCenter:
                    horizontalAttribute = NSLayoutAttributeCenterX;
                    verticalAttribute = NSLayoutAttributeTop;
                    break;
                case LPMBannerPositionTopRight:
                    horizontalAttribute = NSLayoutAttributeTrailing;
                    verticalAttribute = NSLayoutAttributeTop;
                    break;
                case LPMBannerPositionCenterLeft:
                    horizontalAttribute = NSLayoutAttributeLeading;
                    verticalAttribute = NSLayoutAttributeCenterY;
                    break;
                case LPMBannerPositionCenter:
                    horizontalAttribute = NSLayoutAttributeCenterX;
                    verticalAttribute = NSLayoutAttributeCenterY;
                    break;
                case LPMBannerPositionCenterRight:
                    horizontalAttribute = NSLayoutAttributeTrailing;
                    verticalAttribute = NSLayoutAttributeCenterY;
                    break;
                case LPMBannerPositionBottomLeft:
                    horizontalAttribute = NSLayoutAttributeLeading;
                    verticalAttribute = NSLayoutAttributeBottom;
                    break;
                case LPMBannerPositionBottomCenter:
                    horizontalAttribute = NSLayoutAttributeCenterX;
                    verticalAttribute = NSLayoutAttributeBottom;
                    break;
                case LPMBannerPositionBottomRight:
                    horizontalAttribute = NSLayoutAttributeTrailing;
                    verticalAttribute = NSLayoutAttributeBottom;
                    break;
            }

            CGFloat scale = [UIScreen mainScreen].scale;
            CGFloat xInPoints = xOffset / scale;
            CGFloat yInPoints = yOffset / scale;

            CGFloat safeAreaLeft = rootView.safeAreaInsets.left;
            CGFloat safeAreaTop = rootView.safeAreaInsets.top;

            CGPoint positionOffset = bannerPosition == LPMBannerPositionCustom
                ? CGPointMake(xInPoints + safeAreaLeft, yInPoints + safeAreaTop)
                : CGPointZero;

            id safeArea = rootView.safeAreaLayoutGuide ? rootView.safeAreaLayoutGuide : rootView;

            // Apply Horizontal Positioning
            [constraints addObject:
                 [NSLayoutConstraint constraintWithItem:bannerAdView
                                              attribute:horizontalAttribute
                                              relatedBy:NSLayoutRelationEqual
                                                 toItem:safeArea
                                              attribute:horizontalAttribute
                                             multiplier:1.0
                                               constant:positionOffset.x]
            ];

            // Apply Vertical Positioning
            [constraints addObject:
                 [NSLayoutConstraint constraintWithItem:bannerAdView
                                              attribute:verticalAttribute
                                              relatedBy:NSLayoutRelationEqual
                                                 toItem:safeArea
                                              attribute:verticalAttribute
                                             multiplier:1.0
                                               constant:positionOffset.y]
            ];

            // Specify height and width
            [constraints addObject:
                 [NSLayoutConstraint constraintWithItem:bannerAdView
                                              attribute:NSLayoutAttributeHeight
                                              relatedBy:NSLayoutRelationEqual
                                                 toItem:nil
                                              attribute:NSLayoutAttributeNotAnAttribute
                                             multiplier:1.0
                                               constant:bannerAdView.frame.size.height]
            ];

            [constraints addObject:
                 [NSLayoutConstraint constraintWithItem:bannerAdView
                                              attribute:NSLayoutAttributeWidth
                                              relatedBy:NSLayoutRelationEqual
                                                 toItem:nil
                                              attribute:NSLayoutAttributeNotAnAttribute
                                             multiplier:1.0
                                               constant:bannerAdView.frame.size.width]
            ];

            [NSLayoutConstraint activateConstraints:constraints];
        });
    }

    void LPMBannerAdViewShow(void *bannerAdViewRef) {
        LPMBannerAdView *bannerAdView = (__bridge LPMBannerAdView *)bannerAdViewRef;
        dispatch_async(dispatch_get_main_queue(), ^{
            bannerAdView.hidden = false;
        });
    }

    void LPMBannerAdViewHide(void *bannerAdViewRef) {
        LPMBannerAdView *bannerAdView = (__bridge LPMBannerAdView *)bannerAdViewRef;
        dispatch_async(dispatch_get_main_queue(), ^{
            bannerAdView.hidden = true;
        });
    }

    const char *LPMBannerAdViewAdId(void *bannerAdViewRef) {
        LPMBannerAdView *bannerAdView = (__bridge LPMBannerAdView *)bannerAdViewRef;
        return strdup([[bannerAdView adId] UTF8String]);
    }

#ifdef __cplusplus
}
#endif
