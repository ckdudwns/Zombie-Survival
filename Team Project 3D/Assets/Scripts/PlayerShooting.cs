using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

    [Header("오디오 설정")]
    public AudioSource audioSource;

<<<<<<< Updated upstream
=======
    [Header("UI")]
    public AmmoUI ammoUI;

>>>>>>> Stashed changes
    // --- Private 변수 ---
    private int currentAmmo;
    private bool isReloading = false;
    private float nextTimeToFire = 0f;
    private Player playerController;
    private Animator gunAnimator;
    private bool isAiming = false;
    private static readonly int IsAimingHash = Animator.StringToHash("IsAiming");
    private static readonly int FireHash = Animator.StringToHash("Fire");

    private Dictionary<string, int> reserveAmmoCounts = new Dictionary<string, int>();
    private Dictionary<string, int> currentMagazineAmmo = new Dictionary<string, int>();

    void Start()
    {
        playerController = GetComponent<Player>();
        normalFOV = playerCamera.fieldOfView;

        reserveAmmoCounts = new Dictionary<string, int>();
        currentMagazineAmmo = new Dictionary<string, int>();

        if (availableGuns != null && availableGuns.Count > 0)
        {
            foreach (Gun gun in availableGuns)
            {
                if (!reserveAmmoCounts.ContainsKey(gun.gunName))
                {
                    reserveAmmoCounts.Add(gun.gunName, gun.maxReserveAmmo);
                }
                if (!currentMagazineAmmo.ContainsKey(gun.gunName))
                {
                    int startAmmo = Mathf.Min(gun.startMagazineAmmo, gun.maxAmmo);
                    currentMagazineAmmo.Add(gun.gunName, startAmmo);
                }
            }
            EquipGun(0);
        }

        if (scopeOverlay != null) scopeOverlay.SetActive(false);
    }

    void Update()
    {
<<<<<<< Updated upstream
=======
        if (DialogueUI.instance != null && DialogueUI.instance.isDialogueOpen) return;
>>>>>>> Stashed changes
        if (Player.isPaused) return;

        HandleWeaponSwitching();

        if (currentGun == null) return;

        if (currentGun.isScopable) HandleAimingInput();

        if (isReloading) return;

        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
        {
            // 플레어건 자체 발사 로직 시도
            if (currentGun.TryCustomFire())
            {
                nextTimeToFire = Time.time + 1f / currentGun.fireRate;
            }
            else // 일반 총 발사
            {
                if (currentAmmo > 0)
                {
                    nextTimeToFire = Time.time + 1f / currentGun.fireRate;
                    Shoot();
                }
                else
                {
                    nextTimeToFire = Time.time + 0.3f;
                    if (audioSource != null && currentGun.emptyClipSound != null)
                    {
                        audioSource.PlayOneShot(currentGun.emptyClipSound);
                    }

                    string gunNameKey = currentGun.gunName;
                    if (reserveAmmoCounts.ContainsKey(gunNameKey) && reserveAmmoCounts[gunNameKey] > 0)
                    {
                        StartCoroutine(Reload());
                    }
                    else
                    {
                        Debug.Log(currentGun.gunName + "의 총알이 모두 소진되었습니다!");
                    }
                }
            }
        }

        if (!isAiming && Input.GetKeyDown(KeyCode.R))
        {
            if (currentAmmo >= currentGun.maxAmmo)
            {
                Debug.Log("탄창이 이미 가득 찼습니다.");
            }
            else
            {
                string gunNameKey = currentGun.gunName;
                if (reserveAmmoCounts.ContainsKey(gunNameKey) && reserveAmmoCounts[gunNameKey] > 0)
                {
                    StartCoroutine(Reload());
                }
                else
                {
                    Debug.Log(currentGun.gunName + "의 예비 총알이 부족합니다!");
                }
            }
        }
    }

    // ==========================================
    // [추가됨] QuestManager가 무기 해금 여부를 확인할 때 사용
    // ==========================================
    public bool IsGunUnlocked(string gunNameToCheck)
    {
        if (availableGuns == null) return false;

        foreach (Gun gun in availableGuns)
        {
            // 대소문자/공백 무시하고 비교
            string myGunName = gun.gunName.Replace(" ", "").ToLower();
            string targetName = gunNameToCheck.Replace(" ", "").ToLower();

            if (myGunName == targetName)
            {
                return true;
            }
        }
        return false;
    }

    void HandleAimingInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (isReloading) return;

            isAiming = !isAiming;
            if (gunAnimator != null) gunAnimator.SetBool(IsAimingHash, isAiming);

            if (isAiming)
            {
                if (scopeOverlay != null) scopeOverlay.SetActive(true);
                if (crosshairImage != null) crosshairImage.enabled = false;
                playerCamera.fieldOfView = zoomedFOV;
            }
            else
            {
                OnUnaim();
            }
        }
    }

    void OnUnaim()
    {
        if (scopeOverlay != null) scopeOverlay.SetActive(false);
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

        if (gunIndex == -1)
        {
            availableGuns.Add(gunPrefab);
            gunIndex = availableGuns.Count - 1;

            if (!reserveAmmoCounts.ContainsKey(gunPrefab.gunName))
            {
                reserveAmmoCounts.Add(gunPrefab.gunName, gunPrefab.maxReserveAmmo);
            }
            if (!currentMagazineAmmo.ContainsKey(gunPrefab.gunName))
            {
                currentMagazineAmmo.Add(gunPrefab.gunName, 0);
            }
        }
        EquipGun(gunIndex);
    }

    public void AddAmmo(int amount)
    {
        if (availableGuns == null || availableGuns.Count == 0) return;

        int randomIndex = Random.Range(0, availableGuns.Count);
        Gun randomGun = availableGuns[randomIndex];
        string gunNameKey = randomGun.gunName;

        if (!reserveAmmoCounts.ContainsKey(gunNameKey)) reserveAmmoCounts[gunNameKey] = 0;

        int currentReserve = reserveAmmoCounts[gunNameKey];
        int maxReserve = randomGun.maxReserveAmmo;
        int newReserve = Mathf.Min(currentReserve + amount, maxReserve);

        reserveAmmoCounts[gunNameKey] = newReserve;
        Debug.Log($"[총알 획득] '{gunNameKey}' 예비 총알 {amount} 획득 -> {newReserve}발");
    }

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
        currentAmmo = currentMagazineAmmo[gunNameKey];

        if (currentAmmo == 0 && reserveAmmoCounts[gunNameKey] > 0)
        {
            StartCoroutine(Reload());
        }

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
<<<<<<< Updated upstream
=======
        currentAmmo = currentMagazineAmmo[gunNameKey];
        UpdateAmmoUI();
>>>>>>> Stashed changes
    }

    IEnumerator Reload()
    {
        isReloading = true;
        if (audioSource != null && currentGun.reloadSound != null)
        {
            audioSource.PlayOneShot(currentGun.reloadSound);
        }

        if (gunAnimator != null) gunAnimator.SetTrigger("Reload");

        yield return new WaitForSeconds(currentGun.reloadTime);

        string gunNameKey = currentGun.gunName;
        int reserveAmmo = reserveAmmoCounts[gunNameKey];
        int maxMagazine = currentGun.maxAmmo;
        int neededAmmo = maxMagazine - currentAmmo;

        if (neededAmmo <= 0 || reserveAmmo <= 0)
        {
            isReloading = false;
            yield break;
        }

        if (reserveAmmo >= neededAmmo)
        {
            currentAmmo += neededAmmo;
            reserveAmmoCounts[gunNameKey] -= neededAmmo;
        }
        else
        {
            currentAmmo += reserveAmmo;
            reserveAmmoCounts[gunNameKey] = 0;
        }

        currentMagazineAmmo[gunNameKey] = currentAmmo;
        isReloading = false;
<<<<<<< Updated upstream
=======
        UpdateAmmoUI();
>>>>>>> Stashed changes
    }

    void Shoot()
    {
        if (audioSource != null && currentGun.fireSound != null)
        {
            audioSource.PlayOneShot(currentGun.fireSound);
        }

        currentAmmo--;
        currentMagazineAmmo[currentGun.gunName] = currentAmmo;

        if (gunAnimator != null) gunAnimator.SetTrigger(FireHash);

        if (currentGun.muzzleFlashEffect != null)
        {
            if (currentGun.muzzleFlashEffect.gameObject.activeInHierarchy == false)
            {
                currentGun.muzzleFlashEffect.gameObject.SetActive(true);
            }

            var renderer = currentGun.muzzleFlashEffect.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && renderer.enabled == false)
            {
                renderer.enabled = true;
            }

            currentGun.muzzleFlashEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            currentGun.muzzleFlashEffect.Play();
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
<<<<<<< Updated upstream
=======

        currentMagazineAmmo[currentGun.gunName] = currentAmmo;
        UpdateAmmoUI();
>>>>>>> Stashed changes
    }

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

    void SpawnBulletVisual(Vector3 targetPoint)
    {
        if (currentGun.bulletPrefab != null && currentGun.firePoint != null)
        {
            Vector3 direction = targetPoint - currentGun.firePoint.position;
            Quaternion bulletRotation = Quaternion.LookRotation(direction);
            Instantiate(currentGun.bulletPrefab, currentGun.firePoint.position, bulletRotation);
        }
    }
<<<<<<< Updated upstream
=======

    void UpdateAmmoUI()
    {
        if (ammoUI != null && currentGun != null)
        {
            ammoUI.SetAmmo(currentGun.gunName, currentAmmo, currentGun.maxAmmo, reserveAmmoCounts[currentGun.gunName]);
        }
    }
>>>>>>> Stashed changes
}