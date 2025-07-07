#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

extern "C" {
const char *GetiOSKeyboardLanguageNative() {
  UITextInputMode *inputMode = [UITextInputMode currentInputMode];
  NSString *language = inputMode.primaryLanguage;
  if (!language)
    return strdup("");

  // 转换格式 zh-Hans → zh_Hans
  NSString *formatted = [language stringByReplacingOccurrencesOfString:@"-"
                                                            withString:@"_"];
  return formatted ? strdup([formatted UTF8String]) : strdup("");
}

const char *GetIOSRegion() {
  NSLocale *locale = [NSLocale currentLocale];
  NSString *countryCode = [locale objectForKey:NSLocaleCountryCode];
  return countryCode ? strdup(countryCode.UTF8String) : strdup("US");
}

const char *GetIOSTimeZone() {
  NSTimeZone *timeZone = [NSTimeZone localTimeZone];
  return strdup([[timeZone name] UTF8String]);
}

BOOL isVPNConnected() {
  NSDictionary *dict = CFBridgingRelease(CFNetworkCopySystemProxySettings());
  NSArray *keys = [dict[@"__SCOPED__"] allKeys];
  for (NSString *key in keys) {
    if ([key rangeOfString:@"tap"].location != NSNotFound ||
        [key rangeOfString:@"tun"].location != NSNotFound ||
        [key rangeOfString:@"ppp"].location != NSNotFound) {
      return YES;
    }
  }
  return NO;
}
}