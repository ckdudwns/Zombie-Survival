using UnityEngine;
using System.Collections.Generic; // List 사용을 위해 추가

// 어떤 종류의 상호작용인지 구분하는 타입입니다.
public enum ItemPickupType
{
    AddToInventory, // 인벤토리에 아이템 추가 (음식, 열쇠, 군번줄 등)
    EquipWeapon,    // 즉시 무기 장착 (샷건, 스나이퍼)
    AddAmmo,        // 즉시 총알 추가
    ShowSpecificUI  // 특정 UI 표시 (휴대전화)
}

public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("아이템 타입 설정")]
    [Tooltip("이 아이템의 상호작용 방식을 선택하세요.")]
    public ItemPickupType pickupType;

    [Header("타입별 연결 요소")]
    [Tooltip("AddToInventory: 인벤토리에 추가할 ItemData 에셋")]
    public ItemData itemData; // 인벤토리에 저장될 아이템 (음식, 열쇠 등)

    [Tooltip("EquipWeapon: 교체할 총기 프리팹 (Gun 스크립트 포함)")]
    public Gun gunPrefab;     // 즉시 장착할 무기 (샷건, 스나이퍼)

    [Tooltip("AddAmmo: 추가할 총알의 양")]
    public int ammoAmount;    // 즉시 추가할 총알

    [Tooltip("ShowSpecificUI: 활성화할 UI 패널 (휴대전화 UI)")]
    public GameObject uiPanelToShow; // 즉시 보여줄 UI (휴대전화)

    // (참고) 총알 타입을 지정해야 한다면: public AmmoType ammoType; 등을 추가할 수 있습니다.

    /// <summary>
    /// 플레이어가 E키로 상호작용할 때 호출됩니다.
    /// </summary>
    public void Interact(GameObject player)
    {
        // 플레이어의 스크립트들을 미리 가져옵니다.
        PlayerShooting playerShooting = player.GetComponent<PlayerShooting>();
        InventoryManager invManager = InventoryManager.instance;

        // 설정된 타입에 따라 다른 행동을 수행합니다.
        switch (pickupType)
        {
            // 1. 인벤토리에 저장 (음식, 열쇠, 군번줄, 탈출템)
            case ItemPickupType.AddToInventory:
                if (invManager != null && itemData != null)
                {
                    invManager.AddItem(itemData);
                    Debug.Log(itemData.itemName + "을(를) 획득했습니다.");
                }
                break;

            // 2. 무기 즉시 장착 (샷건, 스나이퍼)
            case ItemPickupType.EquipWeapon:
                if (playerShooting != null && gunPrefab != null)
                {
                    // PlayerShooting에 이 함수를 추가해야 합니다. (아래 3번 항목 참고)
                    playerShooting.EquipNewGun(gunPrefab);
                    Debug.Log(gunPrefab.gunName + "을(를) 장착했습니다.");
                }
                break;

            // 3. 총알 즉시 충전
            case ItemPickupType.AddAmmo:
                if (playerShooting != null)
                {
                    // PlayerShooting에 이 함수를 추가해야 합니다. (아래 3번 항목 참고)
                    playerShooting.AddAmmo(ammoAmount);
                    Debug.Log("총알 " + ammoAmount + "발을 획득했습니다.");
                }
                break;

            // 4. 특정 UI 표시 (휴대전화)
            case ItemPickupType.ShowSpecificUI:
                if (uiPanelToShow != null)
                {
                    uiPanelToShow.SetActive(true);
                    // TODO: UI가 떴을 때 게임을 일시정지하고 마우스 커서를 보여줘야 합니다.
                    // Player.PauseGame(true); 
                }
                break;
        }

        // 상호작용이 끝난 아이템은 필드에서 파괴합니다.
        // (단, 휴대전화 UI는 파괴하면 안 될 수 있으므로 예외 처리)
        if (pickupType != ItemPickupType.ShowSpecificUI)
        {
            Destroy(gameObject);
        }
    }
}