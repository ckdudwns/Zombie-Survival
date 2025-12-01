using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI instance;

    [Header("UI Components")]
    public GameObject panel;     // 대사 패널
    public TMP_Text text;        // TextMeshPro 텍스트

    private string[] lines;       // 현재 대사 라인들
    private int index;            // 현재 라인 인덱스
    private System.Action onFinished; // 대사 종료 콜백

    [Header("State")]
    public bool isDialogueOpen = false;
    public bool isFinished = false;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 대사 시작
    /// </summary>
    public void ShowDialogue(string[] dialogueLines, System.Action finishedCallback = null)
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        lines = dialogueLines;
        index = 0;
        onFinished = finishedCallback;

        panel.SetActive(true);
        isDialogueOpen = true;
        isFinished = false;

        text.text = lines[index];

        // 시간 멈추기
        Player.isPaused = true;
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (!panel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            index++;

            if (index < lines.Length)
            {
                text.text = lines[index];
                return;
            }

            // 대사 끝나면 패널 닫기 코루틴 실행
            StartCoroutine(CloseDialogue());
        }
    }

    /// <summary>
    /// 대사 종료 처리
    /// </summary>
    private IEnumerator CloseDialogue()
    {
        // 스페이스 키 계속 누르고 있으면 기다리기
        yield return null;
        while (Input.GetKey(KeyCode.Space))
            yield return null;

        panel.SetActive(false);
        isDialogueOpen = false;
        isFinished = true;

        // 시간 재개
        Player.isPaused = false;
        Time.timeScale = 1f;

        // 콜백 호출: 여기서 QuestManager에서 quest.dialogueShown = true 처리 가능
        yield return null;
        onFinished?.Invoke();
    }
}