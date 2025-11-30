using UnityEngine;
using UnityEngine.UI; // UI 제어를 위해 필요

public class BoatEndingTrigger : MonoBehaviour
{
    [Header("UI 설정")]
    [Tooltip("엔딩 시 활성화될 UI 패널 (예: EndingCanvas의 Panel)")]
    public GameObject endingUIPanel;

    [Header("필요 아이템 설정 (ScriptableObject의 이름과 동일해야 함)")]
    public string boatKeyName = "BoatKey"; // 인벤토리상의 보트키 이름
    public string fuelName = "Fuel";       // 인벤토리상의 기름 이름

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
        if (InventoryManager.instance == null)
        {
            Debug.LogError("InventoryManager가 씬에 없습니다!");
            return;
        }

        // 1. InventoryManager의 HasItem 함수를 호출하여 검사
        bool hasKey = InventoryManager.instance.HasItem(boatKeyName);
        bool hasFuel = InventoryManager.instance.HasItem(fuelName);

        if (hasKey && hasFuel)
        {
            Debug.Log("탈출 조건 충족! 엔딩을 시작합니다.");

            // (선택 사항) 탈출 시 아이템을 소비하고 싶다면 주석을 해제하세요.
            // InventoryManager.instance.RemoveItemByName(boatKeyName);
            // InventoryManager.instance.RemoveItemByName(fuelName);

            ShowEnding();
        }
        else
        {
            // 아이템이 부족할 때
            Debug.Log($"탈출 불가: 키 보유({hasKey}), 기름 보유({hasFuel})");
            // 여기에 "아이템이 부족합니다" 같은 UI 텍스트를 띄우는 코드를 추가할 수 있습니다.
        }
    }

    void ShowEnding()
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
            Debug.Log("보트 탑승 가능 (E키)");
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