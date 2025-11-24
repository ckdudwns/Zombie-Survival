using UnityEngine;

public class ItemRequiredInteractable : MonoBehaviour, IInteractable
{
    [Header("필요 조건 설정")]
    [Tooltip("이 오브젝트와 상호작용하기 위해 필요한 아이템의 이름 (ItemData의 itemName과 똑같아야 함)")]
    public string requiredItemName;

    [Header("결과 메시지")]
    [Tooltip("아이템을 가지고 있을 때 출력할 로그")]
    public string successMessage;

    [Tooltip("아이템이 없을 때 출력할 로그")]
    public string failMessage = "필요한 아이템이 없습니다.";

    public void Interact(GameObject player)
    {
        // 1. 인벤토리 매니저가 존재하는지 확인
        if (InventoryManager.instance == null)
        {
            Debug.LogError("InventoryManager가 없습니다!");
            return;
        }

        // 2. 인벤토리에 해당 이름의 아이템이 있는지 검사 (구현해두신 HasItem 활용)
        if (InventoryManager.instance.HasItem(requiredItemName))
        {
            // 성공 시 로직
            Debug.Log(successMessage);

            // 요구사항: 아이템은 사용 후 사라지면 안 되므로 RemoveItem은 호출하지 않습니다.
        }
        else
        {
            // 실패 시 로직
            Debug.Log(failMessage);
        }
    }
}