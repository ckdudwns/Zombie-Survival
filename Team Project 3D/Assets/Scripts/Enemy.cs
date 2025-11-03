using UnityEngine;
using System.Collections;

// 이 스크립트는 적(Enemy) 오브젝트에 추가합니다.
public class Enemy : MonoBehaviour
{
    [Header("타겟 설정")]
    [Tooltip("적이 추적할 대상입니다. 보통 플레이어를 연결합니다.")]
    public Transform player;
    [Tooltip("공격 판정을 위한 히트박스 오브젝트를 연결합니다.")]
    public GameObject hitbox;

    [Header("AI 행동 설정")]
    [Tooltip("플레이어를 감지하는 최대 거리입니다.")]
    public float detectionRange = 15f;
    [Tooltip("이 거리 안으로 들어오면 이동을 멈추고 공격을 시작합니다.")]
    public float attackRange = 1.5f;
    [Tooltip("적의 이동 속도입니다.")]
    public float moveSpeed = 3.5f;
    [Tooltip("적이 플레이어를 향해 회전하는 속도입니다.")]
    public float rotationSpeed = 10f;

    [Header("공격 설정")]
    [Tooltip("공격 후 다음 공격까지의 대기 시간(쿨타임)입니다.")]
    public float attackCooldown = 2f;
    private bool canAttack = true;

    [Header("함정 효과 설정 (Frenzy)")]
    [Tooltip("함정 발동 시 증가할 탐지 범위")]
    public float frenzyDetectionRange = 30f;
    [Tooltip("함정 발동 시 증가할 이동 속도")]
    public float frenzyMoveSpeed = 7f;

    // --- Private 변수 ---
    private float originalDetectionRange;
    private float originalMoveSpeed;
    private Coroutine frenzyCoroutine;

    void Awake()
    {
        if (hitbox != null)
        {
            hitbox.SetActive(false);
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    void Start()
    {
        // 시작할 때 원래의 능력치를 저장해 둡니다.
        originalDetectionRange = detectionRange;
        originalMoveSpeed = moveSpeed;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 현재 detectionRange를 기준으로 플레이어 감지
        if (distanceToPlayer <= detectionRange)
        {
            RotateTowardsPlayer();

            if (distanceToPlayer > attackRange)
            {
                MoveTowardsPlayer();
            }
            else
            {
                if (canAttack)
                {
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator Attack()
    {
        Debug.Log(gameObject.name + " 공격!");
        canAttack = false;

        yield return new WaitForSeconds(0.5f); // 선딜레이

        if (hitbox != null) hitbox.SetActive(true);
        yield return new WaitForSeconds(0.2f); // 히트박스 활성 시간
        if (hitbox != null) hitbox.SetActive(false);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // --- TrapItem에서 호출할 공개 함수 ---
    public void ActivateFrenzyMode(float duration)
    {
        // 이미 다른 Frenzy 효과가 진행 중이라면 중지하고 새로 시작 (효과 시간 갱신)
        if (frenzyCoroutine != null)
        {
            StopCoroutine(frenzyCoroutine);
        }
        frenzyCoroutine = StartCoroutine(FrenzyCoroutine(duration));
    }

    // --- 효과를 잠시 적용했다가 되돌리는 코루틴 ---
    private IEnumerator FrenzyCoroutine(float duration)
    {
        Debug.Log(gameObject.name + "가 광분 상태에 돌입!");

        // 능력치를 강화된 값으로 변경
        detectionRange = frenzyDetectionRange;
        moveSpeed = frenzyMoveSpeed;

        // 효과 지속 시간만큼 대기
        yield return new WaitForSeconds(duration);

        Debug.Log(gameObject.name + "의 광분 상태가 해제됩니다.");

        // 능력치를 원래 값으로 되돌림
        detectionRange = originalDetectionRange;
        moveSpeed = originalMoveSpeed;

        frenzyCoroutine = null; // 코루틴이 끝났음을 표시
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void MoveTowardsPlayer()
    {
        // 현재 moveSpeed를 사용하므로, 광분 상태일 때는 자동으로 frenzyMoveSpeed가 적용됨
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        // 현재 detectionRange를 기준으로 기즈모를 그림 (광분 상태일 때 커짐)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}