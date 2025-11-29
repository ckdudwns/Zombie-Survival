using UnityEngine;
using System.Collections;

public class Flaregunmodified : MonoBehaviour
{

    public Rigidbody flareBullet;
    public Transform barrelEnd;
    public GameObject muzzleParticles;
    public AudioClip flareShotSound;
    public AudioClip noAmmoSound;
    public AudioClip reloadSound;
    public int bulletSpeed = 2000;
    public int maxSpareRounds = 5;
    public int spareRounds = 3;
    public int currentRound = 0;

    [Header("헬리콥터 구조 시스템")]
    public HelicopterRescueSystem helicopterSystem;  // 헬리콥터 시스템 연결
    public Transform flareZone;  // 발사 가능 구역
    public float zoneRadius = 10f;  // 구역 반경
    public bool requireZone = true;  // 구역 필요 여부

    private bool isInZone = false;
    private Transform player;

    void Start()
    {
        // 플레이어 찾기
        player = transform.parent;
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // 헬리콥터 시스템 자동 찾기
        if (helicopterSystem == null)
        {
            helicopterSystem = FindObjectOfType<HelicopterRescueSystem>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 구역 체크
        if (requireZone && flareZone != null && player != null)
        {
            CheckZone();
        }
        else
        {
            isInZone = true;  // 구역 필요 없으면 항상 true
        }

        if (Input.GetButtonDown("Fire1") && !GetComponent<Animation>().isPlaying)
        {
            if (currentRound > 0)
            {
                // 구역 체크
                if (requireZone && !isInZone)
                {
                    Debug.Log(" 플레어 발사 구역이 아닙니다!");
                    GetComponent<AudioSource>().PlayOneShot(noAmmoSound);
                }
                else
                {
                    Shoot();
                }
            }
            else
            {
                GetComponent<Animation>().Play("noAmmo");
                GetComponent<AudioSource>().PlayOneShot(noAmmoSound);
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !GetComponent<Animation>().isPlaying)
        {
            Reload();
        }
    }

    void CheckZone()
    {
        bool wasInZone = isInZone;
        float distance = Vector3.Distance(player.position, flareZone.position);
        isInZone = distance <= zoneRadius;

        if (isInZone && !wasInZone)
        {
            Debug.Log(" 플레어 발사 구역 진입!");
        }
        else if (!isInZone && wasInZone)
        {
            Debug.Log("플레어 발사 구역을 벗어났습니다!");
        }
    }

    void Shoot()
    {
        currentRound--;
        if (currentRound <= 0)
        {
            currentRound = 0;
        }

        GetComponent<Animation>().CrossFade("Shoot");
        GetComponent<AudioSource>().PlayOneShot(flareShotSound);

        Rigidbody bulletInstance;
        bulletInstance = Instantiate(flareBullet, barrelEnd.position, barrelEnd.rotation) as Rigidbody;
        bulletInstance.AddForce(barrelEnd.forward * bulletSpeed);

        Instantiate(muzzleParticles, barrelEnd.position, barrelEnd.rotation);

        // 헬리콥터 호출!
        if (helicopterSystem != null)
        {
            Vector3 landingPos = flareZone != null ? flareZone.position : player.position;
            helicopterSystem.CallHelicopter(landingPos);
            Debug.Log(" 구조 헬리콥터 호출!");
        }
        else
        {
            Debug.LogWarning("헬리콥터 시스템이 연결되지 않았습니다!");
        }
    }

    void Reload()
    {
        if (spareRounds >= 1 && currentRound == 0)
        {
            GetComponent<AudioSource>().PlayOneShot(reloadSound);
            spareRounds--;
            currentRound++;
            GetComponent<Animation>().CrossFade("Reload");
        }
    }

    // Scene 뷰에서 구역 표시
    void OnDrawGizmos()
    {
        if (flareZone != null && requireZone)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawSphere(flareZone.position, zoneRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(flareZone.position, zoneRadius);
        }
    }
}