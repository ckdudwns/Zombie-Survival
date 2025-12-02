using UnityEngine;
using System.Collections.Generic;

// [추가] 오디오 소스 컴포넌트가 없으면 자동으로 추가
[RequireComponent(typeof(AudioSource))]
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("인벤토리 설정")]
    [Tooltip("인벤토리 UI 패널 (Canvas의 Panel 오브젝트)")]
    public GameObject inventoryUIPanel;

    public List<ItemData> items = new List<ItemData>();
    private Player playerReference;

    // [추가] 소리를 재생할 오디오 소스
    private AudioSource audioSource;

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
        // [추가] 오디오 소스 가져오기
        audioSource = GetComponent<AudioSource>();

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

        if (QuestManager.instance != null)
        {
            QuestManager.instance.OnItemAdded(item.itemName);
        }

        UpdateInventoryUI();
    }

    public void RemoveItem(ItemData item)
    {
        items.Remove(item);
        Debug.Log(item.itemName + " 아이템이 인벤토리에서 제거됨.");
        UpdateInventoryUI();
    }

    // --- (수정됨) UseItem 함수 ---
    public void UseItem(int slotIndex)
    {
        if (playerReference == null)
        {
            playerReference = FindObjectOfType<Player>();
            if (playerReference == null)
            {
                Debug.LogError("플레이어 참조가 없습니다!");
                return;
            }
        }

        if (slotIndex >= 0 && slotIndex < items.Count)
        {
            ItemData itemToUse = items[slotIndex];

            // 사용 가능한 아이템인지 확인
            if (itemToUse.isUsable == false)
            {
                Debug.Log(itemToUse.itemName + "은(는) 사용할 수 없는 아이템입니다.");
                return;
            }

            // [★추가된 부분] 아이템에 설정된 사운드가 있다면 재생
            if (itemToUse.useSound != null && audioSource != null)
            {
                // PlayOneShot은 소리가 겹쳐도 끊기지 않고 재생됩니다.
                audioSource.PlayOneShot(itemToUse.useSound);
            }

            // 1. 아이템 사용 효과 발동
            itemToUse.Use(playerReference);

            // 2. 아이템 사용 후 인벤토리에서 제거
            RemoveItem(itemToUse);
        }
    }

    // ... (이하 나머지 코드는 기존과 동일) ...

    public bool HasItem(string searchName)
    {
        if (items == null) return false;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null) continue;
            if (items[i].itemName == searchName) return true;
        }
        return false;
    }

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
        if (itemToRemove != null) RemoveItem(itemToRemove);
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
        return isActive;
    }
<<<<<<< Updated upstream
=======

    void InitializeSlots(int slotCount = 20)
    {
        if (slotPrefab == null || slotsParent == null) return;
        foreach (Transform child in slotsParent) Destroy(child.gameObject);
        slots.Clear();

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotsParent);
            ItemSlot slot = slotGO.GetComponent<ItemSlot>();
            if (slot != null)
            {
                slot.ClearSlot();
                slots.Add(slot);
                int index = i;
                slotGO.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
                {
                    UseItem(index);
                });
            }
        }
    }

    public void UpdateInventoryUI()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Count) slots[i].SetItem(items[i], i);
            else slots[i].ClearSlot();
        }
    }

    public Player GetPlayer()
    {
        return playerReference;
    }
>>>>>>> Stashed changes
}