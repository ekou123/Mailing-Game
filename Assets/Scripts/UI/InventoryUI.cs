using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [SerializeField] private InventorySlot[] slots;
    [SerializeField] private Image dragIcon;

    [SerializeField] private ItemDetailsUI itemDetailsUI;

    private InventorySlot selectedSlot;


    private Inventory inventory;

    public Image DragIcon => dragIcon;
    public Inventory Inventory => inventory;
    public int GetSlotIndex(InventorySlot slot) => System.Array.IndexOf(slots, slot);

    void Awake()
    {
        Instance = this;

        if (slots == null || slots.Length == 0 || System.Array.Exists(slots, s => s == null))
            slots = GetComponentsInChildren<InventorySlot>(true);

        if (dragIcon == null)
        {
            Canvas rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                GameObject go = new("DragIcon");
                go.transform.SetParent(rootCanvas.transform, false);
                dragIcon = go.AddComponent<Image>();
                dragIcon.raycastTarget = false;
                dragIcon.preserveAspect = true;
                dragIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 64);
                go.transform.SetAsLastSibling();
            }
        }

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
            Debug.Log($"[InventoryUI] SelectItem called with item: {slot.Item?.data.itemName ?? "null"}");
            itemDetailsUI.Show(slot.Item);
        }
        else
        {
            Debug.Log("[InventoryUI] SelectItem called with null slot, clearing details");
            // itemDetailsUI.Clear();
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

    private void OnEnable()
    {
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

}
