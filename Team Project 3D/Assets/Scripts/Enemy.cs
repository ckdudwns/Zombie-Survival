using UnityEngine;
using System.Collections;
using UnityEngine.AI; // [필수] NavMeshAgent 사용

// NavMeshAgent 컴포넌트가 없으면 자동으로 추가해줍니다.
[RequireComponent(typeof(NavMeshAgent))]
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
    [Tooltip("적의 기본 이동 속도입니다.")]
    public float moveSpeed = 3.5f; // NavMeshAgent에 적용될 기본 속도

    // rotationSpeed는 NavMeshAgent의 Angular Speed를 쓰거나, 공격 시 회전에 사용됩니다.
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

<<<<<<< Updated upstream
=======
    public float frenzyDuration = 30f;

    // [삭제됨] NavMeshAgent가 바닥 높이를 자동 조절하므로 groundLayer 관련 변수는 필요 없습니다.

>>>>>>> Stashed changes
    // --- Private 변수 ---
    private float originalDetectionRange;
    private float originalMoveSpeed;
    private Coroutine frenzyCoroutine;
    private Animator animator;
    private EnemyHealth enemyHealth;
    private NavMeshAgent agent; // [추가] NavMeshAgent 참조

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>(); // 컴포넌트 가져오기

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

<<<<<<< Updated upstream
=======
    private void OnEnable()
    {
        GameEvent.OnUSBPicked += ActivateFrenzyMode;
    }

    private void OnDisable()
    {
        GameEvent.OnUSBPicked -= ActivateFrenzyMode;
    }

>>>>>>> Stashed changes
    void Start()
    {
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        // NavMeshAgent 초기 설정
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange - 0.1f; // 공격 사거리보다 살짝 앞에서 멈추게 설정
        agent.updateRotation = true; // 이동 중 회전은 에이전트에게 맡김

        // 능력치 백업
        originalDetectionRange = detectionRange;
        originalMoveSpeed = moveSpeed;
    }

    void Update()
    {
        if (player == null) return;

<<<<<<< Updated upstream
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 현재 detectionRange를 기준으로 플레이어 감지
=======
        // 게임 일시정지 체크
        if ((DialogueUI.instance != null && DialogueUI.instance.isDialogueOpen) ||
            (InventoryManager.instance != null && InventoryManager.instance.inventoryUIPanel.activeSelf) ||
            Player.isPaused)
        {
            // 일시정지 시 에이전트도 멈춰야 함
            if (agent.enabled) agent.isStopped = true;
            animator.SetBool("isWalking", false);
            return;
        }

>>>>>>> Stashed changes
        if (enemyHealth != null && !enemyHealth.isDeath)
        {
            // 에이전트가 다시 움직일 수 있게 설정
            if (agent.enabled) agent.isStopped = false;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // [변경] ApplyGroundSnapping() 제거됨 (Agent가 처리함)

            if (distanceToPlayer <= detectionRange)
            {
                if (distanceToPlayer > attackRange)
                {
                    MoveTowardsPlayer();
                }
                else
                {
                    // 공격 범위 안이면 이동 멈춤
                    agent.isStopped = true;
                    animator.SetBool("isWalking", false);

                    // 공격할 때는 플레이어를 바라보게 직접 회전 (Agent가 멈춰있으므로)
                    RotateTowardsPlayer();

                    if (canAttack)
                    {
                        StartCoroutine(Attack());
                    }
                }
            }
            else
            {
                // 감지 범위 밖이면 멈춤
                agent.isStopped = true;
                animator.SetBool("isWalking", false);
            }
        }
        else
        {
            // 죽었으면 에이전트 비활성화
            agent.enabled = false;
            enabled = false;
        }
    }

    IEnumerator Attack()
    {
        // Debug.Log(gameObject.name + " 공격!");
        canAttack = false;

        if (animator != null) animator.SetTrigger("attack");
        yield return new WaitForSeconds(0.3f); // 선딜레이

        if (hitbox != null) hitbox.SetActive(true);
        yield return new WaitForSeconds(1.0f); // 히트박스 활성 시간
        if (hitbox != null) hitbox.SetActive(false);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

<<<<<<< Updated upstream
    // --- TrapItem에서 호출할 공개 함수 ---
    public void ActivateFrenzyMode(float duration)
=======
    public void ActivateFrenzyMode()
>>>>>>> Stashed changes
    {
        if (frenzyCoroutine != null)
        {
            StopCoroutine(frenzyCoroutine);
        }
        frenzyCoroutine = StartCoroutine(FrenzyCoroutine(duration));
    }

<<<<<<< Updated upstream
    // --- 효과를 잠시 적용했다가 되돌리는 코루틴 ---
    private IEnumerator FrenzyCoroutine(float duration)
=======
    private IEnumerator FrenzyCoroutine()
>>>>>>> Stashed changes
    {
        Debug.Log(gameObject.name + "가 광분 상태에 돌입!");

        // [변경] 단순 변수뿐만 아니라 실제 에이전트 속도도 변경해야 함
        detectionRange = frenzyDetectionRange;
        agent.speed = frenzyMoveSpeed;

<<<<<<< Updated upstream
        // 효과 지속 시간만큼 대기
        yield return new WaitForSeconds(duration);
=======
        yield return new WaitForSeconds(frenzyDuration);
>>>>>>> Stashed changes

        Debug.Log(gameObject.name + "의 광분 상태가 해제됩니다.");

        // [변경] 원래대로 복구
        detectionRange = originalDetectionRange;
        agent.speed = originalMoveSpeed;

        frenzyCoroutine = null;
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true);

        // [변경] 직접 Translate 하는 대신 목적지만 설정하면 알아서 이동함
        agent.SetDestination(player.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}