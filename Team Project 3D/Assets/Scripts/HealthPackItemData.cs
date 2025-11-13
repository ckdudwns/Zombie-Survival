using UnityEngine;

[CreateAssetMenu(fileName = "New HealthPack", menuName = "Inventory/HealthPack Item")]
public class HealthPackItemData : ItemData
{
    public int healAmount = 30; // 이 힐팩이 회복시킬 체력

    public override void Use(Player player)
    {
        // Player 스크립트가 아닌 PlayerHealth 스크립트를 직접 찾아야 합니다.
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.RestoreHealth(healAmount);
            Debug.Log(player.name + "이 " + itemName + "을(를) 사용해 체력을 " + healAmount + " 회복했습니다.");

            // (삭제) 이 코드는 InventoryManager.UseItem()으로 이동했습니다.
            // InventoryManager.instance.RemoveItem(this); 
        }
    }
}