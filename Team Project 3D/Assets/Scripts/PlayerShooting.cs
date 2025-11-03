using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour
{
    [Header("공통 설정")]
    public float range = 100f;

    [Header("무기 설정")]
    public List<Gun> availableGuns;
    public Transform gunHolder;
    private Gun currentGun;
    private int currentGunIndex = -1;

    [Header("조준 설정")]
    public float zoomedFOV = 15f;
    private float normalFOV;

    [Header("필수 연결 요소")]
    public Camera playerCamera;
    public Image crosshairImage;
    public GameObject scopeOverlay;

    // --- Private 변수 ---
    private int currentAmmo;
    private bool isReloading = false;
    private float nextTimeToFire = 0f;
    private Player playerController;
    private Animator gunAnimator;
    private bool isAiming = false;
    private static readonly int IsAimingHash = Animator.StringToHash("IsAiming");

    void Start()
    {
        playerController = GetComponent<Player>();
        normalFOV = playerCamera.fieldOfView;

        if (availableGuns != null && availableGuns.Count > 0)
        {
            EquipGun(0);
        }
        if (scopeOverlay != null)
            scopeOverlay.SetActive(false);
    }

    void Update()
    {
        if (Player.isPaused) return;

        HandleWeaponSwitching();

        if (currentGun == null) return;

        // --- 수정된 부분 ---
        // 현재 총이 '조준 가능(isScopable)'할 때만 조준 입력을 받습니다.
        if (currentGun.isScopable)
        {
            HandleAimingInput();
        }
        // --- 여기까지 ---

        if (isReloading) return;

        // 발사 입력 (마우스 좌클릭)
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
        {
            if (currentAmmo > 0)
            {
                nextTimeToFire = Time.time + 1f / currentGun.fireRate;
                Shoot();
            }
        }

        // 재장전 입력 (R키)
        if (!isAiming && Input.GetKeyDown(KeyCode.R) && currentAmmo < currentGun.maxAmmo)
        {
            StartCoroutine(Reload());
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

    // --- 조준 해제 시 호출할 함수를 따로 만듭니다 ---
    void OnUnaim()
    {
        if (scopeOverlay != null) scopeOverlay.SetActive(false);
        // 현재 총에 조준선 이미지가 설정되어 있다면 다시 보이기
        if (crosshairImage != null && currentGun != null && currentGun.crosshairSprite != null)
        {
            crosshairImage.enabled = true;
        }
        playerCamera.fieldOfView = normalFOV;
    }


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

    void EquipGun(int gunIndex)
    {
        if (isReloading)
        {
            StopAllCoroutines();
            isReloading = false;
        }

        // --- 수정된 부분 ---
        // 무기를 바꿀 때, 이전에 조준 상태였다면 강제로 조준 해제
        if (isAiming)
        {
            isAiming = false;
            OnUnaim(); // UI 및 FOV 즉시 복구
        }
        // --- 여기까지 ---

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

        currentAmmo = currentGun.maxAmmo;
        Debug.Log(currentGun.gunName + "으로 교체! 탄약: " + currentAmmo + "/" + currentGun.maxAmmo);
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

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log(currentGun.gunName + " 장전 중...");

        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("Reload");
        }

        yield return new WaitForSeconds(currentGun.reloadTime);

        currentAmmo = currentGun.maxAmmo;
        Debug.Log("장전 완료! 남은 총알: " + currentAmmo);
        isReloading = false;
    }

    void Shoot()
    {
        currentAmmo--;

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

        // 조준 중이 아닐 때만 탄 퍼짐 적용 (선택적)
        if (!isAiming && currentGun.spreadAngle > 0)
        {
            Vector2 spread = Random.insideUnitCircle * currentGun.spreadAngle;
            rayDirection = Quaternion.Euler(spread.x, spread.y, 0) * rayDirection;
        }

        Vector3 targetPoint;
        // 레이캐스트 사거리를 currentGun.range로 변경
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, currentGun.range))
        {
            targetPoint = hit.point;
            HandleHit(hit, currentGun.damage); // 피격 처리 함수 호출
        }
        else
        {
            // --- 여기가 수정된 부분입니다 ---
            targetPoint = rayOrigin + (rayDirection * currentGun.range);
            // --- 여기까지 ---
        }

        // 총알 프리팹 생성
        SpawnBulletVisual(targetPoint);
    }

    // 샷건(Spread)용 발사 함수
    void FireSpreadShot()
    {
        // 설정된 펠릿 개수만큼 반복
        for (int i = 0; i < currentGun.projectilesPerShot; i++)
        {
            RaycastHit hit;
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;

            // 샷건은 항상 탄 퍼짐 적용
            Vector2 spread = Random.insideUnitCircle * currentGun.spreadAngle;
            rayDirection = Quaternion.Euler(spread.x, spread.y, 0) * rayDirection;

            Vector3 targetPoint;
            // 레이캐스트 사거리를 currentGun.range로 변경
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, currentGun.range))
                {
                targetPoint = hit.point;
                HandleHit(hit, currentGun.damage); // 피격 처리 함수 호출
            }
            else
            {
                // --- 여기가 수정된 부분입니다 ---
                targetPoint = rayOrigin + (rayDirection * currentGun.range);
                // --- 여기까지 ---
            }

            // 총알(펠릿) 프리팹 생성
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