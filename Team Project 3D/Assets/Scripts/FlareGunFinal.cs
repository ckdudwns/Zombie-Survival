using UnityEngine;

// Gun 클래스를 상속받습니다.
public class FlareGunFinal : Gun
{
    [Header("플레어 투사체 설정 (Flare 전용)")]
    [SerializeField] private GameObject flarePrefab;
    [SerializeField] private float flareSpeed = 30f;
    [SerializeField] private float flareLifetime = 10f;
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
    [SerializeField] private AudioClip cannotFireSound;

    private AudioSource audioSource;
    private bool isInZone = false;
    private Transform currentZone;
    private bool hasFired = false;

    void Start()
    {
        gunName = "Flare Gun";
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
        CheckIfInZone();
    }

    public override bool TryCustomFire()
    {
        Fire();
        return true;
    }

    public override void Fire()
    {
        if (hasFired)
        {
            Debug.Log("⛔ 이미 사용한 신호탄입니다.");
            PlaySound(emptyClipSound);
            return;
        }

        if (!isInZone)
        {
            CannotFire();
            return;
        }

        // --- 발사 성공 ---
        hasFired = true;
        Debug.Log("🔥 구조 신호탄 발사 성공!");

        // [★추가됨] 퀘스트 매니저에게 발사 알림 -> q09_heli_defense 시작
        if (QuestManager.instance != null)
        {
            QuestManager.instance.ProcessFlareGunFired();
        }

        // 플레어 생성
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

        if (fireSound != null) PlaySound(fireSound);
        if (muzzleFlashEffect != null)
        {
            muzzleFlashEffect.gameObject.SetActive(true);
            muzzleFlashEffect.Play();
        }

        if (helicopterSystem != null && currentZone != null)
        {
            helicopterSystem.CallHelicopter(currentZone.position);
        }
    }

    void CannotFire()
    {
        float dist = GetDistanceToNearestZone();
        Debug.LogWarning($"❌ 통신 불가 지역! (가장 가까운 구조 지점까지 {dist:F1}m)");
        PlaySound(cannotFireSound);
    }

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
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isInZone ? Color.green : new Color(1f, 0.5f, 0f, 0.3f);
        if (useMultipleZones && flareZones != null)
        {
            foreach (Transform zone in flareZones) if (zone != null) DrawZoneGizmo(zone);
        }
        else if (flareZone != null) DrawZoneGizmo(flareZone);
    }

    void DrawZoneGizmo(Transform zone)
    {
        Gizmos.DrawWireSphere(zone.position, zoneRadius);
    }
}

// (FlareProjectile 클래스는 기존과 동일하게 유지하거나 아래에 포함)
public class FlareProjectile : MonoBehaviour
{
    private Vector3 velocity;
    private float lifetime;
    private float spawnTime;
    public void Initialize(Vector3 vel, float life) { velocity = vel; lifetime = life; spawnTime = Time.time; }
    void Update() { velocity += Physics.gravity * Time.deltaTime; transform.position += velocity * Time.deltaTime; if (Time.time - spawnTime >= lifetime) Destroy(gameObject); }
}