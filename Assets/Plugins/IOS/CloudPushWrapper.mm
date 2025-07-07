// CloudPushWrapper.mm
#import <CloudPushSDK/CloudPushSDK.h>
#import <UserNotifications/UserNotifications.h>

extern "C" {
    // 初始化方法，暴露给 C#
    void _InitCloudPush(const char *appKey, const char *appSecret)
    {
    NSString *nsAppKey = [NSString stringWithUTF8String:appKey];
    NSString *nsAppSecret = [NSString stringWithUTF8String:appSecret];

    [CloudPushSDK setLogLevel:MPLogLevelInfo];
    [CloudPushSDK
        startWithAppkey:nsAppKey
                appSecret:nsAppSecret
                callback:^(CloudPushCallbackResult *result) {
                if (result.success) {
                    NSLog(@"SDK初始化成功 | DeviceID: %@",
                        [CloudPushSDK getDeviceId]);
                    // 可选：通过 UnitySendMessage 发送成功事件到 C#
                    UnitySendMessage("CloudPushManager", "OnInitSuccess", "");
                } else {
                    NSLog(@"初始化失败: %@", result.error);
                    // 可选：发送失败事件到 C#
                    NSString *errorMsg =
                        [NSString stringWithFormat:@"Error: %@", result.error];
                    UnitySendMessage("CloudPushManager", "OnInitFailed",
                                    [errorMsg UTF8String]);
                }
                }];
    }

    void _RequestAPNsPermission() 
    {
        UNUserNotificationCenter *center =
            [UNUserNotificationCenter currentNotificationCenter];
        [center
            requestAuthorizationWithOptions:(UNAuthorizationOptionAlert |
                                            UNAuthorizationOptionSound |
                                            UNAuthorizationOptionBadge)
                        completionHandler:^(BOOL granted,
                                            NSError *_Nullable error) {
                            dispatch_async(dispatch_get_main_queue(), ^{
                            if (granted) {
                                [[UIApplication sharedApplication]
                                    registerForRemoteNotifications];
                                // 发送成功事件到 C#
                                UnitySendMessage("CloudPushManager",
                                                "OnAPNsPermissionGranted", "");
                            } else {
                                // 发送拒绝事件到 C#
                                UnitySendMessage("CloudPushManager",
                                                "OnAPNsPermissionDenied", "");
                            }

                            // 如果有错误，传递错误信息
                            if (error) {
                                NSString *errorMsg = [NSString
                                    stringWithFormat:@"Error: %@",
                                                    error.localizedDescription];
                                UnitySendMessage("CloudPushManager",
                                                "OnAPNsPermissionError",
                                                [errorMsg UTF8String]);
                            }
                            });
                        }];
    }

    unsigned char hexCharToByte(char c) {
        if (c >= '0' && c <= '9') return c - '0';
        if (c >= 'a' && c <= 'f') return 10 + c - 'a';
        if (c >= 'A' && c <= 'F') return 10 + c - 'A';
        return 0xFF; // 标记无效字符
    }

    NSData *dataFromHexString(NSString *hexString) {
        const char *hexChars = [hexString UTF8String];
        NSUInteger length = strlen(hexChars);
        NSMutableData *data = [NSMutableData dataWithCapacity:length / 2];
        
        for (NSUInteger i = 0; i < length; i += 2) {
            unsigned char high = hexCharToByte(hexChars[i]);
            unsigned char low = hexCharToByte(hexChars[i + 1]);
            if (high == 0xFF || low == 0xFF) {
                return nil; // 无效字符
            }
            unsigned char byte = (high << 4) | low;
            [data appendBytes:&byte length:1];
        }
        return data;
    }

         // 从 C# 传入 deviceToken 字符串并注册
    void _RegisterDeviceToken(const char* deviceTokenStr) {
        NSString *nsToken = [NSString stringWithUTF8String:deviceTokenStr];
        NSData *deviceToken = dataFromHexString(nsToken);
        
        if (!deviceToken) {
            NSLog(@"无效的 DeviceToken 格式");
            UnitySendMessage("CloudPushManager", "OnDeviceTokenInvalid", "");
            return;
        }
        
        [CloudPushSDK registerDevice:deviceToken withCallback:^(CloudPushCallbackResult *result) {
            if (result.success) {
                NSLog(@"[阿里云] DeviceToken 注册成功");
                UnitySendMessage("CloudPushManager", "OnDeviceTokenUploaded", "");
            } else {
                NSLog(@"[阿里云] DeviceToken 注册失败: %@", result.error);
                NSString *errorMsg = [NSString stringWithFormat:@"Error: %@", result.error];
                UnitySendMessage("CloudPushManager", "OnDeviceTokenUploadFailed", [errorMsg UTF8String]);
            }
        }];
    }
}