using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List 사용을 위해 추가 (PlayerShooting에서 필요)
using System.Linq; // OrderBy를 사용하기 위해 추가

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
    public LayerMask interactableMask; // Inspector에서 'Interactable' 레이어를 선택해야 합니다.

    [Header("필수 연결 요소")]
    [Tooltip("플레이어의 시점을 담당하는 메인 카메라")]
    public Transform playerCamera;

    // --- Private 변수 ---
    private CharacterController controller;
    private Vector3 playerVelocity; // 수직 속도 (점프, 중력)
    private bool groundedPlayer; // 땅에 닿았는지 확인
    private float verticalLookRotation = 0f; // 카메라 상하 회전 값

    // --- 애니메이터 참조 ---
    private Animator animator;

    // --- 상태 변수 (다른 스크립트에서 읽을 수 있도록 public get) ---
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }
    public static bool isPaused = false; // 일시정지 상태 (static으로 선언하여 다른 스크립트에서 접근 가능)


    void Start()
    {
        currentCoins = 0;
        controller = GetComponent<CharacterController>();

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Player의 자식 오브젝트에서 Animator를 찾지 못했습니다. 무기 애니메이션이 작동하지 않습니다.");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        controller.height = standingHeight;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
        if (isPaused) return;

        HandleCrouch();
        HandleMovement();
        HandleLook();
        HandleInteraction();
    }

    // ... (AddCoins, ApplyRecoil, HandleCrouch, CanStandUp 함수는 변경 없음) ...

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

    /// <summary>
    /// 이동 입력을 처리하고 애니메이터 파라미터를 업데이트합니다.
    /// </summary>
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

        // 'isSprinting'은 Shift를 누르고 앞으로 갈 때만 true가 됩니다.
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


        // --- (수정됨) 애니메이터 파라미터 업데이트 (isSprinting 전용) ---
        if (animator != null)
        {
            // 1. 'isSprinting' 파라미터 전달 (달리기 상태)
            //    이 값 하나로만 'Run' 또는 'Idle' 상태를 오고 갑니다.
            animator.SetBool("isSprinting", IsSprinting);

            // 2. (삭제됨) 'isMoving' 파라미터는 더 이상 사용하지 않습니다.
            // bool isMoving = move.magnitude > 0.1f;
            // animator.SetBool("isMoving", isMoving);
        }
        // --- 여기까지 ---
    }

    // ... (HandleLook, TogglePause, ToggleInventory, HandleInteraction 함수는 변경 없음) ...

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
        bool isCurrentlyInInventory = isPaused && InventoryManager.instance.inventoryUIPanel.activeSelf;
        bool isPausedByEscape = isPaused && !InventoryManager.instance.inventoryUIPanel.activeSelf;
        if (!isPausedByEscape)
        {
            bool isOpening = InventoryManager.instance.ToggleInventory();
            isPaused = isOpening;
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
}

