using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Dictionary 사용을 위해 필수!
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour
{
    [Header("공통 설정")]
    public float range = 100f;

    [Header("무기 설정")]
    public List<Gun> availableGuns; // 소유한 총기 프리팹 목록
    public Transform gunHolder;
    private Gun currentGun; // 현재 손에 든 총의 컴포넌트
    private int currentGunIndex = -1;

    [Header("조준 설정")]
    public float zoomedFOV = 15f;
    private float normalFOV;

    [Header("필수 연결 요소")]
    public Camera playerCamera;
    public Image crosshairImage;
    public GameObject scopeOverlay;

    // --- Private 변수 ---
    private int currentAmmo; // 현재 탄창의 총알
    private bool isReloading = false;
    private float nextTimeToFire = 0f;
    private Player playerController;
    private Animator gunAnimator;
    private bool isAiming = false;
    private static readonly int IsAimingHash = Animator.StringToHash("IsAiming");
    private static readonly int FireHash = Animator.StringToHash("Fire");

    // 총알 관리를 위한 딕셔너리(Dictionary)
    private Dictionary<string, int> reserveAmmoCounts = new Dictionary<string, int>();
    private Dictionary<string, int> currentMagazineAmmo = new Dictionary<string, int>();

    // --- (핵심 수정) Start 함수 ---
    void Start()
    {
        playerController = GetComponent<Player>();
        normalFOV = playerCamera.fieldOfView;

        // 딕셔너리 초기화
        reserveAmmoCounts = new Dictionary<string, int>();
        currentMagazineAmmo = new Dictionary<string, int>();

        if (availableGuns != null && availableGuns.Count > 0)
        {
            // (요청 사항) 시작 시 소유한 모든 총의 탄창/예비탄창 분리
            foreach (Gun gun in availableGuns)
            {
                if (!reserveAmmoCounts.ContainsKey(gun.gunName))
                {
                    // 1. 예비탄창은 '최대 예비탄창'으로 설정
                    reserveAmmoCounts.Add(gun.gunName, gun.maxReserveAmmo);
                }
                if (!currentMagazineAmmo.ContainsKey(gun.gunName))
                {
                    // 2. (핵심) 시작 탄창은 'startMagazineAmmo' 변수(Gun 스크립트에 추가한) 값으로 설정
                    //    단, 최대 탄창(maxAmmo)을 넘지 않도록 함
                    int startAmmo = Mathf.Min(gun.startMagazineAmmo, gun.maxAmmo);
                    currentMagazineAmmo.Add(gun.gunName, startAmmo);
                }
            }

            // 첫 번째 총을 장착
            EquipGun(0);
        }

        if (scopeOverlay != null)
            scopeOverlay.SetActive(false);
    }
    // --- (Start 함수 수정 완료) ---


    void Update()
    {
        if (Player.isPaused) return;

        HandleWeaponSwitching();

        if (currentGun == null) return; // 총이 없으면 아무것도 안 함

        if (currentGun.isScopable)
        {
            HandleAimingInput();
        }

        // 재장전 중이면 발사/재장전 입력 안 받음
        if (isReloading) return;

        // 1. 발사 입력
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
        {
            if (currentAmmo > 0)
            {
                // 탄창에 총알이 있으면 발사
                nextTimeToFire = Time.time + 1f / currentGun.fireRate;
                Shoot();
            }
            else // 탄창이 비었을 때 (currentAmmo <= 0)
            {
                // 탄창이 비었으면 자동 재장전 시도
                string gunNameKey = currentGun.gunName;
                if (reserveAmmoCounts.ContainsKey(gunNameKey) && reserveAmmoCounts[gunNameKey] > 0)
                {
                    StartCoroutine(Reload());
                }
                else
                {
                    // 예비 탄창도 없을 때 (연사 시 로그 스팸 방지를 위해 nextTimeToFire을 살짝 미룸)
                    nextTimeToFire = Time.time + 0.2f; // 0.2초마다 한 번씩만 로그
                    Debug.Log(currentGun.gunName + "의 총알이 모두 소진되었습니다!");
                }
            }
        }

        // 2. 재장전 입력 (R키)
        if (!isAiming && Input.GetKeyDown(KeyCode.R))
        {
            // 탄창이 꽉 찼는지 확인
            if (currentAmmo >= currentGun.maxAmmo)
            {
                Debug.Log("탄창이 이미 가득 찼습니다.");
            }
            else
            {
                string gunNameKey = currentGun.gunName;
                // 예비 탄창이 있는지 확인
                if (reserveAmmoCounts.ContainsKey(gunNameKey) && reserveAmmoCounts[gunNameKey] > 0)
                {
                    StartCoroutine(Reload()); // 예비탄창 있으면 재장전
                }
                else
                {
                    Debug.Log(currentGun.gunName + "의 예비 총알이 부족합니다!");
                }
            }
        }
    }

    // 조준 입력 처리 함수 (토글 방식)
    void HandleAimingInput()
    {
        if (Input.GetMouseButtonDown(1)) // 우클릭 감지
        {
            if (isReloading) return; // 재장전 중 조준 불가

            isAiming = !isAiming; // 조준 상태 토글

            if (gunAnimator != null)
            {
                gunAnimator.SetBool(IsAimingHash, isAiming);
            }

            if (isAiming)
            {
                // 조준 시작: 스코프 보이기, 조준선 숨기기, 줌인
                if (scopeOverlay != null) scopeOverlay.SetActive(true);
                if (crosshairImage != null) crosshairImage.enabled = false;
                playerCamera.fieldOfView = zoomedFOV;
            }
            else
            {
                // 조준 해제: 스코프 숨기기, 조준선 보이기, 줌아웃
                OnUnaim();
            }
        }
    }

    // 조준 해제 시 호출할 함수
    void OnUnaim()
    {
        if (scopeOverlay != null) scopeOverlay.SetActive(false);
        if (crosshairImage != null && currentGun != null && currentGun.crosshairSprite != null)
        {
            crosshairImage.enabled = true;
        }
        playerCamera.fieldOfView = normalFOV;
    }

    // 무기 교체 입력 처리
    void HandleWeaponSwitching()
    {
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                int targetIndex = i - 1;
                if (targetIndex < availableGuns.Count && targetIndex != currentGunIndex)
                {
                    EquipGun(targetIndex);
                }
                break;
            }
        }
    }

    /// <summary>
    /// (ItemPickup용) 지정된 총기 프리팹을 찾아 장착합니다.
    /// </summary>
    public void EquipNewGun(Gun gunPrefab)
    {
        if (gunPrefab == null) return;

        int gunIndex = -1;
        for (int i = 0; i < availableGuns.Count; i++)
        {
            if (availableGuns[i].gunName == gunPrefab.gunName)
            {
                gunIndex = i;
                break;
            }
        }

        // 리스트에 없는 새로운 총이라면, 리스트에 추가
        if (gunIndex == -1)
        {
            availableGuns.Add(gunPrefab);
            gunIndex = availableGuns.Count - 1;

            // 새로 주운 총의 예비탄창/탄창 정보 초기화
            if (!reserveAmmoCounts.ContainsKey(gunPrefab.gunName))
            {
                // 새로 줍는 총도 최대 예비탄창으로 설정
                reserveAmmoCounts.Add(gunPrefab.gunName, gunPrefab.maxReserveAmmo);
                Debug.Log(gunPrefab.gunName + " (새 총) 획득. 예비탄창: " + gunPrefab.maxReserveAmmo);
            }
            if (!currentMagazineAmmo.ContainsKey(gunPrefab.gunName))
            {
                // (참고) 새로 줍는 총은 탄창 0발로 시작 (원한다면 startMagazineAmmo로 변경 가능)
                currentMagazineAmmo.Add(gunPrefab.gunName, 0);
            }
        }

        EquipGun(gunIndex);
    }

    /// <summary>
    /// (총알 획득 시) 소유한 총 중 랜덤으로 예비탄창을 채웁니다.
    /// </summary>
    public void AddAmmo(int amount)
    {
        // 1. 소유한 총이 있는지 확인
        if (availableGuns == null || availableGuns.Count == 0)
        {
            Debug.LogWarning("소유한 총이 없어 총알을 추가할 수 없습니다.");
            return;
        }

        // 2. 소유한 총 목록(availableGuns) 중에서 랜덤으로 하나 선택
        int randomIndex = Random.Range(0, availableGuns.Count);
        Gun randomGun = availableGuns[randomIndex]; // 선택된 총의 *프리팹*
        string gunNameKey = randomGun.gunName;

        // 3. 해당 총의 예비탄창 정보가 있는지 확인
        if (!reserveAmmoCounts.ContainsKey(gunNameKey))
        {
            Debug.LogError(gunNameKey + "의 예비탄창 정보가 사전에 초기화되지 않았습니다. (버그 확인 필요)");
            reserveAmmoCounts[gunNameKey] = 0; // 비상용으로 0으로 설정
        }

        // 4. 총알 추가
        int currentReserve = reserveAmmoCounts[gunNameKey];
        int maxReserve = randomGun.maxReserveAmmo; // 프리팹에서 최대치 가져오기

        // 최대 보유량을 넘지 않도록 계산
        int newReserve = Mathf.Min(currentReserve + amount, maxReserve);

        reserveAmmoCounts[gunNameKey] = newReserve;

        Debug.Log($"[총알 획득] '{gunNameKey}'의 예비 총알 {amount} 획득! (랜덤 선택됨) -> 현재 {newReserve}/{maxReserve} 발");
    }

    // 총기 교체 로직
    void EquipGun(int gunIndex)
    {
        if (isReloading)
        {
            StopAllCoroutines();
            isReloading = false;
        }
        if (isAiming)
        {
            isAiming = false;
            OnUnaim();
        }

        // 0. 현재 총의 탄창 상태를 딕셔너리에 저장
        if (currentGun != null)
        {
            currentMagazineAmmo[currentGun.gunName] = currentAmmo;
        }

        currentGunIndex = gunIndex;

        if (gunHolder.childCount > 0)
        {
            Destroy(gunHolder.GetChild(0).gameObject);
        }

        Gun newGunPrefab = availableGuns[gunIndex];
        GameObject newGunObject = Instantiate(newGunPrefab.gameObject, gunHolder.position, gunHolder.rotation, gunHolder);
        currentGun = newGunObject.GetComponent<Gun>();
        gunAnimator = newGunObject.GetComponent<Animator>();

        if (gunAnimator != null) gunAnimator.SetBool(IsAimingHash, false);

        if (currentGun != null)
        {
            newGunObject.transform.localPosition = currentGun.positionOffset;
            newGunObject.transform.localEulerAngles = currentGun.rotationOffset;
        }

        string gunNameKey = currentGun.gunName;

        // 1. 새로 장착할 총의 '저장된 탄창' 상태를 불러오기
        currentAmmo = currentMagazineAmmo[gunNameKey];

        // 2. (수정) 만약 불러온 탄창이 0발이라면 (새로 주운 총 등), 자동 재장전 시도
        //    (Start에서 설정한 총은 currentAmmo가 0이 아니므로 이 부분이 실행되지 않음)
        if (currentAmmo == 0 && reserveAmmoCounts[gunNameKey] > 0)
        {
            StartCoroutine(Reload());
        }

        Debug.Log($"{currentGun.gunName}으로 교체! [탄창: {currentAmmo}/{currentGun.maxAmmo}] [예비: {reserveAmmoCounts[gunNameKey]}/{currentGun.maxReserveAmmo}]");

        // (UI 업데이트)
        // UIManager.instance.UpdateAmmo(currentAmmo, reserveAmmoCounts[gunNameKey]);

        // 크로스헤어 설정
        if (crosshairImage != null)
        {
            if (currentGun.crosshairSprite != null)
            {
                crosshairImage.sprite = currentGun.crosshairSprite;
                crosshairImage.enabled = true;
            }
            else
            {
                crosshairImage.enabled = false;
            }
        }
    }

    // 재장전 로직
    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log(currentGun.gunName + " 장전 중...");

        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("Reload");
        }

        yield return new WaitForSeconds(currentGun.reloadTime);

        string gunNameKey = currentGun.gunName;
        int reserveAmmo = reserveAmmoCounts[gunNameKey];  // 현재 예비탄창
        int maxMagazine = currentGun.maxAmmo;           // 최대 탄창 크기
        int neededAmmo = maxMagazine - currentAmmo;       // 필요한 총알 수

        if (neededAmmo <= 0 || reserveAmmo <= 0) // 이미 꽉 찼거나 예비탄창이 없으면
        {
            isReloading = false;
            yield break; // 재장전 중지
        }

        // 예비탄창에서 총알을 꺼내옴
        if (reserveAmmo >= neededAmmo) // 예비탄창이 충분할 때
        {
            currentAmmo += neededAmmo; // 탄창 채우기
            reserveAmmoCounts[gunNameKey] -= neededAmmo; // 예비탄창 감소
        }
        else // 예비탄창이 부족할 때 (남은거 다 씀)
        {
            currentAmmo += reserveAmmo; // 남은 만큼만 채우기
            reserveAmmoCounts[gunNameKey] = 0; // 예비탄창 0
        }

        // 재장전된 탄창 상태를 딕셔너리에 저장
        currentMagazineAmmo[gunNameKey] = currentAmmo;

        Debug.Log($"장전 완료! [탄창: {currentAmmo}/{maxMagazine}] [예비: {reserveAmmoCounts[gunNameKey]}]");
        isReloading = false;

        // (UI 업데이트)
        // UIManager.instance.UpdateAmmo(currentAmmo, reserveAmmoCounts[gunNameKey]);
    }

    // 발사 로직
    void Shoot()
    {
        currentAmmo--; // 탄창에서 1발 감소

        // 발사 후 탄창 상태를 딕셔너리에 즉시 저장
        currentMagazineAmmo[currentGun.gunName] = currentAmmo;

        // (UI 업데이트)
        // UIManager.instance.UpdateAmmo(currentAmmo, reserveAmmoCounts[currentGun.gunName]);

        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger(FireHash);
        }

        if (playerController != null)
        {
            float currentRecoil = currentGun.normalRecoil;
            if (playerController.IsCrouching) currentRecoil = currentGun.crouchingRecoil;
            else if (playerController.IsSprinting) currentRecoil = currentGun.sprintingRecoil;
            playerController.ApplyRecoil(currentRecoil);
        }

        if (currentGun.fireMode == FireMode.Spread)
        {
            FireSpreadShot();
        }
        else
        {
            FireSingleShot();
        }
    }

    // 단발(Single) 또는 연사(Auto)용 발사 함수
    void FireSingleShot()
    {
        RaycastHit hit;
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        if (!isAiming && currentGun.spreadAngle > 0)
        {
            Vector2 spread = Random.insideUnitCircle * currentGun.spreadAngle;
            rayDirection = Quaternion.Euler(spread.x, spread.y, 0) * rayDirection;
        }

        Vector3 targetPoint;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, currentGun.range))
        {
            targetPoint = hit.point;
            HandleHit(hit, currentGun.damage);
        }
        else
        {
            targetPoint = rayOrigin + (rayDirection * currentGun.range);
        }

        SpawnBulletVisual(targetPoint);
    }

    // 샷건(Spread)용 발사 함수
    void FireSpreadShot()
    {
        for (int i = 0; i < currentGun.projectilesPerShot; i++)
        {
            RaycastHit hit;
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;

            Vector2 spread = Random.insideUnitCircle * currentGun.spreadAngle;
            rayDirection = Quaternion.Euler(spread.x, spread.y, 0) * rayDirection;

            Vector3 targetPoint;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, currentGun.range))
            {
                targetPoint = hit.point;
                HandleHit(hit, currentGun.damage);
            }
            else
            {
                targetPoint = rayOrigin + (rayDirection * currentGun.range);
            }

            SpawnBulletVisual(targetPoint);
        }
    }

    // 피격 처리를 담당하는 별도 함수
    void HandleHit(RaycastHit hit, int damageToDeal)
    {
        EnemyHealth enemy = hit.transform.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damageToDeal);
            if (currentGun.bloodImpactPrefab != null)
                Instantiate(currentGun.bloodImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
        else
        {
            if (currentGun.genericImpactPrefab != null)
                Instantiate(currentGun.genericImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }

    // 총알 프리팹 생성 로직
    void SpawnBulletVisual(Vector3 targetPoint)
    {
        if (currentGun.bulletPrefab != null && currentGun.firePoint != null)
        {
            Vector3 direction = targetPoint - currentGun.firePoint.position;
            Quaternion bulletRotation = Quaternion.LookRotation(direction);
            Instantiate(currentGun.bulletPrefab, currentGun.firePoint.position, bulletRotation);
        }
    }
}