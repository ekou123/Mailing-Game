using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Consumable")]
public class ConsumableData : ItemData
{
    [Header("Consumable Stats")]
    public int healthRestore;
    public float duration;

    public override IEnumerable<ItemStat> GetDisplayStats()
    {
        var stats = new List<ItemStat>();

        if (healthRestore != 0)
            stats.Add(new() { label = "Health Restore", value = healthRestore.ToString() });

        if (duration > 0f)
            stats.Add(new() { label = "Duration", value = $"{duration:0.#}s" });

        return stats;
    }
}
