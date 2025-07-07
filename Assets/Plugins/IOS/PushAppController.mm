#import "UnityAppController.h"
#import <UserNotifications/UserNotifications.h>


// 自定义 AppController 继承自 UnityAppController
@interface CustomAppController : UnityAppController
@end

@implementation CustomAppController

static BOOL isPushInitialized = NO;

//-----------------------------------------------------------
// 手动触发推送注册的方法（由 Unity 调用）
//-----------------------------------------------------------
- (void)manualRegisterForRemoteNotifications
{
    if (isPushInitialized) return;
    isPushInitialized = YES;

    dispatch_async(dispatch_get_main_queue(), ^{
        if (@available(iOS 10.0, *))
        {
            UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
            [center requestAuthorizationWithOptions:(UNAuthorizationOptionAlert | UNAuthorizationOptionBadge | UNAuthorizationOptionSound)
                                  completionHandler:^(BOOL granted, NSError * _Nullable error) {
                if (granted)
                {
                    dispatch_async(dispatch_get_main_queue(), ^{
                        [[UIApplication sharedApplication] registerForRemoteNotifications];
                    });
                }
                else
                {
                    UnitySendMessage("PushManager", "OniOSPushTokenFailed", "User denied permission");
                }
            }];
        }
        else
        {
            UIUserNotificationType types = UIUserNotificationTypeAlert | UIUserNotificationTypeBadge | UIUserNotificationTypeSound;
            UIUserNotificationSettings* settings = [UIUserNotificationSettings settingsForTypes:types categories:nil];
            [[UIApplication sharedApplication] registerUserNotificationSettings:settings];
            [[UIApplication sharedApplication] registerForRemoteNotifications];
        }
    });
}

//-----------------------------------------------------------
// 系统回调
//-----------------------------------------------------------
- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    return [super application:application didFinishLaunchingWithOptions:launchOptions];
}

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken
{
    const char *tokenBytes = (const char *)[deviceToken bytes];
    NSMutableString *tokenString = [NSMutableString string];
    for (NSUInteger i = 0; i < [deviceToken length]; i++) {
        [tokenString appendFormat:@"%02.2hhx", tokenBytes[i]];
    }
    UnitySendMessage("PushManager", "OniOSPushTokenReceived", [tokenString UTF8String]);
}

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error
{
    UnitySendMessage("PushManager", "OniOSPushTokenFailed", [[error description] UTF8String]);
}

@end

//-----------------------------------------------------------
// 关键宏：注册自定义 AppController
//-----------------------------------------------------------
IMPL_APP_CONTROLLER_SUBCLASS(CustomAppController)

//-----------------------------------------------------------
// 导出给 Unity 调用的 C 函数
//-----------------------------------------------------------
extern "C" {
    void RequestPushToken() {
        // 安全获取 AppDelegate
        id delegate = [UIApplication sharedApplication].delegate;
        if ([delegate isKindOfClass:[CustomAppController class]]) {
            [(CustomAppController*)delegate manualRegisterForRemoteNotifications];
        } else {
            NSLog(@"Error: AppDelegate is not CustomAppController!");
        }
    }
}