using UnityEngine;

// Gun 클래스를 상속받습니다.
public class FlareGunFinal : Gun
{
    [Header("플레어 투사체 설정 (Flare 전용)")]
    [SerializeField] private GameObject flarePrefab;
    [SerializeField] private float flareSpeed = 30f;
    [SerializeField] private float flareLifetime = 10f; // 체공 시간 조금 늘림
    [SerializeField] private float fireAngle = 75f;

    [Header("발사 가능 구역 (Zone)")]
    [SerializeField] private Transform flareZone;
    [SerializeField] private Transform[] flareZones;
    [SerializeField] private bool useMultipleZones = false;
    [SerializeField] private float zoneRadius = 10f;

    [Header("참조 연결")]
    [SerializeField] private Transform player;
    [SerializeField] private FlareHelicopterRescueV2 helicopterSystem;

    [Header("오디오 (Flare 전용 추가)")]
    [SerializeField] private AudioClip cannotFireSound; // 구역 아님 소리

    // 내부 상태 변수
    private AudioSource audioSource;
    private bool isInZone = false;
    private Transform currentZone;
    private bool hasFired = false; // 일회용 체크

    void Start()
    {
        // Gun 클래스의 기본 정보 설정
        gunName = "Flare Gun";

        audioSource = GetComponent<AudioSource>();

        // 플레이어 자동 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // 헬기 시스템 자동 찾기
        if (helicopterSystem == null)
        {
            helicopterSystem = FindObjectOfType<FlareHelicopterRescueV2>();
        }
    }

    void Update()
    {
        // 매 프레임 플레이어가 구역 안에 있는지 체크 (UI 표시용)
        CheckIfInZone();

        // *주의: 여기서 Input.GetKeyDown(fireKey)를 쓰지 않습니다.
        // 입력은 PlayerShooting 스크립트가 받아서 아래 TryCustomFire()를 호출합니다.
    }

    // [핵심 연결고리] PlayerShooting이 호출하는 함수
    public override bool TryCustomFire()
    {
        // 이 함수가 호출되었다는 건, PlayerShooting에서 발사 버튼을 눌렀다는 뜻입니다.
        Fire(); // 실제 발사 로직 실행

        return true; // "내가 처리했으니 PlayerShooting 너는 기본 발사(레이캐스트) 하지 마" 라는 뜻
    }

    // 실제 발사 로직 (Gun의 Fire를 덮어쓰기)
    public override void Fire()
    {
        // 1. [일회용 체크] 이미 쐈다면 빈 소리만 재생
        if (hasFired)
        {
            Debug.Log("⛔ 이미 사용한 신호탄입니다.");
            PlaySound(emptyClipSound);
            return;
        }

        // 2. [구역 체크] 구역 밖이라면 경고음 재생 후 취소
        if (!isInZone)
        {
            CannotFire();
            return;
        }

        // --- 검사 통과: 발사 시작 ---

        hasFired = true; // 사용 처리 (이제 두 번 다시 못 쏨)
        Debug.Log("🔥 구조 신호탄 발사 성공!");

        // 3. 플레어 투사체 생성
        if (flarePrefab != null)
        {
            // 총구 위치가 없으면 내 위치 앞
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + transform.forward;
            // 위쪽(fireAngle)을 향해 쏘도록 각도 계산
            Vector3 shootDirection = Quaternion.Euler(-fireAngle, 0, 0) * transform.forward;

            GameObject flare = Instantiate(flarePrefab, spawnPos, Quaternion.identity);

            Rigidbody rb = flare.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = shootDirection * flareSpeed;
            }
            else
            {
                // Rigidbody가 없으면 직접 만든 스크립트 붙이기
                FlareProjectile flareScript = flare.AddComponent<FlareProjectile>();
                flareScript.Initialize(shootDirection * flareSpeed, flareLifetime);
            }

            Destroy(flare, flareLifetime);
        }

        // 4. 사운드 및 이펙트
        if (fireSound != null) PlaySound(fireSound);
        if (muzzleFlashEffect != null)
        {
            muzzleFlashEffect.gameObject.SetActive(true);
            muzzleFlashEffect.Play();
        }

        // 5. 헬리콥터 호출
        if (helicopterSystem != null && currentZone != null)
        {
            helicopterSystem.CallHelicopter(currentZone.position);
        }
    }

    // 발사 실패 처리
    void CannotFire()
    {
        float dist = GetDistanceToNearestZone();
        Debug.LogWarning($"❌ 통신 불가 지역! (가장 가까운 구조 지점까지 {dist:F1}m)");
        PlaySound(cannotFireSound);
    }

    // 구역 진입 체크 로직
    void CheckIfInZone()
    {
        if (player == null) return;

        isInZone = false;
        currentZone = null;

        if (useMultipleZones && flareZones != null)
        {
            foreach (Transform zone in flareZones)
            {
                if (zone != null && Vector3.Distance(player.position, zone.position) <= zoneRadius)
                {
                    isInZone = true;
                    currentZone = zone;
                    break;
                }
            }
        }
        else if (flareZone != null)
        {
            if (Vector3.Distance(player.position, flareZone.position) <= zoneRadius)
            {
                isInZone = true;
                currentZone = flareZone;
            }
        }
    }

    float GetDistanceToNearestZone()
    {
        if (player == null) return 0f;

        if (useMultipleZones && flareZones != null)
        {
            float min = float.MaxValue;
            foreach (var z in flareZones)
            {
                if (z != null)
                {
                    float d = Vector3.Distance(player.position, z.position);
                    if (d < min) min = d;
                }
            }
            return min;
        }
        else if (flareZone != null)
        {
            return Vector3.Distance(player.position, flareZone.position);
        }
        return 0f;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isInZone ? Color.green : new Color(1f, 0.5f, 0f, 0.3f);

        if (useMultipleZones && flareZones != null)
        {
            foreach (Transform zone in flareZones)
            {
                if (zone != null) DrawZoneGizmo(zone);
            }
        }
        else if (flareZone != null)
        {
            DrawZoneGizmo(flareZone);
        }
    }

    void DrawZoneGizmo(Transform zone)
    {
        Gizmos.DrawWireSphere(zone.position, zoneRadius);
    }
}

// ▼▼▼ FlareProjectile 클래스 (CS0246 에러 방지용) ▼▼▼
public class FlareProjectile : MonoBehaviour
{
    private Vector3 velocity;
    private float lifetime;
    private float spawnTime;

    public void Initialize(Vector3 vel, float life)
    {
        velocity = vel;
        lifetime = life;
        spawnTime = Time.time;
    }

    void Update()
    {
        velocity += Physics.gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}