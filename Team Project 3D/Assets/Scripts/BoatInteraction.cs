using UnityEngine;

public class BoatInteraction : MonoBehaviour
{
    [Header("속도 복구 설정")]
    [Tooltip("복구할 걷기 속도 (기본값: 6)")]
    public float targetMoveSpeed = 6.0f;
    [Tooltip("복구할 달리기 속도 (기본값: 10)")]
    public float targetSprintSpeed = 10.0f;

    [Header("배 출발 설정")]
    [Tooltip("배가 이동할 속도")]
    public float boatMoveSpeed = 5.0f;
    [Tooltip("배가 출발한 후 사라지기까지의 시간 (초)")]
    public float destroyDelay = 15.0f;

    // 내부 변수
    private bool isPlayerNear = false; // 플레이어 접근 여부
    private bool isDeparting = false;  // 배 출발 여부
    private Player playerScript;       // 플레이어 스크립트 참조

    void Update()
    {
        // 1. 상호작용 체크 (플레이어가 근처에 있고, E키를 눌렀으며, 아직 출발 안 함)
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && !isDeparting)
        {
            DepartBoat();
        }

        // 2. 배 이동 로직 (출발 상태라면 계속 앞으로 이동)
        if (isDeparting)
        {
            // 배가 바라보는 방향(Forward)으로 이동
            transform.Translate(Vector3.forward * boatMoveSpeed * Time.deltaTime);
        }
    }

    void DepartBoat()
    {
        isDeparting = true; // 출발 상태로 변경

        Debug.Log("배가 항구를 떠납니다.");

        // [추가됨] QuestManager에게 탈출 신호를 보냄 -> 엔딩 퀘스트(q08_save_end) 시작
        if (QuestManager.instance != null)
        {
            QuestManager.instance.ProcessBoatEscape();
        }
        else
        {
            Debug.LogError("QuestManager가 존재하지 않습니다!");
        }

        // 플레이어 속도 복구 (VIP를 배에 태웠으므로 무거움 해제)
        if (playerScript != null)
        {
            playerScript.moveSpeed = targetMoveSpeed;
            playerScript.sprintSpeed = targetSprintSpeed;
            Debug.Log("치료 완료! 속도가 정상으로 돌아왔습니다.");
        }

        // (선택 사항) 더 이상 상호작용 못하게 Trigger 끄기
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 일정 시간 뒤 배 삭제
        Destroy(gameObject, destroyDelay);
    }

    // --- Trigger 감지 로직 (NPC와 동일) ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerScript = other.GetComponent<Player>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerScript = null;
        }
    }
}