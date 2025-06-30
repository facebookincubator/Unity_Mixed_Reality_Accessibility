// Copyright (c) Meta Platforms, Inc. and affiliates.

namespace QuestAccessibility
{
    /// <summary>
    /// Types of accessibility events.
    /// Reference: https://android.googlesource.com/platform/frameworks/base.git/+/master/core/java/android/view/accessibility/AccessibilityEvent.java
    /// </summary>
    public enum AccessibilityEventType
    {
        /// <summary>
        /// Event triggered when a Unity view is selected.
        /// </summary>
        TYPE_VIEW_SELECTED = 1 << 2,

        /// <summary>
        /// Event triggered when a Unity view has text changed.
        /// </summary>
        TYPE_VIEW_TEXT_CHANGED = 1 << 4,

        /// <summary>
        /// Event triggered when a user's pointer enters a Unity view.
        /// </summary>
        TYPE_VIEW_HOVER_ENTER = 1 << 7,

        /// <summary>
        /// Event triggered when a user's pointer exits a Unity view.
        /// </summary>
        TYPE_VIEW_HOVER_EXIT = 1 << 8,

        /// <summary>
        /// Event triggered when a Unity view is scrolled.
        /// </summary>
        TYPE_VIEW_SCROLLED = 1 << 12,

        /// <summary>
        /// Event triggered when a Unity view gains accessibility focus.
        /// </summary>
        TYPE_VIEW_ACCESSIBILITY_FOCUSED = 1 << 15,

        /// <summary>
        /// Event triggered when a Unity view loses accessibility focus.
        /// </summary>
        TYPE_VIEW_ACCESSIBILITY_FOCUS_CLEARED = 1 << 16,
    }
}
