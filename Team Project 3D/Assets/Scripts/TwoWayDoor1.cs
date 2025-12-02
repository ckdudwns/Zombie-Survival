using UnityEngine;

public class TwoWayDoor1 : MonoBehaviour
{
    public Transform destination;
    public TwoWayDoor1 linkedDoor;

    [Header("잠금 설정")]
    public bool isLocked = true; // 기본적으로 잠겨있음
    private bool canTeleport = true;

    // 외부(패널)에서 호출하여 문을 잠금 해제하는 함수
    public void UnlockDoor()
    {
        isLocked = false;
        Debug.Log("문 잠금이 해제되었습니다! 이제 이동할 수 있습니다.");
    }

    void OnTriggerEnter(Collider other)
    {
        // 문이 잠겨있으면 이동 불가
        if (isLocked)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("문이 잠겨있습니다. 카드키가 필요합니다.");
            }
            return;
        }

        if (other.CompareTag("Player") && canTeleport)
        {
            if (destination == null)
            {
                Debug.LogError("도착 지점이 설정되지 않았습니다!");
                return;
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