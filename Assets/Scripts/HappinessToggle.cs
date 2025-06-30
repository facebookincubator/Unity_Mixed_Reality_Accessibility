// Copyright (c) Meta Platforms, Inc. and affiliates.

using QuestAccessibility;
using UnityEngine;
using UnityEngine.UI;

public class HappinessToggle : MonoBehaviour
{
    private AccessibilityData accessibilityData;
    private Toggle toggle;

    void Start()
    {
        accessibilityData = GetComponent<AccessibilityData>();
        accessibilityData.AccessibilityLabel = "Toggle: are you happy?";
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnCheckChanged);
    }

    void OnCheckChanged(bool isOn)
    {
        if (isOn)
        {
            accessibilityData.AccessibilityLabel = "I'm happy";
        }
        else
        {
            accessibilityData.AccessibilityLabel = "I'm not happy";
        }
    }
}
