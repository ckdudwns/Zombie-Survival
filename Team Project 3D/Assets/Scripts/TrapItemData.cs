// TrapItemData.cs
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
        // 예: 카메라 Raycast로 바닥 위치를 찾은 뒤 Instantiate
        Debug.Log(player.name + "이 " + itemName + "을(를) 설치했습니다.");

        // TODO: 아이템 사용 후 인벤토리에서 제거 로직 필요
        // InventoryManager.instance.RemoveItem(this);
    }
}