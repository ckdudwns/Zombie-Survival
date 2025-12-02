using UnityEngine;
using System.Collections.Generic;

public enum ItemPickupType
{
    AddToInventory,
    EquipWeapon,
    AddAmmo,
    ShowSpecificUI
}

public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("아이템 타입 설정")]
    public ItemPickupType pickupType;

    [Header("타입별 연결 요소")]
    public ItemData itemData;
    public Gun gunPrefab;
    public int ammoAmount;
    public GameObject uiPanelToShow;

    public void Interact(GameObject player)
    {
        PlayerShooting playerShooting = player.GetComponent<PlayerShooting>();
        InventoryManager invManager = InventoryManager.instance;

        switch (pickupType)
        {
            case ItemPickupType.AddToInventory:
                if (invManager != null && itemData != null)
                {
<<<<<<< Updated upstream
                    invManager.AddItem(itemData);
                    Debug.Log(itemData.itemName + "을(를) 획득했습니다.");
=======
                    invManager.AddItem(itemData); // 여기서 QuestManager 호출됨 (InventoryManager 수정본 기준)
>>>>>>> Stashed changes
                }
                break;

            case ItemPickupType.EquipWeapon:
                if (playerShooting != null && gunPrefab != null)
                {
                    playerShooting.EquipNewGun(gunPrefab);
                    Debug.Log(gunPrefab.GetComponent<Gun>().gunName + "을(를) 장착했습니다.");

                    // [★추가됨] 무기 획득 시에도 QuestManager에게 알림 (대사 출력용)
                    if (QuestManager.instance != null)
                    {
                        // Gun 스크립트의 gunName을 가져와서 알림
                        string gunName = gunPrefab.GetComponent<Gun>().gunName;
                        QuestManager.instance.OnItemAdded(gunName);
                    }
                }
                break;

            case ItemPickupType.AddAmmo:
                if (playerShooting != null)
                {
                    playerShooting.AddAmmo(ammoAmount);
                    Debug.Log("총알 " + ammoAmount + "발을 획득했습니다.");
                }
                break;

            case ItemPickupType.ShowSpecificUI:
                if (uiPanelToShow != null)
                {
                    uiPanelToShow.SetActive(true);
                }
                break;
        }

        if (pickupType != ItemPickupType.ShowSpecificUI)
        {
            Destroy(gameObject);
        }
    }
}