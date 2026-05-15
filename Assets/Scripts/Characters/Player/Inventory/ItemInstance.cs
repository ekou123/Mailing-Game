using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemInstance
{
    public ItemData data;
    public int quantity;
    public List<ItemStat> statOverrides = new();

    public ItemInstance(ItemData data, int quantity = 1)
    {
        this.data = data;
        this.quantity = quantity;
    }

    // Returns base stats merged with any per-instance overrides.
    public IEnumerable<ItemStat> GetDisplayStats()
    {
        var overrides = new Dictionary<string, string>();
        foreach (var s in statOverrides)
            overrides[s.label] = s.value;

        foreach (var stat in data.GetDisplayStats())
            yield return overrides.TryGetValue(stat.label, out var val)
                ? new ItemStat { label = stat.label, value = val }
                : stat;
    }
}
