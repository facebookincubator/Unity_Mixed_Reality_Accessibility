// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using static QuestAccessibility.AccessibilityConstants;

namespace QuestAccessibility
{
    public class AccessibilityManager : MonoBehaviour
    {
        public static GUIStyle focusHighlightStyle { get; private set; } = new GUIStyle();
        private static AccessibilityManager instance;
        public static bool IsInitialized { get; private set; } = false;

        [SerializeField]
        private FontScaleManager fontScaleManager = new FontScaleManager();
        private Dictionary<int, AccessibilityNode> accessibilityNodes = new Dictionary<int, AccessibilityNode>();
        private AndroidJavaClass accessibilityManagerAndroidClass;

        private int idCounter = 0;
        private int focusedNodeId = undefinedNodeId;
        private static readonly Dictionary<OperationType, List<int>> batchedNodeOperations = new() {
            {OperationType.CREATE, new List<int>()},
            {OperationType.INSERT, new List<int>()},
            {OperationType.REMOVE, new List<int>()}
        };


        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("More than one instance of AccessibilityManager in the scene.");
                Destroy(this);
                return;
            }
            instance = this;
            IsInitialized = true;
            CreateHighlightStyle();
            fontScaleManager.Init();
            InitializeAndroidAccessibilityManager();
        }

        private void Start()
        {
            InvokeRepeating(nameof(ExecuteBatchedNodeOperations), 0.0f, 0.16f);
        }

        private void CreateHighlightStyle()
        {
            var borderColor = new Color(0.278f, 0.647f, 0.98f);
            var texture = new Texture2D(3, 3);
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (x == 1 && y == 1)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }
                    texture.SetPixel(x, y, borderColor);
                }
            }
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            focusHighlightStyle.normal.background = texture;
            focusHighlightStyle.border = new RectOffset(
                focusBorderWidth,
                focusBorderWidth,
                focusBorderWidth,
                focusBorderWidth
            );
        }

        public static void OnHoverEnter(int nodeId)
        {
            SendAndroidAccessibilityEvent(nodeId, AccessibilityEventType.TYPE_VIEW_HOVER_ENTER);
        }

        public static void OnHoverExit(int nodeId)
        {
            SendAndroidAccessibilityEvent(nodeId, AccessibilityEventType.TYPE_VIEW_HOVER_EXIT);
        }

        public static void OnViewSelected(int nodeId)
        {
            SendAndroidAccessibilityEvent(nodeId, AccessibilityEventType.TYPE_VIEW_SELECTED);
        }

        public static void OnViewScrolled(int nodeId, int _offsetX, int _offsetY, int _contentWidth, int _contentHeight)
        {
            SendAndroidAccessibilityEvent(nodeId, AccessibilityEventType.TYPE_VIEW_SCROLLED);
        }

        public static void OnTextChanged(int nodeId, string text, string beforeText)
        {
            SendAndroidAccessibilityEventForTextChange(nodeId, AccessibilityEventType.TYPE_VIEW_TEXT_CHANGED, text, beforeText);
        }

        public static void OnAccessibilityFocused(int nodeId)
        {
            if (instance.focusedNodeId != undefinedNodeId)
            {
                instance.accessibilityNodes[instance.focusedNodeId].IsAccessibilityFocused = false;
            }
            instance.focusedNodeId = nodeId;
            instance.accessibilityNodes[nodeId].IsAccessibilityFocused = true;
            SendAndroidAccessibilityEvent(nodeId, AccessibilityEventType.TYPE_VIEW_ACCESSIBILITY_FOCUSED);
        }

        public static void OnAccessibilityFocusCleared(int nodeId)
        {
            instance.accessibilityNodes[nodeId].IsAccessibilityFocused = false;
            instance.focusedNodeId = undefinedNodeId;
            SendAndroidAccessibilityEvent(nodeId, AccessibilityEventType.TYPE_VIEW_ACCESSIBILITY_FOCUS_CLEARED);
        }

        public static int AddAccessibilityNode(AccessibilityNode node)
        {
            int nodeId = instance.idCounter;
            instance.idCounter++;
            instance.accessibilityNodes.Add(nodeId, node);
            batchedNodeOperations[OperationType.CREATE].Add(nodeId);
            return nodeId;
        }

        public static void RemoveAccessibilityNode(int nodeId, int parentNodeId)
        {
            if (batchedNodeOperations[OperationType.CREATE].Contains(nodeId))
            {
                batchedNodeOperations[OperationType.CREATE].Remove(nodeId);
            }
            instance.accessibilityNodes.Remove(nodeId);
            batchedNodeOperations[OperationType.REMOVE].Add(nodeId);
            batchedNodeOperations[OperationType.REMOVE].Add(parentNodeId);
        }

        public static float GetFontScale(float fontSize)
        {
            return instance.fontScaleManager.GetScaledFontSize(fontSize);
        }

        private void InitializeAndroidAccessibilityManager()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                accessibilityManagerAndroidClass = new AndroidJavaClass(androidClassAccessibilityManager);
                var unityPlayerObject = new AndroidJavaObject(androidClassUnityPlayer);
                var context = unityPlayerObject.GetStatic<AndroidJavaObject>(androidFieldCurrentActivity);
                accessibilityManagerAndroidClass.CallStatic(androidMethodInitialize, context, new PerformActionCallback());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AccessibilityManager: Failed to initailize Android AccessibilityManager: {e.Message}");
            }
#endif
        }

        private void ExecuteBatchedNodeOperations()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (batchedNodeOperations[OperationType.CREATE].Count > 0)
            {
                CreateBatchedAndroidAccessibilityNodeInfo(batchedNodeOperations[OperationType.CREATE]);
            }
            if (batchedNodeOperations[OperationType.INSERT].Count > 0)
            {
                InsertBatchedAndroidAccessibilityNodeInfo(batchedNodeOperations[OperationType.INSERT]);
            }
            if (batchedNodeOperations[OperationType.REMOVE].Count > 0)
            {
                RemoveBatchedAndroidAccessibilityNodeInfo(batchedNodeOperations[OperationType.REMOVE]);
            }
#endif
        }

        public static void UpdateNodeInfo(int nodeId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        var node = instance.accessibilityNodes[nodeId];
        var metadata = new AndroidJavaObject(androidClassAccessibilityMetadata, nodeId, node.GetText(), node.AccessibilityData?.AccessibilityLabel ?? "", node.GetRectInAndroid(), node.IsCheckable, node.IsChecked, node.IsClickable, node.IsFocusable, node.IsScrollable, node.IsEditable, node.IsSlider);
        instance.accessibilityManagerAndroidClass.CallStatic(androidMethodUpdateAccessibilityNodeInfo, metadata);
#endif
        }

        private static void CreateBatchedAndroidAccessibilityNodeInfo(List<int> createNodeCommands)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                var metadataArray = new AndroidJavaObject[createNodeCommands.Count];
                for (int i = 0; i < createNodeCommands.Count; i++)
                {
                    var nodeId = createNodeCommands[i];
                    var node = instance.accessibilityNodes[nodeId];
                    metadataArray[i] = new AndroidJavaObject(androidClassAccessibilityMetadata, nodeId, node.GetText(), node.AccessibilityData?.AccessibilityLabel ?? "", node.GetRectInAndroid(), node.IsCheckable, node.IsChecked, node.IsClickable, node.IsFocusable, node.IsScrollable, node.IsEditable, node.IsSlider);
                }
                createNodeCommands.Clear();
                instance.accessibilityManagerAndroidClass.CallStatic(androidMethodCreateBatchedAccessibilityNodeInfos, metadataArray);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AccessibilityManager: Failed to create AccessibilityNodeInfo in Android: {e.Message}");
            }
#endif
        }

        private void InsertBatchedAndroidAccessibilityNodeInfo(List<int> insertNodeCommands)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                var data = insertNodeCommands.ToArray();
                insertNodeCommands.Clear();
                instance.accessibilityManagerAndroidClass.CallStatic(androidMethodInsertBatchedAccessibilityNodeInfos, data);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AccessibilityManager: Failed to insert AccessibilityNodeInfo in Android: {e.Message}");
            }
#endif
        }

        private static void RemoveBatchedAndroidAccessibilityNodeInfo(List<int> removeNodeCommands)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                var data = removeNodeCommands.ToArray();
                removeNodeCommands.Clear();
                instance.accessibilityManagerAndroidClass.CallStatic(androidMethodRemoveBatchedAccessibilityNodeInfos, data);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AccessibilityManager: Failed to remove AccessibilityNodeInfo in Android: {e.Message}");
            }
#endif
        }

        private static void SendAndroidAccessibilityEventForTextChange(int nodeId, AccessibilityEventType eventType, string text, string beforeText)
        {
            // Ignore canvas from hover events
            if (nodeId == 0)
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            try {
                instance.accessibilityManagerAndroidClass.CallStatic(androidMethodSendAndroidAccessibilityEvent + "ForTextChange", nodeId, (int)eventType, text, beforeText);
            } catch (System.Exception e) {
                Debug.LogError($"AccessibilityManager: Failed to send accessibility event {eventType} for nodeId: {nodeId}. Error: {e.Message}");
            }
            return;
#endif
        }

        private static void SendAndroidAccessibilityEvent(int nodeId, AccessibilityEventType eventType)
        {
            // Ignore canvas from hover events
            if (nodeId == 0)
            {
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            try {
                instance.accessibilityManagerAndroidClass.CallStatic(androidMethodSendAndroidAccessibilityEvent, nodeId, (int)eventType);
            } catch (System.Exception e) {
                Debug.LogError($"AccessibilityManager: Failed to send accessibility event {eventType} for nodeId: {nodeId}. Error: {e.Message}");
            }
            return;
#endif
        }

        public static void AddInsertNodeCommands(int nodeId, int parentNodeId)
        {
            batchedNodeOperations[OperationType.INSERT].Add(nodeId);
            batchedNodeOperations[OperationType.INSERT].Add(parentNodeId);
        }

        class PerformActionCallback : AndroidJavaProxy
        {
            public PerformActionCallback() : base(androidClassPerformActionListener) { }

            void performActionAccessibilityFocus(int nodeId)
            {
                if (!instance.accessibilityNodes.ContainsKey(nodeId))
                {
                    Debug.LogError($"AccessibilityManager: performActionAccessibilityFocus. nodeId = {nodeId} not found.");
                    return;
                }
                OnAccessibilityFocused(nodeId);
            }

            void performActionClearAccessibilityFocus(int nodeId)
            {
                if (!instance.accessibilityNodes.ContainsKey(nodeId))
                {
                    Debug.LogError($"AccessibilityManager: performActionClearAccessibilityFocus. nodeId = {nodeId} not found.");
                    return;
                }
                OnAccessibilityFocusCleared(nodeId);
            }

            void performActionClick(int nodeId)
            {
                if (!instance.accessibilityNodes.ContainsKey(nodeId))
                {
                    Debug.LogError($"AccessibilityManager: performActionClick. nodeId = {nodeId} not found.");
                    return;
                }
                instance.accessibilityNodes[nodeId].Click();
            }

            void performActionSelect(int nodeId)
            {
                if (!instance.accessibilityNodes.ContainsKey(nodeId))
                {
                    Debug.LogError($"AccessibilityManager: performActionSelect. nodeId = {nodeId} not found.");
                    return;
                }
                instance.accessibilityNodes[nodeId].Select();
            }

            void performActionFocus(int nodeId)
            {
                if (!instance.accessibilityNodes.ContainsKey(nodeId))
                {
                    Debug.LogError($"AccessibilityManager: performActionFocus. nodeId = {nodeId} not found.");
                    return;
                }
                instance.accessibilityNodes[nodeId].Focus();
            }

            void performActionScroll(int nodeId, int direction)
            {
                if (!instance.accessibilityNodes.ContainsKey(nodeId))
                {
                    Debug.LogError($"AccessibilityManager: performActionScroll. nodeId = {nodeId} not found.");
                    return;
                }
                var node = instance.accessibilityNodes[nodeId];
                if (node.ScrollView == null)
                {
                    Debug.Log($"No scrollable component in node Id {nodeId}");
                    return;
                }

                switch ((AccessibilityScrollDirection)direction)
                {
                    case AccessibilityScrollDirection.UP:
                        node.ScrollPageUp();
                        break;
                    case AccessibilityScrollDirection.DOWN:
                        node.ScrollPageDown();
                        break;
                    case AccessibilityScrollDirection.LEFT:
                        node.ScrollPageLeft();
                        break;
                    case AccessibilityScrollDirection.RIGHT:
                        node.ScrollPageRight();
                        break;
                    case AccessibilityScrollDirection.BACKWARD:
                        if (node.ScrollView.vertical)
                        {
                            node.ScrollPageUp();
                            break;
                        }
                        node.ScrollPageLeft();
                        break;
                    case AccessibilityScrollDirection.FORWARD:
                        if (node.ScrollView.vertical)
                        {
                            node.ScrollPageDown();
                            break;
                        }
                        node.ScrollPageRight();
                        break;
                    default:
                        break;
                }
            }

            void performActionSetProgress(int nodeId, float progress)
            {
                instance.accessibilityNodes[nodeId].SetProgress(progress);
            }

            void performActionSetText(int nodeId, string text)
            {
                instance.accessibilityNodes[nodeId].SetText(text);
            }
        }
    }
}
