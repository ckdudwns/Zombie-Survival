using UnityEngine;
using System.Collections;

public class SimpleDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("문이 열렸을 때의 Y축 회전 각도 (예: 90)")]
    public float openAngle = 90f;

    [Tooltip("문이 닫혔을 때의 Y축 회전 각도 (예: 0)")]
    public float closedAngle = 0f;

    [Tooltip("문이 회전하는 속도 (클수록 빠름)")]
    public float rotationSpeed = 2f;

    [Header("State")]
    public bool isDoorOpen = false;
    private bool isMoving = false;

    private Coroutine movementCoroutine;

    // 외부에서 문 열림/닫힘을 토글할 때 호출하는 함수
    public void ToggleDoor()
    {
        if (isMoving) return;

        if (isDoorOpen)
        {
            StartMoving(closedAngle, false);
        }
        else
        {
            StartMoving(openAngle, true);
        }
    }

    private void StartMoving(float targetAngle, bool targetOpenState)
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        movementCoroutine = StartCoroutine(MoveDoorCoroutine(targetAngle, targetOpenState));
    }

    // 부드러운 움직임을 위한 코루틴
    private IEnumerator MoveDoorCoroutine(float targetAngle, bool targetOpenState)
    {
        isMoving = true;

        // 목표 Quaternion (회전 값) 생성
        Quaternion targetRotation = Quaternion.Euler(
            transform.localRotation.eulerAngles.x,
            targetAngle,
            transform.localRotation.eulerAngles.z
        );

        // 현재 회전 값이 목표 회전 값과 충분히 가까워질 때까지 반복
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            // 스크립트가 붙은 오브젝트(DoorRoot)의 Transform을 회전시킵니다.
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );

            yield return null;
        }

        // 회전 완료 후 정확한 값으로 설정
        transform.localRotation = targetRotation;

        isDoorOpen = targetOpenState;
        isMoving = false;
        movementCoroutine = null;
    }
}