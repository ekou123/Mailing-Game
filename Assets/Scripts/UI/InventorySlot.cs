using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;

    public ItemData Item { get; private set; }

    void Awake()
    {
        if (itemIcon == null)
        {
            // Prefer a child Image (item icon), fall back to root Image (slot background)
            foreach (Image img in GetComponentsInChildren<Image>())
            {
                if (img.gameObject != gameObject) { itemIcon = img; break; }
            }
            if (itemIcon == null)
                itemIcon = GetComponent<Image>();
        }

        if (itemIcon == null)
            Debug.LogWarning($"[InventorySlot] No Image found on {gameObject.name} or its children.");
        else
            itemIcon.enabled = false;
    }

    public void SetItem(ItemData item)
    {
        Debug.Log("Setting item...");
        if (item == null) { ClearSlot(); return; }

        Item = item;

        if (itemIcon == null)
        {
            Debug.LogWarning($"{name} has no icon Image assigned.");
            return;
        }
        itemIcon.sprite = item.icon;
        itemIcon.enabled = true;

        Debug.Log("Set Item.");

        if (quantityText != null) quantityText.text = "";
    }

    public void ClearSlot()
    {
        Item = null;
        itemIcon.sprite = null;
        itemIcon.enabled = false;

        if (quantityText != null) quantityText.text = "";
    }

    public void SetIconAlpha(float alpha)
    {
        if (itemIcon == null) return;

        Color c = itemIcon.color;
        c.a = alpha;
        itemIcon.color = c;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Item == null) return;
        InventoryUI.Instance.BeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        InventoryUI.Instance.UpdateDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        InventoryUI.Instance.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventoryUI.Instance.Drop(this);
    }
}
