#import <Foundation/Foundation.h>

typedef NS_ENUM(NSInteger, LPMBannerPosition) {
    LPMBannerPositionTopLeft,
    LPMBannerPositionTopCenter,
    LPMBannerPositionTopRight,
    LPMBannerPositionCenterLeft,
    LPMBannerPositionCenter,
    LPMBannerPositionCenterRight,
    LPMBannerPositionBottomLeft,
    LPMBannerPositionBottomCenter,
    LPMBannerPositionBottomRight,
    LPMBannerPositionCustom
};

FOUNDATION_EXPORT NSString *NSStringFromLPMBannerPosition(LPMBannerPosition position);
FOUNDATION_EXPORT LPMBannerPosition LPMBannerPositionFromString(NSString *position);
