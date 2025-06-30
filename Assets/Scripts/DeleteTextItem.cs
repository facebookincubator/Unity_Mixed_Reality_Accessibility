// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEngine.UI;

public class DeleteTextItemOnButtonClick : MonoBehaviour
{
    public GameObject textItem;

    void Start()
    {
        if (textItem == null)
        {
            Debug.LogError("Text To Delete GameObject is not assigned in the Inspector!");
            enabled = false;
            return;
        }

        var button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("Button is not detected in the Inspector!");
            enabled = false;
            return;
        }

        button.onClick.AddListener(DeleteTextItem);
    }

    void DeleteTextItem()
    {
        if (textItem != null)
        {
            Destroy(textItem);
        }
        else
        {
            Debug.Log("Text To Delete GameObject is already null or destroyed.");
        }
    }
}
