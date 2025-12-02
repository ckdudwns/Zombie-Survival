using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DynamiteController : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float delayTime = 3.0f;
    public float explosionRadius = 5.0f;
    public GameObject explosionEffect;

    [Header("Position Adjustment")]
    [Tooltip("이펙트 생성 위치를 조절합니다. (로컬 기준)\nZ값을 조절하여 이펙트가 벽 앞쪽에서 터지게 하세요.")]
    public Vector3 explosionOffset = new Vector3(0, 0, 1.0f);

    [Header("Tag Settings")]
    [Tooltip("폭발 시 같이 파괴될 주변 물체들의 태그 (보통 자기 자신도 이 태그를 가집니다)")]
    public string targetTag = "Destructible";

    [Header("Inventory Settings")]
    public string requiredItemName = "Dynamite";
    public bool consumeItem = true;

    [Header("Interaction Settings")]
    public string playerTag = "Player";

    // ▼ 사운드 관련 변수
    [Header("Audio Settings")]
    public AudioClip plantSound;
    public AudioClip explosionSound; // 👈 1. 인스펙터에 AudioClip 할당 필수
    private AudioSource audioSource;

    private bool isPlayerNearby = false;
    private bool isPlanted = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isPlanted) return;
            if (isPlayerNearby) AttemptPlanting();
        }
    }

    void AttemptPlanting()
    {
        if (InventoryManager.instance == null) return;

        if (InventoryManager.instance.HasItem(requiredItemName))
        {
            if (consumeItem) InventoryManager.instance.RemoveItemByName(requiredItemName);
            StartCoroutine(StartFuse());
        }
        else
        {
            Debug.Log($"설치 실패: '{requiredItemName}' 필요");
        }
    }

    IEnumerator StartFuse()
    {
        isPlanted = true;
        Debug.Log("카운트다운 시작! " + delayTime + "초 뒤 폭발합니다.");

        if (audioSource != null && plantSound != null)
        {
            audioSource.PlayOneShot(plantSound);
        }

        yield return new WaitForSeconds(delayTime);

        Explode();
    }

    void Explode()
    {
        Vector3 finalPos = transform.TransformPoint(explosionOffset);

        // 1. [수정] 설치 소리가 아직 재생 중이라면 멈춤 (깔끔한 처리)
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // 1. [수정] 폭발 소리 재생 로직 (할당되었는지 확인 필수)
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, 1.0f); // 볼륨 1.0f 명시
            Debug.Log($"[Sound] 폭발 소리 재생 완료: {explosionSound.name}");
        }
        else
        {
            // 폭발음이 할당되지 않은 경우 에러 로그 출력
            Debug.LogError("[Sound Error] DynamiteController의 Explosion Sound가 인스펙터에 할당되지 않았습니다!");
        }

        // 이펙트 생성 (로직 유지)
        if (explosionEffect != null)
        {
            GameObject effectInstance = Instantiate(explosionEffect, finalPos, transform.rotation);

            var ps = effectInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = 50;
                }
            }
        }

        // 범위 내 다른 파괴 가능 물체 제거 (로직 유지)
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.CompareTag(targetTag))
            {
                Destroy(nearbyObject.gameObject);
            }
        }

        // 퀘스트 매니저에게 폭발 알림 (로직 유지)
        if (QuestManager.instance != null)
        {
            QuestManager.instance.ProcessTunnelExplosion();
        }

        Debug.Log("장애물이 폭파되었습니다!");
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) isPlayerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag)) isPlayerNearby = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        Gizmos.color = Color.yellow;
        Vector3 previewPos = transform.TransformPoint(explosionOffset);
        Gizmos.DrawSphere(previewPos, 0.2f);
        Gizmos.DrawLine(transform.position, previewPos);
    }
}