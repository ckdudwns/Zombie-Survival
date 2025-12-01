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
        ShowStartUI();
    }

    public void ShowStartUI()
    {
        startUIPanel.SetActive(true);

        // 게임 정지
        Time.timeScale = 0f;

        // 커서 활성화
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Player 입력 막기
        Player.isPaused = true;

        // 2) 인게임용 UI 비활성화
        if (healthUI != null)
            healthUI.SetActive(false);

        if (ammoUI != null)
            ammoUI.SetActive(false);


    }

    public void ShowFailUI()
    {
        failUIPanel.SetActive(true);
    }

    public void ShowSuccessUI()
    {
        successUIPanel.SetActive(true);
    }

    public void UpdateHealth(int current, int max)
    {

    }

    public void UpdateAmmo(int current, int max)
    {

    }

    public void StartGame()
    {
        // 1) 시작 UI 비활성화
        if (startUIPanel != null)
            startUIPanel.SetActive(false);

        // 게임 재개
        Time.timeScale = 1f;

        // 2) 인게임용 UI 활성화
        if (healthUI != null)
            healthUI.SetActive(true);

        if (ammoUI != null)
            ammoUI.SetActive(true);

        // 게임 시작 플래그 활성화
        Player.isGameStarted = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        // 플레이어 다시 활성화
        Player.isPaused = false;

        //(+) 시작 퀘스트 강제로 시작
        QuestManager.instance.StartQuest("q00");

    }
}