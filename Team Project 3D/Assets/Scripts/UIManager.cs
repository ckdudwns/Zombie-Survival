using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startUIPanel;
    public GameObject failUIPanel;
    public GameObject successUIPanel;

    public GameObject healthUI;
    public GameObject ammoUI;

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
        // [변경점] 시작 화면을 띄우지 않고 바로 게임 시작 로직을 실행합니다.
        // ShowStartUI();  <-- 기존 코드 주석 처리 또는 삭제
        StartGame();    // <-- 바로 게임 시작
    }

    // 이 함수는 이제 사용되지 않지만, 나중에 일시정지 기능 등에 재활용할 수 있어 남겨두었습니다.
    public void ShowStartUI()
    {
        if (startUIPanel != null) startUIPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Player.isPaused = true;

        if (healthUI != null) healthUI.SetActive(false);
        if (ammoUI != null) ammoUI.SetActive(false);
    }

    public void ShowFailUI()
    {
        failUIPanel.SetActive(true);
        // 실패 시 커서를 다시 보이게 설정 (필요 시 추가)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowSuccessUI()
    {
        successUIPanel.SetActive(true);
        // 성공 시 커서를 다시 보이게 설정 (필요 시 추가)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UpdateHealth(int current, int max)
    {
        // 체력 UI 업데이트 로직 (구현 필요)
    }

    public void UpdateAmmo(int current, int max)
    {
        // 탄약 UI 업데이트 로직 (구현 필요)
    }

    public void StartGame()
    {
        // 1) 시작 UI가 켜져 있다면 끄기
        if (startUIPanel != null)
            startUIPanel.SetActive(false);

        // 게임 재개 (시간 흐름 정상화)
        Time.timeScale = 1f;

        // 2) 인게임용 UI 활성화
        if (healthUI != null)
            healthUI.SetActive(true);

        if (ammoUI != null)
            ammoUI.SetActive(true);

        // 게임 시작 플래그 활성화
        Player.isGameStarted = true;

        // 마우스 커서 잠그기 (FPS 게임 등에서 필요)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 플레이어 조작 활성화
        Player.isPaused = false;

        // (+) 시작 퀘스트 강제로 시작
        // QuestManager가 씬에 존재하는지 확인 후 호출 (오류 방지)
        if (QuestManager.instance != null)
        {
            QuestManager.instance.StartQuest("q00");
        }
    }
}