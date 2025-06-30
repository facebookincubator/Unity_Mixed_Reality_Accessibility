// Copyright (c) Meta Platforms, Inc. and affiliates.

package com.meta.unity.accessibility

import android.content.Context
import android.graphics.Rect
import android.os.Bundle
import android.util.Log
import android.view.View
import android.view.accessibility.AccessibilityNodeInfo
import android.view.accessibility.AccessibilityNodeInfo.AccessibilityAction
import android.view.accessibility.AccessibilityNodeProvider
import kotlin.collections.mutableMapOf

class AccessibilityNodeProvider(
    private val context: Context,
    private val rootView: View,
    private val performActionListener: PerformActionListener
) : android.view.accessibility.AccessibilityNodeProvider() {

  private val rootNode: AccessibilityNodeInfo
  private val accessibilityNodeInfoMap = mutableMapOf<Int, AccessibilityNodeInfo>()
  private val rootChildrenList = mutableListOf<Int>()
  private val scrollActionToDirectionMap =
      mapOf(
          AccessibilityAction.ACTION_SCROLL_FORWARD.getId() to View.FOCUS_FORWARD,
          AccessibilityAction.ACTION_SCROLL_BACKWARD.getId() to View.FOCUS_BACKWARD,
          AccessibilityAction.ACTION_SCROLL_UP.getId() to View.FOCUS_UP,
          AccessibilityAction.ACTION_SCROLL_DOWN.getId() to View.FOCUS_DOWN,
          AccessibilityAction.ACTION_SCROLL_LEFT.getId() to View.FOCUS_LEFT,
          AccessibilityAction.ACTION_SCROLL_RIGHT.getId() to View.FOCUS_RIGHT,
          AccessibilityAction.ACTION_PAGE_UP.getId() to View.FOCUS_UP,
          AccessibilityAction.ACTION_PAGE_DOWN.getId() to View.FOCUS_DOWN,
          AccessibilityAction.ACTION_PAGE_LEFT.getId() to View.FOCUS_LEFT,
          AccessibilityAction.ACTION_PAGE_RIGHT.getId() to View.FOCUS_RIGHT)

  init {
    rootNode = AccessibilityNodeInfo(rootView)
    accessibilityNodeInfoMap.put(HOST_VIEW_ID, rootNode)
  }

  fun getAccessibilityNode(viewId: Int): AccessibilityNodeInfo? {
    return accessibilityNodeInfoMap[viewId]
  }

  /**
   * Retrieves the accessibility node info for a Unity view with the given ID.
   *
   * @param viewId the ID of the Unity view
   * @return the accessibility node info, or null if not found
   */
  override fun createAccessibilityNodeInfo(viewId: Int): AccessibilityNodeInfo? {

    if (!accessibilityNodeInfoMap.containsKey(viewId)) {
      createAccessibilityNodeForUnityView(AccessibilityMetadata(viewId = viewId))
    }

    return accessibilityNodeInfoMap[viewId]
  }

  /**
   * Performs an accessibility action on a Unity view with the given ID.
   *
   * @param viewId the ID of the Unity view
   * @param action the accessibility action to perform (e.g. click, focus, etc.)
   * @param bundle optional arguments for the action
   * @return true if the action was performed successfully, false otherwise
   */
  override fun performAction(viewId: Int, action: Int, bundle: Bundle?): Boolean {
    return when (action) {
      AccessibilityNodeInfo.ACTION_ACCESSIBILITY_FOCUS -> {
        accessibilityNodeInfoMap[viewId]?.isAccessibilityFocused = true
        performActionListener.performActionAccessibilityFocus(viewId)
        true
      }
      AccessibilityNodeInfo.ACTION_CLEAR_ACCESSIBILITY_FOCUS -> {
        accessibilityNodeInfoMap[viewId]?.isAccessibilityFocused = false
        performActionListener.performActionClearAccessibilityFocus(viewId)
        true
      }
      AccessibilityNodeInfo.ACTION_CLICK -> {
        performActionListener.performActionClick(viewId)
        true
      }
      AccessibilityNodeInfo.ACTION_SELECT -> {
        performActionListener.performActionSelect(viewId)
        true
      }
      AccessibilityNodeInfo.ACTION_FOCUS -> {
        performActionListener.performActionFocus(viewId)
        true
      }
      AccessibilityAction.ACTION_SCROLL_IN_DIRECTION.getId() -> {
        if (bundle == null ||
            !bundle.containsKey(AccessibilityNodeInfo.ACTION_ARGUMENT_DIRECTION_INT)) {
          Log.w(TAG, "Invalid paramenters for ACTION_SCROLL_IN_DIRECTION")
          return false
        }
        val direction = bundle.getInt(AccessibilityNodeInfo.ACTION_ARGUMENT_DIRECTION_INT)
        performActionListener.performActionScroll(viewId, direction)
        true
      }
      AccessibilityAction.ACTION_PAGE_DOWN.getId(),
      AccessibilityAction.ACTION_SCROLL_DOWN.getId(),
      AccessibilityAction.ACTION_PAGE_UP.getId(),
      AccessibilityAction.ACTION_SCROLL_UP.getId(),
      AccessibilityAction.ACTION_PAGE_LEFT.getId(),
      AccessibilityAction.ACTION_SCROLL_LEFT.getId(),
      AccessibilityAction.ACTION_PAGE_RIGHT.getId(),
      AccessibilityAction.ACTION_SCROLL_RIGHT.getId(),
      AccessibilityAction.ACTION_SCROLL_BACKWARD.getId(),
      AccessibilityAction.ACTION_SCROLL_FORWARD.getId() -> {
        performActionListener.performActionScroll(
            viewId, scrollActionToDirectionMap[action] ?: return false)
        true
      }
      AccessibilityAction.ACTION_SET_PROGRESS.getId() -> {
        if (bundle == null ||
            !bundle.containsKey(AccessibilityNodeInfo.ACTION_ARGUMENT_PROGRESS_VALUE)) {
          Log.w(TAG, "Invalid paramenters for ACTION_SET_PROGRESS")
          return false
        }

        performActionListener.performActionSetProgress(
            viewId, bundle.getFloat(AccessibilityNodeInfo.ACTION_ARGUMENT_PROGRESS_VALUE))
        true
      }
      AccessibilityAction.ACTION_SET_TEXT.getId() -> {
        if (bundle == null ||
            !bundle.containsKey(AccessibilityNodeInfo.ACTION_ARGUMENT_SET_TEXT_CHARSEQUENCE)) {
          Log.w(TAG, "Invalid paramenters for ACTION_SET_TEXT")
          return false
        }
        performActionListener.performActionSetText(
            viewId,
            bundle.getString(AccessibilityNodeInfo.ACTION_ARGUMENT_SET_TEXT_CHARSEQUENCE, ""))
        true
      }
      else -> false
    }
  }

  fun createAccessibilityNodeForUnityView(data: AccessibilityMetadata) {
    if (accessibilityNodeInfoMap.containsKey(data.viewId)) {
      Log.e(TAG, "Accessibility node already exists for shadow view: ${data.viewId}")
      return
    }
    val node = AccessibilityNodeInfo(rootView, data.viewId)
    node.setSource(rootView, data.viewId)
    node.setPackageName(context.packageName)
    node.setClassName(View::class.java.name)
    node.setText(data.nodeText)
    node.setContentDescription(data.accessibilityLabel)
    node.setBoundsInScreen(
        Rect(
            data.rectInAndroid[0],
            data.rectInAndroid[1],
            data.rectInAndroid[2],
            data.rectInAndroid[3]))
    node.setClickable(data.isClickable)
    node.setCheckable(data.isCheckable)
    node.setChecked(data.isChecked)
    node.setFocusable(data.isFocusable)
    node.setScrollable(data.isScrollable)
    node.setEditable(data.isEditable)
    setSupportedActions(node, data)
    accessibilityNodeInfoMap.put(data.viewId, node)
    rootChildrenList.add(data.viewId)
    rootNode.addChild(rootView, data.viewId)
  }

  fun insertAccessibilityNodeForUnityView(viewId: Int, parentViewId: Int) {
    if (rootChildrenList.contains(viewId)) {
      rootChildrenList.remove(viewId)
      rootNode.removeChild(rootView, viewId)
    }
    accessibilityNodeInfoMap[viewId]?.setParent(rootView, parentViewId)
    accessibilityNodeInfoMap[parentViewId]?.addChild(rootView, viewId)
  }

  fun removeAccessibilityNodeInfo(viewId: Int) {
    accessibilityNodeInfoMap.remove(viewId)
  }

  fun removeAccessibilityNodeForUnityView(viewId: Int, parentViewId: Int) {
    if (!accessibilityNodeInfoMap.containsKey(viewId)) {
      Log.e(TAG, "Accessibility node does not exist for shadow view: $viewId")
      return
    }

    getAccessibilityNode(viewId)?.setParent(rootView, View.NO_ID)
    getAccessibilityNode(parentViewId)?.removeChild(rootView, viewId)
    accessibilityNodeInfoMap.remove(viewId)
  }

  fun updateAccessibilityNodeForUnityView(data: AccessibilityMetadata) {
    if (!accessibilityNodeInfoMap.containsKey(data.viewId)) {
      Log.e(TAG, "Accessibility node does not exist for shadow view: ${data.viewId}")
      return
    }
    accessibilityNodeInfoMap[data.viewId]?.let { nodeInfo ->
      nodeInfo.setText(data.nodeText)
      nodeInfo.setContentDescription(data.accessibilityLabel)
      nodeInfo.setBoundsInScreen(
          Rect(
              data.rectInAndroid[0],
              data.rectInAndroid[1],
              data.rectInAndroid[2],
              data.rectInAndroid[3]))
      nodeInfo.setClickable(data.isClickable)
      nodeInfo.setCheckable(data.isCheckable)
      nodeInfo.setChecked(data.isChecked)
      nodeInfo.setFocusable(data.isFocusable)
      nodeInfo.setScrollable(data.isScrollable)
      nodeInfo.setEditable(data.isEditable)
      setSupportedActions(nodeInfo, data)
    }
  }

  private fun setSupportedActions(node: AccessibilityNodeInfo, data: AccessibilityMetadata) {
    node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_ACCESSIBILITY_FOCUS)
    node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_CLEAR_ACCESSIBILITY_FOCUS)
    node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_SELECT)
    node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_CLEAR_SELECTION)
    if (data.isSlider) {
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_SET_PROGRESS)
    }
    if (node.isClickable()) {
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_CLICK)
    }
    if (node.isFocusable()) {
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_FOCUS)
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_CLEAR_FOCUS)
    }
    if (node.isScrollable()) {
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_SCROLL_IN_DIRECTION)
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_SCROLL_FORWARD)
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_SCROLL_BACKWARD)
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_SCROLL_UP)
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_SCROLL_DOWN)
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_SCROLL_LEFT)
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_SCROLL_RIGHT)
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_PAGE_UP)
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_PAGE_DOWN)
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_PAGE_LEFT)
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_PAGE_RIGHT)
    }
    if (node.isEditable()) {
      node.addAction(AccessibilityNodeInfo.AccessibilityAction.ACTION_SET_TEXT)
    }
  }

  companion object {
    @JvmField val TAG = AccessibilityNodeProvider::class.java.simpleName
  }

  /**
   * The listener interface for performing actions on the accessibility node.
   *
   * Implement this interface to define the behavior on the Unity side when an action is performed
   * on an accessibility node.
   *
   * @param viewId the ID of the Unity view on which the accessibility action is performed
   * @param action the accessibility action to be performed
   * @param bundle optional arguments for the action
   */
  public interface PerformActionListener {
    /**
     * Performs actions on a Unity view with the given ID for ACTION_ACCESSIBILITY_FOCUS.
     *
     * @param viewId the ID of the Unity view to focus
     */
    fun performActionAccessibilityFocus(viewId: Int)

    /**
     * Performs actions on a Unity view with the given ID for ACTION_CLEAR_ACCESSIBILITY_FOCUS.
     *
     * @param viewId the ID of the Unity view to clear focus
     */
    fun performActionClearAccessibilityFocus(viewId: Int)

    /**
     * Performs actions on a Unity view with the given ID for ACTION_CLICK.
     *
     * @param viewId the ID of the Unity view to click
     */
    fun performActionClick(viewId: Int)

    /**
     * Performs actions on a Unity view with the given ID for ACTION_SELECT.
     *
     * @param viewId the ID of the Unity view to select
     */
    fun performActionSelect(viewId: Int)

    /**
     * Performs actions on a Unity view with the given ID for ACTION_FOCUS.
     *
     * @param viewId the ID of the Unity view to focus
     */
    fun performActionFocus(viewId: Int)

    /**
     * Performs actions on a Unity view with the given ID for ACTION_SCROLL_<direction> or
     * ACTION_PAGE_<direction>.
     *
     * @param viewId the ID of the Unity view to scroll
     */
    fun performActionScroll(viewId: Int, direction: Int)

    /**
     * Performs actions on a Unity view with the given ID for ACTION_SET_PROGRESS.
     *
     * @param viewId the ID of the Unity view to set progress.
     */
    fun performActionSetProgress(viewId: Int, progress: Float)

    /**
     * Performs actions on a Unity view with the given ID for ACTION_SET_TEXT.
     *
     * @param viewId the ID of the Unity view to set text
     */
    fun performActionSetText(viewId: Int, text: String)
  }
}
