using UnityEngine;

[CreateAssetMenu(fileName = "New TrapItem", menuName = "Inventory/Trap Item")]
public class TrapItemData : ItemData
{
    public GameObject trapPrefab; // 바닥에 설치할 트랩 프리팹

    public override void Use(Player player)
    {
        if (trapPrefab == null)
        {
            Debug.LogError(itemName + "에 trapPrefab이 연결되지 않았습니다.");
            return;
        }

        // TODO: 플레이어 앞 바닥에 trapPrefab을 설치(Instantiate)하는 로직 구현
        // 이 예시에서는 플레이어의 발밑에 설치합니다.
        Instantiate(trapPrefab, player.transform.position, player.transform.rotation);
        Debug.Log(player.name + "이 " + itemName + "을(를) 설치했습니다.");

        // (삭제) 이 코드는 InventoryManager.UseItem()으로 이동했습니다.
        // InventoryManager.instance.RemoveItem(this);
    }
}