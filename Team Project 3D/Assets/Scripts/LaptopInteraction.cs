using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI; // [필수] UI 컴포넌트 사용을 위해 추가

public class LaptopInteraction : MonoBehaviour, IInteractable
{
    [Header("설정")]
    public string requiredItemName = "USBKey_Metal";
    public float downloadTime = 30f;

    [Header("UI 설정")]
    // [추가] 다운로드 UI 전체를 감싸는 부모 오브젝트 (평소엔 꺼두기 위함)
    public GameObject downloadUIPanel;
    // [추가] 진행률을 보여줄 슬라이더
    public Slider progressSlider;

    [Header("이벤트")]
    public UnityEvent onDownloadComplete;

    private bool isDownloading = false;
    private bool isDownloadComplete = false;

    private void Start()
    {
        // 시작 시 UI 숨기기
        if (downloadUIPanel != null)
            downloadUIPanel.SetActive(false);
    }

    public void Interact(GameObject player)
    {
        if (isDownloadComplete)
        {
            Debug.Log("이미 다운로드가 완료된 단말기입니다.");
            return;
        }

        if (isDownloading)
        {
            Debug.Log("다운로드가 진행 중입니다...");
            return;
        }

        if (InventoryManager.instance.HasItem(requiredItemName))
        {
            StartCoroutine(ProcessDownload());
        }
        else
        {
            Debug.Log("USB 키가 필요합니다.");
        }
    }

    IEnumerator ProcessDownload()
    {
        isDownloading = true;
        Debug.Log("다운로드가 시작됩니다...");

        // 1. UI 켜기 및 초기화
        if (downloadUIPanel != null)
        {
            downloadUIPanel.SetActive(true);
            progressSlider.value = 0f; // 슬라이더 0으로 시작
        }

        // 2. 시간 진행에 따른 UI 업데이트 루프
        float currentTimer = 0f;

        while (currentTimer < downloadTime)
        {
            currentTimer += Time.deltaTime; // 흐른 시간 더하기

            // 진행률 계산 (0.0 ~ 1.0)
            float progress = Mathf.Clamp01(currentTimer / downloadTime);

            // 슬라이더 업데이트
            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }

            yield return null; // 다음 프레임까지 대기
        }

        // 3. 다운로드 완료 처리
        Debug.Log("다운로드 완료!");

        // UI 끄기 (원한다면 잠시 100%를 보여주고 끄도록 딜레이를 줄 수도 있음)
        if (downloadUIPanel != null)
            downloadUIPanel.SetActive(false);

        isDownloading = false;
        isDownloadComplete = true;

        onDownloadComplete.Invoke();
    }
}