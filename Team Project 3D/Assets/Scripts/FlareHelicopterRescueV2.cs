using UnityEngine;
using System.Collections;

public class FlareHelicopterRescueV2 : MonoBehaviour
{
    [Header("필수 연결")]
    [SerializeField] private Transform helicopter;
    [SerializeField] private Transform player;

    [Header("착륙 지점 설정 (중요)")]
    [Tooltip("헬리콥터가 착륙할 고정 위치입니다. 비워두면 플레어 쏜 위치로 옵니다.")]
    [SerializeField] private Transform fixedLandingZone;

    [Header("프로펠러")]
    [SerializeField] private Transform propeller;
    [SerializeField] private float propellerSpeed = 2000f;

    [Header("타이밍 설정")]
    [SerializeField] private float arrivalTime = 300f;
    [SerializeField] private bool useTestTime = true;
    [SerializeField] private float testTime = 10f;

    [Header("비행 설정")]
    [SerializeField] private float flySpeed = 15f;
    [SerializeField] private float descendSpeed = 3f;
    [SerializeField] private float flyHeight = 25f;
    [SerializeField] private float hoverHeight = 3f;   // 착륙 높이 (지면보다 약간 위)
    [SerializeField] private float startDistance = 100f;

    [Header("상호작용 설정")]
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("엔딩 설정")]
    [SerializeField] private string endingMessage = "🎉 탈출 성공! ENDING";
    [SerializeField] private GameObject endingUI;

    private bool isCalled = false;
    private bool hasArrived = false;
    private bool isPlayerNearby = false;
    private bool missionComplete = false;
    private float callTime;
    private float remainingTime;
    private Vector3 flarePosition; // 실제 착륙할 목표 좌표
    private bool hasFlarePosition = false;

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
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (helicopter != null)
        {
            helicopter.position = new Vector3(0, -1000, 0);
            helicopter.gameObject.SetActive(false);
        }

        Debug.Log("🚁 헬리콥터 대기 중...");
    }

    void Update()
    {
        if (propeller != null && helicopter.gameObject.activeSelf)
        {
            propeller.Rotate(Vector3.up, propellerSpeed * Time.deltaTime);
        }

        if (isCalled && !hasArrived)
        {
            remainingTime = (useTestTime ? testTime : arrivalTime) - (Time.time - callTime);

            if (remainingTime <= 0 && !hasArrived)
            {
                hasArrived = true;
                StartCoroutine(HelicopterArrival());
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                Debug.Log($"⏱️ 헬리콥터 도착까지: {minutes}분 {seconds}초");
            }
        }

        if (currentState == HelicopterState.WaitingForPlayer && !missionComplete)
        {
            CheckPlayerInteraction();
        }
    }

    // 외부(총 등)에서 호출하는 함수
    public void CallHelicopter(Vector3 flarePos)
    {
        if (isCalled)
        {
            Debug.Log("⚠️ 헬리콥터가 이미 호출되었습니다!");
            return;
        }

        isCalled = true;
        callTime = Time.time;
        hasFlarePosition = true;

        // ★★★ [핵심 수정 부분] ★★★
        if (fixedLandingZone != null)
        {
            // 고정된 착륙 지점이 있다면 그곳의 위치를 사용
            flarePosition = fixedLandingZone.position;
            Debug.Log($"🎯 고정된 착륙장({fixedLandingZone.name})으로 좌표 설정됨: {flarePosition}");
        }
        else
        {
            // 없다면 매개변수로 넘어온 위치(총 쏜 곳) 사용
            flarePosition = flarePos;
            Debug.Log($"📍 플레어 위치로 좌표 설정됨: {flarePosition}");
        }
        // ★★★★★★★★★★★★★★★★★

        float waitTime = useTestTime ? testTime : arrivalTime;
        int minutes = Mathf.FloorToInt(waitTime / 60f);
        int seconds = Mathf.FloorToInt(waitTime % 60f);

        Debug.Log($"📡 구조 헬리콥터 호출! 약 {minutes}분 {seconds}초 후 도착.");

        if (helicopter != null)
        {
            helicopter.gameObject.SetActive(true);
        }
    }

    IEnumerator HelicopterArrival()
    {
        if (!hasFlarePosition)
        {
            Debug.LogError("❌ 착륙 위치 오류!");
            yield break;
        }

        Debug.Log("🚁 헬리콥터 접근 중!");
        currentState = HelicopterState.Approaching;

        // 시작 지점 계산 (목표 지점에서 일정 거리 떨어진 곳)
        Vector3 startPos = flarePosition + new Vector3(-startDistance, flyHeight, 0);
        helicopter.position = startPos;

        // 1. 공중 목표 지점 (착륙장 바로 위)
        Vector3 targetPos = flarePosition + new Vector3(0, flyHeight, 0);

        // 접근
        while (Vector3.Distance(helicopter.position, targetPos) > 2f)
        {
            helicopter.position = Vector3.MoveTowards(helicopter.position, targetPos, flySpeed * Time.deltaTime);

            Vector3 direction = (targetPos - helicopter.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                helicopter.rotation = Quaternion.Lerp(helicopter.rotation, lookRotation, Time.deltaTime * 2f);
            }
            yield return null;
        }

        Debug.Log("🚁 착륙 지점 도착! 하강 시작...");
        currentState = HelicopterState.Landing;

        // 2. 하강 목표 지점 (착륙장 바닥 + 호버링 높이)
        targetPos = flarePosition + new Vector3(0, hoverHeight, 0);

        while (Vector3.Distance(helicopter.position, targetPos) > 0.5f)
        {
            helicopter.position = Vector3.MoveTowards(helicopter.position, targetPos, descendSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log("🚁 착륙 완료! 탑승 대기.");
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
                Debug.Log("💡 [E] 키를 눌러 탈출하세요!");
            }

            if (Input.GetKeyDown(interactKey))
            {
                StartCoroutine(PlayerEscape());
            }
        }
        else
        {
            if (isPlayerNearby) isPlayerNearby = false;
        }
    }

    IEnumerator PlayerEscape()
    {
        missionComplete = true;
        currentState = HelicopterState.Escaping;

        Debug.Log("🚁 탑승 완료! 이륙합니다.");

        CharacterController charController = player.GetComponent<CharacterController>();
        if (charController != null) charController.enabled = false;

        player.SetParent(helicopter);
        player.localPosition = new Vector3(0, -1.5f, 0); // 좌석 위치 조정 필요

        MonoBehaviour[] playerScripts = player.GetComponents<MonoBehaviour>();
        foreach (var script in playerScripts)
        {
            if (script != null &&
                !script.GetType().Name.Contains("Flare") &&
                !script.GetType().Name.Contains("Camera"))
            {
                script.enabled = false;
            }
        }

        yield return new WaitForSeconds(1.5f);

        // 수직 상승
        Vector3 targetPos = helicopter.position + new Vector3(0, flyHeight, 0);
        while (Vector3.Distance(helicopter.position, targetPos) > 1f)
        {
            helicopter.position = Vector3.MoveTowards(helicopter.position, targetPos, descendSpeed * 1.5f * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // 멀리 이동
        targetPos = helicopter.position + new Vector3(100f, 20f, 100f);
        float flyTime = 0f;
        while (flyTime < 5f)
        {
            helicopter.position = Vector3.MoveTowards(helicopter.position, targetPos, flySpeed * 1.2f * Time.deltaTime);

            Vector3 direction = (targetPos - helicopter.position).normalized;
            if (direction != Vector3.zero)
            {
                helicopter.rotation = Quaternion.Lerp(helicopter.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 2f);
            }

            flyTime += Time.deltaTime;
            yield return null;
        }

        currentState = HelicopterState.MissionComplete;
        ShowEnding();
    }

    void ShowEnding()
    {
        Debug.Log(endingMessage);
        if (endingUI != null) endingUI.SetActive(true);
    }

    void OnDrawGizmos()
    {
        // 고정 착륙 지점이 설정되어 있다면 그곳을 우선적으로 표시
        Vector3 drawPos = (fixedLandingZone != null) ? fixedLandingZone.position : flarePosition;

        if (fixedLandingZone != null || hasFlarePosition)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(drawPos, 1f);
            Gizmos.DrawWireCube(drawPos + Vector3.up * hoverHeight, new Vector3(4f, 0.2f, 4f));

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(drawPos + Vector3.up * hoverHeight, interactionDistance);
        }
    }
}