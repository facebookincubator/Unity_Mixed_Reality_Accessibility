// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace QuestAccessibility
{

    public class FontScaleComponent : MonoBehaviour
    {
        private TMP_Text tmpText;
        private Text legacyText;

        private TMP_InputField tmpInputField;

        private FontScaleComponentType fontScaleComponentType;

        void Start()
        {
            FindComponent();
            ApplyFontScale();
        }

        private void ApplyFontScale()
        {
            switch (fontScaleComponentType)
            {
                case FontScaleComponentType.TEXT:
                    ApplyFontScaleText();
                    break;
                case FontScaleComponentType.INPUT:
                    ApplyFontScaleInput();
                    break;
                case FontScaleComponentType.TEXT_LEGACY:
                    ApplyFontScaleTextLegacy();
                    break;
            }
        }

        private void ApplyFontScaleText()
        {
            var scale = (int)AccessibilityManager.GetFontScale(tmpText.fontSize);
            tmpText.enableAutoSizing = true;
            tmpText.fontSizeMin = 1;
            tmpText.fontSizeMax = scale;
        }

        private void ApplyFontScaleInput()
        {
            tmpInputField.pointSize = (int)AccessibilityManager.GetFontScale(tmpInputField.pointSize);
        }

        private void ApplyFontScaleTextLegacy()
        {
            legacyText.fontSize = (int)AccessibilityManager.GetFontScale(legacyText.fontSize);
        }

        private void FindComponent()
        {
            if (GetComponent<TMP_Text>() != null)
            {
                tmpText = GetComponent<TMP_Text>();
                fontScaleComponentType = FontScaleComponentType.TEXT;
                return;
            }

            if (GetComponent<Text>() != null)
            {
                legacyText = GetComponent<Text>();
                fontScaleComponentType = FontScaleComponentType.TEXT_LEGACY;
                return;
            }

            if (GetComponent<TMP_InputField>() != null)
            {
                tmpInputField = GetComponent<TMP_InputField>();
                fontScaleComponentType = FontScaleComponentType.INPUT;
                return;
            }

            Debug.LogError($"No text component found for {gameObject.name}, removing component.");
            Destroy(this);
        }
    }

    public enum FontScaleComponentType
    {
        TEXT, INPUT, TEXT_LEGACY
    }
}
