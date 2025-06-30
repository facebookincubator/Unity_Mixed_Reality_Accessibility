// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace QuestAccessibility
{
    public class AccessibilityData : MonoBehaviour
    {
        [SerializeField]
        private string accessibilityLabel;
        public delegate void ValueChangedDelegate();
        public event ValueChangedDelegate OnValueChanged;

        public string AccessibilityLabel
        {
            get { return accessibilityLabel; }
            set
            {
                if (accessibilityLabel != value)
                {
                    accessibilityLabel = value;
                    OnValueChanged?.Invoke();
                }
            }
        }
    }
}
