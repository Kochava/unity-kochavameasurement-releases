//
//  KochavaMeasurement (Unity)
//
//  Copyright (c) 2013 - 2026 Kochava, Inc. All rights reserved.
//

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

// Imports
using UnityEngine;
using System;

// Kochava SDK
namespace Kochava
{
    // Kochava Measurement SDK Configuration Object
    public class KochavaConfiguration : MonoBehaviour {

        // Editor-configurable settings
        #region Settings

        [Header("App GUID Configuration")]
        [Tooltip("App GUID when running in the Unity Editor regardless of the selected platform.")]
        public AppGuid editorAppGuid = new AppGuid();
        [Tooltip("App GUID for the Android Platform.")]
        public AppGuid androidAppGuid = new AppGuid();
        [Tooltip("App GUID for the Apple Platform.")]
        public AppGuid appleAppGuid = new AppGuid();
        [Tooltip("App GUID for the WebGL Platform.")]
        public AppGuid webGlAppGuid = new AppGuid();
        [Tooltip("App GUID for the macOS Platform.")]
        public AppGuid macOsAppGuid = new AppGuid();
        [Tooltip("App GUID for the Linux Platform.")]
        public AppGuid linuxAppGuid = new AppGuid();
        [Tooltip("App GUID for the Windows Platform.")]
        public AppGuid windowsAppGuid = new AppGuid();
        [Tooltip("App GUID for the Universal Windows Platform.")]
        public AppGuid uwpAppGuid = new AppGuid();
        [Tooltip("App GUID for use if no other App GUID was specified.")]
        public AppGuid fallbackAppGuid = new AppGuid();

        [Header("Logging")]
        [Tooltip("Log Level used in a Production build.")]
        public KochavaMeasurementLogLevel productionLogLevel;
        [Tooltip("Log Level used in a Development build.")]
        public KochavaMeasurementLogLevel developmentLogLevel;

        [Header("App Tracking Transparency (Apple)")]
        public AppleAttConfig appleATTConfiguration = new AppleAttConfig();

        [Header("App Clips (Apple)")]
        [Tooltip("App Group Identifier for Apple App Clips support.")]
        public string appleAppClipIdentifier = "";

        [Header("Partner Configuration (Advanced)")]
        [Tooltip("Only use if directed to by your Client Success Manager.")]
        public string kochavaPartnerName = "";

        #endregion

        // initialize kochava -- it will be ready to use during Start()
        private void Awake()
        {
            // Ensure this config game object is not destroyed.
            DontDestroyOnLoad(gameObject);

            // App GUIDs
            KochavaMeasurement.Instance.RegisterEditorAppGuid(editorAppGuid.Get());
            KochavaMeasurement.Instance.RegisterAndroidAppGuid(androidAppGuid.Get());
            KochavaMeasurement.Instance.RegisterAppleAppGuid(appleAppGuid.Get());
            KochavaMeasurement.Instance.RegisterWebGlAppGuid(webGlAppGuid.Get());
            KochavaMeasurement.Instance.RegisterMacOsAppGuid(macOsAppGuid.Get());
            KochavaMeasurement.Instance.RegisterLinuxAppGuid(linuxAppGuid.Get());
            KochavaMeasurement.Instance.RegisterWindowsAppGuid(windowsAppGuid.Get());
            KochavaMeasurement.Instance.RegisterUwpAppGuid(uwpAppGuid.Get());
            KochavaMeasurement.Instance.RegisterFallbackAppGuid(fallbackAppGuid.Get());

            // Log Level
            KochavaMeasurement.Instance.SetLogLevel(Debug.isDebugBuild ? developmentLogLevel : productionLogLevel);

            // App Clips
#if KVA_IOS
            if (!string.IsNullOrEmpty(appleAppClipIdentifier))
            {
                KochavaMeasurement.Instance.EnableAppleAppClips(appleAppClipIdentifier);
            }
#endif

            // App Tracking Transparency options
#if KVA_IOS
            if (appleATTConfiguration.enabled)
            {
                KochavaMeasurement.Instance.EnableAppleAtt();
            }
            KochavaMeasurement.Instance.SetAppleAttAuthorizationAutoRequest(appleATTConfiguration.autoRequest);
            KochavaMeasurement.Instance.SetAppleAttAuthorizationWaitTime(appleATTConfiguration.waitTime);
#endif
            // Partner Name
            KochavaMeasurement.Instance.RegisterPartnerName(kochavaPartnerName);

            // Start
            KochavaMeasurement.Instance.Start();
        }

        // App GUID container for specifying Production and Development App GUIDs.
        [Serializable]
        public class AppGuid
        {
            [Tooltip("App GUID used in a Production build.")]
            public string production = "";
            [Tooltip("Optional App GUID to use in a Development build.")]
            public string development = "";

            // Return the App GUID to use.
            public string Get()
            {
                if (Debug.isDebugBuild && !string.IsNullOrEmpty(development))
                {
                    return development;
                }
                return production;
            }
        }

        // Apple App Tracking Transparency Configuration.
        [Serializable]
        public class AppleAttConfig
        {
            [Tooltip("Enable SDK ATT enforcement")]
            public bool enabled = false;
            [Tooltip("How many seconds the SDK should wait for a response.")]
            public double waitTime = 30;
            [Tooltip("If the SDK should automatically request authorization.")]
            public bool autoRequest = true;
        }
    }
}
