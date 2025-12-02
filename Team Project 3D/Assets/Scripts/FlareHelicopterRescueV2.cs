using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlareHelicopterRescueV2 : MonoBehaviour
{
    [Header("필수 연결")]
    [SerializeField] private Transform helicopter;
    [SerializeField] private Transform player;

    // [복구됨] 착륙 지점 설정 변수
    [Header("착륙 지점 설정")]
    [Tooltip("헬리콥터가 착륙할 고정 위치. 비워두면 플레어 위치(Dynamic)를 사용합니다.")]
    [SerializeField] private Transform fixedLandingZone;

    // [복구됨] 카메라 연출 설정 (이전에 추가된 기능)
    [Header("카메라 연출 설정")]
    [Tooltip("헬기 탈출 장면을 찍을 시네마틱 카메라")]
    [SerializeField] private GameObject escapeCamera;
    [Tooltip("원래 플레이어가 보고 있던 메인 카메라")]
    [SerializeField] private GameObject mainPlayerCamera;

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
    [SerializeField] private float hoverHeight = 3f;
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
    private Vector3 flarePosition;
    private bool hasFlarePosition = false;

    // [CS0103 FIX] 헬기에 붙은 AudioSource 전역 변수 선언
    private AudioSource heliAudioSource;

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
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (mainPlayerCamera == null && Camera.main != null)
        {
            mainPlayerCamera = Camera.main.gameObject;
        }
        if (escapeCamera != null) escapeCamera.SetActive(false);

        if (helicopter != null)
        {
            helicopter.position = new Vector3(0, -1000, 0);
            helicopter.gameObject.SetActive(false);

            // [CS0103 FIX] Start에서 AudioSource 캐싱 및 3D 설정
            heliAudioSource = helicopter.GetComponent<AudioSource>();
            if (heliAudioSource != null)
            {
                heliAudioSource.loop = true; // 비행 내내 소리가 나야 함
                heliAudioSource.playOnAwake = false;
                heliAudioSource.spatialBlend = 1.0f; // 3D 사운드
            }
        }

        Debug.Log("🚁 헬리콥터 대기 중... 플레어 발사 구역에서 플레어를 쏘세요!");
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

    public void CallHelicopter(Vector3 flarePos)
    {
        if (isCalled)
        {
            Debug.Log("⚠️ 헬리콥터가 이미 호출되었습니다!");
            return;
        }

        isCalled = true;
        callTime = Time.time;

        if (fixedLandingZone != null)
        {
            flarePosition = fixedLandingZone.position;
            Debug.Log($"🚁 고정 착륙 지점 사용: {flarePosition}");
        }
        else
        {
            flarePosition = flarePos;
            Debug.Log($"🚁 플레어 위치 사용: {flarePosition}");
        }

        hasFlarePosition = true;

        float waitTime = useTestTime ? testTime : arrivalTime;
        int minutes = Mathf.FloorToInt(waitTime / 60f);
        int seconds = Mathf.FloorToInt(waitTime % 60f);

        Debug.Log($"📡 구조 헬리콥터 호출! 약 {minutes}분 {seconds}초 후 도착 예정!");

        if (helicopter != null)
        {
            helicopter.gameObject.SetActive(true);

            // 3D 사운드 재생 시작
            if (heliAudioSource != null && !heliAudioSource.isPlaying)
            {
                heliAudioSource.Play();
            }
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

        Vector3 startPos = flarePosition + new Vector3(-startDistance, flyHeight, 0);
        helicopter.position = startPos;
        Vector3 targetPos = flarePosition + new Vector3(0, flyHeight, 0);

        // 접근 로직
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

        Debug.Log("🚁 착륙 지점 도착! 하강 중...");
        currentState = HelicopterState.Landing;

        // 하강 로직
        targetPos = flarePosition + new Vector3(0, hoverHeight, 0);

        while (Vector3.Distance(helicopter.position, targetPos) > 0.5f)
        {
            helicopter.position = Vector3.MoveTowards(helicopter.position, targetPos, descendSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log("🚁 착륙 완료! 헬리콥터로 가서 E키를 눌러 탑승하세요!");
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
                if (QuestManager.instance != null)
                {
                    QuestManager.instance.PlayDialogueOnly("q09_heli_end");
                    QuestManager.instance.ProcessHeliBoarding();
                }
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

        // 1. 플레이어 조작 끄기 및 헬기에 태우기
        CharacterController charController = player.GetComponent<CharacterController>();
        if (charController != null) charController.enabled = false;

        player.SetParent(helicopter);
        player.localPosition = new Vector3(0, -1.5f, 0);

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

        // 2. 플레이어 모델 및 충돌체 비활성화 (시야에서 완전히 숨김)
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        Collider[] colliders = player.GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            c.enabled = false;
        }

        // 3. 카메라 전환 (시네마틱 뷰)
        if (mainPlayerCamera != null) mainPlayerCamera.SetActive(false);
        if (escapeCamera != null) escapeCamera.SetActive(true);

        // 헬기 오디오 리스너 문제 해결
        if (escapeCamera != null && escapeCamera.GetComponent<AudioListener>() == null)
            escapeCamera.AddComponent<AudioListener>();

        yield return new WaitForSeconds(1.5f);

        Debug.Log("🚁 이륙!");
        Vector3 targetPos = helicopter.position + new Vector3(0, flyHeight, 0);

        // 상승 로직
        while (Vector3.Distance(helicopter.position, targetPos) > 1f)
        {
            helicopter.position = Vector3.MoveTowards(helicopter.position, targetPos, descendSpeed * 1.5f * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        Debug.Log("🚁 안전 지대로 이동!");
        targetPos = helicopter.position + new Vector3(50f, 10f, 50f);

        float flyTime = 0f;
        // 비행 로직 (5초 동안)
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

        // 엔딩
        currentState = HelicopterState.MissionComplete;
        ShowEnding();

        // 헬기 소리 끄기 (장면 전환 직전에)
        if (heliAudioSource != null)
        {
            heliAudioSource.Stop();
        }
    }

    void ShowEnding()
    {
        Debug.Log("==========================================");
        Debug.Log(endingMessage);
        Debug.Log("당신은 헬리콥터를 타고 무사히 탈출했습니다!");
        Debug.Log("==========================================");

        if (endingUI != null)
        {
            if (QuestManager.instance != null)
            {
                // QuestManager가 엔딩 씬 로드를 담당하므로, 여기서는 UI 대신 QuestManager를 호출하여 페이드아웃 시작
                QuestManager.instance.QuestComplete("q09_heli_end");
            }
            else
            {
                endingUI.SetActive(true);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!hasFlarePosition && fixedLandingZone == null) return;

        Vector3 drawPos = fixedLandingZone != null ? fixedLandingZone.position : flarePosition;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(drawPos, 2f);
        Gizmos.DrawWireCube(drawPos + Vector3.up * hoverHeight, new Vector3(4f, 0.2f, 4f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(drawPos + Vector3.up * hoverHeight, interactionDistance);

        if (isCalled)
        {
            Gizmos.color = Color.cyan;
            Vector3 startPos = drawPos + new Vector3(-startDistance, flyHeight, 0);
            Vector3 approachPos = drawPos + new Vector3(0, flyHeight, 0);
            Gizmos.DrawLine(startPos, approachPos);
            Gizmos.DrawLine(approachPos, drawPos + Vector3.up * hoverHeight);
        }
    }
}