using UnityEngine;
using System.Collections;

// 보급 상자와 드롭 확률을 묶는 데이터 구조
[System.Serializable]
public struct LootBoxDrop
{
    public GameObject lootBoxPrefab;
    [Range(0f, 100f)]
    public float dropChance;
}

public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public int maxHealth = 100;

    [Header("아이템 드롭 설정")]
    [Tooltip("설정된 확률에 따라 드롭될 보급 상자입니다.")]
    public LootBoxDrop lootBoxDrop;

    [Tooltip("보급 상자 드롭에 실패했을 때 대신 드롭될 코인 프리팹입니다.")]
    public GameObject coinPrefab;

    [Header("드롭 위치 설정")]
    [Tooltip("아이템을 드롭할 때 감지할 바닥의 레이어입니다.")]
    public LayerMask groundLayer;

    // --- 여기가 수정된 부분입니다 ---
    [Tooltip("코인이 바닥에서 얼마나 위쪽에 생성될지 정합니다.")]
    public float coinDropHeightOffset = 0.1f;

    [Tooltip("상자가 바닥에서 얼마나 위쪽에 생성될지 정합니다.")]
    public float lootBoxDropHeightOffset = 0.5f;
    // --- 여기까지 ---

    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Die();
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (!enabled) return;

        Debug.Log(gameObject.name + "가 쓰러졌습니다.");
        HandleDrops();
        enabled = false;
        Destroy(gameObject);
    }

    // 아이템 드롭을 총괄하는 함수
    void HandleDrops()
    {
        if (lootBoxDrop.lootBoxPrefab != null && lootBoxDrop.dropChance > 0)
        {
            float randomRoll = Random.Range(0f, 100f);
            if (randomRoll <= lootBoxDrop.dropChance)
            {
                // 상자를 드롭할 때는 상자 높이 오프셋을 전달
                DropSpecificItem(lootBoxDrop.lootBoxPrefab, lootBoxDropHeightOffset);
                return;
            }
        }

        if (coinPrefab != null)
        {
            // 코인을 드롭할 때는 코인 높이 오프셋을 전달
            DropSpecificItem(coinPrefab, coinDropHeightOffset);
        }
    }

    // 아이템을 실제로 바닥에 생성하는 함수 (높이 오프셋 값을 받도록 수정)
    void DropSpecificItem(GameObject itemPrefab, float heightOffset)
    {
        if (itemPrefab == null) return;

        Vector3 dropPosition = transform.position;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f, groundLayer))
        {
            // 전달받은 heightOffset 값을 사용하여 생성 높이를 조절
            dropPosition = hit.point + new Vector3(0, heightOffset, 0);
        }
        else
        {
            Debug.LogWarning(gameObject.name + " 아래에서 바닥을 찾지 못해 공중에 아이템을 드롭합니다.");
        }

        Instantiate(itemPrefab, dropPosition, Quaternion.identity);
    }
}