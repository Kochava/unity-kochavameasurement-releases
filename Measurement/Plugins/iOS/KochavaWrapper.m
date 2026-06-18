//
//  KochavaMeasurement (Unity)
//
//  Copyright (c) 2013 - 2026 Kochava, Inc. All rights reserved.
//

#pragma mark - Import
#import <StoreKit/StoreKit.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <JavaScriptCore/JavaScriptCore.h>
#import <KochavaNetworking/KochavaNetworking-Swift.h>
#import <KochavaMeasurement/KochavaMeasurement-Swift.h>

#pragma mark - Util

@interface KochavaMeasurementUtil : NSObject
@end

@implementation KochavaMeasurementUtil

+ (void)log:(nonnull NSString *)message {
    NSLog(@"KVA/Measurement: %@", message);
}

+ (nullable NSString *)serializeJsonObject:(nullable NSDictionary *)dictionary {
    return [NSString kva_stringFromJSONObject:dictionary prettyPrintBool:NO];
}

@end

#pragma mark - UnityUtil

// Copy a string in a way that can be passed to the C# layer without being freed too early.
// The mono layer will automatically free this after it is received there.
char *autonomousStringCopy(const char *string) {
    if (string == NULL) {
        return NULL;
    }
    char *res = (char *) malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

@interface KochavaMeasurementPlugin : NSObject
@end

@implementation KochavaMeasurementPlugin

+ (nullable NSString *)convertCStringToNSString:(nullable const char *)cString {
    if(cString == NULL) {
        return nil;
    }
    return [NSString stringWithUTF8String:cString];
}

@end

#pragma mark - Methods

// void executeAdvancedInstruction(string name, string value)
// Dispatches all wrapper_* EAI commands to the native SDK.
void iosNativeExecuteAdvancedInstruction(const char *nameUtf8, const char *valueUtf8) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSString *value = [KochavaMeasurementPlugin convertCStringToNSString:valueUtf8];
    [KVAMeasurement.shared.networking executeAdvancedInstructionWithUniversalIdentifier:name parameter:value prerequisiteTaskIdentifierArray:nil closure_didComplete:nil];
}

// void executeAdvancedInstructionWithCallback(string name, string value, string requestId, string listenerMethod)
// Dispatches an EAI command and routes the response to a Unity listener method.
// When requestId is non-empty, the response is wrapped as {"id": requestId, "value": <data>}.
// Both one-shot (NativeEaiCallbackListener) and persistent (NativePersistentCallbackListener) callbacks pass a
// non-empty requestId; the empty-requestId path forwards raw data and is not used by the current C# layer.
void iosNativeExecuteAdvancedInstructionWithCallback(const char *nameUtf8, const char *valueUtf8, const char *requestIdUtf8, const char *listenerMethodUtf8) {
    NSString *name = [KochavaMeasurementPlugin convertCStringToNSString:nameUtf8];
    NSString *value = [KochavaMeasurementPlugin convertCStringToNSString:valueUtf8];
    NSString *requestId = [KochavaMeasurementPlugin convertCStringToNSString:requestIdUtf8] ?: @"";
    NSString *listenerMethod = [KochavaMeasurementPlugin convertCStringToNSString:listenerMethodUtf8] ?: @"";

    [KVAMeasurement.shared.networking executeAdvancedInstructionWithUniversalIdentifier:name
            parameter:value
            prerequisiteTaskIdentifierArray:nil
            closure_didComplete:^(NSString *_Nullable data) {
                NSString *response;
                if (requestId.length > 0) {
                    response = [KochavaMeasurementUtil serializeJsonObject:@{
                            @"id": requestId,
                            @"value": data ?: @""
                    }] ?: @"{}";
                } else {
                    response = data ?: @"{}";
                }
                const char *a = "KochavaMeasurement";
                UnitySendMessage(a, autonomousStringCopy([listenerMethod UTF8String]), autonomousStringCopy([response UTF8String]));
            }];
}
