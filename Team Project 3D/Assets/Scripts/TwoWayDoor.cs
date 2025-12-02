using UnityEngine;

// [★추가] 이 스크립트를 넣으면 자동으로 AudioSource 컴포넌트가 추가됩니다.
[RequireComponent(typeof(AudioSource))]
public class TwoWayDoor : MonoBehaviour
{
    public Transform destination;
    public TwoWayDoor linkedDoor; // 연결된 반대편 문

    [Header("Sound Settings")]
    public AudioClip lockedSound; // [★추가] 잠겨있을 때 나는 소리 (철컥)
    public AudioClip openSound;   // [★추가] 문 열릴 때/텔레포트 소리 (끼익 or 슉)

    private bool canTeleport = true;
    private AudioSource audioSource; // [★추가] 소리 재생기

    void Start()
    {
        // [★추가] 오디오 소스 가져오기
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canTeleport)
        {
            // 1. 문이 잠겨있는 경우 (목적지가 없음)
            if (destination == null)
            {
                Debug.LogError("문이 잠겨있다!");

                // [★추가] 잠긴 소리 재생
                if (audioSource != null && lockedSound != null)
                {
                    audioSource.PlayOneShot(lockedSound);
                }
                return;
            }

            // 2. 텔레포트 성공 (문 열림)
            // [★추가] 문 열리는 소리 재생
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

            // 양쪽 문 모두 쿨다운 시작
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
        Debug.Log("쿨다운 끝!");
    }
}