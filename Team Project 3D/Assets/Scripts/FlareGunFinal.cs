using UnityEngine;

// Gun 클래스를 상속받습니다.
public class FlareGunFinal : Gun
{
    [Header("플레어 투사체 설정 (Flare 전용)")]
    [SerializeField] private GameObject flarePrefab;
    [SerializeField] private float flareSpeed = 30f;
    [SerializeField] private float flareLifetime = 5f;
    [SerializeField] private float fireAngle = 75f;

    [Header("입력 설정")]
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;

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
    private int currentAmmo;
    private bool isReloading = false;
    private bool isInZone = false;
    private Transform currentZone;

    void Start()
    {
        gunName = "Flare Gun";
        currentAmmo = startMagazineAmmo;

        audioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (helicopterSystem == null)
        {
            helicopterSystem = FindObjectOfType<FlareHelicopterRescueV2>();
        }
    }

    void Update()
    {
        // 1. 매 프레임 플레이어가 구역 안에 있는지 체크
        CheckIfInZone();

        // 2. 발사 입력 감지
        if (Input.GetKeyDown(fireKey))
        {
            TryFire();
        }
    }

    // [중요 수정] 발사 시도 로직 재정리
    void TryFire()
    {
        // 1. [최우선] 구역 밖이라면 절대 발사 불가 (탄약이 있어도 안됨)
        if (!isInZone)
        {
            CannotFire(); // 실패 사운드 및 로그
            return;       // 여기서 함수 강제 종료! (아래 코드 실행 안 됨)
        }

        // 2. 재장전 중인가?
        if (isReloading)
        {
            return;
        }

        // 3. 탄약이 없는가?
        if (currentAmmo <= 0)
        {
            // 부모(Gun)의 빈 탄창 소리 재생
            PlaySound(emptyClipSound);
            return;
        }

        // 위 3가지 관문을 모두 통과해야만 실제 발사
        Fire();
    }

    // 실제 발사 로직
    void Fire()
    {
        isReloading = true;
        currentAmmo--;

        Debug.Log("🔥 플레어 발사!");

        // 1. 플레어 생성
        if (flarePrefab != null)
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + transform.forward;
            Vector3 shootDirection = Quaternion.Euler(-fireAngle, 0, 0) * transform.forward;

            GameObject flare = Instantiate(flarePrefab, spawnPos, Quaternion.identity);

            Rigidbody rb = flare.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = shootDirection * flareSpeed;
            }
            else
            {
                FlareProjectile flareScript = flare.AddComponent<FlareProjectile>();
                flareScript.Initialize(shootDirection * flareSpeed, flareLifetime);
            }

            Destroy(flare, flareLifetime);
        }

        // 2. 사운드 및 이펙트
        if (fireSound != null) PlaySound(fireSound);
        if (muzzleFlashEffect != null) muzzleFlashEffect.Play();

        // 3. 헬리콥터 호출 (구역 안에서 쐈으므로 호출됨)
        if (helicopterSystem != null && currentZone != null)
        {
            helicopterSystem.CallHelicopter(currentZone.position);
        }

        // 4. 재장전 대기
        Invoke(nameof(FinishReload), reloadTime);
    }

    // 발사 실패 처리 (구역 밖일 때)
    void CannotFire()
    {
        float distance = GetDistanceToNearestZone();
        // 화면 중앙이나 콘솔에 메시지 출력
        Debug.LogWarning($"❌ 여기서는 신호를 보낼 수 없습니다! (가장 가까운 구조 지점까지 {distance:F1}m)");

        // "삑-" 하는 경고음 재생
        PlaySound(cannotFireSound);
    }

    void FinishReload()
    {
        isReloading = false;
        if (currentAmmo > 0 && reloadSound != null)
            PlaySound(reloadSound);
    }

    // 구역 진입 체크
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
        float minDistance = float.MaxValue;

        if (useMultipleZones && flareZones != null)
        {
            foreach (Transform zone in flareZones)
            {
                if (zone != null)
                {
                    float dist = Vector3.Distance(player.position, zone.position);
                    if (dist < minDistance) minDistance = dist;
                }
            }
        }
        else if (flareZone != null)
        {
            minDistance = Vector3.Distance(player.position, flareZone.position);
        }
        return minDistance;
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
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);

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
        Gizmos.DrawSphere(zone.position, zoneRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(zone.position, zoneRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
    }
}
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
        // 중력 적용
        velocity += Physics.gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}