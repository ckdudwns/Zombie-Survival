using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("능력치 설정")]
    public int currentCoins; // 현재 코인 (public으로 변경하여 Inspector에서 확인 가능)

    [Header("이동 속도")]
    public float moveSpeed = 6.0f;
    public float sprintSpeed = 10.0f;

    [Header("앉기 설정")]
    public float crouchSpeed = 3.0f;
    public float standingHeight = 2.0f;
    public float crouchingHeight = 1.0f;

    [Header("점프 높이")]
    public float jumpHeight = 1.0f;
    [Header("중력 값")]
    public float gravityValue = -9.81f;

    [Header("마우스 감도")]
    public float mouseSensitivity = 2.0f;

    [Header("카메라 오브젝트")]
    public Transform playerCamera;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float verticalLookRotation = 0f;

    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }
    public static bool isPaused = false;

    void Start()
    {
        // --- 1. 시작 시 코인 설정 및 로그 출력 ---
        currentCoins = 1000;
        // 현재 보유 코인을 로그로 출력
        Debug.Log("게임 시작! 현재 보유 코인: " + currentCoins);
        // --- 여기까지 ---

        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        controller.height = standingHeight;
    }

    // 코인을 추가하고 로그를 출력하는 함수
    public void AddCoins(int amount)
    {
        currentCoins += amount;
        // --- 2. 코인 획득 시 로그 출력 ---
        // 획득한 코인과 함께 현재 총 보유 코인을 로그로 출력
        Debug.Log(amount + " 코인 획득! 현재 보유 코인: " + currentCoins);
        // --- 여기까지 ---
    }

    // --- 이하 코드는 이전과 동일합니다 ---
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
        if (isPaused) return;
        HandleCrouch();
        HandleMovement();
        HandleLook();
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
        if (groundedPlayer && playerVelocity.y < 0) playerVelocity.y = -2f;

        IsSprinting = Input.GetKey(KeyCode.LeftShift) && !IsCrouching;

        float currentSpeed = moveSpeed;
        if (IsCrouching) currentSpeed = crouchSpeed;
        else if (IsSprinting) currentSpeed = sprintSpeed;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
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
    }
}