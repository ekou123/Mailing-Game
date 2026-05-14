using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon")]
public class WeaponData : ItemData
{
    [Header("Weapon Stats")]
    public int damage;
    public float attackSpeed;
    public float range;
    public WeaponType weaponType;

    public override IEnumerable<ItemStat> GetDisplayStats()
    {
        return new List<ItemStat>
        {
            new() { label = "Type",        value = weaponType.ToString() },
            new() { label = "Damage",      value = damage.ToString() },
            new() { label = "Attack Speed", value = attackSpeed.ToString("0.##") },
            new() { label = "Range",       value = range.ToString("0.##") },
        };
    }
}

public enum WeaponType
{
    Sword,
    Axe,
    Bow,
    Staff,
    Dagger
}