// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

using static QuestAccessibility.AccessibilityConstants;

namespace QuestAccessibility
{
    public class AccessibilityNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
    {
        private int nodeId = undefinedNodeId;
        private int parentNodeId = undefinedNodeId;
        public AccessibilityData AccessibilityData;
        private Canvas canvas;
        private RectTransform rectTransform;
        private Vector2 previousNormalizedPositionForScroll;
        private string prevText;
        private TMP_Text tmpText;
        public ScrollRect ScrollView { get; private set; }
        private float VerticalPageSize
        {
            get
            {
                return ScrollView == null ? 0f : Math.Min(1.0f, ScrollView.viewport.rect.height / ScrollView.content.rect.height);
            }
        }
        private float HorizontalPageSize
        {
            get
            {
                return ScrollView == null ? 0f : Math.Min(1.0f, ScrollView.viewport.rect.width / ScrollView.content.rect.width);
            }
        }

        public bool IsAccessibilityFocused = false;
        public bool IsScrollable { get; private set; } = false;
        public bool IsClickable { get; private set; } = false;
        public bool IsCheckable { get; private set; } = false;
        public bool IsChecked { get; private set; } = false;
        public bool IsFocusable { get; private set; } = false;
        public bool IsEditable { get; private set; } = false;
        public bool IsSlider { get; private set; } = false;

        private void CheckForTmpText()
        {
            tmpText = GetComponent<TMP_Text>();
            if (tmpText != null)
            {
                TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChange);
            }
        }

        private void OnAccessibilityDataChanged()
        {
            AccessibilityManager.UpdateNodeInfo(nodeId);
        }

        private void OnChecked(bool isOn)
        {
            IsChecked = isOn;
            AccessibilityManager.UpdateNodeInfo(nodeId);
        }

        private void OnTextChange(object obj)
        {
            if (!obj.Equals(tmpText))
            {
                return;
            }
            AccessibilityManager.UpdateNodeInfo(nodeId);
        }

        private void OnRectChange()
        {
            AccessibilityManager.UpdateNodeInfo(nodeId);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            AccessibilityManager.OnHoverEnter(nodeId);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            AccessibilityManager.OnHoverExit(nodeId);
        }

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            AccessibilityManager.OnViewSelected(nodeId);
        }

        public void OnScroll(Vector2 pos)
        {
            ScrollRect scrollView = GetComponent<ScrollRect>();
            var offsetX = 0f;
            var offsetY = 0f;
            if (previousNormalizedPositionForScroll.x == pos.x && previousNormalizedPositionForScroll.y == pos.y)
            {
                return;
            }
            else if (previousNormalizedPositionForScroll.x != pos.x)
            {
                offsetX = pos.x * (scrollView.content.rect.width - scrollView.viewport.rect.width);
            }
            else if (previousNormalizedPositionForScroll.y != pos.y)
            {
                offsetY = pos.y * (scrollView.content.rect.height - scrollView.viewport.rect.height);
            }
            var contentWidth = 1.0f * scrollView.content.rect.width;
            var contentHeight = 1.0f * scrollView.content.rect.height;
            AccessibilityManager.OnViewScrolled(nodeId, (int)offsetX, (int)offsetY, (int)contentWidth, (int)contentHeight);
            previousNormalizedPositionForScroll = pos;
        }

        private void OnGUI()
        {
            if (IsAccessibilityFocused)
            {
                var rect = RectTransformToScreenSpace(rectTransform);
                GUI.Box(rect, string.Empty, AccessibilityManager.focusHighlightStyle);
            }
        }

        private Rect RectTransformToScreenSpace(RectTransform rt)
        {
            if (rt == null)
            {
                return new Rect(0, 0, 0, 0);
            }
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = Camera.main.WorldToScreenPoint(corners[i]);
            }

            var position = (Vector2)corners[1];
            position.y = Screen.height - position.y;
            var size = corners[2] - corners[0];

            return new Rect(position, size);
        }

        public string GetText()
        {
            if (GetComponent<TMP_Text>() != null)
            {
                return GetComponent<TMP_Text>().text;
            }
            if (GetComponent<Text>() != null)
            {
                return GetComponent<Text>().text;
            }
            if (GetComponent<TMP_InputField>() != null)
            {
                return GetComponent<TMP_InputField>().text;
            }
            return "";
        }

        public int[] GetRectInAndroid()
        {
            var rect = RectTransformToScreenSpace(rectTransform);
            return new int[] { (int)rect.x, (int)rect.y, (int)(rect.x + rect.width), (int)(rect.y + rect.height) };
        }

        public void Click()
        {
            PointerEventData eventData = new(EventSystem.current)
            {
                position = gameObject.transform.position
            };
            ExecuteEvents.Execute(gameObject, eventData, ExecuteEvents.pointerClickHandler);
        }

        public void Select()
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void Focus()
        {
            if (GetComponent<TMP_InputField>() != null)
            {
                var tmpInputField = GetComponent<TMP_InputField>();
                tmpInputField.Select();
                tmpInputField.onFocusSelectAll = true;
            }
            if (GetComponent<InputField>() != null)
            {
                var inputField = GetComponent<InputField>();
                inputField.ActivateInputField();
            }
        }

        public void ScrollPageUp()
        {
            Scroll(new Vector2(0, VerticalPageSize * scrollPageRatio));
        }

        public void ScrollPageDown()
        {
            Scroll(new Vector2(0, -VerticalPageSize * scrollPageRatio));
        }

        public void ScrollPageLeft()
        {
            Scroll(new Vector2(-HorizontalPageSize * scrollPageRatio, 0));
        }

        public void ScrollPageRight()
        {
            Scroll(new Vector2(HorizontalPageSize * scrollPageRatio, 0));
        }

        private void Scroll(Vector2 vector)
        {
            if (ScrollView == null)
            {
                Debug.Log($"No scrollable component in node Id {nodeId}");
                return;
            }
            ScrollView.normalizedPosition += vector;
        }

        public void SetProgress(float progress)
        {
            if (GetComponent<Slider>() == null)
            {
                Debug.Log($"No Slide component in node Id {nodeId} to set progress");
                return;
            }
            GetComponent<Slider>().value = progress;
        }

        public void SetText(string text)
        {
            if (GetComponent<TMP_Text>() != null)
            {
                GetComponent<TMP_Text>().text = text;
            }
            else if (GetComponent<Text>() != null)
            {
                GetComponent<Text>().text = text;
            }
            else if (GetComponent<TMP_InputField>() != null)
            {
                GetComponent<TMP_InputField>().text = text;
            }
            else
            {
                Debug.Log($"Cannot set text for component: Id {nodeId}");
            }
        }

        private void OnEnable()
        {
            RegisterNode();
            CheckForNodeText();
            CheckForNewNode();
            RegisterAccessibilityData();
            RegisterScrollView();
            RegisterInputField();
            RegisterSlider();
            RegisterClickableComponents();
            RegisterCheckableComponents();
            CheckForTmpText();
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = transform.root.GetComponent<Canvas>();
            AccessibilityData = GetComponent<AccessibilityData>();
        }

        private void Start()
        {
            if (nodeId == undefinedNodeId)
            {
                RegisterNode();
            }
        }

        private void Update()
        {
            if (rectTransform.hasChanged)
            {
                OnRectChange();
                rectTransform.hasChanged = false;
            }
        }

        private void RegisterAccessibilityData()
        {
            if (AccessibilityData != null)
            {
                AccessibilityData.OnValueChanged += OnAccessibilityDataChanged;
            }
        }

        private void RegisterNode()
        {
            if (AccessibilityManager.IsInitialized)
            {
                nodeId = AccessibilityManager.AddAccessibilityNode(this);
            }
        }

        private void CheckForNodeText()
        {
            if (NodeHasText() && GetComponent<FontScaleComponent>() == null)
            {
                gameObject.AddComponent<FontScaleComponent>();
            }
        }

        private void RegisterScrollView()
        {
            ScrollView = GetComponent<ScrollRect>();
            if (ScrollView != null)
            {
                previousNormalizedPositionForScroll = ScrollView.normalizedPosition;
                ScrollView.onValueChanged.AddListener(OnScroll);
                IsScrollable = true;
            }
        }

        private void RegisterInputField()
        {
            if (GetComponent<TMP_InputField>() != null)
            {
                var inputField = GetComponent<TMP_InputField>();
                IsFocusable = true;
                IsEditable = true;
                prevText = inputField.text;
                inputField.onValueChanged.AddListener((newText) =>
                {
                    AccessibilityManager.OnTextChanged(nodeId, inputField.text, prevText);
                    prevText = inputField.text;
                });
                return;
            }
            if (GetComponent<InputField>() != null)
            {
                var inputField = GetComponent<InputField>();
                IsFocusable = true;
                IsEditable = true;
                prevText = inputField.text;
                inputField.onValueChanged.AddListener((newText) =>
                {
                    AccessibilityManager.OnTextChanged(nodeId, inputField.text, prevText);
                    prevText = inputField.text;
                });
                return;
            }
        }

        private void RegisterSlider()
        {
            if (GetComponent<Slider>() != null)
            {
                IsSlider = true;
            }
        }

        private void RegisterClickableComponents()
        {
            if (GetComponent<Button>() != null ||
                GetComponent<Toggle>() != null ||
                GetComponent<Dropdown>() != null ||
                GetComponent<InputField>() != null ||
                GetComponent<TMP_InputField>() != null)
            {
                IsClickable = true;
            }
        }

        private void RegisterCheckableComponents()
        {
            var toggle = GetComponent<Toggle>();
            if (toggle != null)
            {
                IsCheckable = true;
                IsChecked = toggle.isOn;
                toggle.onValueChanged.AddListener(v => OnChecked(v));
            }
        }

        private bool NodeHasText()
        {
            return GetComponent<TMP_Text>() != null
                || GetComponent<Text>() != null
                || GetComponent<TMP_InputField>() != null;
        }

        private void OnTransformChildrenChanged()
        {
            CheckForNewNode();
        }

        private void OnDisable()
        {
            AccessibilityManager.RemoveAccessibilityNode(nodeId, parentNodeId);
        }

        public void CheckForNewNode()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.GetComponent<AccessibilityNode>() == null)
                {
                    var childNode = child.gameObject.AddComponent<AccessibilityNode>();
                    childNode.parentNodeId = nodeId;
                    AccessibilityManager.AddInsertNodeCommands(childNode.nodeId, nodeId);
                }
            }
        }
    }
}
