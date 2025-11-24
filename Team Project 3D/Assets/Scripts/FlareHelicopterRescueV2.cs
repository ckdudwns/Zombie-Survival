using UnityEngine;
using System.Collections;

public class FlareHelicopterRescueV2 : MonoBehaviour
{
    [Header("필수 연결")]
    [SerializeField] private Transform helicopter;
    [SerializeField] private Transform player;
    
    [Header("프로펠러")]
    [SerializeField] private Transform propeller;
    [SerializeField] private float propellerSpeed = 2000f;
    
    [Header("타이밍 설정")]
    [SerializeField] private float arrivalTime = 300f;  // 5분 = 300초
    [SerializeField] private bool useTestTime = true;   // 테스트용
    [SerializeField] private float testTime = 10f;      // 테스트: 10초
    
    [Header("비행 설정")]
    [SerializeField] private float flySpeed = 15f;
    [SerializeField] private float descendSpeed = 3f;
    [SerializeField] private float flyHeight = 25f;
    [SerializeField] private float hoverHeight = 3f;
    [SerializeField] private float startDistance = 100f;
    
    [Header("상호작용 설정")]
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Header("엔딩 설정")]
    [SerializeField] private string endingMessage = "🎉 탈출 성공! ENDING";
    [SerializeField] private GameObject endingUI;  // 엔딩 UI (선택)
    
    private bool isCalled = false;
    private bool hasArrived = false;
    private bool isPlayerNearby = false;
    private bool missionComplete = false;
    private float callTime;
    private float remainingTime;
    private Vector3 flarePosition;  // 플레어를 쏜 위치 저장
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
        // 플레이어 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        // 헬리콥터 숨기기
        if (helicopter != null)
        {
            helicopter.position = new Vector3(0, -1000, 0);  // 멀리 숨김
            helicopter.gameObject.SetActive(false);
        }
        
        Debug.Log("🚁 헬리콥터 대기 중... 플레어 발사 구역에서 플레어를 쏘세요!");
    }

    void Update()
    {
        // 프로펠러 회전
        if (propeller != null && helicopter.gameObject.activeSelf)
        {
            propeller.Rotate(Vector3.up, propellerSpeed * Time.deltaTime);
        }
        
        // 호출되었을 때 타이머
        if (isCalled && !hasArrived)
        {
            remainingTime = (useTestTime ? testTime : arrivalTime) - (Time.time - callTime);
            
            if (remainingTime <= 0 && !hasArrived)
            {
                hasArrived = true;
                StartCoroutine(HelicopterArrival());
            }
            
            // 타이머 체크
            if (Input.GetKeyDown(KeyCode.T))
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                Debug.Log($"⏱️ 헬리콥터 도착까지: {minutes}분 {seconds}초");
            }
        }
        
        // 상호작용 체크
        if (currentState == HelicopterState.WaitingForPlayer && !missionComplete)
        {
            CheckPlayerInteraction();
        }
    }

    // 플레어건에서 호출
    public void CallHelicopter(Vector3 flarePos)
    {
        if (isCalled)
        {
            Debug.Log("⚠️ 헬리콥터가 이미 호출되었습니다!");
            return;
        }
        
        isCalled = true;
        callTime = Time.time;
        flarePosition = flarePos;  // 플레어 발사 위치 저장
        hasFlarePosition = true;
        
        float waitTime = useTestTime ? testTime : arrivalTime;
        int minutes = Mathf.FloorToInt(waitTime / 60f);
        int seconds = Mathf.FloorToInt(waitTime % 60f);
        
        Debug.Log($"📡 구조 헬리콥터 호출!");
        Debug.Log($"📍 착륙 지점: {flarePosition}");
        Debug.Log($"⏱️ 약 {minutes}분 {seconds}초 후 도착 예정!");
        
        if (helicopter != null)
        {
            helicopter.gameObject.SetActive(true);
        }
    }

    IEnumerator HelicopterArrival()
    {
        if (!hasFlarePosition)
        {
            Debug.LogError("❌ 착륙 위치를 찾을 수 없습니다!");
            yield break;
        }
        
        Debug.Log("🚁 헬리콥터 접근 중!");
        currentState = HelicopterState.Approaching;
        
        // 시작 위치 (플레어 위치 기준)
        Vector3 startPos = flarePosition + new Vector3(-startDistance, flyHeight, 0);
        helicopter.position = startPos;
        
        // 목표 위치 (플레어 위치 위)
        Vector3 targetPos = flarePosition + new Vector3(0, flyHeight, 0);
        
        // 접근
        while (Vector3.Distance(helicopter.position, targetPos) > 2f)
        {
            helicopter.position = Vector3.MoveTowards(
                helicopter.position,
                targetPos,
                flySpeed * Time.deltaTime
            );
            
            // 목표 방향 바라보기
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
        
        // 하강 (플레어 발사 위치로)
        targetPos = flarePosition + new Vector3(0, hoverHeight, 0);
        
        while (Vector3.Distance(helicopter.position, targetPos) > 0.5f)
        {
            helicopter.position = Vector3.MoveTowards(
                helicopter.position,
                targetPos,
                descendSpeed * Time.deltaTime
            );
            yield return null;
        }
        
        Debug.Log("🚁 착륙 완료! 헬리콥터로 가서 E키를 눌러 탑승하세요!");
        Debug.Log($"💡 헬리콥터 위치: {helicopter.position}");
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
                Debug.Log("💡 [E] 키를 눌러 헬리콥터에 탑승하세요!");
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
                Debug.Log($"📍 헬리콥터까지 거리: {distance:F1}m");
            }
        }
        
        // 거리 체크 (디버그용)
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
        
        // 플레이어를 헬리콥터에 태우기
        CharacterController charController = player.GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
        }
        
        player.SetParent(helicopter);
        player.localPosition = new Vector3(0, -1.5f, 0);
        
        // 플레이어 컨트롤 비활성화
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
        
        // 엔딩 UI 표시
        if (endingUI != null)
        {
            endingUI.SetActive(true);
        }
        
        // 게임 일시정지 (선택)
        // Time.timeScale = 0;
        
        // 씬 전환 (선택)
        // SceneManager.LoadScene("EndingScene");
    }

    // Scene 뷰에서 시각화
    void OnDrawGizmos()
    {
        if (!hasFlarePosition) return;
        
        // 착륙 지점
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(flarePosition, 2f);
        Gizmos.DrawWireCube(flarePosition + Vector3.up * hoverHeight, new Vector3(4f, 0.2f, 4f));
        
        // 상호작용 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(flarePosition + Vector3.up * hoverHeight, interactionDistance);
        
        // 접근 경로
        if (isCalled)
        {
            Gizmos.color = Color.cyan;
            Vector3 startPos = flarePosition + new Vector3(-startDistance, flyHeight, 0);
            Vector3 approachPos = flarePosition + new Vector3(0, flyHeight, 0);
            Gizmos.DrawLine(startPos, approachPos);
            Gizmos.DrawLine(approachPos, flarePosition + Vector3.up * hoverHeight);
        }
    }
}
