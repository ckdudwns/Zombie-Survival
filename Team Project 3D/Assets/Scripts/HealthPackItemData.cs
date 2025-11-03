// HealthPackItemData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New HealthPack", menuName = "Inventory/HealthPack Item")]
public class HealthPackItemData : ItemData
{
    public int healAmount = 30; // 이 힐팩이 회복시킬 체력

    public override void Use(Player player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.RestoreHealth(healAmount);
            Debug.Log(player.name + "이 " + itemName + "을(를) 사용해 체력을 " + healAmount + " 회복했습니다.");

            // TODO: 아이템 사용 후 인벤토리에서 제거 로직 필요
            // InventoryManager.instance.RemoveItem(this); 
        }
    }
}