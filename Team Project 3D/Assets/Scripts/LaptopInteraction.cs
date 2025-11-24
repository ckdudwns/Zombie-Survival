using UnityEngine;
using System.Collections; // IEnumerator 사용을 위해 필수

public class LaptopInteraction : MonoBehaviour, IInteractable
{
    [Header("설정")]
    public string requiredItemName = "USBKey_Metal"; // 필요한 아이템 이름
    public float downloadTime = 30f; // 다운로드 소요 시간 (30초)

    private bool isDownloading = false; // 현재 다운로드 중인지 확인
    private bool isDownloadComplete = false; // 다운로드가 이미 끝났는지 확인

    public void Interact(GameObject player)
    {
        // 1. 이미 다운로드가 끝났다면 더 이상 반응하지 않음 (선택 사항)
        if (isDownloadComplete)
        {
            Debug.Log("이미 다운로드가 완료된 단말기입니다.");
            return;
        }

        // 2. 다운로드 중이라면 중복 실행 방지
        if (isDownloading)
        {
            Debug.Log("다운로드가 진행 중입니다... 잠시만 기다려주세요.");
            return;
        }

        // 3. 아이템 보유 여부 확인
        if (InventoryManager.instance.HasItem(requiredItemName))
        {
            // 코루틴 시작 (시간 지연 로직)
            StartCoroutine(ProcessDownload());
        }
        else
        {
            Debug.Log("USB 키가 필요합니다.");
        }
    }

    // 시간 지연을 처리하는 코루틴 함수
    IEnumerator ProcessDownload()
    {
        isDownloading = true;

        // 시작 로그
        Debug.Log("다운로드가 시작됩니다...");

        // 30초 대기 (이 코드에서 30초 동안 멈춰있다가 다음 줄로 넘어감)
        yield return new WaitForSeconds(downloadTime);

        // 완료 로그
        Debug.Log("다운로드가 완료되었습니다!");

        isDownloading = false;
        isDownloadComplete = true; // 완료 상태로 변경
    }
}