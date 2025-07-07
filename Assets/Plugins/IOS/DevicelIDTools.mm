#import <Foundation/Foundation.h>
#import <Security/Security.h>

// Keychain 配置
static NSString *const kServiceName = @"com.luck.thiredparty";
static NSString *const kAccountName = @"device_unique_id";

extern "C" {
    const char* GetDeviceID() {
        NSMutableDictionary *query = @{
            (__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
            (__bridge id)kSecAttrService: kServiceName,
            (__bridge id)kSecAttrAccount: kAccountName,
            (__bridge id)kSecReturnData: @YES,
            (__bridge id)kSecMatchLimit: (__bridge id)kSecMatchLimitOne
        }.mutableCopy;
        
        CFTypeRef dataTypeRef = NULL;
        OSStatus status = SecItemCopyMatching((__bridge CFDictionaryRef)query, &dataTypeRef);
        
        if (status == errSecSuccess) {
            NSData *data = (__bridge_transfer NSData *)dataTypeRef;
            NSString *uuid = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
            return strdup([uuid UTF8String]);
        } else {
            // 生成新UUID
            NSString *newUUID = [[[UIDevice currentDevice] identifierForVendor] UUIDString];
            NSData *uuidData = [newUUID dataUsingEncoding:NSUTF8StringEncoding];
            
            [query setObject:uuidData forKey:(__bridge id)kSecValueData];
            [query removeObjectForKey:(__bridge id)kSecReturnData];
            [query removeObjectForKey:(__bridge id)kSecMatchLimit];
            
            status = SecItemAdd((__bridge CFDictionaryRef)query, NULL);
            if (status == errSecSuccess) {
                return strdup([newUUID UTF8String]);
            }
        }
        return strdup(""); // 容错处理
    }
}