using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ItemStat
{
    public string label;
    public string value;
}

[CreateAssetMenu(menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public ItemType itemType;

    [Header("World Prefab")]
    public GameObject worldPrefab;

    [Header("Stats")]
    public List<ItemStat> stats;

    public virtual IEnumerable<ItemStat> GetDisplayStats()
    {
        return stats ?? new List<ItemStat>();
    }
}

public enum ItemType
{
    Material,
    Weapon,
    Armour,
    Consumable,
    Key,
    Tool,
    QuestItem
}
