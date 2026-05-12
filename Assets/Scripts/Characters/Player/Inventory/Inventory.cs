using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int slotCount = 31;
    public ItemData[] items;
    public ItemData[] slots => items;

    [SerializeField] private ItemData testItem;

    public event Action OnInventoryChanged;

    void Awake()
    {
        items = new ItemData[slotCount];
    }

    void Start()
    {
        if (testItem != null)
            AddItem(testItem);
    }

    public bool AddItem(ItemData item)
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
        return false; // inventory full
    }

    public void RemoveItem(ItemData item)
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

    public void SetSlot(int index, ItemData item)
    {
        if (index < 0 || index >= items.Length) return;
        items[index] = item;
        OnInventoryChanged?.Invoke();
    }
}
