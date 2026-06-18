//
//  KochavaMeasurement (Unity)
//
//  Copyright (c) 2013 - 2026 Kochava, Inc. All rights reserved.
//
#nullable enable

// Build defines to assign each platform.
#if UNITY_EDITOR
#define KVA_EDITOR
#elif UNITY_ANDROID
#define KVA_ANDROID
#elif UNITY_IOS
#define KVA_IOS
#elif UNITY_WEBGL
#define KVA_WEBGL
#elif UNITY_STANDALONE_OSX
#define KVA_MACOS
#elif UNITY_STANDALONE_LINUX
#define KVA_LINUX
#elif UNITY_STANDALONE_WIN
#define KVA_WINDOWS
#elif WINDOWS_UWP
#define KVA_UWP
#else
#define KVA_OTHER
#endif

// Define cases where the Net Standard SDK is used.
#if !KVA_ANDROID && !KVA_IOS
#define KVA_NETSTD
#endif

// In test mode, suppress Unity and platform-specific code so Kochava.cs compiles as a plain .NET source file.
#if KVA_TEST
#undef KVA_NETSTD
#endif

// Imports
using System.Collections.Generic;
#if !KVA_TEST
using UnityEngine;
using UnityEngine.Networking;
#endif
using System;
using System.Collections;
using System.Runtime.InteropServices;
using Kochava.Internal;
using Newtonsoft.Json.Linq;

// Kochava SDK
namespace Kochava
{
    #region PublicAPI

    // Log Levels
    public enum KochavaMeasurementLogLevel
    {
        None,
        Error,
        Warn,
        Info,
        Debug,
        Trace
    }

    // Standard event types
    // For samples and expected usage see: https://support.kochava.com/reference-information/post-install-event-examples/
    public enum KochavaMeasurementEventType
    {
        Achievement,
        AddToCart,
        AddToWishList,
        CheckoutStart,
        LevelComplete,
        Purchase,
        Rating,
        RegistrationComplete,
        Search,
        TutorialComplete,
        View,
        AdView,
        PushReceived,
        PushOpened,
        ConsentGranted,
        Deeplink,
        AdClick,
        StartTrial,
        Subscribe
    }

    // Install Attribution result.
    public class KochavaMeasurementInstallAttribution
    {
        // if attribution has been retrieved from the server.
        public bool Retrieved { get; }
        // the raw attribution response from the server as a dictionary.
        public JObject Raw { get; }
        // if this install or a de-duplicated install on this device is attributed.
        public bool Attributed { get; }
        // if this is the first install on this device.
        public bool FirstInstall { get; }

        internal KochavaMeasurementInstallAttribution(JObject json)
        {
            Retrieved = Util.OptBool(json["retrieved"], false);
            Raw = Util.OptJObject(json["raw"], new JObject());
            Attributed = Util.OptBool(json["attributed"], false);
            FirstInstall = Util.OptBool(json["firstInstall"], false);
        }
    }

    // Deeplink result.
    public class KochavaMeasurementDeeplink
    {
        // Destination path or url.
        // Will be empty if no deeplink was passed in and there was no deferred deeplink.
        public string Destination { get; }
        // The raw response as a dictionary. Will always include "destination" but may include other metadata.
        public JObject Raw { get; }

        internal KochavaMeasurementDeeplink(JObject json)
        {
            Destination = Util.OptString(json["destination"], "");
            Raw = Util.OptJObject(json["raw"], new JObject());
        }
    }
    
    // Config result.
    public class KochavaMeasurementConfig
    {
        // If consent gdpr currently applies.
        public bool ConsentGdprApplies { get; }

        internal KochavaMeasurementConfig(bool consentGdprApplies)
        {
            ConsentGdprApplies = consentGdprApplies;
        }

        internal KochavaMeasurementConfig(JObject json)
        {
            ConsentGdprApplies = Util.OptBool(json["consentGdprApplies"], false);
        }
    }

    // Init result. Deprecated: use KochavaMeasurementConfig.
    [Obsolete("Use KochavaMeasurementConfig.")]
    public class KochavaMeasurementInit : KochavaMeasurementConfig
    {
        internal KochavaMeasurementInit(bool consentGdprApplies) : base(consentGdprApplies) { }
    }

    // Standard Event.
    public class KochavaMeasurementEvent
    {
        private readonly string EventName;
        private readonly JObject EventData = new JObject();
        private string? AppleAppStoreReceiptBase64String;
        private string? AndroidGooglePlayReceiptData;
        private string? AndroidGooglePlayReceiptSignature;

        // Creates an Event with a Standard event type.
        public KochavaMeasurementEvent(KochavaMeasurementEventType eventType)
        {
            switch (eventType)
            {
                case KochavaMeasurementEventType.Achievement:
                    EventName = "Achievement";
                    break;
                case KochavaMeasurementEventType.AddToCart:
                    EventName = "Add to Cart";
                    break;
                case KochavaMeasurementEventType.AddToWishList:
                    EventName = "Add to Wish List";
                    break;
                case KochavaMeasurementEventType.CheckoutStart:
                    EventName = "Checkout Start";
                    break;
                case KochavaMeasurementEventType.LevelComplete:
                    EventName = "Level Complete";
                    break;
                case KochavaMeasurementEventType.Purchase:
                    EventName = "Purchase";
                    break;
                case KochavaMeasurementEventType.Rating:
                    EventName = "Rating";
                    break;
                case KochavaMeasurementEventType.RegistrationComplete:
                    EventName = "Registration Complete";
                    break;
                case KochavaMeasurementEventType.Search:
                    EventName = "Search";
                    break;
                case KochavaMeasurementEventType.TutorialComplete:
                    EventName = "Tutorial Complete";
                    break;
                case KochavaMeasurementEventType.View:
                    EventName = "View";
                    break;
                case KochavaMeasurementEventType.AdView:
                    EventName = "Ad View";
                    break;
                case KochavaMeasurementEventType.PushReceived:
                    EventName = "Push Received";
                    break;
                case KochavaMeasurementEventType.PushOpened:
                    EventName = "Push Opened";
                    break;
                case KochavaMeasurementEventType.ConsentGranted:
                    EventName = "Consent Granted";
                    break;
                case KochavaMeasurementEventType.Deeplink:
                    EventName = "_Deeplink";
                    break;
                case KochavaMeasurementEventType.AdClick:
                    EventName = "Ad Click";
                    break;
                case KochavaMeasurementEventType.StartTrial:
                    EventName = "Start Trial";
                    break;
                case KochavaMeasurementEventType.Subscribe:
                    EventName = "Subscribe";
                    break;
                default:
                    EventName = "";
                    break;
            }
        }

        // Creates an Event with a custom name.
        public KochavaMeasurementEvent(string eventName)
        {
            EventName = eventName ?? "";
        }

        // Sends the event.
        public void Send()
        {
#if !KVA_TEST
            KochavaMeasurement.Instance.SendEventWithEvent(this);
#endif
        }

        // Sets a custom key/value to the event of type string.
        public void SetCustomStringValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) return;
            EventData[key] = value;
        }

        // Sets a custom key/value to the event of type bool.
        public void SetCustomBoolValue(string key, bool value)
        {
            if (string.IsNullOrEmpty(key)) return;
            EventData[key] = value;
        }

        // Sets a custom key/value to the event of type double.
        public void SetCustomNumberValue(string key, double value)
        {
            if (string.IsNullOrEmpty(key)) return;
            EventData[key] = value;
        }

        // Sets a custom key/value to the event of type object.
        private void SetCustomDictionaryValue(string key, JObject value)
        {
            if (string.IsNullOrEmpty(key) || value == null) return;
            EventData[key] = value;
        }

        // (Android Only) Sets the receipt from the Android Google Play Store.
        public void SetAndroidGooglePlayReceipt(string androidGooglePlayReceiptData, string androidGooglePlayReceiptSignature)
        {
            if (string.IsNullOrEmpty(androidGooglePlayReceiptData) || string.IsNullOrEmpty(androidGooglePlayReceiptSignature)) return;
            AndroidGooglePlayReceiptData = androidGooglePlayReceiptData;
            AndroidGooglePlayReceiptSignature = androidGooglePlayReceiptSignature;
        }

        // Sets the receipt from the Apple App Store.
        public void SetAppleAppStoreReceipt(string appleAppStoreReceiptBase64String)
        {
            if (string.IsNullOrEmpty(appleAppStoreReceiptBase64String)) return;
            AppleAppStoreReceiptBase64String = appleAppStoreReceiptBase64String;
        }

        // Deprecated: use SetAppleAppStoreReceipt.
        [Obsolete("Use SetAppleAppStoreReceipt.")]
        public void SetIosAppStoreReceipt(string iosAppStoreReceiptBase64String)
        {
            SetAppleAppStoreReceipt(iosAppStoreReceiptBase64String);
        }

        // Standard Parameters.
        public void SetAction(string value) => SetCustomStringValue("action", value);
        public void SetBackground(bool value) => SetCustomBoolValue("background", value);
        public void SetCheckoutAsGuest(string value) => SetCustomStringValue("checkout_as_guest", value);
        public void SetCompleted(bool value) => SetCustomBoolValue("completed", value);
        public void SetContentId(string value) => SetCustomStringValue("content_id", value);
        public void SetContentType(string value) => SetCustomStringValue("content_type", value);
        public void SetCurrency(string value) => SetCustomStringValue("currency", value);
        public void SetDate(string value) => SetCustomStringValue("date", value);
        public void SetDescription(string value) => SetCustomStringValue("description", value);
        public void SetDestination(string value) => SetCustomStringValue("destination", value);
        public void SetDuration(double value) => SetCustomNumberValue("duration", value);
        public void SetEndDate(string value) => SetCustomStringValue("end_date", value);
        public void SetItemAddedFrom(string value) => SetCustomStringValue("item_added_from", value);
        public void SetLevel(string value) => SetCustomStringValue("level", value);
        public void SetMaxRatingValue(double value) => SetCustomNumberValue("max_rating_value", value);
        public void SetName(string value) => SetCustomStringValue("name", value);
        public void SetOrderId(string value) => SetCustomStringValue("order_id", value);
        public void SetOrigin(string value) => SetCustomStringValue("origin", value);
        public void SetPayload(JObject value) => SetCustomDictionaryValue("payload", value);
        public void SetPrice(double value) => SetCustomNumberValue("price", value);
        public void SetQuantity(double value) => SetCustomNumberValue("quantity", value);
        public void SetRatingValue(double value) => SetCustomNumberValue("rating_value", value);
        public void SetReceiptId(string value) => SetCustomStringValue("receipt_id", value);
        public void SetReferralFrom(string value) => SetCustomStringValue("referral_from", value);
        public void SetRegistrationMethod(string value) => SetCustomStringValue("registration_method", value);
        public void SetResults(string value) => SetCustomStringValue("results", value);
        public void SetScore(string value) => SetCustomStringValue("score", value);
        public void SetSearchTerm(string value) => SetCustomStringValue("search_term", value);
        public void SetSource(string value) => SetCustomStringValue("source", value);
        public void SetSpatialX(double value) => SetCustomNumberValue("spatial_x", value);
        public void SetSpatialY(double value) => SetCustomNumberValue("spatial_y", value);
        public void SetSpatialZ(double value) => SetCustomNumberValue("spatial_z", value);
        public void SetStartDate(string value) => SetCustomStringValue("start_date", value);
        public void SetSuccess(string value) => SetCustomStringValue("success", value);
        public void SetUri(string value) => SetCustomStringValue("uri", value);
        public void SetUserId(string value) => SetCustomStringValue("user_id", value);
        public void SetUserName(string value) => SetCustomStringValue("user_name", value);
        public void SetValidated(string value) => SetCustomStringValue("validated", value);

        // Ad LTV Parameters
        public void SetAdCampaignId(string value) => SetCustomStringValue("ad_campaign_id", value);
        public void SetAdCampaignName(string value) => SetCustomStringValue("ad_campaign_name", value);
        public void SetAdDeviceType(string value) => SetCustomStringValue("device_type", value);
        public void SetAdGroupId(string value) => SetCustomStringValue("ad_group_id", value);
        public void SetAdGroupName(string value) => SetCustomStringValue("ad_group_name", value);
        public void SetAdMediationName(string value) => SetCustomStringValue("ad_mediation_name", value);
        public void SetAdNetworkName(string value) => SetCustomStringValue("ad_network_name", value);
        public void SetAdPlacement(string value) => SetCustomStringValue("placement", value);
        public void SetAdSize(string value) => SetCustomStringValue("ad_size", value);
        public void SetAdType(string value) => SetCustomStringValue("ad_type", value);

        // Returns the event name.
        internal string GetEventName()
        {
            return EventName;
        }

        // Returns the event data
        internal JObject GetEventData()
        {
            return EventData;
        }

        // Returns the Apple App Store receipt.
        internal string? GetAppleAppStoreReceiptBase64String()
        {
            return AppleAppStoreReceiptBase64String;
        }

        // Returns the Android Google Receipt Data.
        internal string? GetAndroidGooglePlayReceiptData()
        {
            return AndroidGooglePlayReceiptData;
        }

        // Returns the Android Google Receipt Signature.
        internal string? GetAndroidGooglePlayReceiptSignature()
        {
            return AndroidGooglePlayReceiptSignature;
        }
    }

    // Main SDK Entrypoint.
    public class KochavaMeasurement
    {
        // Version data that is updated via a script. Do not change.
        private const string SdkName = "Unity";
        private const string SdkVersion = "7.0.0"; // {{SDK_VERSION}}
        private const string SdkBuildDate = "2026-06-18T18:10:07Z"; // {{SDK_BUILD_DATE}}

        private static string GetLogLevelString(KochavaMeasurementLogLevel logLevel)
        {
            switch (logLevel)
            {
                case KochavaMeasurementLogLevel.None: return "none";
                case KochavaMeasurementLogLevel.Error: return "error";
                case KochavaMeasurementLogLevel.Warn: return "warn";
                case KochavaMeasurementLogLevel.Info: return "info";
                case KochavaMeasurementLogLevel.Debug: return "debug";
                case KochavaMeasurementLogLevel.Trace: return "trace";
                default: return "info";
            }
        }

        // Native SDK API Handler
        private readonly IEaiAdapter NativeApi;

        // Registered app guids
        private string? RegisteredEditorAppGuid;
        private string? RegisteredAndroidAppGuid;
        private string? RegisteredAppleAppGuid;
        private string? RegisteredWebGlAppGuid;
        private string? RegisteredMacOsAppGuid;
        private string? RegisteredLinuxAppGuid;
        private string? RegisteredWindowsAppGuid;
        private string? RegisteredUwpAppGuid;
        private string? RegisteredFallbackAppGuid;
        private string? RegisteredPartnerName;

        // Initialize with the Native API Handler for the current platform.
        internal KochavaMeasurement(IEaiAdapter nativeApi)
        {
            NativeApi = nativeApi;
        }

        // Singleton instance.
#if !KVA_TEST
        public static KochavaMeasurement Instance => SingletonHandler.Instance.Measurement;
#endif

        // Helper: register a one-shot string callback and return its request ID.
        private string AddStringRequest(Action<string> callback)
        {
#if KVA_TEST
            return Guid.NewGuid().ToString();
#else
            return SingletonHandler.Instance.AddStringRequest(callback);
#endif
        }

        // Helper: register a persistent callback keyed by EAI name.
        private void AddPersistentRequest(string name, Action<string> callback)
        {
#if !KVA_TEST
            SingletonHandler.Instance.AddPersistentRequest(name, callback);
#endif
        }

        // Helper: remove a persistent callback keyed by EAI name.
        private void RemovePersistentRequest(string name)
        {
#if !KVA_TEST
            SingletonHandler.Instance.RemovePersistentRequest(name);
#endif
        }

        // Helper: clear all callback state on shutdown.
        private void ShutdownCallbacks()
        {
#if !KVA_TEST
            SingletonHandler.Instance.Shutdown();
#endif
        }

        // Reserved function, only use if directed to by your Client Success Manager.
        public void ExecuteAdvancedInstruction(string name, string value)
        {
            NativeApi.ExecuteAdvancedInstruction(name, value);
        }

        // Reserved function, only use if directed to by your Client Success Manager.
        public void ExecuteAdvancedInstructionWithCallback(string name, string value, bool persistent, Action<string> callback)
        {
            if (persistent)
            {
                if (callback == null)
                {
                    RemovePersistentRequest(name);
                    NativeApi.ExecuteAdvancedInstruction(name, "");
                    return;
                }
                AddPersistentRequest(name, callback);
                NativeApi.ExecuteAdvancedInstructionWithCallback(name, value, name, "NativePersistentCallbackListener");
            }
            else
            {
                var requestId = AddStringRequest(callback);
                NativeApi.ExecuteAdvancedInstructionWithCallback(name, value, requestId, "NativeEaiCallbackListener");
            }
        }

        // Sets the log level. This should be set prior to starting the SDK.
        public void SetLogLevel(KochavaMeasurementLogLevel logLevel)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_setLogLevel",
                new JObject { ["logLevel"] = GetLogLevelString(logLevel) }.ToString());
        }

        // Sets the sleep state.
        public void SetSleep(bool sleep)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_setSleep",
                new JObject { ["enabled"] = sleep }.ToString());
        }

        // Sets if app level advertising tracking should be limited.
        public void SetAppLimitAdTracking(bool appLimitAdTracking)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_setAppLimitAdTracking",
                new JObject { ["enabled"] = appLimitAdTracking }.ToString());
        }

        // Register a custom device identifier for install attribution.
        public void RegisterCustomDeviceIdentifier(string name, string? value)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_registerCustomDeviceIdentifier",
                new JObject { ["name"] = name, ["value"] = value }.ToString());
        }

        // Register a custom value to be included in SDK payloads.
        public void RegisterCustomStringValue(string name, string? value)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_registerCustomValue",
                new JObject { ["name"] = name, ["value"] = value }.ToString());
        }

        // Register a custom value to be included in SDK payloads.
        public void RegisterCustomBoolValue(string name, bool? value)
        {
            var json = new JObject { ["name"] = name };
            json["value"] = value.HasValue ? (JToken)value.Value : JValue.CreateNull();
            NativeApi.ExecuteAdvancedInstruction("wrapper_registerCustomValue", json.ToString());
        }

        // Register a custom value to be included in SDK payloads.
        public void RegisterCustomNumberValue(string name, double? value)
        {
            var json = new JObject { ["name"] = name };
            json["value"] = value.HasValue ? (JToken)value.Value : JValue.CreateNull();
            NativeApi.ExecuteAdvancedInstruction("wrapper_registerCustomValue", json.ToString());
        }

        // Registers an Identity Link that allows linking different identities together in the form of key and value pairs.
        public void RegisterIdentityLink(string name, string? value)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_registerIdentityLink",
                new JObject { ["name"] = name, ["value"] = value }.ToString());
        }

        // (Apple Only) Enables App Clips by setting the Container App Group Identifier for App Clips data migration.
        public void EnableAppleAppClips(string containerAppGroupIdentifier)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_enableAppleAppClips",
                new JObject { ["identifier"] = containerAppGroupIdentifier }.ToString());
        }

        // (Apple Only) Enables App Tracking Transparency.
        public void EnableAppleAtt()
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_enableAppleAtt", "{}");
        }

        // (Apple Only) Sets the amount of time in seconds to wait for App Tracking Transparency Authorization. Default 30 seconds.
        public void SetAppleAttAuthorizationWaitTime(double waitTime)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_setAppleAttAuthorizationWait",
                new JObject { ["timeInterval"] = waitTime }.ToString());
        }

        // (Apple Only) Sets if the SDK should automatically request App Tracking Transparency Authorization on start. Default true.
        public void SetAppleAttAuthorizationAutoRequest(bool autoRequest)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_setAppleAttAuthorizationAutoRequest",
                new JObject { ["enabled"] = autoRequest }.ToString());
        }

        // (Apple Only) Sets if a custom authorization prompt should be used for App Tracking Transparency.
        public void SetAppleAttAuthorizationCustomPrompt(bool enabled)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_setAppleAttAuthorizationCustomPrompt",
                new JObject { ["enabled"] = enabled }.ToString());
        }

        // (Apple Only) Notifies the SDK that the custom App Tracking Transparency prompt has completed.
        public void AppleAttAuthorizationCustomPromptDidComplete()
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_appleAttAuthorizationCustomPromptDidComplete", "{}");
        }

        [Obsolete("Use EnableAppleAppClips.")]
        public void EnableIosAppClips(string containerAppGroupIdentifier) => EnableAppleAppClips(containerAppGroupIdentifier);

        [Obsolete("Use EnableAppleAtt.")]
        public void EnableIosAtt() => EnableAppleAtt();

        [Obsolete("Use SetAppleAttAuthorizationWaitTime.")]
        public void SetIosAttAuthorizationWaitTime(double waitTime) => SetAppleAttAuthorizationWaitTime(waitTime);

        [Obsolete("Use SetAppleAttAuthorizationAutoRequest.")]
        public void SetIosAttAuthorizationAutoRequest(bool autoRequest) => SetAppleAttAuthorizationAutoRequest(autoRequest);

        // Register a privacy profile, creating or overwriting an existing pofile.
        public void RegisterPrivacyProfile(string name, string[]? keys)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_registerPrivacyProfile",
                new JObject
                {
                    ["name"] = name,
                    ["keys"] = keys != null ? JArray.FromObject(keys) : new JArray()
                }.ToString());
        }

        // Enable or disable an existing privacy profile.
        public void SetPrivacyProfileEnabled(string name, bool enabled)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_setPrivacyProfileEnabled",
                new JObject { ["name"] = name, ["enabled"] = enabled }.ToString());
        }

        // Register a deeplink wrapper domain for enhanced deeplink ESP integrations.
        public void RegisterDeeplinkWrapperDomain(string domain)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_registerDeeplinkWrapperDomain",
                new JObject { ["domain"] = domain }.ToString());
        }
        
        // Set the config completed callback listener. Pass null to clear the listener.
        public void SetConfigCompletedListener(Action<KochavaMeasurementConfig>? callback)
        {
            if (callback == null)
            {
                RemovePersistentRequest("wrapper_setConfigCompletedListener");
                NativeApi.ExecuteAdvancedInstruction("wrapper_setConfigCompletedListener", "");
                return;
            }
            AddPersistentRequest("wrapper_setConfigCompletedListener", value =>
            {
                try
                {
                    var parsed = JObject.Parse(value);
                    var config = new KochavaMeasurementConfig(Util.OptJObject(parsed["config"], new JObject()));
                    callback(config);
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    Util.Log("Warn: Malformed JSON in config callback");
                }
            });
            NativeApi.ExecuteAdvancedInstructionWithCallback("wrapper_setConfigCompletedListener", "", "wrapper_setConfigCompletedListener", "NativePersistentCallbackListener");
        }

        // Set the init completed callback listener. Deprecated: use SetConfigCompletedListener.
        [Obsolete("Use SetConfigCompletedListener.")]
        public void SetInitCompletedListener(Action<KochavaMeasurementInit>? callback)
        {
            if (callback == null) { SetConfigCompletedListener(null); return; }
            SetConfigCompletedListener(config => callback(new KochavaMeasurementInit(config.ConsentGdprApplies)));
        }

        // Set if consent has been explicitly opted in or out by the user.
        public void SetIntelligentConsentGranted(bool granted)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_setIntelligentConsentGranted",
                new JObject { ["granted"] = granted }.ToString());
        }

        // Returns if the SDK is currently started via a callback.
        public void GetStarted(Action<bool> callback)
        {
            var requestId = AddStringRequest(json =>
            {
                try
                {
                    var parsed = JObject.Parse(json);
                    callback(Util.OptBool(parsed["started"], false));
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    Util.Log("Warn: Malformed JSON in getStarted callback");
                }
            });
            NativeApi.ExecuteAdvancedInstructionWithCallback("wrapper_getStarted", "", requestId, "NativeEaiCallbackListener");
        }

        // Register the Editor App GUID. Do this prior to calling Start.
        public void RegisterEditorAppGuid(string editorAppGuid)
        {
            RegisteredEditorAppGuid = editorAppGuid;
        }

        // Register the Android App GUID. Do this prior to calling Start.
        public void RegisterAndroidAppGuid(string androidAppGuid)
        {
            RegisteredAndroidAppGuid = androidAppGuid;
        }

        // Register the Apple App GUID. Do this prior to calling Start.
        public void RegisterAppleAppGuid(string appleAppGuid)
        {
            RegisteredAppleAppGuid = appleAppGuid;
        }

        [Obsolete("Use RegisterAppleAppGuid.")]
        public void RegisterIosAppGuid(string iosAppGuid) => RegisterAppleAppGuid(iosAppGuid);

        // Register the WebGL App GUID. Do this prior to calling Start.
        public void RegisterWebGlAppGuid(string webGlAppGuid)
        {
            RegisteredWebGlAppGuid = webGlAppGuid;
        }

        // Register the MacOS App GUID. Do this prior to calling Start.
        public void RegisterMacOsAppGuid(string macOsAppGuid)
        {
            RegisteredMacOsAppGuid = macOsAppGuid;
        }

        // Register the Linux App GUID. Do this prior to calling Start.
        public void RegisterLinuxAppGuid(string linuxAppGuid)
        {
            RegisteredLinuxAppGuid = linuxAppGuid;
        }

        // Register the Windows App GUID. Do this prior to calling Start.
        public void RegisterWindowsAppGuid(string windowsAppGuid)
        {
            RegisteredWindowsAppGuid = windowsAppGuid;
        }

        // Register the Universal Windows Platform (UWP) App GUID. Do this prior to calling Start.
        public void RegisterUwpAppGuid(string uwpAppGuid)
        {
            RegisteredUwpAppGuid = uwpAppGuid;
        }

        // Register the Fallback App GUID. Do this prior to calling Start.
        // This App GUID will be used if an App GUID was not specifically set for the current platform.
        public void RegisterFallbackAppGuid(string fallbackAppGuid)
        {
            RegisteredFallbackAppGuid = fallbackAppGuid;
        }

        // Register your Partner Name. Do this prior to calling Start.
        // NOTE: Only use this method if directed to by your Client Success Manager.
        public void RegisterPartnerName(string partnerName)
        {
            RegisteredPartnerName = partnerName;
        }

        // Start the SDK with the previously registered App GUID or Partner Name.
        public void Start()
        {
            // Inject version extension.
            const string wrapper = "{\"name\":\"" + SdkName + "\",\"version\":\"" + SdkVersion + "\",\"build_date\":\"" + SdkBuildDate + "\"}";
            ExecuteAdvancedInstruction("wrapper", wrapper);

            // Build the start payload with all registered App GUIDs and Partner Name.
            // appGuid is resolved at compile time from the platform-specific GUID, falling back to the fallback GUID.
            var json = new JObject();
            if (!string.IsNullOrEmpty(RegisteredEditorAppGuid)) json["dotnetEditorAppGuid"] = RegisteredEditorAppGuid;
            if (!string.IsNullOrEmpty(RegisteredAndroidAppGuid)) json["androidAppGuid"] = RegisteredAndroidAppGuid;
            if (!string.IsNullOrEmpty(RegisteredAppleAppGuid)) json["appleAppGuid"] = RegisteredAppleAppGuid;
            if (!string.IsNullOrEmpty(RegisteredWebGlAppGuid)) json["dotnetWebGlAppGuid"] = RegisteredWebGlAppGuid;
            if (!string.IsNullOrEmpty(RegisteredMacOsAppGuid)) json["dotnetMacOsAppGuid"] = RegisteredMacOsAppGuid;
            if (!string.IsNullOrEmpty(RegisteredLinuxAppGuid)) json["dotnetLinuxAppGuid"] = RegisteredLinuxAppGuid;
            if (!string.IsNullOrEmpty(RegisteredWindowsAppGuid)) json["dotnetWindowsAppGuid"] = RegisteredWindowsAppGuid;
            if (!string.IsNullOrEmpty(RegisteredUwpAppGuid)) json["dotnetUwpAppGuid"] = RegisteredUwpAppGuid;
#if KVA_EDITOR
            var platformAppGuid = RegisteredEditorAppGuid;
#elif KVA_ANDROID
            var platformAppGuid = RegisteredAndroidAppGuid;
#elif KVA_IOS
            var platformAppGuid = RegisteredAppleAppGuid;
#elif KVA_WEBGL
            var platformAppGuid = RegisteredWebGlAppGuid;
#elif KVA_MACOS
            var platformAppGuid = RegisteredMacOsAppGuid;
#elif KVA_LINUX
            var platformAppGuid = RegisteredLinuxAppGuid;
#elif KVA_WINDOWS
            var platformAppGuid = RegisteredWindowsAppGuid;
#elif KVA_UWP
            var platformAppGuid = RegisteredUwpAppGuid;
#else
            var platformAppGuid = (string?)null;
#endif
            var appGuid = !string.IsNullOrEmpty(platformAppGuid) ? platformAppGuid : RegisteredFallbackAppGuid;
            if (!string.IsNullOrEmpty(appGuid)) json["appGuid"] = appGuid;
            if (!string.IsNullOrEmpty(RegisteredPartnerName)) json["partnerName"] = RegisteredPartnerName;
            NativeApi.ExecuteAdvancedInstruction("wrapper_start", json.ToString());
        }

        // Shuts down the SDK and optionally deletes all local SDK data.
        // NOTE: Care should be taken when using this method as deleting the SDK data will make it reset back to a first install state.
        public void Shutdown(bool deleteData)
        {
            // Clear registered app guids and partner name.
            RegisteredEditorAppGuid = null;
            RegisteredAndroidAppGuid = null;
            RegisteredAppleAppGuid = null;
            RegisteredWebGlAppGuid = null;
            RegisteredMacOsAppGuid = null;
            RegisteredLinuxAppGuid = null;
            RegisteredWindowsAppGuid = null;
            RegisteredUwpAppGuid = null;
            RegisteredFallbackAppGuid = null;
            RegisteredPartnerName = null;

            // Clear callbacks
            ShutdownCallbacks();

            // Shutdown the native
            NativeApi.ExecuteAdvancedInstruction("wrapper_shutdown",
                new JObject { ["deleteData"] = deleteData }.ToString());
        }

        // Returns the Kochava Install ID via a callback.
        public void RetrieveInstallId(Action<string> callback)
        {
            if (callback == null)
            {
                Util.Log("Warn: Invalid Callback");
                return;
            }
            var requestId = AddStringRequest(json =>
            {
                try
                {
                    var parsed = JObject.Parse(json);
                    callback(Util.OptString(parsed["installId"], ""));
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    Util.Log("Warn: Malformed JSON in installId callback");
                }
            });
            NativeApi.ExecuteAdvancedInstructionWithCallback("wrapper_retrieveInstallId", "", requestId, "NativeEaiCallbackListener");
        }

        // Retrieves the latest install attribution data from the server.
        public void RetrieveInstallAttribution(Action<KochavaMeasurementInstallAttribution> callback)
        {
            if (callback == null)
            {
                Util.Log("Warn: Invalid Callback");
                return;
            }
            var requestId = AddStringRequest(json =>
            {
                try
                {
                    var parsed = JObject.Parse(json);
                    var attributionJson = Util.OptJObject(parsed["installAttribution"], new JObject());
                    callback(new KochavaMeasurementInstallAttribution(attributionJson));
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    Util.Log("Warn: Malformed JSON in installAttribution callback");
                }
            });
            NativeApi.ExecuteAdvancedInstructionWithCallback("wrapper_retrieveInstallAttribution", "", requestId, "NativeEaiCallbackListener");
        }

        // Process a launch deeplink using the default 10 second timeout.
        public void ProcessDeeplink(string path, Action<KochavaMeasurementDeeplink> callback)
        {
            if (callback == null)
            {
                Util.Log("Warn: Invalid Callback");
                return;
            }
            var requestId = AddStringRequest(json =>
            {
                try
                {
                    var parsed = JObject.Parse(json);
                    var deeplinkJson = Util.OptJObject(parsed["deeplink"], new JObject());
                    callback(new KochavaMeasurementDeeplink(deeplinkJson));
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    Util.Log("Warn: Malformed JSON in deeplink callback");
                }
            });
            NativeApi.ExecuteAdvancedInstructionWithCallback("wrapper_processDeeplink",
                new JObject { ["path"] = path ?? "" }.ToString(), requestId, "NativeEaiCallbackListener");
        }

        // Process a launch deeplink using a custom timeout in seconds.
        public void ProcessDeeplinkWithOverrideTimeout(string path, double timeout, Action<KochavaMeasurementDeeplink> callback)
        {
            if (callback == null)
            {
                Util.Log("Warn: Invalid Callback");
                return;
            }
            var requestId = AddStringRequest(json =>
            {
                try
                {
                    var parsed = JObject.Parse(json);
                    var deeplinkJson = Util.OptJObject(parsed["deeplink"], new JObject());
                    callback(new KochavaMeasurementDeeplink(deeplinkJson));
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    Util.Log("Warn: Malformed JSON in deeplink callback");
                }
            });
            NativeApi.ExecuteAdvancedInstructionWithCallback("wrapper_processDeeplink",
                new JObject { ["path"] = path ?? "", ["timeout"] = timeout }.ToString(), requestId, "NativeEaiCallbackListener");
        }
        
        // Registers a default parameter on every event.
        public void RegisterDefaultEventStringParameter(string name, string? value)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_registerEventDefaultParameter",
                new JObject { ["name"] = name, ["value"] = value }.ToString());
        }

        // Registers a default parameter on every event.
        public void RegisterDefaultEventBoolParameter(string name, bool? value)
        {
            var json = new JObject { ["name"] = name };
            json["value"] = value.HasValue ? (JToken)value.Value : JValue.CreateNull();
            NativeApi.ExecuteAdvancedInstruction("wrapper_registerEventDefaultParameter", json.ToString());
        }

        // Registers a default parameter on every event.
        public void RegisterDefaultEventNumberParameter(string name, double? value)
        {
            var json = new JObject { ["name"] = name };
            json["value"] = value.HasValue ? (JToken)value.Value : JValue.CreateNull();
            NativeApi.ExecuteAdvancedInstruction("wrapper_registerEventDefaultParameter", json.ToString());
        }

        // Registers a default user_id value on every event.
        public void RegisterDefaultEventUserId(string? value)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_registerEventDefaultParameter",
                new JObject { ["name"] = "user_id", ["value"] = value }.ToString());
        }

        // Send an event.
        public void SendEvent(string name)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_sendEvent",
                new JObject { ["eventName"] = name }.ToString());
        }

        // Send an event with string data.
        public void SendEventWithString(string name, string data)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_sendEvent",
                new JObject { ["eventName"] = name, ["eventDataString"] = data }.ToString());
        }

        // Send an event with dictionary data.
        public void SendEventWithDictionary(string name, JObject data)
        {
            NativeApi.ExecuteAdvancedInstruction("wrapper_sendEvent",
                new JObject { ["eventName"] = name, ["eventData"] = data }.ToString());
        }

        // Send an event object (Called via Event.send().
        public void SendEventWithEvent(KochavaMeasurementEvent standardEvent)
        {
            if (standardEvent == null)
            {
                Util.Log("Warn: Invalid Event");
                return;
            }

            var json = new JObject
            {
                ["eventName"] = standardEvent.GetEventName(),
                ["eventData"] = standardEvent.GetEventData()
            };
            var appleReceipt = standardEvent.GetAppleAppStoreReceiptBase64String();
            if (!string.IsNullOrEmpty(appleReceipt))
                json["appleAppStoreReceiptBase64String"] = appleReceipt;
            var androidData = standardEvent.GetAndroidGooglePlayReceiptData();
            var androidSig = standardEvent.GetAndroidGooglePlayReceiptSignature();
            if (!string.IsNullOrEmpty(androidData))
                json["androidGooglePlayReceiptData"] = androidData;
            if (!string.IsNullOrEmpty(androidSig))
                json["androidGooglePlayReceiptSignature"] = androidSig;
            NativeApi.ExecuteAdvancedInstruction("wrapper_sendEvent", json.ToString());
        }

        // Build and return an event using a Standard Event Type.
        public KochavaMeasurementEvent BuildEventWithEventType(KochavaMeasurementEventType eventType)
        {
            return new KochavaMeasurementEvent(eventType);
        }

        // Build and return an event using a custom name.
        public KochavaMeasurementEvent BuildEventWithEventName(string eventName)
        {
            return new KochavaMeasurementEvent(eventName);
        }

    }

    #endregion

    // Internal Kochava SDK
    namespace Internal
    {
        // EAI dispatch interface — the single seam between KochavaMeasurement and the platform layer.
        internal interface IEaiAdapter
        {
            void ExecuteAdvancedInstruction(string name, string value);
            void ExecuteAdvancedInstructionWithCallback(string name, string value, string requestId, string listenerMethod);
        }

#if !KVA_TEST
        // Internal Singleton Handler for all MonoBehaviour actions.
        internal class SingletonHandler : MonoBehaviour
        {
            // Singleton instance
            private static readonly object SingletonLock = new object();
            private static volatile SingletonHandler SingletonInstance;

            // Measurement instance.
            private NativeApi NativeApi;
            internal KochavaMeasurement Measurement;

            // Callbacks
            private static readonly object CallbackLock = new object();
            private readonly Dictionary<string, Action<string>> StringRequests = new Dictionary<string, Action<string>>();
            private readonly Dictionary<string, string> StringResponses = new Dictionary<string, string>();
            private readonly Dictionary<string, Action<string>> PersistentRequests = new Dictionary<string, Action<string>>();
            private readonly List<KeyValuePair<string, string>> PersistentResponses = new List<KeyValuePair<string, string>>();
            
#if KVA_NETSTD
            private readonly List<KochavaNetStd.NativeRequest> NetStdNativeRequests = new List<KochavaNetStd.NativeRequest>();
#endif

            // Singleton instance. This creates a Unity GameObject called KochavaMeasurement that is set to survive level loading.
            public static SingletonHandler Instance
            {
                get
                {
                    if (SingletonInstance != null) return SingletonInstance;
                    lock (SingletonLock)
                    {
                        if (SingletonInstance != null) return SingletonInstance;
                        
                        // Create the Kochava Game Object.
                        var kochavaMeasurementGameObject = new GameObject("KochavaMeasurement");
                        DontDestroyOnLoad(kochavaMeasurementGameObject);

                        // Create Singleton Instance on that game object.
                        SingletonInstance = kochavaMeasurementGameObject.AddComponent<SingletonHandler>();

                        // Create Native API Handler for the current platform.
#if KVA_ANDROID
                        SingletonInstance.NativeApi = kochavaMeasurementGameObject.AddComponent<NativeAndroid>();
#elif KVA_IOS
                        SingletonInstance.NativeApi = kochavaMeasurementGameObject.AddComponent<NativeIos>();
#else
                        SingletonInstance.NativeApi = kochavaMeasurementGameObject.AddComponent<NativeDotNet>();
#endif
                        SingletonInstance.Measurement = new KochavaMeasurement(SingletonInstance.NativeApi);
                    }

                    return SingletonInstance;
                }
            }

            // Process actions from native SDKs that must occur on the Unity main thread.
            private void Update()
            {
                // Call the nativeApi update handler.
                NativeApi.Update();

                // Process each callback handler.
                ProcessStringCallbacks();
                ProcessPersistentCallbacks();
#if KVA_NETSTD
                ProcessNetStdNativeRequests();
#endif
            }

            // Shutdown and clear callbacks.
            internal void Shutdown()
            {
                lock (CallbackLock)
                {
                    StringRequests.Clear();
                    StringResponses.Clear();
                    PersistentRequests.Clear();
                    PersistentResponses.Clear();
#if KVA_NETSTD
                    NetStdNativeRequests.Clear();
#endif
                }
            }

            // Add a string request with callback. Use the returned requestId to match the response.
            internal string AddStringRequest(Action<string> callback)
            {
                var requestId = Guid.NewGuid().ToString();
                lock (CallbackLock)
                {
                    StringRequests.Add(requestId, callback);
                }
                return requestId;
            }

            // Add a string response. Use the generated requestId from the request to match up.
            internal void AddStringResponse(string requestId, string value)
            {
                lock (CallbackLock)
                {
                    StringResponses.Add(requestId, value);
                }
            }

            // Add a persistent response by id. Used by the NetStd path to route callbacks to registered persistent handlers.
            internal void AddPersistentResponse(string id, string value)
            {
                lock (CallbackLock)
                {
                    PersistentResponses.Add(new KeyValuePair<string, string>(id, value));
                }
            }

            // Process String Responses. Must be called from the Unity Update thread.
            private void ProcessStringCallbacks()
            {
                List<(string key, string value, Action<string> callback)> toFire;
                lock (CallbackLock)
                {
                    if (StringResponses.Count == 0) return;
                    toFire = new List<(string, string, Action<string>)>(StringResponses.Count);
                    foreach (var entry in StringResponses)
                    {
                        if (StringRequests.TryGetValue(entry.Key, out var cb))
                        {
                            toFire.Add((entry.Key, entry.Value, cb));
                            StringRequests.Remove(entry.Key);
                        }
                    }
                    StringResponses.Clear();
                }
                foreach (var (_, value, callback) in toFire)
                    callback(value);
            }

            // Callback method for EAI one-shot callbacks via UnitySendMessage. Name and location cannot change.
            private void NativeEaiCallbackListener(string msg)
            {
                if (string.IsNullOrEmpty(msg)) return;
                try
                {
                    var response = JObject.Parse(msg);
                    var id = (string)response["id"] ?? "";
                    var value = (string)response["value"] ?? "";
                    AddStringResponse(id, value);
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    Util.Log("Warn: Malformed JSON in EAI callback");
                }
            }

            // Register a persistent callback keyed by id. Survives repeated native callbacks until cleared or shutdown.
            internal void AddPersistentRequest(string id, Action<string> callback)
            {
                lock (CallbackLock)
                {
                    PersistentRequests[id] = callback;
                }
            }

            // Remove a persistent callback by id and stops dispatching to it.
            internal void RemovePersistentRequest(string id)
            {
                lock (CallbackLock)
                {
                    PersistentRequests.Remove(id);
                }
            }

            // Process persistent callback events. Handlers are NOT removed after firing. Must be called from the Unity Update thread.
            private void ProcessPersistentCallbacks()
            {
                List<(Action<string> callback, string value)> toFire;
                lock (CallbackLock)
                {
                    if (PersistentResponses.Count == 0) return;
                    toFire = new List<(Action<string>, string)>(PersistentResponses.Count);
                    foreach (var entry in PersistentResponses)
                    {
                        if (PersistentRequests.TryGetValue(entry.Key, out var cb))
                            toFire.Add((cb, entry.Value));
                    }
                    PersistentResponses.Clear();
                }
                foreach (var (callback, value) in toFire)
                    callback(value);
            }

            // Callback method for persistent callbacks via UnitySendMessage. Name and location cannot change.
            private void NativePersistentCallbackListener(string msg)
            {
                if (string.IsNullOrEmpty(msg)) return;
                try
                {
                    var response = JObject.Parse(msg);
                    var id = (string)response["id"] ?? "";
                    var value = (string)response["value"] ?? "";
                    lock (CallbackLock)
                    {
                        PersistentResponses.Add(new KeyValuePair<string, string>(id, value));
                    }
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    Util.Log("Warn: Malformed JSON in persistent callback");
                }
            }

#if KVA_NETSTD
            // Add a Net Standard Native Request to the queue.
            internal void AddNetStdNativeRequest(KochavaNetStd.NativeRequest nativeRequest)
            {
                lock (CallbackLock)
                {
                    NetStdNativeRequests.Add(nativeRequest);
                }
            }

            // Process Net Std Native Requests. Must be called from the Unity Update thread.
            private void ProcessNetStdNativeRequests()
            {
                // Snapshot and clear while holding the lock so that any new requests queued
                // during Fulfill() (which runs synchronously on WebGL — no background threads)
                // are captured on the next frame instead of corrupting the active iteration.
                List<KochavaNetStd.NativeRequest> snapshot;
                lock (CallbackLock)
                {
                    if (NetStdNativeRequests.Count == 0) return;
                    snapshot = new List<KochavaNetStd.NativeRequest>(NetStdNativeRequests);
                    NetStdNativeRequests.Clear();
                }

                foreach (var nativeRequest in snapshot)
                {
                    switch (nativeRequest.Action)
                    {
                        // Gather the specified datapoint.
                        case KochavaNetStd.NativeRequest.NativeRequestType.GatherDatapoint:
                            ProcessNetStdNativeRequestGatherDatapoint(nativeRequest);
                            break;
                        // Write or Delete a key/value string to disk.
                        case KochavaNetStd.NativeRequest.NativeRequestType.WriteStringToDisk:
                            ProcessNetStdNativeRequestWriteStringToDisk(nativeRequest);
                            break;
                        // Read a key/value string from disk.
                        case KochavaNetStd.NativeRequest.NativeRequestType.GetStringFromDisk:
                            ProcessNetStdNativeRequestGetStringFromDisk(nativeRequest);
                            break;
                        // Delete all persisted data.
                        case KochavaNetStd.NativeRequest.NativeRequestType.ErasePersistedData:
                            ProcessNetStdNativeRequestErasePersistedData(nativeRequest);
                            break;
                        // Perform a network request.
                        case KochavaNetStd.NativeRequest.NativeRequestType.NetworkRequest:
                            StartCoroutine(ProcessNetStdNativeRequestNetworkRequest(nativeRequest));
                            break;
                        // Unknown command.
                        default:
                            nativeRequest.Fulfill();
                            break;
                    }
                }
            }
            
            // Gather Net Std data points.
            private void ProcessNetStdNativeRequestGatherDatapoint(KochavaNetStd.NativeRequest nativeRequest)
            {
                if (nativeRequest.Key == "package") nativeRequest.Fulfill(GetPackage());
                else if (nativeRequest.Key == "platform") nativeRequest.Fulfill(GetPlatform());
                else if (nativeRequest.Key == "device") nativeRequest.Fulfill(SystemInfo.deviceModel);
                else if (nativeRequest.Key == "disp_w") nativeRequest.Fulfill(Screen.width);
                else if (nativeRequest.Key == "disp_h") nativeRequest.Fulfill(Screen.height);
                else if (nativeRequest.Key == "screen_dpi") nativeRequest.Fulfill(Screen.dpi);
                else if (nativeRequest.Key == "os_version") nativeRequest.Fulfill(SystemInfo.operatingSystem);
                else if (nativeRequest.Key == "battery_level") nativeRequest.Fulfill(SystemInfo.batteryLevel);
                else if (nativeRequest.Key == "battery_status") nativeRequest.Fulfill(GetBatteryStatus());
                else if (nativeRequest.Key == "architecture") nativeRequest.Fulfill(GetArchitecture());
                else if (nativeRequest.Key == "device_orientation") nativeRequest.Fulfill(GetOrientation());
                else if (nativeRequest.Key == "app_version") nativeRequest.Fulfill(Application.version);
                else if (nativeRequest.Key == "app_short_string") nativeRequest.Fulfill(Application.version);
                else if (nativeRequest.Key == "app_name") nativeRequest.Fulfill(Application.productName);
                else if (nativeRequest.Key == "language") nativeRequest.Fulfill(Application.systemLanguage.ToString());
                else if (nativeRequest.Key == "form_factor") nativeRequest.Fulfill(GetDeviceType());
                else if (nativeRequest.Key == "network_conn_type") nativeRequest.Fulfill(GetNetworkConnType());
                else if (nativeRequest.Key == "iab_usp") nativeRequest.Fulfill(PlayerPrefs.GetString("IABUSPrivacy_String", null));
                else if (nativeRequest.Key == "user_agent") nativeRequest.Fulfill(GetUserAgent());
#if KVA_UWP
                // Gather the waid (advertising ID) only for the UWP platform.
                // RequestAdvertisingIdentifierAsync returns false when not supported and does not invoke the delegate.
                else if (nativeRequest.Key == "waid")
                {
                    if (!Application.RequestAdvertisingIdentifierAsync((advertisingId, trackingEnabled, error) => { nativeRequest.Fulfill(advertisingId); }))
                        nativeRequest.Fulfill();
                }
                else if (nativeRequest.Key == "device_limit_tracking")
                {
                    if (!Application.RequestAdvertisingIdentifierAsync((advertisingId, trackingEnabled, error) => { nativeRequest.Fulfill(trackingEnabled); }))
                        nativeRequest.Fulfill();
                }
#endif
                else nativeRequest.Fulfill();
            }

            // Write Net Std data to storage.
            private void ProcessNetStdNativeRequestWriteStringToDisk(KochavaNetStd.NativeRequest nativeRequest)
            {
                if (nativeRequest.Value == null)
                {
                    PlayerPrefs.DeleteKey(nativeRequest.Key);
                }
                else
                {
                    PlayerPrefs.SetString(nativeRequest.Key, nativeRequest.Value.ToString());
                }

                nativeRequest.Fulfill();
            }
            
            // Read Net Std data from storage.
            private void ProcessNetStdNativeRequestGetStringFromDisk(KochavaNetStd.NativeRequest nativeRequest)
            {
                nativeRequest.Fulfill(PlayerPrefs.GetString(nativeRequest.Key, null));
            }
            
            // Erase all Net Std data from storage.
            private void ProcessNetStdNativeRequestErasePersistedData(KochavaNetStd.NativeRequest nativeRequest)
            {
                var keys = JArray.Parse(nativeRequest.Key ?? "[]");
                foreach (var key in keys)
                {
                    PlayerPrefs.DeleteKey(key.ToObject<string>());
                }

                nativeRequest.Fulfill();
            }

            // Perform a Net Std network request.
            private IEnumerator ProcessNetStdNativeRequestNetworkRequest(KochavaNetStd.NativeRequest nativeRequest)
            {
                // Parse the request properties.
                var value = JObject.Parse(nativeRequest.Key ?? "{}");
                var url = Util.OptString(value["url"], "");
                var method = Util.OptBool(value["isGET"], false) ? "GET" : "POST";
                var body = Util.OptString(value["jsonContentBody"], "");
                var userAgent = Util.OptString(value["userAgent"], "");
                var noRedirect = Util.OptBool(value["noRedirect"], false);

                // Return as error if there is no url.
                if (url == "")
                {
                    nativeRequest.Fulfill(new KochavaNetStd.NetworkRequestResult { Success = false });
                    yield break;
                }

                // Build the network request.
                var www = new UnityWebRequest(url, method);
                www.timeout = 20;
                www.downloadHandler = new DownloadHandlerBuffer();
                www.disposeDownloadHandlerOnDispose = true;
#if !KVA_WEBGL
                // ESP unwrap requests must not auto-follow redirects — the Location header is the unwrapped URL.
                if (noRedirect) www.redirectLimit = 0;

                // Note: The user agent cannot be overridden on the WebGL platform.
                if(!string.IsNullOrEmpty(userAgent))
                {
                    www.SetRequestHeader("User-Agent", userAgent);
                }
#endif
                if(!string.IsNullOrEmpty(body))
                {
                    www.uploadHandler = new UploadHandlerRaw (System.Text.Encoding.UTF8.GetBytes (body));
                    www.disposeUploadHandlerOnDispose = true;
                    www.SetRequestHeader("Content-Type", "application/json");
                }

                // Perform the network request
                yield return www.SendWebRequest();

#if !KVA_WEBGL
                // For ESP unwrap requests, a redirect response carries the unwrapped URL in the Location header.
                if (noRedirect && www.responseCode >= 300 && www.responseCode < 400)
                {
                    var location = www.GetResponseHeader("Location") ?? "";
                    nativeRequest.Fulfill(new KochavaNetStd.NetworkRequestResult
                    {
                        Success = !string.IsNullOrEmpty(location),
                        Body = location,
                        StatusCode = (int)www.responseCode
                    });
                    www.Dispose();
                    yield break;
                }
#endif

                var succeeded = www.result == UnityWebRequest.Result.Success;
                nativeRequest.Fulfill(new KochavaNetStd.NetworkRequestResult
                {
                    Success = succeeded,
                    Body = succeeded ? (www.downloadHandler.text ?? "") : null,
                    StatusCode = www.responseCode > 0 ? (int)www.responseCode : (int?)null,
                    ErrorMessage = succeeded ? null : www.error
                });
                www.Dispose();
            }
#endif

            // Returns the current platform.
            private static string GetPlatform()
            {
#if UNITY_EDITOR
                return "UnityEditor";
#elif UNITY_WEBGL
                return "WebGL";
#elif UNITY_STANDALONE_OSX
                return "MacOSX";
#elif UNITY_STANDALONE_LINUX
                return "Linux";
#elif UNITY_STANDALONE_WIN
                return "WindowsDesktop";
#elif UNITY_WII
                return "Wii";
#elif UNITY_PS4
                return "PS4";
#elif UNITY_XBOXONE
                return "XboxOne";
#elif UNITY_TIZEN
                return "Tizen";
#elif UNITY_TVOS
                return "tvos";
#elif UNITY_IOS
                return "ios";
#elif UNITY_ANDROID
                return "android";
#elif UNITY_WSA
                return "windows";
#else
                return Application.platform.ToString();
#endif
            }

            // Returns a generated user agent.
            private static string GetUserAgent()
            {
                return "Mozilla/5.0 (" + SystemInfo.operatingSystem + ")";
            }

            // Returns the application package name.
            // If the platform supports a bundle/package directly that is returned. Otherwise a package name is generated.
            private static string GetPackage()
            {
                if (!string.IsNullOrEmpty(Application.identifier))
                {
                    return Application.identifier;
                }

                return (Application.companyName + "." + Application.productName + "." + GetPlatform()).ToLowerInvariant().Replace(" ", "");
            }

            // Returns the device CPU architecture.
            private static string GetArchitecture()
            {
                return RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
            }

            // Returns the current device screen orientation.
            private static string GetOrientation()
            {
                if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
                {
                    return "landscape";
                }

                if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown ||
                    (Screen.width < Screen.height))
                {
                    return "portrait";
                }

                return "landscape";
            }

            // Returns the device type or form factor.
            private static string GetDeviceType()
            {
                var deviceType = SystemInfo.deviceType;
                switch (deviceType)
                {
                    case DeviceType.Handheld:
                        return "handheld";
                    case DeviceType.Desktop:
                        return "desktop";
                    case DeviceType.Console:
                        return "console";
                    default:
                        return "unknown";
                }
            }

            // Returns the current network connection type or none if not connected.
            private static string GetNetworkConnType()
            {
                var reachability = Application.internetReachability;
                switch (reachability)
                {
                    case NetworkReachability.ReachableViaCarrierDataNetwork:
                        return "cellular";
                    case NetworkReachability.ReachableViaLocalAreaNetwork:
                        return "wifi";
                    default:
                        return "none";
                }
            }

            // Returns the current battery status. Will be unknown on devices that are not battery powered.
            private static string GetBatteryStatus()
            {
                var batteryStatus = SystemInfo.batteryStatus;
                switch (batteryStatus)
                {
                    case BatteryStatus.Charging:
                        return "charging";
                    case BatteryStatus.Discharging:
                        return "discharging";
                    case BatteryStatus.Full:
                        return "full";
                    case BatteryStatus.NotCharging:
                        return "not_charging";
                    default:
                        return "unknown";
                }
            }
        }
#endif // !KVA_TEST

#if !KVA_TEST
        // Native SDK API Interface — platform-specific methods only.
        internal abstract class NativeApi : MonoBehaviour, IEaiAdapter
        {
            internal virtual void Update() { }
            internal abstract void ExecuteAdvancedInstruction(string name, string value);
            internal abstract void ExecuteAdvancedInstructionWithCallback(string name, string value, string requestId, string listenerMethod);
            void IEaiAdapter.ExecuteAdvancedInstruction(string name, string value) => ExecuteAdvancedInstruction(name, value);
            void IEaiAdapter.ExecuteAdvancedInstructionWithCallback(string name, string value, string requestId, string listenerMethod) => ExecuteAdvancedInstructionWithCallback(name, value, requestId, listenerMethod);
        }
#endif // !KVA_TEST

        // Common utility methods
        internal static class Util
        {
            // Log a message with the Kochava prefix.
            public static void Log(string message)
            {
#if KVA_TEST
                Console.WriteLine("KVA/Measurement: " + message);
#else
                Debug.Log("KVA/Measurement: " + message);
#endif
            }
            
            public static JObject? OptJObject(JToken? item)
            {
                return item?.ToObject<JObject>();
            }

            public static JObject OptJObject(JToken? item, JObject defaultValue)
            {
                return OptJObject(item) ?? defaultValue;
            }

            public static string? OptString(JToken? item)
            {
                return item?.ToObject<string>();
            }

            public static string OptString(JToken? item, string defaultValue)
            {
                return OptString(item) ?? defaultValue;
            }

            public static bool? OptBool(JToken? item)
            {
                return item?.ToObject<bool?>();
            }

            public static bool OptBool(JToken? item, bool defaultValue)
            {
                return OptBool(item) ?? defaultValue;
            }
        }

        #region Android

#if KVA_ANDROID
        // API access the native Android SDK
        internal class NativeAndroid : NativeApi
        {
            private AndroidJavaObject AndroidContext;
            private AndroidJavaObject AndroidMeasurement;

            // Callback proxy for EAI callbacks with response routing.
            private class AndroidEaiCallbackHandler : AndroidJavaProxy
            {
                private readonly string RequestId;
                private readonly string ListenerMethod;

                public AndroidEaiCallbackHandler(string requestId, string listenerMethod)
                    : base("com.kochava.measurement.base.internal.ExecutedAdvancedInstructionListener")
                {
                    RequestId = requestId ?? "";
                    ListenerMethod = listenerMethod ?? "";
                }

                void onExecutedAdvancedInstruction(string data)
                {
                    string response;
                    if (!string.IsNullOrEmpty(RequestId))
                    {
                        response = new JObject { ["id"] = RequestId, ["value"] = data ?? "" }.ToString();
                    }
                    else
                    {
                        response = data ?? "{}";
                    }
                    using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        player.CallStatic("UnitySendMessage", "KochavaMeasurement", ListenerMethod, response);
                    }
                }
            }

            // Retrieve the Android Application Context and v6 Measurement singleton.
            internal NativeAndroid()
            {
                using (var androidUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    var androidActivity = androidUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidContext = androidActivity.Call<AndroidJavaObject>("getApplicationContext");
                }

                using (var measurementClass = new AndroidJavaClass("com.kochava.measurement.base.Measurement"))
                {
                    AndroidMeasurement = measurementClass.CallStatic<AndroidJavaObject>("getInstance");
                }
            }

            // Reserved function, only use if directed to by your Client Success Manager.
            internal override void ExecuteAdvancedInstruction(string name, string value)
            {
                AndroidMeasurement.Call("executeAdvancedInstruction", AndroidContext, name, value, null);
            }

            // Execute an advanced instruction and route the response to a Unity listener method.
            internal override void ExecuteAdvancedInstructionWithCallback(string name, string value, string requestId, string listenerMethod)
            {
                AndroidMeasurement.Call("executeAdvancedInstruction", AndroidContext, name, value, new AndroidEaiCallbackHandler(requestId, listenerMethod));
            }
        }
#endif

        #endregion

        #region iOS

#if KVA_IOS
        // API access for the native iOS SDK. All EAI commands are dispatched through ExecuteAdvancedInstruction.
        internal class NativeIos : NativeApi
        {
            // Reserved function, only use if directed to by your Client Success Manager.
            internal override void ExecuteAdvancedInstruction(string name, string value)
            {
                iosNativeExecuteAdvancedInstruction(name, value);
            }

            // Execute an advanced instruction and route the response to a Unity listener method.
            internal override void ExecuteAdvancedInstructionWithCallback(string name, string value, string requestId, string listenerMethod)
            {
                iosNativeExecuteAdvancedInstructionWithCallback(name, value, requestId, listenerMethod);
            }

            // .m layer interface — functions defined in KochavaWrapper.m.
            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeExecuteAdvancedInstruction(string name, string value);

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern void iosNativeExecuteAdvancedInstructionWithCallback(string name, string value, string requestId, string listenerMethod);
        }
#endif

        #endregion

        #region NetStd

#if KVA_NETSTD
        // API access for the native .NET SDK. This is used for all platforms that are not Android, iOS, or WebGL.
        internal class NativeDotNet : NativeApi
        {
            private KochavaNetStd.Measurement Measurement;

            internal NativeDotNet()
            {
                InitializeMeasurement();
            }

            // Create a new instance of the Measurement and initialize logging.
            private void InitializeMeasurement()
            {
                // Create the Measurement instance.
                if (Measurement == null)
                {
                    Measurement = new KochavaNetStd.Measurement(nativeRequest =>
                    {
                        // Queue known actions to run on the Unity Thread.
                        if (nativeRequest.Action == KochavaNetStd.NativeRequest.NativeRequestType.GatherDatapoint
                            || nativeRequest.Action == KochavaNetStd.NativeRequest.NativeRequestType.WriteStringToDisk
                            || nativeRequest.Action == KochavaNetStd.NativeRequest.NativeRequestType.GetStringFromDisk
                            || nativeRequest.Action == KochavaNetStd.NativeRequest.NativeRequestType.ErasePersistedData
                            || nativeRequest.Action == KochavaNetStd.NativeRequest.NativeRequestType.AttemptRestoreInstallIdBackup
                            || nativeRequest.Action == KochavaNetStd.NativeRequest.NativeRequestType.AttemptWriteInstallIdBackup
                            || nativeRequest.Action == KochavaNetStd.NativeRequest.NativeRequestType.NetworkRequest)
                        {
                            SingletonHandler.Instance.AddNetStdNativeRequest(nativeRequest);
                        }
                        // Immediately fulfill any other actions.
                        else
                        {
                            nativeRequest.Fulfill();
                        }
                    });
                }

                // Initialize logging.
                KochavaNetStd.Global.PrettyPrintJson = true;
                KochavaNetStd.Global.InitializeLogging(entry => Debug.Log(entry.ToString()), KochavaNetStd.Global.LogLevel.Info);
            }

            // Unity update tick
            internal override void Update()
            {
                Measurement.Update();
            }

            // Reserved function, only use if directed to by your Client Success Manager.
            internal override void ExecuteAdvancedInstruction(string name, string value)
            {
                Measurement.ExecuteAdvancedInstruction(name, value);
            }

            internal override void ExecuteAdvancedInstructionWithCallback(string name, string value, string requestId, string listenerMethod)
            {
                Measurement.ExecuteAdvancedInstruction(name, value, response =>
                {
                    var safeResponse = string.IsNullOrEmpty(response) ? "{}" : response;
                    if (listenerMethod == "NativePersistentCallbackListener")
                        SingletonHandler.Instance.AddPersistentResponse(requestId, safeResponse);
                    else
                        SingletonHandler.Instance.AddStringResponse(requestId, safeResponse);
                });
            }
        }
#endif

        #endregion
    }
}