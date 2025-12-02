using UnityEngine;

// [★추가] 오디오 소스 컴포넌트 자동 추가
[RequireComponent(typeof(AudioSource))]
public class TwoWayDoor1 : MonoBehaviour
{
    public Transform destination;
    public TwoWayDoor1 linkedDoor;

    [Header("잠금 설정")]
    public bool isLocked = true; // 기본적으로 잠겨있음
    private bool canTeleport = true;

    // ▼ [★추가] 사운드 설정
    [Header("Sound Settings")]
    public AudioClip lockedSound; // 잠겨있을 때 나는 소리 (철컥)
    public AudioClip openSound;   // 문 열릴 때/이동 소리 (끼익 or 슉)

    private AudioSource audioSource; // 소리 재생기

    void Start()
    {
        // [★추가] AudioSource 가져오기
        audioSource = GetComponent<AudioSource>();
    }

    // 외부(패널)에서 호출하여 문을 잠금 해제하는 함수
    public void UnlockDoor()
    {
        isLocked = false;
        Debug.Log("문 잠금이 해제되었습니다! 이제 이동할 수 있습니다.");
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. 문이 잠겨있는 경우
        if (isLocked)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("문이 잠겨있습니다. 카드키가 필요합니다.");

                // [★추가] 잠긴 소리 재생
                if (audioSource != null && lockedSound != null)
                {
                    audioSource.PlayOneShot(lockedSound);
                }
            }
            return;
        }

        // 2. 이동 가능한 경우
        if (other.CompareTag("Player") && canTeleport)
        {
            if (destination == null)
            {
                Debug.LogError("도착 지점이 설정되지 않았습니다!");
                return;
            }

            // [★추가] 이동 소리 재생
            if (audioSource != null && openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }

            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                other.transform.position = destination.position;
                cc.enabled = true;
            }
            else
            {
                other.transform.position = destination.position;
            }

            Debug.Log("텔레포트 완료!");

            StartCoroutine(TeleportCooldown());
            if (linkedDoor != null)
            {
                linkedDoor.StartCoroutine(linkedDoor.TeleportCooldown());
            }
        }
    }

    public System.Collections.IEnumerator TeleportCooldown()
    {
        canTeleport = false;
        yield return new WaitForSeconds(1f);
        canTeleport = true;
    }
}