using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int slotCount = 27;
    public ItemData[] slots;

    [SerializeField] private ItemData testItem;

    public event Action OnInventoryChanged;

    void Awake()
    {
        slots = new ItemData[slotCount];
    }

    void Start()
    {
        if (testItem != null)
            AddItem(testItem);
    }

    public bool AddItem(ItemData item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        return false; // inventory full
    }

    public void RemoveItem(ItemData item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == item)
            {
                slots[i] = null;
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

    public void SetSlot(int index, ItemData item)
    {
        if (index < 0 || index >= slots.Length) return;
        slots[index] = item;
        OnInventoryChanged?.Invoke();
    }
}
