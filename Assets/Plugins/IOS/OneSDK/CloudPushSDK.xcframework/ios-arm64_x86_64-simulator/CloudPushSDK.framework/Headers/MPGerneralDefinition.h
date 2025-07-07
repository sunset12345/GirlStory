//
//  MPGerneralDefinition.h
//  CloudPushSDK
//
//  Created by junmo on 16/10/11.
//  Copyright © 2016年 aliyun.mobileService. All rights reserved.
//

#ifndef MPGerneralDefinition_h
#define MPGerneralDefinition_h

#import "CloudPushCallbackResult.h"

typedef void (^RetrierFailedCallback)();

typedef void (^CallbackHandler)(CloudPushCallbackResult * _Nonnull res);

// 保证callback不为空且回调不在主线程上执行
#define NotNilCallback(funcName, paras)\
if (funcName) {\
    if ([NSThread isMainThread]) {\
        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{\
            funcName(paras);\
        });\
    } else {\
        funcName(paras);\
    }\
}

typedef NS_ENUM(NSInteger, MPLogLevel) {
    MPLogLevelNone = 0,
    MPLogLevelError = 1,
    MPLogLevelWarn = 2,
    MPLogLevelInfo = 3,
    MPLogLevelDebug = 4
};

#endif /* MPGerneralDefinition_h */
