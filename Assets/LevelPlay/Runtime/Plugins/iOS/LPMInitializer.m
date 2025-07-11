//
//  LPMInitializer.m
//  iOSBridge
//
// Copyright © 2024 IronSource. All rights reserved.
//

#import "LPMInitializer.h"
#import "LPMUtilities.h"

#define LPMGetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

#ifdef __cplusplus
extern "C" {
#endif
    static bool isUnityPauseGame = false;
    void UnitySendMessage(const char* obj, const char* method, const char* msg);

    void LPMInitialize(const char *appKey, const char *userId, const char **adFormats) {
        NSMutableArray *formatsArray = [NSMutableArray array];
        const char **current = adFormats;
        if(current != NULL){
            while (*current != NULL) {
                NSString *format = [LPMUtilities getStringFromCString:*current];
                if (format) {
                    [formatsArray addObject:format];
                }
                current++;
            }
        }
        [[LPMInitializer sharedInstance] LPMInitialize:[LPMUtilities getStringFromCString:appKey]
                                                userId:[LPMUtilities getStringFromCString:userId]
                                             adFormats:formatsArray];
    }

    void setPluginData(const char *pluginType, const char *pluginVersion, const char *pluginFrameworkVersion) {
        NSString *type = [LPMUtilities getStringFromCString:pluginType];
        NSString *version = [LPMUtilities getStringFromCString:pluginVersion];
        NSString *frameworkVersion = [LPMUtilities getStringFromCString:pluginFrameworkVersion];

        // Use the sharedInstance to set plugin data
        [[LPMInitializer sharedInstance] setPluginData:type pluginVersion:version pluginFrameworkVersion:frameworkVersion];
    }

    void LPMSetPauseGame(BOOL pause) {
        isUnityPauseGame = pause;
    }

    BOOL LPMSetDynamicUserId(const char *dynamicUserId) {
        return [LevelPlay setDynamicUserId:[LPMUtilities getStringFromCString:dynamicUserId]];
    }

    void LPMLaunchTestSuite() {
        [LevelPlay launchTestSuite:[UIApplication sharedApplication].keyWindow.rootViewController];
    }

    void LPMSetAdaptersDebug(BOOL enabled) {
        [LevelPlay setAdaptersDebug:enabled];
    }

    void LPMValidateIntegration() {
        [LevelPlay validateIntegration];
    }

    void LPMSetMetaData(char *key, char *value) {
        [LevelPlay setMetaDataWithKey:LPMGetStringParam(key) value:LPMGetStringParam(value)];
    }

    void LPMSetMetaDataWithValues(char *key, const char *values[]) {
        NSMutableArray *valuesArray = [NSMutableArray new];
        if (!values) {
            NSLog(@"values is nil");
            return;
        }
        int i = 0;

        while (values[i] != nil) {
            [valuesArray addObject:[NSString stringWithCString:values[i] encoding:NSASCIIStringEncoding]];
            i++;
        }

        [LevelPlay setMetaDataWithKey:LPMGetStringParam(key) values:valuesArray];
    }

    void LPMSetNetworkData(const char *networkKey, const char *networkData) {
        NSString *networkDataString = LPMGetStringParam(networkData);
        NSData *data = [networkDataString dataUsingEncoding:NSUTF8StringEncoding];
        if (!data) {
            NSLog(@"Set network data failed, data creation failed");
            return;
        }

        NSError *error;
        NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:0 error:&error];
        if (!dict) {
            NSLog(@"Set network data failed with error: %@", error);
            return;
        }

        [LevelPlay setNetworkDataWithNetworkKey:networkDataString
                                 andNetworkData:dict];
    }

    void LPMSetConsent(BOOL consent) {
        [LevelPlay setConsent:consent];
    }

    void LPMSetSegment(char* jsonString) {
      NSString *segmentJSON = LPMGetStringParam(jsonString);
      LPMSegment *segment = [[LPMSegment alloc] init];
      NSError* error;
      if (!segmentJSON)
          return;

      NSData *data = [segmentJSON dataUsingEncoding:NSUTF8StringEncoding];
      if (!data)
          return;

      NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:0 error:&error];

      if (!dict)
          return;

      NSMutableArray *allKeys = [[dict allKeys] mutableCopy];
      for (id key in allKeys)
      {
          NSString* keyString = (NSString*)key;
          NSString *object = [dict objectForKey: keyString];
          if ([keyString isEqualToString:@"level"]){
              segment.level =  [object intValue];
          }
          else if ([keyString isEqualToString:@"isPaying"]){
              segment.paying = [object boolValue];
          }
          else if ([keyString isEqualToString:@"userCreationDate"]){
              NSDate *date = [NSDate dateWithTimeIntervalSince1970: [object longLongValue]/1000];
              segment.userCreationDate = date;

          }
          else if ([keyString isEqualToString:@"segmentName"]){
              segment.segmentName = object;

          } else if ([keyString isEqualToString:@"iapt"]){
              segment.iapTotal = [object doubleValue];
          }
          else{
              [segment setCustomValue:object forKey:keyString];
          }

      }

      [LevelPlay setSegment:segment];
    }

#ifdef __cplusplus
}
#endif

@implementation LPMInitializer

+ (instancetype)sharedInstance {
    static LPMInitializer *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[self alloc] init];
    });
    return sharedInstance;
}

- (void)LPMInitialize:(NSString *)appKey userId:(NSString *)userId adFormats:(NSArray *)adFormats {

    LPMInitRequestBuilder *requestBuilder = [[LPMInitRequestBuilder alloc] initWithAppKey:appKey];
    [requestBuilder withUserId:userId];
    [requestBuilder withLegacyAdFormats:adFormats];
    LPMInitRequest *request = [requestBuilder build];

    [LevelPlay initWithRequest:request completion:^(LPMConfiguration * _Nullable config, NSError * _Nullable error) {
        if (error) {
            [self initializationDidFailWithError:error];
        } else {
            [self initializationDidCompleteWithConfiguration: config];
            [LevelPlay addImpressionDataDelegate:self];
        }
    }];
}

- (void)setPluginData:(NSString *)pluginType pluginVersion:(NSString *)pluginVersion pluginFrameworkVersion:(NSString *)pluginFrameworkVersion {
    if (pluginType) {
        [ISConfigurations getConfigurations].plugin = pluginType;
    }

    if (pluginVersion) {
        [ISConfigurations getConfigurations].pluginVersion = pluginVersion;
    }

    if (pluginFrameworkVersion) {
        [ISConfigurations getConfigurations].pluginFrameworkVersion = pluginFrameworkVersion;
    }
    NSLog(@"PLUGIN IS: %@ %@ %@", [ISConfigurations getConfigurations].plugin, [ISConfigurations getConfigurations].pluginVersion, [ISConfigurations getConfigurations].pluginFrameworkVersion);
}

- (void)LPMSetPauseGame:(BOOL)pause {
    isUnityPauseGame = pause;
}

- (BOOL)isUnityPauseGame {
    return isUnityPauseGame;
}

- (NSString *)serializeConfigToJSON:(LPMConfiguration *)config {
    NSDictionary *configDict = @{
        @"isAdQualityEnabled": config.isAdQualityEnabled == true ? @"true" : @"false"
    };
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:configDict options:0 error:&error];
    return jsonData ? [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding] : @"";
}

- (void)initializationDidCompleteWithConfiguration:(LPMConfiguration *)config {
    NSString *jsonString = [self serializeConfigToJSON:config];
    const char *configString = [jsonString UTF8String];
    UnitySendMessage("IosLevelPlaySdk", "OnInitializationSuccess", configString);
}

- (void)initializationDidFailWithError:(NSError *)error {
    NSString *jsonString = [LPMUtilities serializeErrorToJSON:error];
    const char *errorString = [jsonString UTF8String];
    UnitySendMessage("IosLevelPlaySdk", "OnInitializationFailed", errorString);
}

- (void)impressionDataDidSucceed:(LPMImpressionData *)impressionData {
    NSLog(@"impressionDataDidSucceed: %@", impressionData);
    UnitySendMessage("IosLevelPlaySdk", "onImpressionSuccess", [self getJsonFromObj:[impressionData allData]].UTF8String);
}

- (NSString *)getJsonFromObj:(id)obj {
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:obj options:0 error:&error];
    if (!jsonData) {
        NSLog(@"Got an error: %@", error);
        return @"";
    } else {
        NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        return jsonString;
    }
}

@end
