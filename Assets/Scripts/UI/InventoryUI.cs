using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [SerializeField] private InventorySlot[] slots;
    [SerializeField] private Image dragIcon;

    [SerializeField] private ItemDetailsUI itemDetailsUI;

    private InventorySlot selectedSlot;


    private Inventory inventory;
    private InventorySlot dragSource;
    private int dragSourceIndex = -1;
    private ItemData draggedItem;

    void Awake()
    {
        Instance = this;

        if (slots == null || slots.Length == 0 || System.Array.Exists(slots, s => s == null))
            slots = GetComponentsInChildren<InventorySlot>(true);

        if (dragIcon != null)
            dragIcon.gameObject.SetActive(false);

        // Auto-find ItemDetailsUI if not assigned
        if (itemDetailsUI == null)
        {
            itemDetailsUI = GetComponentInChildren<ItemDetailsUI>(true);
            if (itemDetailsUI == null)
            {
                itemDetailsUI = FindObjectOfType<ItemDetailsUI>(true);
            }

            if (itemDetailsUI == null)
            {
                Debug.LogWarning("[InventoryUI] ItemDetailsUI not found! Please assign it in the Inspector or ensure it exists in the scene.");
            }
            else
            {
                Debug.Log("[InventoryUI] ItemDetailsUI auto-found and assigned.");
            }
        }
    }

    public void SelectItem(InventorySlot slot)
    {
        selectedSlot = slot;

        if (itemDetailsUI == null)
        {
            itemDetailsUI = GetComponentInChildren<ItemDetailsUI>(true);
            if (itemDetailsUI == null)
                itemDetailsUI = FindObjectOfType<ItemDetailsUI>(true);
        }

        if (itemDetailsUI == null)
        {
            Debug.LogWarning("[InventoryUI] itemDetailsUI is still not assigned when calling SelectItem.");
            return;
        }

        if (slot != null)
        {
            Debug.Log($"[InventoryUI] SelectItem called with item: {slot.Item?.itemName ?? "null"}");
            itemDetailsUI.Show(slot.Item);
        }
        else
        {
            Debug.Log("[InventoryUI] SelectItem called with null slot, clearing details");
            itemDetailsUI.Clear();
        }
    }

    public void SetInventory(Inventory newInventory)
    {
        if (newInventory == null) return;

        if (inventory != null)
            inventory.OnInventoryChanged -= Refresh;

        inventory = newInventory;
        inventory.OnInventoryChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= Refresh;
    }

    public void Refresh()
    {
        if (inventory == null || slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            if (i < inventory.slots.Length)
                slots[i].SetItem(inventory.slots[i]);
            else
                slots[i].ClearSlot();
        }
    }

    public void BeginDrag(InventorySlot source, PointerEventData eventData)
    {
        dragSource = source;
        draggedItem = source.Item;
        dragSourceIndex = System.Array.IndexOf(slots, source);

        if (dragIcon != null)
        {
            dragIcon.sprite = draggedItem.icon;
            dragIcon.gameObject.SetActive(true);
            dragIcon.transform.position = eventData.position;
        }

        source.SetIconAlpha(0.4f);
    }

    public void UpdateDrag(PointerEventData eventData)
    {
        if (dragIcon != null && dragIcon.gameObject.activeSelf)
            dragIcon.transform.position = eventData.position;
    }

    public void Drop(InventorySlot target)
    {
        if (dragSource == null || draggedItem == null) return;

        int targetIndex = System.Array.IndexOf(slots, target);

        // Swap in inventory data
        if (dragSourceIndex >= 0 && targetIndex >= 0)
        {
            inventory.SetSlot(dragSourceIndex, target.Item);
            inventory.SetSlot(targetIndex, draggedItem);
        }

        // Swap visuals directly
        dragSource.SetItem(target.Item);
        target.SetItem(draggedItem);

        Cleanup();
    }

    public void EndDrag()
    {
        if (dragSource != null)
            Cleanup();
    }

    private void Cleanup()
    {
        if (dragSource != null)
            dragSource.SetIconAlpha(1f);

        if (dragIcon != null)
            dragIcon.gameObject.SetActive(false);

        dragSource = null;
        draggedItem = null;
        dragSourceIndex = -1;
    }
}
