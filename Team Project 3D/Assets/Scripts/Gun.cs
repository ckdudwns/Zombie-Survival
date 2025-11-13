using UnityEngine;
using UnityEngine.UI;

public enum FireMode
{
    Single, // 단발
    Auto,   // 연사
    Spread  // 산탄
}

public class Gun : MonoBehaviour
{
    [Header("총기 정보")]
    public string gunName = "Rifle";

    [Header("총기 고유 능력치")]
    public FireMode fireMode = FireMode.Auto;

    [Tooltip("총알(레이캐스트)이 도달하는 최대 사거리입니다.")]
    public float range = 100f;

    [Tooltip("발사체 하나당 데미지입니다.")]
    public int damage = 10;

    [Header("탄약 설정")] // 헤더를 하나로 합쳤습니다.
    [Tooltip("한 탄창에 들어가는 최대 총알 수입니다.")]
    public int maxAmmo = 30;

    [Tooltip("소지 가능한 최대 예비탄창 수")]
    public int maxReserveAmmo = 100;

    // --- (여기가 추가된 부분입니다) ---
    [Tooltip("게임 시작 시 탄창에 채워져 있을 총알 수")]
    public int startMagazineAmmo = 10;
    // --- (여기까지) ---

    [Tooltip("재장전에 걸리는 시간(초)입니다.")]
    public float reloadTime = 1.5f;
    [Tooltip("연사 속도 (초당 발사 수).")]
    public float fireRate = 10f;

    [Header("샷건 전용 설정")]
    [Tooltip("산탄(Spread) 모드일 때만 사용: 한 번에 발사되는 펠릿(총알) 수")]
    public int projectilesPerShot = 8;
    [Tooltip("총알이 퍼지는 정도(집탄도). 0이면 정확히 중앙으로 나갑니다.")]
    public float spreadAngle = 5f;

    [Header("반동 설정")]
    public float normalRecoil = 1.5f;
    public float crouchingRecoil = 0.2f;
    public float sprintingRecoil = 4.0f;

    [Header("조준경 설정")]
    [Tooltip("이 총이 조준경(줌)을 사용하는지 여부입니다.")]
    public bool isScopable = false;

    [Header("총기 필수 요소")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject bloodImpactPrefab;
    public GameObject genericImpactPrefab;

    [Header("UI 설정")]
    public Sprite crosshairSprite;

    [Header("위치/회전 오프셋")]
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
}