using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("인벤토리 설정")]
    [Tooltip("인벤토리 UI 패널 (Canvas의 Panel 오브젝트)")]
    public GameObject inventoryUIPanel;

    public List<ItemData> items = new List<ItemData>();
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
        playerReference = FindObjectOfType<Player>();
        if (inventoryUIPanel != null)
        {
            inventoryUIPanel.SetActive(false);
        }
    }

    public void AddItem(ItemData item)
    {
        items.Add(item);
        Debug.Log("인벤토리에 " + item.itemName + " 추가됨! (총 " + items.Count + "개)");
        // UpdateInventoryUI();
    }

    public void RemoveItem(ItemData item)
    {
        items.Remove(item);
        Debug.Log(item.itemName + " 아이템이 인벤토리에서 제거됨.");
        // UpdateInventoryUI();
    }

    // --- (수정됨) UseItem 함수 ---
    /// <summary>
    /// 지정된 슬롯 인덱스의 아이템을 사용합니다.
    /// </summary>
    public void UseItem(int slotIndex)
    {
        if (playerReference == null)
        {
            playerReference = FindObjectOfType<Player>(); // 혹시 모르니 다시 찾아봄
            if (playerReference == null)
            {
                Debug.LogError("플레이어 참조가 없습니다!");
                return;
            }
        }

        if (slotIndex >= 0 && slotIndex < items.Count)
        {
            ItemData itemToUse = items[slotIndex];

            // 1. 아이템 사용 효과 발동 (예: 체력 회복)
            itemToUse.Use(playerReference);

            // 2. 아이템 사용 후 즉시 인벤토리에서 제거
            // (HealthPackItemData 등에서 따로 Remove를 호출할 필요가 없어짐)
            RemoveItem(itemToUse);
        }
    }

    // --- (새로 추가) ---
    /// <summary>
    /// 인벤토리에 특정 이름의 아이템이 있는지 확인합니다. (열쇠, USB 등에 사용)
    /// </summary>
    /// <param name="itemName">찾을 아이템의 이름 (ItemData의 itemName)</param>
    public bool HasItem(string itemName)
    {
        foreach (ItemData item in items)
        {
            if (item.itemName == itemName)
            {
                return true;
            }
        }
        return false;
    }

    // --- (새로 추가) ---
    /// <summary>
    /// 인벤토리에서 특정 이름의 아이템을 찾아 제거합니다. (열쇠 사용 후 제거 등)
    /// </summary>
    public void RemoveItemByName(string itemName)
    {
        ItemData itemToRemove = null;
        foreach (ItemData item in items)
        {
            if (item.itemName == itemName)
            {
                itemToRemove = item;
                break;
            }
        }

        if (itemToRemove != null)
        {
            RemoveItem(itemToRemove);
        }
    }


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