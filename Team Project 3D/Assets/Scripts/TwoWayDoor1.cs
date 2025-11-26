using UnityEngine;

public class TwoWayDoor1 : MonoBehaviour
{
    public Transform destination;
    public TwoWayDoor1 linkedDoor; // 연결된 반대편 문
    private bool canTeleport = true;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canTeleport)
        {
            if (destination == null)
            {
                Debug.LogError("문이 잠겨있다!");
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
        yield return new WaitForSeconds(1f); // 1초로 늘림
        canTeleport = true;
        Debug.Log("쿨다운 끝!");
    }
}