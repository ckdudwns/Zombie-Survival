// LootTable.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Loot Table", menuName = "Inventory/Loot Table")]
public class LootTable : ScriptableObject
{
    public LootTableEntry[] possibleItems;
}