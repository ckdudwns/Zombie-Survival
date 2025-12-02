using UnityEngine;
using System.Collections;

// 사용법: 이 스크립트를 "파괴할 장애물(벽, 바위 등)" 오브젝트에 추가하세요.
public class DynamiteController : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float delayTime = 3.0f;       // 설치 후 폭발까지 걸리는 시간
    public float explosionRadius = 5.0f; // 폭발 범위
    public GameObject explosionEffect;   // 폭발 시 나올 이펙트 (파티클)

    [Header("Position Adjustment")]
    [Tooltip("이펙트 생성 위치를 조절합니다. (로컬 기준)\nZ값을 조절하여 이펙트가 벽 앞쪽에서 터지게 하세요.")]
    public Vector3 explosionOffset = new Vector3(0, 0, 1.0f); // 기본값: 앞쪽(Z)으로 1만큼

    [Header("Tag Settings")]
    [Tooltip("폭발 시 같이 파괴될 주변 물체들의 태그 (보통 자기 자신도 이 태그를 가집니다)")]
    public string targetTag = "Destructible";

    [Header("Inventory Settings")]
    public string requiredItemName = "Dynamite";
    public bool consumeItem = true;

    [Header("Interaction Settings")]
    public string playerTag = "Player";

    private bool isPlayerNearby = false;
    private bool isPlanted = false;

    void Update()
    {
        // E키 입력 감지
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

        yield return new WaitForSeconds(delayTime);

        Explode();
    }

    void Explode()
    {
        // 로컬 좌표(Offset)를 월드 좌표로 변환하여 최종 폭발 위치 계산
        Vector3 finalPos = transform.TransformPoint(explosionOffset);

        // 이펙트 생성
        if (explosionEffect != null)
        {
            GameObject effectInstance = Instantiate(explosionEffect, finalPos, transform.rotation);

            var ps = effectInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();

                // 렌더링 순서 보정 (화면 맨 앞에 보이도록 설정)
                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = 50;
                }
            }
        }

        // 범위 내 다른 파괴 가능 물체 제거
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.CompareTag(targetTag))
            {
                Destroy(nearbyObject.gameObject);
            }
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

    // 에디터에서 범위와 위치를 확인하기 위한 Gizmo (게임 실행 중에는 안 보임)
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