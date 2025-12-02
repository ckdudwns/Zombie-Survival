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
        // [������] ���� ȭ���� ����� �ʰ� �ٷ� ���� ���� ������ �����մϴ�.
        // ShowStartUI();  <-- ���� �ڵ� �ּ� ó�� �Ǵ� ����
        StartGame();    // <-- �ٷ� ���� ����
    }

    // �� �Լ��� ���� ������ ������, ���߿� �Ͻ����� ��� � ��Ȱ���� �� �־� ���ܵξ����ϴ�.
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
        // ���� �� Ŀ���� �ٽ� ���̰� ���� (�ʿ� �� �߰�)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowSuccessUI()
    {
        successUIPanel.SetActive(true);
        // ���� �� Ŀ���� �ٽ� ���̰� ���� (�ʿ� �� �߰�)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UpdateHealth(int current, int max)
    {
        // ü�� UI ������Ʈ ���� (���� �ʿ�)
    }

    public void UpdateAmmo(int current, int max)
    {
        // ź�� UI ������Ʈ ���� (���� �ʿ�)
    }

    public void StartGame()
    {
        // 1) ���� UI�� ���� �ִٸ� ����
        if (startUIPanel != null)
            startUIPanel.SetActive(false);

        // ���� �簳 (�ð� �帧 ����ȭ)
        Time.timeScale = 1f;

        // 2) �ΰ��ӿ� UI Ȱ��ȭ
        if (healthUI != null)
            healthUI.SetActive(true);

        if (ammoUI != null)
            ammoUI.SetActive(true);

        // ���� ���� �÷��� Ȱ��ȭ
        Player.isGameStarted = true;

        // ���콺 Ŀ�� ��ױ� (FPS ���� ��� �ʿ�)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // �÷��̾� ���� Ȱ��ȭ
        Player.isPaused = false;

        // (+) ���� ����Ʈ ������ ����
        // QuestManager�� ���� �����ϴ��� Ȯ�� �� ȣ�� (���� ����)
        if (QuestManager.instance != null)
        {
            QuestManager.instance.StartQuest("q00");
        }
    }
}