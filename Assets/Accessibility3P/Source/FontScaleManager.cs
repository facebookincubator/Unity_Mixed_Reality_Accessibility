// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace QuestAccessibility
{

    [System.Serializable]
    public class FontScaleManager
    {
        public float fontScale { get; private set; }

        [SerializeField]
        private FontScaleType fontScaleType;
        private List<TMP_Text> staticText = new List<TMP_Text>();
        private List<Text> staticTextLegacy = new List<Text>();

        public void Init()
        {
            this.fontScale = getFontScaleFromAndroid();
        }

        public float GetScaledFontSize(float fontSize)
        {
            switch (this.fontScaleType)
            {
                case FontScaleType.LINEAR:
                    return fontSize * this.fontScale;

                case FontScaleType.NONLINEAR:
                    //TODO: Implement once non-linear scaling is implemented
                    Debug.LogError("Non-linear scaling not implemented yet");
                    break;
            }
            return fontSize;
        }

        private float getFontScaleFromAndroid()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        try {
            var unityPlayerObject = new AndroidJavaObject("com.unity3d.player.UnityPlayer");
            var context = unityPlayerObject.GetStatic<AndroidJavaObject>("currentActivity");
            var fontScaleAndriodClass = new AndroidJavaClass("com.meta.unity.accessibility.FontScale");
            return fontScaleAndriodClass.CallStatic<float>("getLinearScaleValue", context);
        } catch (System.Exception e) {
            Debug.LogError($"FontScaleManager: Failed to get font scale from Android: {e.Message}");
            return 1.0f;
        }
#endif
            Debug.LogWarning("FontScaleManager: not running on Android platform, returning fontscale of 1.0");
            return 1.0f;
        }
    }

    public enum FontScaleType
    {
        LINEAR,
        NONLINEAR
    }
}
