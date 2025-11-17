using UnityEngine;

public class PlayerInteraction1 : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            float rayDistance = 2f; // 2미터

            // 1. Raycast 발사 및 시각화 (선이 보이거나 색이 바뀌는지 확인)
            Debug.DrawRay(transform.position, transform.forward * rayDistance, Color.red, 1.0f);

            if (Physics.Raycast(transform.position, transform.forward, out hit, rayDistance))
            {
                // 충돌 감지 시 초록색으로 변경
                Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.green, 1.0f);

                // 2. 어떤 오브젝트에 맞았는지 출력
                Debug.Log($"Raycast Hit: {hit.collider.gameObject.name}");

                SimpleDoor door = hit.collider.GetComponent<SimpleDoor>();

                // 부모에서 다시 찾기 (만약 문 모델에만 콜라이더가 있고 스크립트는 부모에 있다면)
                if (door == null && hit.collider.transform.parent != null)
                {
                    door = hit.collider.transform.parent.GetComponent<SimpleDoor>();
                }

                if (door != null)
                {
                    // 3. SimpleDoor 컴포넌트를 찾았는지 출력 (가장 중요)
                    Debug.Log("SimpleDoor Found! Toggling Door.");
                    door.ToggleDoor();
                }
                else
                {
                    Debug.Log("Hit object, but SimpleDoor component not found.");
                }
            }
            else
            {
                Debug.Log("Raycast Missed.");
            }
        }
    }
}