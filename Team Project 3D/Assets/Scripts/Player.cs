using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("능력치 설정")]
    [Tooltip("현재 보유한 코인")]
    public int currentCoins;

    [Header("이동 속도")]
    [Tooltip("기본 걷기 속도")]
    public float moveSpeed = 6.0f;
    [Tooltip("Shift 키를 눌렀을 때의 달리기 속도")]
    public float sprintSpeed = 10.0f;
    [Tooltip("Ctrl 키를 눌러 앉았을 때의 이동 속도")]
    public float crouchSpeed = 3.0f;

    [Header("앉기 설정")]
    [Tooltip("서 있을 때의 캐릭터 컨트롤러 높이")]
    public float standingHeight = 2.0f;
    [Tooltip("앉았을 때의 캐릭터 컨트롤러 높이")]
    public float crouchingHeight = 1.0f;

    [Header("점프 높이")]
    [Tooltip("점프 시 도달하는 최대 높이")]
    public float jumpHeight = 1.0f;
    [Header("중력 값")]
    public float gravityValue = -9.81f;

    [Header("마우스 감도")]
    public float mouseSensitivity = 2.0f;

    [Header("상호작용 설정")]
    [Tooltip("아이템을 줍거나 상호작용할 수 있는 최대 거리")]
    public float pickupRange = 3f;
    [Tooltip("상호작용 가능한 대상을 감지할 레이어 마스크")]
    public LayerMask interactableMask;

    [Header("필수 연결 요소")]
    [Tooltip("플레이어의 시점을 담당하는 메인 카메라")]
    public Transform playerCamera;

    // --- [중요] 스마트폰 설정 ---
    [Header("스마트폰 설정")]
    [Tooltip("MainCamera의 자식으로 있는 휴대폰 오브젝트 (UI 포함)")]
    public GameObject handHeldPhone;

    // (hasSmartphone 변수는 제거했습니다. 이제 InventoryManager를 직접 확인합니다.)
    private bool isPhoneActive = false; // 폰 화면 켜짐 여부

    [Header("UI 설정")]
    [Tooltip("평소에 떠있는 조준점 UI (Crosshair_Shotgun)")]
    public GameObject crosshairUI; // 👈 여기에 Crosshair_Shotgun을 넣을 겁니다.

    [Tooltip("총을 들고있는 모습")]
    public GameObject GunHolder;

    // --- Private 변수 ---
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float verticalLookRotation = 0f;

    // --- 애니메이터 참조 ---
    private Animator animator;

    // --- 상태 변수 ---
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }
    public static bool isPaused = false;

    void Start()
    {
        currentCoins = 0;
        controller = GetComponent<CharacterController>();

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Player의 자식 오브젝트에서 Animator를 찾지 못했습니다.");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        controller.height = standingHeight;

        // 시작할 때 손에 든 폰은 꺼두기
        if (handHeldPhone != null)
            handHeldPhone.SetActive(false);
    }

    void Update()
    {
        // 1. ESC 키 입력 (일시정지 또는 폰 끄기)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPhoneActive)
            {
                ToggleSmartPhone(); // 폰이 켜져있으면 폰만 끔
            }
            else
            {
                TogglePause(); // 아니면 게임 일시정지
            }
        }
        // 2. I 키 입력 (인벤토리)
        else if (Input.GetKeyDown(KeyCode.I))
        {
            // 폰이 꺼져있을 때만 인벤토리 열기
            if (!isPhoneActive)
            {
                ToggleInventory();
            }
        }
        // 3. P 키 입력 (스마트폰)
        else if (Input.GetKeyDown(KeyCode.P))
        {
            // [핵심 수정] 인벤토리에 "Phone"이라는 이름의 아이템이 있는지 검사
            if (InventoryManager.instance != null && InventoryManager.instance.HasItem("Phone"))
            {
                // 인벤토리가 닫혀있을 때만 폰 열기
                if (!InventoryManager.instance.inventoryUIPanel.activeSelf)
                {
                    ToggleSmartPhone();
                }
            }
            else
            {
                Debug.Log("인벤토리에 'Phone' 아이템이 없습니다.");
            }
        }

        // 일시정지 상태면 움직임/회전/상호작용 중단
        if (isPaused) return;

        HandleCrouch();
        HandleMovement();
        HandleLook();
        HandleInteraction();
    }

    // ... (기존 함수들 유지) ...

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        Debug.Log(amount + " 코인 획득! 현재 보유 코인: " + currentCoins);
    }

    public void ApplyRecoil(float verticalRecoil)
    {
        verticalLookRotation -= verticalRecoil;
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (!IsCrouching)
            {
                controller.height = crouchingHeight;
                IsCrouching = true;
            }
            else
            {
                if (CanStandUp())
                {
                    controller.height = standingHeight;
                    IsCrouching = false;
                }
            }
        }
    }

    bool CanStandUp()
    {
        return !Physics.Raycast(transform.position, Vector3.up, standingHeight);
    }

    void HandleMovement()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        IsSprinting = Input.GetKey(KeyCode.LeftShift) && !IsCrouching && vertical > 0;

        float currentSpeed = moveSpeed;
        if (IsCrouching) currentSpeed = crouchSpeed;
        else if (IsSprinting) currentSpeed = sprintSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && groundedPlayer && !IsCrouching)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;

        if (InventoryManager.instance != null && InventoryManager.instance.inventoryUIPanel.activeSelf)
        {
            InventoryManager.instance.inventoryUIPanel.SetActive(false);
        }
    }

    void ToggleInventory()
    {
        if (InventoryManager.instance == null)
        {
            Debug.LogError("InventoryManager가 씬에 없습니다!");
            return;
        }

        bool isPausedByEscape = isPaused && !InventoryManager.instance.inventoryUIPanel.activeSelf;

        if (!isPausedByEscape)
        {
            bool isOpening = InventoryManager.instance.ToggleInventory();
            isPaused = isOpening; // 인벤토리 열리면 게임 일시정지

            Time.timeScale = isPaused ? 0f : 1f;
            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isPaused;
        }
        else
        {
            Debug.Log("게임이 일시정지(Esc) 중이라 인벤토리를 열 수 없습니다.");
        }
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, pickupRange, interactableMask))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(this.gameObject);
                }
            }
        }
    }

    // --- [스마트폰 기능] ---
    void ToggleSmartPhone()
    {
        isPhoneActive = !isPhoneActive;

        // 1. 폰 오브젝트 켜기/끄기
        if (handHeldPhone != null)
        {
            handHeldPhone.SetActive(isPhoneActive);
        }

        if (crosshairUI != null)
        {
            crosshairUI.SetActive(!isPhoneActive);
        }

        if (GunHolder != null)
        {
            GunHolder.SetActive(!isPhoneActive);
        }

        // 2. 폰이 켜지면 일시정지 상태로 취급 (이동 멈춤)
        isPaused = isPhoneActive;

        // 3. 마우스 커서 및 시간 설정
        if (isPhoneActive)
        {
            Cursor.lockState = CursorLockMode.None; // 커서 잠금 해제
            Cursor.visible = true;                  // 커서 보이기
            Time.timeScale = 0f;                    // 게임 시간 정지 (원치 않으면 1f로)
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // 커서 다시 잠금
            Cursor.visible = false;                   // 커서 숨기기
            Time.timeScale = 1f;                      // 게임 시간 정상화
        }
    }
}