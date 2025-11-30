using UnityEngine;

[RequireComponent(typeof(Camera))] // 이 스크립트는 카메라에만 붙일 수 있음
public class MapFollow : MonoBehaviour
{
    [Header("추적 대상")]
    public Transform player; // 플레이어

    [Header("줌 설정")]
    public float zoomSpeed = 5.0f;   // 휠 감도
    public float minZoom = 5.0f;     // 최대 확대 (작을수록 확대됨)
    public float maxZoom = 20.0f;    // 최대 축소 (클수록 멀리 보임)

    private Camera mapCamera;

    void Start()
    {
        mapCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // 1. 플레이어 따라다니기 (기존 기능)
            Vector3 newPosition = player.position;
            newPosition.y = transform.position.y;
            transform.position = newPosition;

            // 2. [추가됨] 마우스 휠로 확대/축소
            // 스마트폰이 켜져있을 때만 작동하도록 조건 추가 가능 (여기선 항상 작동)
            HandleZoom();
        }
    }

    void HandleZoom()
    {
        // 마우스 휠 입력 받기 (위로 굴리면 양수, 아래로 굴리면 음수)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            // Orthographic Size 조절 (이 값이 작아지면 확대, 커지면 축소)
            mapCamera.orthographicSize -= scrollInput * zoomSpeed;

            // 너무 작아지거나 커지지 않게 제한(Clamp)
            mapCamera.orthographicSize = Mathf.Clamp(mapCamera.orthographicSize, minZoom, maxZoom);
        }
    }
}