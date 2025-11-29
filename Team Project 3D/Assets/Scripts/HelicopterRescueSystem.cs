using UnityEngine;
using System.Collections;

public class HelicopterRescueSystem : MonoBehaviour
{
    [Header("필수 연결")]
    public Transform helicopter;
    public Transform player;

    [Header("프로펠러")]
    public Transform propeller;
    public float propellerSpeed = 2000f;

    [Header("타이밍 설정")]
    public float arrivalTime = 300f;  // 5분 = 300초
    public bool useTestTime = true;   // 테스트용
    public float testTime = 10f;      // 테스트: 10초

    [Header("비행 설정")]
    public float flySpeed = 15f;
    public float descendSpeed = 3f;
    public float flyHeight = 25f;
    public float hoverHeight = 3f;
    public float startDistance = 100f;

    [Header("상호작용 설정")]
    public float interactionDistance = 5f;
    public KeyCode interactKey = KeyCode.E;

    [Header("엔딩 설정")]
    public string endingMessage = "🎉 탈출 성공! ENDING";
    public GameObject endingUI;

    [Header("사운드 (선택)")]
    public AudioClip helicopterSound;
    public AudioClip landingSound;
    public AudioClip escapeSound;

    private AudioSource audioSource;
    private bool isCalled = false;
    private bool hasArrived = false;
    private bool isPlayerNearby = false;
    private bool missionComplete = false;
    private float callTime;
    private float remainingTime;
    private Vector3 landingPosition;
    private bool hasLandingPosition = false;

    private enum HelicopterState
    {
        Waiting,
        Approaching,
        Landing,
        WaitingForPlayer,
        Escaping,
        MissionComplete
    }

    private HelicopterState currentState = HelicopterState.Waiting;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (helicopter != null)
        {
            helicopter.position = new Vector3(0, -1000, 0);
            helicopter.gameObject.SetActive(false);
        }

        Debug.Log("🚁 헬리콥터 대기 중... 플레어를 발사하세요!");
    }

    void Update()
    {
        // 프로펠러 회전
        if (propeller != null && helicopter != null && helicopter.gameObject.activeSelf)
        {
            propeller.Rotate(Vector3.up, propellerSpeed * Time.deltaTime);
        }

        // 타이머
        if (isCalled && !hasArrived)
        {
            remainingTime = (useTestTime ? testTime : arrivalTime) - (Time.time - callTime);

            if (remainingTime <= 0 && !hasArrived)
            {
                hasArrived = true;
                StartCoroutine(HelicopterArrival());
            }

            // 타이머 표시 (T키)
            if (Input.GetKeyDown(KeyCode.T))
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                Debug.Log($"⏱️ 헬리콥터 도착까지: {minutes}분 {seconds}초");
            }
        }

        // 상호작용
        if (currentState == HelicopterState.WaitingForPlayer && !missionComplete)
        {
            CheckPlayerInteraction();
        }
    }

    public void CallHelicopter(Vector3 landingPos)
    {
        if (isCalled)
        {
            Debug.Log("⚠️ 헬리콥터가 이미 호출되었습니다!");
            return;
        }

        isCalled = true;
        callTime = Time.time;
        landingPosition = landingPos;
        hasLandingPosition = true;

        float waitTime = useTestTime ? testTime : arrivalTime;
        int minutes = Mathf.FloorToInt(waitTime / 60f);
        int seconds = Mathf.FloorToInt(waitTime % 60f);

        Debug.Log($"📡 구조 헬리콥터 호출!");
        Debug.Log($"📍 착륙 지점: {landingPosition}");
        Debug.Log($"⏱️ 약 {minutes}분 {seconds}초 후 도착!");

        if (helicopter != null)
        {
            helicopter.gameObject.SetActive(true);
        }

        // 헬리콥터 사운드
        if (audioSource != null && helicopterSound != null)
        {
            audioSource.clip = helicopterSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    IEnumerator HelicopterArrival()
    {
        if (!hasLandingPosition)
        {
            Debug.LogError("❌ 착륙 위치를 찾을 수 없습니다!");
            yield break;
        }

        Debug.Log("🚁 헬리콥터 접근 중!");
        currentState = HelicopterState.Approaching;

        // 시작 위치
        Vector3 startPos = landingPosition + new Vector3(-startDistance, flyHeight, 0);
        helicopter.position = startPos;

        // 목표 위치
        Vector3 targetPos = landingPosition + new Vector3(0, flyHeight, 0);

        // 접근
        while (Vector3.Distance(helicopter.position, targetPos) > 2f)
        {
            helicopter.position = Vector3.MoveTowards(
                helicopter.position,
                targetPos,
                flySpeed * Time.deltaTime
            );

            Vector3 direction = (targetPos - helicopter.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                helicopter.rotation = Quaternion.Lerp(helicopter.rotation, lookRotation, Time.deltaTime * 2f);
            }

            yield return null;
        }

        Debug.Log("🚁 착륙 지점 도착! 하강 중...");
        currentState = HelicopterState.Landing;

        // 하강
        targetPos = landingPosition + new Vector3(0, hoverHeight, 0);

        while (Vector3.Distance(helicopter.position, targetPos) > 0.5f)
        {
            helicopter.position = Vector3.MoveTowards(
                helicopter.position,
                targetPos,
                descendSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 착륙 사운드
        if (audioSource != null && landingSound != null)
        {
            audioSource.PlayOneShot(landingSound);
        }

        Debug.Log("🚁 착륙 완료!");
        Debug.Log($"💡 헬리콥터로 가서 [{interactKey}]키를 눌러 탑승하세요!");
        currentState = HelicopterState.WaitingForPlayer;
    }

    void CheckPlayerInteraction()
    {
        if (player == null || helicopter == null) return;

        float distance = Vector3.Distance(player.position, helicopter.position);

        if (distance <= interactionDistance)
        {
            if (!isPlayerNearby)
            {
                isPlayerNearby = true;
                Debug.Log($"💡 [{interactKey}] 키를 눌러 헬리콥터에 탑승하세요!");
            }

            if (Input.GetKeyDown(interactKey))
            {
                StartCoroutine(PlayerEscape());
            }
        }
        else
        {
            if (isPlayerNearby)
            {
                isPlayerNearby = false;
            }
        }

        // 거리 체크 (H키)
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log($"📍 헬리콥터까지 거리: {distance:F1}m");
        }
    }

    IEnumerator PlayerEscape()
    {
        missionComplete = true;
        currentState = HelicopterState.Escaping;

        Debug.Log("🚁 탑승 완료! 탈출 중...");

        // 플레이어 컨트롤 비활성화
        CharacterController charController = player.GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
        }

        // 플레이어를 헬리콥터에 태우기
        player.SetParent(helicopter);
        player.localPosition = new Vector3(0, -1.5f, 0);

        // 플레이어 스크립트 비활성화 (카메라 제외)
        MonoBehaviour[] playerScripts = player.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in playerScripts)
        {
            if (script != null &&
                !script.GetType().Name.Contains("Camera") &&
                !script.GetType().Name.Contains("Audio"))
            {
                script.enabled = false;
            }
        }

        // 탈출 사운드
        if (audioSource != null && escapeSound != null)
        {
            audioSource.PlayOneShot(escapeSound);
        }

        yield return new WaitForSeconds(1.5f);

        // 상승
        Debug.Log("🚁 이륙!");
        Vector3 targetPos = helicopter.position + new Vector3(0, flyHeight, 0);

        while (Vector3.Distance(helicopter.position, targetPos) > 1f)
        {
            helicopter.position = Vector3.MoveTowards(
                helicopter.position,
                targetPos,
                descendSpeed * 1.5f * Time.deltaTime
            );
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // 날아가기
        Debug.Log("🚁 안전 지대로 이동!");
        targetPos = helicopter.position + new Vector3(50f, 10f, 50f);

        float flyTime = 0f;
        while (flyTime < 5f)
        {
            helicopter.position = Vector3.MoveTowards(
                helicopter.position,
                targetPos,
                flySpeed * 1.2f * Time.deltaTime
            );

            Vector3 direction = (targetPos - helicopter.position).normalized;
            if (direction != Vector3.zero)
            {
                helicopter.rotation = Quaternion.Lerp(
                    helicopter.rotation,
                    Quaternion.LookRotation(direction),
                    Time.deltaTime * 2f
                );
            }

            flyTime += Time.deltaTime;
            yield return null;
        }

        // 엔딩
        currentState = HelicopterState.MissionComplete;
        ShowEnding();
    }

    void ShowEnding()
    {
        Debug.Log("==========================================");
        Debug.Log(endingMessage);
        Debug.Log("당신은 헬리콥터를 타고 무사히 탈출했습니다!");
        Debug.Log("==========================================");

        if (endingUI != null)
        {
            endingUI.SetActive(true);
        }

        // 게임 일시정지 또는 씬 전환
        // Time.timeScale = 0;
        // SceneManager.LoadScene("EndingScene");
    }

    void OnDrawGizmos()
    {
        if (!hasLandingPosition) return;

        // 착륙 지점
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(landingPosition, 2f);
        Gizmos.DrawWireCube(landingPosition + Vector3.up * hoverHeight, new Vector3(4f, 0.2f, 4f));

        // 상호작용 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(landingPosition + Vector3.up * hoverHeight, interactionDistance);

        // 접근 경로
        if (isCalled)
        {
            Gizmos.color = Color.cyan;
            Vector3 startPos = landingPosition + new Vector3(-startDistance, flyHeight, 0);
            Vector3 approachPos = landingPosition + new Vector3(0, flyHeight, 0);
            Gizmos.DrawLine(startPos, approachPos);
            Gizmos.DrawLine(approachPos, landingPosition + Vector3.up * hoverHeight);
        }
    }
}