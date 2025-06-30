// Copyright (c) Meta Platforms, Inc. and affiliates.

package com.meta.unity.accessibility

import android.app.Activity
import android.content.Context
import android.util.Log
import android.view.View
import android.view.View.AccessibilityDelegate
import android.view.accessibility.AccessibilityEvent

class AccessibilityManager(
    private val context: Context,
    private val performActionListener: AccessibilityNodeProvider.PerformActionListener
) {

  private var accessibilityNodeProvider: AccessibilityNodeProvider
  private var accessibilityManager: android.view.accessibility.AccessibilityManager
  private val rootView: View

  init {
    val activity = context as Activity
    accessibilityManager =
        activity.applicationContext.getSystemService(Context.ACCESSIBILITY_SERVICE)
            as android.view.accessibility.AccessibilityManager
    rootView = activity.findViewById(android.R.id.content)
    accessibilityNodeProvider =
        AccessibilityNodeProvider(activity.applicationContext, rootView, performActionListener)
    rootView.setAccessibilityDelegate(
        object : AccessibilityDelegate() {
          override fun getAccessibilityNodeProvider(host: View): AccessibilityNodeProvider {
            return accessibilityNodeProvider
          }
        })
    isInsertOnInit = true
  }

  fun sendAccessibilityEvent(viewId: Int, eventType: Int) {
    if (!accessibilityManager.isEnabled) {
      return
    }
    val event = getBaseAccessibilityEvent(eventType, viewId)
    val nodeInfo = accessibilityNodeProvider.getAccessibilityNode(viewId)
    nodeInfo?.let { info ->
      event.text.add(info.text)
      event.contentDescription = info.contentDescription
    }
    accessibilityManager.sendAccessibilityEvent(event)
  }

  fun sendAccessibilityEventForTextChange(
      viewId: Int,
      eventType: Int,
      text: String,
      beforeText: String
  ) {
    if (!accessibilityManager.isEnabled) {
      return
    }
    val event = getBaseAccessibilityEvent(eventType, viewId)
    val nodeInfo = accessibilityNodeProvider.getAccessibilityNode(viewId)
    event.text.add(text)
    event.setBeforeText(beforeText)
    accessibilityManager.sendAccessibilityEvent(event)
  }

  fun sendWindowContentChangedEvent(contentChangeTypes: Int, data: AccessibilityMetadata? = null) {
    if (!accessibilityManager.isEnabled) {
      return
    }
    val event =
        if (data != null)
            getBaseAccessibilityEvent(AccessibilityEvent.TYPE_WINDOW_CONTENT_CHANGED, data.viewId)
        else getBaseAccessibilityEvent(AccessibilityEvent.TYPE_WINDOW_CONTENT_CHANGED, null)

    event.setContentChangeTypes(contentChangeTypes)
    accessibilityManager.sendAccessibilityEvent(event)
  }

  private fun getBaseAccessibilityEvent(eventType: Int, viewId: Int?): AccessibilityEvent {
    val event = AccessibilityEvent()
    event.className = rootView.javaClass.name
    event.packageName = context.packageName
    event.eventType = eventType
    if (viewId != null) {
      event.setSource(rootView, viewId)
    } else {
      event.setSource(rootView)
    }
    return event
  }

  companion object {
    @JvmStatic private var isInsertOnInit: Boolean = false
    @JvmField val TAG = AccessibilityManager::class.java.simpleName
    @Volatile private var instance: AccessibilityManager? = null

    @JvmStatic
    fun initialize(
        context: Context,
        performActionCallback: AccessibilityNodeProvider.PerformActionListener
    ) {
      if (instance != null) {
        throw IllegalStateException("Android AccessibilityManager is already initialized")
      }
      synchronized(this) {
        if (instance == null) {
          instance = AccessibilityManager(context, performActionCallback)
          Log.d(TAG, "Android AccessibilityManager is initialized successfully")
        }
      }
    }

    @JvmStatic
    fun getInstance(): AccessibilityManager {
      return instance ?: throw IllegalStateException("AccessibilityManager is not initialized")
    }

    @JvmStatic
    fun sendAndroidAccessibilityEvent(viewId: Int, eventType: Int) {
      getInstance().sendAccessibilityEvent(viewId, eventType)
    }

    @JvmStatic
    fun sendAndroidAccessibilityEventForTextChange(
        viewId: Int,
        eventType: Int,
        text: String,
        beforeText: String
    ) {
      getInstance().sendAccessibilityEventForTextChange(viewId, eventType, text, beforeText)
    }

    @JvmStatic
    fun updateAccessibilityNodeInfo(data: AccessibilityMetadata) {
      var contentChangeBitMask = AccessibilityEvent.CONTENT_CHANGE_TYPE_SUBTREE
      var node = getInstance().accessibilityNodeProvider.getAccessibilityNode(data.viewId)
      if (node == null) {
        return
      }
      if (node?.getText() != data.nodeText) {
        contentChangeBitMask = contentChangeBitMask or AccessibilityEvent.CONTENT_CHANGE_TYPE_TEXT
      }
      if (node?.getContentDescription() != data.accessibilityLabel) {
        contentChangeBitMask =
            contentChangeBitMask or AccessibilityEvent.CONTENT_CHANGE_TYPE_CONTENT_DESCRIPTION
      }
      getInstance().accessibilityNodeProvider.updateAccessibilityNodeForUnityView(data)
      getInstance().sendWindowContentChangedEvent(contentChangeBitMask, data)
    }

    @JvmStatic
    fun createBatchedAccessibilityNodeInfos(dataArray: Array<AccessibilityMetadata>) {
      for (data in dataArray) {
        getInstance().accessibilityNodeProvider.createAccessibilityNodeForUnityView(data)
      }
      getInstance().sendWindowContentChangedEvent(AccessibilityEvent.CONTENT_CHANGE_TYPE_SUBTREE)
    }

    @JvmStatic
    fun insertBatchedAccessibilityNodeInfos(data: IntArray) {
      for (i in data.indices step 2) {
        getInstance()
            .accessibilityNodeProvider
            .insertAccessibilityNodeForUnityView(data[i], data[i + 1])
      }
      if (isInsertOnInit) {
        isInsertOnInit = false
      } else {
        getInstance().sendWindowContentChangedEvent(AccessibilityEvent.CONTENT_CHANGE_TYPE_SUBTREE)
      }
    }

    @JvmStatic
    fun removeBatchedAccessibilityNodeInfos(data: IntArray) {
      for (i in data.indices step 2) {
        getInstance()
            .accessibilityNodeProvider
            .removeAccessibilityNodeForUnityView(data[i], data[i + 1])
      }
      getInstance().sendWindowContentChangedEvent(AccessibilityEvent.CONTENT_CHANGE_TYPE_SUBTREE)
    }
  }
}
