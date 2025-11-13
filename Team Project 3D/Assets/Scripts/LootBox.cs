using UnityEngine;

// IInteractable 인터페이스를 상속받습니다.
public class LootBox : MonoBehaviour, IInteractable
{
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

        // --- (수정됨) ---
        // DropRandomItem 함수에 'player'를 전달해줍니다.
        // (총알을 줘야 할 수도 있으므로 PlayerShooting 스크립트를 찾아야 함)
        DropRandomItem(invManager, assignedLootTable, player);
        // --- (여기까지) ---

        // 3. 상자 오브젝트 파괴
        Destroy(gameObject);
    }

    /// <summary>
    /// 주입받은 LootTable을 기반으로 보상을 지급합니다.
    /// </summary>
    void DropRandomItem(InventoryManager invManager, LootTable table, GameObject player)
    {
        if (table == null)
        {
            Debug.LogWarning("LootBox에 LootTable이 할당되지 않았습니다.");
            return;
        }

        float randomRoll = Random.Range(0f, 100f);
        float cumulativeChance = 0f;

        foreach (var drop in table.possibleItems)
        {
            cumulativeChance += drop.dropChance;
            if (randomRoll <= cumulativeChance)
            {
                // --- (핵심 수정 부분) ---
                // 보상 타입(LootType)에 따라 다른 처리를 합니다.
                switch (drop.type)
                {
                    // 1. 타입이 'Item'일 경우 (음식 등)
                    case LootType.Item:
                        if (drop.itemData != null)
                        {
                            invManager.AddItem(drop.itemData);
                            Debug.Log("상자에서 " + drop.itemData.itemName + " 아이템을 획득했습니다!");
                        }
                        break;

                    // 2. 타입이 'Ammo'일 경우
                    case LootType.Ammo:
                        PlayerShooting playerShooting = player.GetComponent<PlayerShooting>();
                        if (playerShooting != null && drop.ammoAmount > 0)
                        {
                            playerShooting.AddAmmo(drop.ammoAmount); // PlayerShooting의 AddAmmo 함수 호출
                            Debug.Log("상자에서 총알 " + drop.ammoAmount + "발을 획득했습니다!");
                        }
                        break;
                }
                // --- (여기까지) ---
                return; // 아이템 하나만 지급
            }
        }

        Debug.Log("상자에서 추가 아이템은 나오지 않았습니다.");
    }
}