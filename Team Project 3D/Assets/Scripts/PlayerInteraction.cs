using UnityEngine;
using UnityEngine.UI; // (선택적) 상호작용 UI를 위해 추가

public class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용 설정")]
    [Tooltip("상호작용이 가능한 최대 거리")]
    public float interactionDistance = 3f;
    [Tooltip("광선을 쏠 플레이어의 메인 카메라")]
    public Camera playerCamera;

    [Header("UI 설정 (선택 사항)")]
    [Tooltip("[E] 상호작용 텍스트나 이미지를 담은 UI 오브젝트")]
    public GameObject interactionPromptUI; // "E키를 누르세요" 같은 UI

    private IInteractable currentInteractable; // 현재 바라보고 있는 상호작용 가능한 오브젝트

    void Start()
    {
        // 카메라가 할당되지 않았다면 자동으로 MainCamera를 찾음
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // 상호작용 UI가 있다면 시작할 때 숨김
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    void Update()
    {
        // 1. 매 프레임마다 바라보는 오브젝트 확인
        CheckForInteractable();

        // 2. E키를 눌렀을 때
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 3. 바라보고 있는 오브젝트가 상호작용 가능하다면
            if (currentInteractable != null)
            {
                // 4. 해당 오브젝트의 Interact() 함수 실행!
                // (this.gameObject는 플레이어 오브젝트 자신을 의미)
                currentInteractable.Interact(this.gameObject);
            }
        }
    }

    /// <summary>
    /// 카메라 정면으로 레이캐스트를 쏴서 IInteractable을 찾습니다.
    /// </summary>
    void CheckForInteractable()
    {
        if (playerCamera == null) return;

        RaycastHit hit;
        // 카메라 위치에서 카메라 정면 방향으로 레이(광선) 생성
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        IInteractable interactable = null;

        // interactionDistance 거리만큼 레이캐스트 발사
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // 1. 맞은 오브젝트에서 IInteractable 컴포넌트(규칙)를 찾음
            interactable = hit.collider.GetComponent<IInteractable>();
        }

        // 2. 바라보는 대상이 바뀌었는지 확인 (새로 찾았거나, 시야에서 놓쳤거나)
        if (currentInteractable != interactable)
        {
            currentInteractable = interactable;

            // 3. UI 업데이트 (선택 사항)
            if (interactionPromptUI != null)
            {
                // 상호작용 가능하면 UI를 켜고, 아니면 끈다
                interactionPromptUI.SetActive(currentInteractable != null);
            }
        }
    }
}