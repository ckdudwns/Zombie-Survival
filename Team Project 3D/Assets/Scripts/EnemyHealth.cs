using UnityEngine;
using System.Collections;

// (참고) 기존의 LootBoxDrop 구조체는 더 이상 사용하지 않습니다.
// [System.Serializable]
// public struct LootBoxDrop { ... }

public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public int maxHealth = 100;

    [Header("아이템 드롭 설정")]
    [Tooltip("모든 적이 공통으로 사용할 '빈 상자' LootBox 프리팹")]
    public GameObject lootBoxPrefab; // 공통 루트박스 프리팹

    [Tooltip("이 적이 실제로 드롭할 아이템 목록 데이터 (LootTable 에셋)")]
    public LootTable enemyLootTable; // 이 적 전용 루트 테이블

    [Tooltip("루트박스 드롭 확률")]
    [Range(0f, 100f)]
    public float dropChance = 100f;

    [Header("드롭 위치 설정")]
    [Tooltip("아이템을 드롭할 때 감지할 바닥의 레이어입니다.")]
    public LayerMask groundLayer;

    [Tooltip("상자가 바닥에서 얼마나 위쪽에 생성될지 정합니다.")]
    public float lootBoxDropHeightOffset = 0.5f;

    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        // 테스트용: K키로 즉시 사망
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
        // 중복 사망 방지
        if (!enabled) return;

        Debug.Log(gameObject.name + "가 쓰러졌습니다.");
        HandleDrops(); // 드롭 로직 호출

        enabled = false; // 스크립트 비활성화
        Destroy(gameObject); // 오브젝트 파괴
    }

    /// <summary>
    /// 아이템 드롭을 총괄하는 함수 (LootTable 방식)
    /// </summary>
    void HandleDrops()
    {
        // 루트박스 프리팹과 루트 테이블이 모두 연결되어 있는지 확인
        if (lootBoxPrefab != null && enemyLootTable != null)
        {
            float randomRoll = Random.Range(0f, 100f);
            if (randomRoll <= dropChance)
            {
                // LootBox를 생성하고, 이 적의 LootTable을 주입합니다.
                DropLootBox(lootBoxPrefab, lootBoxDropHeightOffset, enemyLootTable);
            }
            else
            {
                Debug.Log(gameObject.name + "가 루트박스를 드롭하지 않았습니다. (확률 실패)");
            }
        }
        else
        {
            Debug.LogWarning(gameObject.name + "의 EnemyHealth에 lootBoxPrefab 또는 enemyLootTable이 연결되지 않았습니다.");
        }
    }

    /// <summary>
    /// LootBox를 생성하고 LootTable 데이터를 주입하는 함수
    /// </summary>
    void DropLootBox(GameObject itemPrefab, float heightOffset, LootTable tableToAssign)
    {
        Vector3 dropPosition = transform.position;
        RaycastHit hit;

        // 바닥 감지
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f, groundLayer))
        {
            // 바닥에서 설정한 높이만큼 위에 생성
            dropPosition = hit.point + new Vector3(0, heightOffset, 0);
        }
        else
        {
            Debug.LogWarning(gameObject.name + " 아래에서 바닥을 찾지 못해 공중에 아이템을 드롭합니다.");
        }

        // 1. LootBox 인스턴스 생성
        GameObject boxInstance = Instantiate(itemPrefab, dropPosition, Quaternion.identity);

        // 2. 생성된 LootBox에서 LootBox.cs 스크립트를 찾음
        LootBox boxScript = boxInstance.GetComponent<LootBox>();
        if (boxScript != null)
        {
            // 3. (핵심) LootBox에 이 적의 LootTable 데이터를 주입
            boxScript.Initialize(tableToAssign);
        }
        else
        {
            Debug.LogError("LootBox 프리팹에 LootBox.cs 스크립트가 없습니다!");
        }
    }
}