using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    [Header("UI 요소")]
    public Image iconImage;
    public TMP_Text itemNameText; // ← 추가됨

    private ItemData item;
    private int slotIndex = -1;

    public void SetItem(ItemData newItem, int index)
    {
        item = newItem;
        slotIndex = index;

        if (item != null)
        {
            // 아이콘 적용
            if (iconImage != null)
            {
                iconImage.sprite = item.icon;
                iconImage.enabled = true;
            }

            // 이름 적용
            if (itemNameText != null)
            {
                itemNameText.text = item.itemName;
                itemNameText.enabled = true;
            }
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        item = null;
        slotIndex = -1;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (itemNameText != null)
        {
            itemNameText.text = "";
            itemNameText.enabled = false;
        }
    }

    public void OnClickUse()
    {
        if (InventoryManager.instance == null || item == null) return;

        Player player = InventoryManager.instance.GetPlayer();
        if (player != null)
        {
            item.Use(player);
            InventoryManager.instance.RemoveItem(item);
            InventoryManager.instance.UpdateInventoryUI();
        }
    }
}
