using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Armour")]
public class ArmourData : ItemData
{
    [Header("Armour Stats")]
    public int defense;
    public int durability;
    public ArmourSlot slot;

    public override IEnumerable<ItemStat> GetDisplayStats()
    {
        return new List<ItemStat>
        {
            new() { label = "Slot",      value = slot.ToString() },
            new() { label = "Defense",   value = defense.ToString() },
            new() { label = "Durability", value = durability.ToString() },
        };
    }
}

public enum ArmourSlot { Head, Chest, Legs, Feet, Hands }
