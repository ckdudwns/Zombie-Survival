// CurrencyItemData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New CoinItem", menuName = "Inventory/Currency Item")]
public class CurrencyItemData : ItemData
{
    public int minAmount = 50;
    public int maxAmount = 200;

    public override void Use(Player player)
    {
        int amount = Random.Range(minAmount, maxAmount + 1);
        player.AddCoins(amount); // Player.cs의 AddCoins 함수 재활용
        Debug.Log(player.name + "이 " + itemName + "을(를) 사용해 코인 " + amount + "을(를) 획득했습니다.");

        // TODO: 아이템 사용 후 인벤토리에서 제거 로직 필요
        // InventoryManager.instance.RemoveItem(this);
    }
}