// Copyright (c) Meta Platforms, Inc. and affiliates.

package com.meta.unity.accessibility

import android.content.Context
import android.util.Log

class FontScale() {
  companion object {
    @JvmField val TAG = FontScale::class.java.simpleName

    @JvmStatic
    fun getLinearScaleValue(context: Context): Float {
      val scale = context.getResources().getConfiguration().fontScale
      Log.d(TAG, "Font Scale: $scale")
      return scale
    }
  }
}
