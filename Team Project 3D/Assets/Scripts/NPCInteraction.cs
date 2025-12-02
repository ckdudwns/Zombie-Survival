using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("속도를 얼마나 줄일지 설정 (0.5는 50%)")]
    public float slowDownFactor = 0.5f;

    private bool isPlayerNear = false;
    private bool isInteracting = false;
    private bool isSlowed = false;

    private Player playerScript;

    void Update()
    {
        // 플레이어가 범위 내에 있고, E키를 눌렀으며, 대화 중이 아닐 때
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && !isInteracting)
        {
            StartInteraction();
        }
    }

    void StartInteraction()
    {
        isInteracting = true;
        Debug.Log("대화 시작...");

        // 대화 시스템 연결 (테스트용으로 2초 뒤 종료 설정)
        PlayDialogue();
    }

    void PlayDialogue()
    {
        // 2초 뒤에 EndInteraction 함수 실행 (테스트용)
        Invoke("EndInteraction", 2.0f);
    }

    // ★ 대화 종료 시 호출되는 함수
    public void EndInteraction()
    {
        isInteracting = false;

        // 1. 플레이어 속도 감소 적용
        if (playerScript != null && !isSlowed)
        {
            playerScript.moveSpeed *= slowDownFactor;
            playerScript.sprintSpeed *= slowDownFactor;

            isSlowed = true;
            Debug.Log($"플레이어 속도가 {slowDownFactor * 100}%로 줄었습니다.");
        }

        // 2. ★ NPC 사라지게 만들기
        // gameObject는 이 스크립트가 붙어있는 NPC 자신을 의미합니다.
        gameObject.SetActive(false);

        // 만약 게임에서 아예 삭제하고 싶다면 위 코드 대신 아래 코드를 쓰세요.
        // Destroy(gameObject); 
    }

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