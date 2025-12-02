using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable // IInteractable 인터페이스가 있다면 유지
{
    [Header("Settings")]
    [Tooltip("속도를 얼마나 줄일지 설정 (0.5는 50%)")]
    public float slowDownFactor = 0.5f;

    private bool isPlayerNear = false;
<<<<<<< Updated upstream
    private bool isInteracting = false;
    private bool isSlowed = false;
=======
    private bool isInteracting = false; // 중복 실행 방지용
>>>>>>> Stashed changes

    private Player playerScript;

    void Update()
    {
        // 플레이어가 범위 내에 있고, E키를 눌렀으며, 아직 상호작용 안 했을 때
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && !isInteracting)
        {
            StartInteraction();
        }
    }

    // 외부(Raycast 등)에서 Interact를 호출할 때와, Update에서 E키를 누를 때 공통으로 사용
    public void Interact(GameObject player)
    {
        // 플레이어 스크립트 참조가 비어있다면 할당
        if (playerScript == null)
            playerScript = player.GetComponent<Player>();

        StartInteraction();
    }

    void StartInteraction()
    {
        if (isInteracting) return; // 중복 실행 방지
        isInteracting = true;

        Debug.Log("NPC(VIP) 상호작용 시작...");

        // 1. QuestManager에게 VIP 상호작용 알림 (퀘스트 시작 및 대사 출력)
        if (QuestManager.instance != null)
        {
            QuestManager.instance.ProcessVIPInteraction();
        }
        else
        {
            Debug.LogError("QuestManager 인스턴스를 찾을 수 없습니다!");
        }

        // 2. 상호작용 결과 처리 (속도 감소 및 NPC 사라짐)
        ApplyInteractEffects();
    }

    // 대화 종료 대기 없이 즉시 효과 적용 (퀘스트 대사는 UI로 뜨고, NPC는 업힌 설정 등으로 사라짐)
    void ApplyInteractEffects()
    {
        // 플레이어 속도 감소 적용
        if (playerScript != null)
        {
            playerScript.moveSpeed *= slowDownFactor;
            playerScript.sprintSpeed *= slowDownFactor; // 달리기 속도도 감소

            Debug.Log($"플레이어 속도가 {slowDownFactor * 100}%로 줄었습니다.");
        }

        // NPC 사라지게 만들기 (플레이어가 업거나 동행하는 연출을 위해 맵에서 제거)
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            // 플레이어 컴포넌트 미리 가져오기
            playerScript = other.GetComponent<Player>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            // 나갈 때는 null 처리하지 않음 (상호작용 도중 조금 멀어져도 로직이 끊기지 않게 하기 위함)
        }
    }
}