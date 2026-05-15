using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class InventorySlot : MonoBehaviour, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI quantityText;

    public ItemInstance Item { get; private set; }

    private void Awake()
    {
        GetComponent<Image>().raycastTarget = true;
    }

    private InventoryItem ItemChild => GetComponentInChildren<InventoryItem>(true);

    public void SetItem(ItemInstance item)
    {
        if (item == null) { ClearSlot(); return; }
        Item = item;

        InventoryItem invItem = ItemChild;
        if (invItem == null)
        {
            var go = new GameObject("ItemIcon", typeof(RectTransform), typeof(Image), typeof(InventoryItem));
            go.transform.SetParent(transform, false);
            invItem = go.GetComponent<InventoryItem>();
            invItem.image = go.GetComponent<Image>();
            invItem.image.preserveAspect = true;
        }

        invItem.gameObject.SetActive(true);
        invItem.image.sprite = item.data.icon;
        invItem.image.raycastTarget = true;
        invItem.itemInstance = item;
        invItem.FillParent();

        if (quantityText != null) quantityText.text = item.data.itemName;
    }

    public void ClearSlot()
    {
        Item = null;
        InventoryItem invItem = ItemChild;
        if (invItem != null)
            invItem.gameObject.SetActive(false);
        if (quantityText != null) quantityText.text = "";
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItem draggedItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        if (draggedItem == null) return;

        InventorySlot sourceSlot = null;
        if (draggedItem.parentAfterDrag != null)
            draggedItem.parentAfterDrag.TryGetComponent(out sourceSlot);

        if (sourceSlot == this) return;

        InventoryItem existingItem = ItemChild;
        bool hasExisting = existingItem != null && existingItem.gameObject.activeSelf;

        if (hasExisting)
        {
            if (sourceSlot != null)
            {
                existingItem.transform.SetParent(sourceSlot.transform, false);
                existingItem.FillParent();
                sourceSlot.Item = existingItem.itemInstance;
            }
            else
            {
                existingItem.gameObject.SetActive(false);
            }
        }
        else if (sourceSlot != null)
        {
            sourceSlot.Item = null;
        }

        draggedItem.parentAfterDrag = transform;
        Item = draggedItem.itemInstance;

        if (sourceSlot != null)
        {
            int srcIdx = InventoryUI.Instance.GetSlotIndex(sourceSlot);
            int dstIdx = InventoryUI.Instance.GetSlotIndex(this);
            Inventory inv = InventoryUI.Instance.Inventory;
            if (inv != null && srcIdx >= 0 && dstIdx >= 0)
                (inv.items[srcIdx], inv.items[dstIdx]) = (inv.items[dstIdx], inv.items[srcIdx]);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Item == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
            InventoryUI.Instance.SelectItem(this);

        if (eventData.button == PointerEventData.InputButton.Right)
            DropItemToGround();
    }

    private void DropItemToGround()
    {
        if (Item == null) return;

        Character player = FindObjectOfType<Character>();
        if (player == null) return;

        GameObject groundItemPrefab = Resources.Load<GameObject>("GroundItem");
        if (groundItemPrefab != null)
        {
            Vector3 dropPosition = player.transform.position + player.transform.forward * 2f;
            GameObject groundItemObj = Instantiate(groundItemPrefab, dropPosition, Quaternion.identity);
            GroundItem groundItem = groundItemObj.GetComponent<GroundItem>();
            if (groundItem != null)
            {
                groundItem.itemData = Item.data;
                groundItem.quantity = Item.quantity;
            }
        }
        else
        {
            Debug.LogWarning("GroundItem prefab not found in Resources/GroundItem.prefab");
        }

        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory != null)
            inventory.RemoveItem(Item);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Item == null) return;
        InventoryUI.Instance.SelectItem(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryUI.Instance != null)
            InventoryUI.Instance.SelectItem(null);
    }
}
