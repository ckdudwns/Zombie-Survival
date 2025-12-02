using UnityEngine;
using UnityEngine.UI; // UI 제어를 위해 필요

public class CarEndingTrigger : MonoBehaviour
{
    [Header("UI 설정")]
    [Tooltip("엔딩 시 활성화될 UI 패널 (예: EndingCanvas의 Panel)")]
    public GameObject endingUIPanel;

    [Header("필요 아이템 설정 (ScriptableObject의 이름과 동일해야 함)")]
    public string carKeyName = "CarKey"; // 자동차 키 이름 (기존 BoatKey -> CarKey 수정)
    public string fuelName = "JerryCan"; // [수정됨] 연료 이름 JerryCan

    [Header("상호작용 설정")]
    public KeyCode interactKey = KeyCode.E; // 상호작용 키
    private bool isPlayerNear = false;      // 플레이어 감지 여부

    void Start()
    {
        // 시작할 때 엔딩 UI가 켜져있다면 끕니다.
        if (endingUIPanel != null)
            endingUIPanel.SetActive(false);
    }

    void Update()
    {
        // 플레이어가 범위 안에 있고, E키를 눌렀을 때
        if (isPlayerNear && Input.GetKeyDown(interactKey))
        {
            AttemptEscape();
        }
    }

    void AttemptEscape()
    {
        // 싱글톤 인스턴스가 없는 경우 에러 방지
        if (QuestManager.instance == null)
        {
            Debug.LogError("QuestManager가 씬에 없습니다!");
            return;
        }

        // [핵심 변경] QuestManager에게 자동차 상호작용 위임
        // QuestManager 내부에서 터널 폭파 여부와 연료(JerryCan) 소지 여부를 확인하고,
        // 조건이 맞으면 엔딩 퀘스트(q10_car_end)를 시작하거나 부족 메시지를 띄웁니다.
        QuestManager.instance.ProcessCarInteraction();

        // 만약 퀘스트 매니저를 안 쓰고 직접 여기서 끝내고 싶다면 아래 주석을 참고하세요.
        /*
        bool hasKey = InventoryManager.instance.HasItem(carKeyName);
        bool hasFuel = InventoryManager.instance.HasItem(fuelName); // JerryCan 확인

        if (hasKey && hasFuel)
        {
            Debug.Log("탈출 조건 충족! 엔딩을 시작합니다.");
            ShowEnding();
        }
        else
        {
            Debug.Log($"탈출 불가: 키 보유({hasKey}), 연료 보유({hasFuel})");
        }
        */
    }

    // 이 함수는 QuestManager에서 엔딩 조건을 달성했을 때 호출하거나,
    // QuestManager의 q10_car_end 완료 시점에 호출할 수 있습니다.
    public void ShowEnding()
    {
        if (endingUIPanel != null)
        {
            endingUIPanel.SetActive(true); // 엔딩 패널 켜기

            // 게임 시간 멈추기 & 마우스 커서 활성화 (UI 클릭을 위해)
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // --- 충돌 감지 (Trigger) ---
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 태그 확인
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("자동차 탑승 가능 (E키)");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}