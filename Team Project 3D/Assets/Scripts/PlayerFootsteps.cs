using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    public CharacterController characterController;
    private AudioSource audioSource;

    [Header("발자국 소리 파일")]
    public AudioClip[] footstepSounds;

    [Header("이동 설정")]
    public float walkStepInterval = 0.5f; // 걷는 소리 간격
    public float runStepInterval = 0.3f;  // 뛰는 소리 간격
    public float runThreshold = 5.0f;     // 뛰기 기준 속도

    [Header("소리 크기 조절")]
    [Range(0f, 1f)] public float minVolume = 0.3f; // 최소 볼륨 (슬라이더)
    [Range(0f, 1f)] public float maxVolume = 0.5f; // 최대 볼륨 (슬라이더)

    [Header("판정 보정 (중요)")]
    public float groundCheckDistance = 1.2f; // 캐릭터 키에 맞춰 조절 (보통 1.1 ~ 1.3)
    public LayerMask groundLayer;            // 바닥 레이어

    private float stepTimer;
    private Vector3 lastPosition;
    private float currentSpeed;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (characterController == null) characterController = GetComponent<CharacterController>();

        // 초기화
        lastPosition = transform.position;
        if (groundLayer == 0) groundLayer = ~0; // 레이어 설정 없으면 Everything
    }

    void Update()
    {
        // 1. 실제 이동 속도 계산 (수평 이동만)
        Vector3 movement = transform.position - lastPosition;
        movement.y = 0;
        currentSpeed = movement.magnitude / Time.deltaTime;

        lastPosition = transform.position; // 위치 갱신

        // 2. 바닥 체크 (Raycast 사용 - 점프/착지 문제 해결용)
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // 디버그용 (씬 뷰에서 초록색 선이 땅에 닿아야 함)
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

        // 3. 소리 재생 로직
        // 땅에 있고 + 속도가 0.1보다 빠를 때
        if (isGrounded && currentSpeed > 0.1f)
        {
            float currentInterval = (currentSpeed > runThreshold) ? runStepInterval : walkStepInterval;

            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0)
            {
                PlayFootstep();
                stepTimer = currentInterval;
            }
        }
        else
        {
            // 멈추거나 공중에 있으면 타이머 0으로 (착지하자마자 소리나게)
            stepTimer = 0;
        }
    }

    void PlayFootstep()
    {
        if (footstepSounds.Length == 0) return;

        int randomIndex = Random.Range(0, footstepSounds.Length);

        // [설정된 볼륨 범위 내에서 랜덤 재생]
        audioSource.volume = Random.Range(minVolume, maxVolume);

        // 톤을 살짝 바꿔서 자연스럽게
        audioSource.pitch = Random.Range(0.9f, 1.1f);

        audioSource.PlayOneShot(footstepSounds[randomIndex]);
    }
}