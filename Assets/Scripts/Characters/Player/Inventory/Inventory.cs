using System;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int slotCount = 31;
    public ItemInstance[] items;
    public ItemInstance[] slots => items;

    [SerializeField] private ItemData testItem;

    public event Action OnInventoryChanged;

    void Awake()
    {
        items = new ItemInstance[slotCount];
    }

    void Start()
    {
    }

    public bool AddItem(ItemInstance item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    public void RemoveItem(ItemInstance item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
            {
                items[i] = null;
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

    public void SetSlot(int index, ItemInstance item)
    {
        if (index < 0 || index >= items.Length) return;
        items[index] = item;
        OnInventoryChanged?.Invoke();
    }

    public void SwapSlots(int a, int b)
    {
        if (a < 0 || a >= items.Length || b < 0 || b >= items.Length) return;
        (items[a], items[b]) = (items[b], items[a]);
        OnInventoryChanged?.Invoke();
    }
}
