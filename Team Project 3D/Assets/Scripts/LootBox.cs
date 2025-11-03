using UnityEngine;

// ----------------------------------------------------------------------
// (중요!) 오류가 발생한 'LootTableEntry'의 정의입니다.
// LootTable.cs가 참조할 수 있도록 class 바깥에 선언합니다.
// ----------------------------------------------------------------------
[System.Serializable]
public struct LootTableEntry
{
    [Tooltip("드롭될 아이템 (ItemData 에셋)")]
    public ItemData itemData;

    [Range(0f, 100f)]
    public float dropChance;
}
// ----------------------------------------------------------------------


// IInteractable 인터페이스를 상속받습니다.
public class LootBox : MonoBehaviour, IInteractable
{
    // (삭제됨) 아이템 목록이 EnemyHealth로부터 주입되므로, 
    // LootBox 자체는 더 이상 아이템 목록을 가지고 있지 않습니다.
    // public LootTableEntry[] possibleItems;

    private LootTable assignedLootTable; // EnemyHealth로부터 주입받을 루트 테이블
    private bool isOpened = false;

    /// <summary>
    /// EnemyHealth가 LootBox를 생성할 때 이 함수를 호출해 테이블을 주입합니다.
    /// </summary>
    public void Initialize(LootTable lootTable)
    {
        assignedLootTable = lootTable;
    }

    /// <summary>
    /// 플레이어가 E키로 상호작용할 때 호출됩니다.
    /// </summary>
    public void Interact(GameObject player)
    {
        if (isOpened) return;
        isOpened = true;

        InventoryManager invManager = InventoryManager.instance;
        if (invManager == null)
        {
            Debug.LogError("InventoryManager가 씬에 없습니다! 아이템을 추가할 수 없습니다.");
            Destroy(gameObject);
            return;
        }

        // 2. 주입받은 LootTable을 사용해 아이템 드롭 시도
        DropRandomItem(invManager, assignedLootTable);

        // 3. 상자 오브젝트 파괴
        Destroy(gameObject);
    }

    /// <summary>
    /// 주입받은 LootTable을 기반으로 인벤토리에 아이템을 추가합니다.
    /// </summary>
    void DropRandomItem(InventoryManager invManager, LootTable table)
    {
        // EnemyHealth로부터 LootTable을 잘 받았는지 확인
        if (table == null)
        {
            Debug.LogWarning("LootBox에 LootTable이 할당되지 않았습니다. (EnemyHealth.cs 확인 필요)");
            return;
        }

        float randomRoll = Random.Range(0f, 100f);
        float cumulativeChance = 0f;

        // 'table.possibleItems' (주입받은 테이블의 목록)을 순회합니다.
        foreach (var drop in table.possibleItems)
        {
            cumulativeChance += drop.dropChance;
            if (randomRoll <= cumulativeChance)
            {
                if (drop.itemData != null)
                {
                    // (핵심) 아이템을 생성하는 대신 인벤토리 매니저에 바로 추가
                    invManager.AddItem(drop.itemData);
                    Debug.Log("상자에서 " + drop.itemData.itemName + " 아이템을 획득했습니다!");
                }
                return; // 아이템 하나만 지급
            }
        }

        Debug.Log("상자에서 추가 아이템은 나오지 않았습니다.");
    }
}