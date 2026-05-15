using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class ItemDetailsUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private void Awake()
    {
        // Auto-find UI elements if not assigned
        if (icon == null)
        {
            icon = GetComponentInChildren<Image>(true);
            if (icon != null)
                Debug.Log("[ItemDetailsUI] Icon Image auto-found.");
        }

        if (itemNameText == null)
        {
            TextMeshProUGUI[] textElements = GetComponentsInChildren<TextMeshProUGUI>(true);
            if (textElements.Length > 0)
            {
                itemNameText = textElements[0];
                Debug.Log("[ItemDetailsUI] Item name text auto-found.");
            }
        }

        if (descriptionText == null)
        {
            TextMeshProUGUI[] textElements = GetComponentsInChildren<TextMeshProUGUI>(true);
            if (textElements.Length > 1)
            {
                descriptionText = textElements[1];
                Debug.Log("[ItemDetailsUI] Description text auto-found.");
            }
            else if (textElements.Length > 0)
            {
                descriptionText = textElements[0];
                Debug.Log("[ItemDetailsUI] Description text auto-found (using same as item name).");
            }
        }

        // Validate
        if (icon == null)
            Debug.LogWarning("[ItemDetailsUI] Could not find icon Image component!");
        if (itemNameText == null)
            Debug.LogWarning("[ItemDetailsUI] Could not find item name TextMeshProUGUI component!");
        if (descriptionText == null)
            Debug.LogWarning("[ItemDetailsUI] Could not find description TextMeshProUGUI component!");
    }

    public void Show(ItemInstance item)
    {
        Debug.Log("Showing item");
        if (item == null)
        {
            Clear();
            return;
        }

        if (icon != null)
        {
            icon.sprite = item.data.icon;
            icon.enabled = true;
        }

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (itemNameText != null)
            itemNameText.text = item.data.itemName;

        if (descriptionText != null)
        {
            var sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(item.data.description))
                sb.AppendLine(item.data.description);
            foreach (var stat in item.GetDisplayStats())
                sb.AppendLine($"{stat.label}: {stat.value}");
            descriptionText.text = sb.ToString().TrimEnd();
        }

        Debug.Log($"[ItemDetailsUI] Showing item: {item.data.itemName}");
    }

    public void Clear()
    {
        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        if (itemNameText != null)
            itemNameText.text = "";
        if (descriptionText != null)
            descriptionText.text = "";
    }
}
