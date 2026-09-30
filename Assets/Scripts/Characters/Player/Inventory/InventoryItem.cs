using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Image image;
    [HideInInspector] public ItemInstance itemInstance;
    [HideInInspector] public Transform parentAfterDrag;

    private void Awake()
    {
        if (image == null)
            image = GetComponent<Image>();
    }

    
    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(64, 64);
        rt.position = eventData.position;

        if (image != null) image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (image != null) image.raycastTarget = true;

        bool overSlot = false;
        foreach (GameObject hovered in eventData.hovered)
        {
            if (hovered.GetComponent<InventorySlot>() != null)
            {
                overSlot = true;
                break;
            }
        }

        if (!overSlot)
        {
            DropToGround();
            return;
        }

        if (parentAfterDrag != null)
        {
            transform.SetParent(parentAfterDrag);
            FillParent();
        }
    }

    private void DropToGround()
    {
        Inventory inv = InventoryUI.Instance != null ? InventoryUI.Instance.Inventory : null;
        if (inv != null)
            inv.RemoveItem(itemInstance);

        if (itemInstance != null && itemInstance.data != null && itemInstance.data.worldPrefab != null)
        {
            Character player = FindObjectOfType<Character>();
            if (player != null)
            {
                Vector3 dropPos = player.transform.position + player.transform.forward * 2f;
                Instantiate(itemInstance.data.worldPrefab, dropPos, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }

    public void FillParent()
    {
        RectTransform rect = GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }


    
}
