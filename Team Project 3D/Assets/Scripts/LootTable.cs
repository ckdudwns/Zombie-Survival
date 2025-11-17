using UnityEngine;

// 1. 어떤 종류의 보상인지 구분하는 Enum(열거형)을 새로 만듭니다.
[System.Serializable]
public enum LootType
{
    Item,  // 인벤토리에 들어갈 아이템 (음식, 열쇠 등)
    Ammo   // 플레이어에게 즉시 지급될 총알
}

// 2. LootTableEntry 구조체를 LootTable.cs 파일로 옮기고 수정합니다.
[System.Serializable]
public struct LootTableEntry
{
    [Tooltip("드롭될 보상의 종류 (아이템 또는 총알)")]
    public LootType type; // 방금 만든 Enum 사용

    [Tooltip("Type이 Item일 경우: 드롭될 아이템 (ItemData 에셋)")]
    public ItemData itemData;

    [Tooltip("Type이 Ammo일 경우: 지급될 총알의 양")]
    public int ammoAmount;

    [Tooltip("이 보상이 나올 확률 (0 ~ 100)")]
    [Range(0f, 100f)]
    public float dropChance;
}

// 3. LootTable 스크립트 본체 (변경 없음)
[CreateAssetMenu(fileName = "New Loot Table", menuName = "Inventory/Loot Table")]
public class LootTable : ScriptableObject
{
    public LootTableEntry[] possibleItems;
}