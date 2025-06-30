// Copyright (c) Meta Platforms, Inc. and affiliates.

namespace QuestAccessibility
{
    public static class AccessibilityConstants
    {
        // For Android Class Names
        public const string androidClassAccessibilityManager = "com.meta.unity.accessibility.AccessibilityManager";
        public const string androidClassPreferencesManager = "horizonos.os.preferences.PreferencesManager";
        public const string androidClassUnityPlayer = "com.unity3d.player.UnityPlayer";
        public const string androidClassAccessibilityMetadata = "com.meta.unity.accessibility.AccessibilityMetadata";
        public const string androidClassPerformActionListener = "com.meta.unity.accessibility.AccessibilityNodeProvider$PerformActionListener";

        // For Android Method Names
        public const string androidMethodInitialize = "initialize";
        public const string androidMethodUpdateAccessibilityNodeInfo = "updateAccessibilityNodeInfo";
        public const string androidMethodCreateBatchedAccessibilityNodeInfos = "createBatchedAccessibilityNodeInfos";
        public const string androidMethodInsertBatchedAccessibilityNodeInfos = "insertBatchedAccessibilityNodeInfos";
        public const string androidMethodRemoveBatchedAccessibilityNodeInfos = "removeBatchedAccessibilityNodeInfos";
        public const string androidMethodSendAndroidAccessibilityEvent = "sendAndroidAccessibilityEvent";
        public const string androidMethodGetSystemService = "getSystemService";
        public const string androidMethodGetBoolean = "getBoolean";
        public const string androidMethodGetInt = "getInt";
        public const string androidMethodGetLong = "getLong";
        public const string androidMethodGetFloat = "getFloat";
        public const string androidMethodGetDouble = "getDouble";

        // For Android Static Field Names
        public const string androidFieldCurrentActivity = "currentActivity";

        // For Android Permissions
        public const string androidPermissionReadSettings = "horizonos.permission.READ_SETTINGS";

        // For Font Scaling
        public const int focusBorderWidth = 2;

        // Misc
        public const int undefinedNodeId = -1;
        public const float scrollPageRatio = 0.5f; // scroll only half page
    }

    // Scroll directions defined here should be the same as Android
    // https://developer.android.com/reference/android/view/accessibility/AccessibilityNodeInfo#ACTION_ARGUMENT_DIRECTION_INT
    // https://android.googlesource.com/platform/frameworks/base.git/+/refs/heads/main/core/java/android/view/View.java
    public enum AccessibilityScrollDirection
    {
        UP = 33,
        DOWN = 130,
        LEFT = 17,
        RIGHT = 66,
        BACKWARD = 1,
        FORWARD = 2
    }

    public enum OperationType
    {
        CREATE,
        INSERT,
        REMOVE,
    }
}
