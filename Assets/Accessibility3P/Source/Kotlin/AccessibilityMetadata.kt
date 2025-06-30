// Copyright (c) Meta Platforms, Inc. and affiliates.

package com.meta.unity.accessibility

data class AccessibilityMetadata(
    val viewId: Int,
    val nodeText: String = "",
    val accessibilityLabel: String = "",
    val rectInAndroid: IntArray = intArrayOf(0, 0, 0, 0),
    val isCheckable: Boolean = false,
    val isChecked: Boolean = false,
    val isClickable: Boolean = false,
    val isFocusable: Boolean = false,
    val isScrollable: Boolean = false,
    val isEditable: Boolean = false,
    val isSlider: Boolean = false,
)
