#import "LPMBannerPosition.h"

NSString *NSStringFromLPMBannerPosition(LPMBannerPosition position) {
    static NSDictionary<NSNumber *, NSString *> *map;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        map = @{
            @(LPMBannerPositionTopLeft): @"TopLeft",
            @(LPMBannerPositionTopCenter): @"TopCenter",
            @(LPMBannerPositionTopRight): @"TopRight",
            @(LPMBannerPositionCenterLeft): @"CenterLeft",
            @(LPMBannerPositionCenter): @"Center",
            @(LPMBannerPositionCenterRight): @"CenterRight",
            @(LPMBannerPositionBottomLeft): @"BottomLeft",
            @(LPMBannerPositionBottomCenter): @"BottomCenter",
            @(LPMBannerPositionBottomRight): @"BottomRight",
            @(LPMBannerPositionCustom): @"Custom"
        };
    });

    return map[@(position)];
}

LPMBannerPosition LPMBannerPositionFromString(NSString *position) {
    static NSDictionary<NSString *, NSNumber *> *stringToEnumMap;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        stringToEnumMap = @{
            @"TopLeft": @(LPMBannerPositionTopLeft),
            @"TopCenter": @(LPMBannerPositionTopCenter),
            @"TopRight": @(LPMBannerPositionTopRight),
            @"CenterLeft": @(LPMBannerPositionCenterLeft),
            @"Center": @(LPMBannerPositionCenter),
            @"CenterRight": @(LPMBannerPositionCenterRight),
            @"BottomLeft": @(LPMBannerPositionBottomLeft),
            @"BottomCenter": @(LPMBannerPositionBottomCenter),
            @"BottomRight": @(LPMBannerPositionBottomRight),
            @"Custom": @(LPMBannerPositionCustom)
        };
    });

    return stringToEnumMap[position]
        ? (LPMBannerPosition)stringToEnumMap[position].integerValue
        : LPMBannerPositionCustom;
}
