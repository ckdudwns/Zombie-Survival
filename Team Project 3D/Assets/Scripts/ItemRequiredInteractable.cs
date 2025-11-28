using UnityEngine;
using UnityEngine.Events; // UnityEvent를 사용하기 위해 추가

public class ItemRequiredInteractable : MonoBehaviour, IInteractable
{
    [Header("필요 조건 설정")]
    [Tooltip("이 오브젝트와 상호작용하기 위해 필요한 아이템의 이름")]
    public string requiredItemName;

    [Header("결과 메시지")]
    [Tooltip("아이템을 가지고 있을 때 출력할 로그")]
    public string successMessage;

    [Tooltip("아이템이 없을 때 출력할 로그")]
    public string failMessage = "필요한 아이템이 없습니다.";

    [Header("이벤트 설정")]
    [Tooltip("아이템 검사에 통과했을 때 실행할 함수들을 이곳에 등록하세요.")]
    public UnityEvent onInteractSuccess; // 성공 시 실행될 이벤트

    public void Interact(GameObject player)
    {
        if (InventoryManager.instance == null)
        {
            Debug.LogError("InventoryManager가 없습니다!");
            return;
        }

        // 아이템 보유 여부 확인
        if (InventoryManager.instance.HasItem(requiredItemName))
        {
            // 1. 성공 메시지 출력
            Debug.Log(successMessage);

            // 2. 등록된 이벤트 실행 (여기서 문을 여는 함수를 호출하게 됩니다)
            onInteractSuccess.Invoke();

            // 아이템 제거가 필요하다면 여기에 InventoryManager.instance.RemoveItemByName(requiredItemName); 추가
        }
        else
        {
            // 실패 메시지 출력
            Debug.Log(failMessage);
        }
    }
}