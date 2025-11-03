// InventoryManager.cs
using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("인벤토리 설정")]
    [Tooltip("인벤토리 UI 패널 (Canvas의 Panel 오브젝트)")]
    public GameObject inventoryUIPanel;

    // 현재 보유 중인 아이템 리스트
    public List<ItemData> items = new List<ItemData>();

    // 아이템 사용을 위한 플레이어 참조
    private Player playerReference;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 플레이어 참조를 찾아둡니다.
        playerReference = FindObjectOfType<Player>();

        if (inventoryUIPanel != null)
        {
            inventoryUIPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 인벤토리에 아이템을 추가합니다. (LootBox.cs에서 호출)
    /// </summary>
    public void AddItem(ItemData item)
    {
        items.Add(item);
        Debug.Log("인벤토리에 " + item.itemName + " 추가됨! (총 " + items.Count + "개)");

        // TODO: 인벤토리 UI를 업데이트하는 함수를 호출해야 합니다.
        // UpdateInventoryUI();
    }

    /// <summary>
    /// 인벤토리에서 아이템을 제거합니다. (아이템 사용 시 호출)
    /// </summary>
    public void RemoveItem(ItemData item)
    {
        items.Remove(item);
        Debug.Log(item.itemName + " 아이템이 인벤토리에서 제거됨.");

        // TODO: 인벤토리 UI를 업데이트하는 함수를 호출해야 합니다.
        // UpdateInventoryUI();
    }

    // TODO: UI 슬롯에서 아이템 사용 버튼을 눌렀을 때 이 함수가 호출되도록 연결해야 합니다.
    /// <summary>
    /// 지정된 슬롯 인덱스의 아이템을 사용합니다.
    /// </summary>
    public void UseItem(int slotIndex)
    {
        if (playerReference == null)
        {
            Debug.LogError("플레이어 참조가 없습니다!");
            return;
        }

        if (slotIndex >= 0 && slotIndex < items.Count)
        {
            ItemData itemToUse = items[slotIndex];
            itemToUse.Use(playerReference);

            // (중요) Use 함수 내부에서 Remove를 호출하는 대신,
            // 여기서 Remove를 호출하는 것이 더 안정적일 수 있습니다.
            // 위 ItemData들의 Use 함수 내부의 RemoveItem 호출을 주석 처리하고
            // 아래 라인의 주석을 해제하는 것을 권장합니다.
            // RemoveItem(itemToUse); 
        }
    }


    /// <summary>
    /// 인벤토리 UI를 켜고 끕니다. (Player.cs에서 호출)
    /// </summary>
    public bool ToggleInventory()
    {
        if (inventoryUIPanel == null)
        {
            Debug.LogWarning("인벤토리 UI 패널이 연결되지 않았습니다.");
            return false;
        }

        bool isActive = !inventoryUIPanel.activeSelf;
        inventoryUIPanel.SetActive(isActive);
        return isActive; // 현재 UI 상태 반환
    }
}