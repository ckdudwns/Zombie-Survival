using UnityEngine;

// 아이템과 드롭 확률을 묶는 데이터 구조
[System.Serializable]
public struct ItemDrop
{
    public GameObject itemPrefab;
    [Range(0f, 100f)]
    public float dropChance;
}

public class LootBox : MonoBehaviour
{
    [Header("확정 보상 (자동 획득)")]
    [Tooltip("상자를 열면 무조건 얻는 코인의 최소값")]
    public int minCoinValue = 50;
    [Tooltip("상자를 열면 무조건 얻는 코인의 최대값")]
    public int maxCoinValue = 200;

    [Header("랜덤 보상 목록 (바닥에 드롭)")]
    [Tooltip("이 상자에서 나올 수 있는 추가 아이템과 확률입니다.")]
    public ItemDrop[] possibleItems; // 상자에 들어있는 랜덤 아이템 목록

    private bool isOpened = false; // 상자가 이미 열렸는지 확인

    // 플레이어가 상자에 닿았을 때
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어만 상자를 열 수 있고, 한 번만 열리도록 함
        if (other.CompareTag("Player") && !isOpened)
        {
            isOpened = true; // 중복 실행 방지

            Player playerScript = other.GetComponent<Player>();
            if (playerScript != null)
            {
                // 1. 코인은 즉시, 자동으로 지급 (프리팹 생성 X)
                int coinAmount = Random.Range(minCoinValue, maxCoinValue + 1);
                playerScript.AddCoins(coinAmount); // Player.cs의 AddCoins 함수 직접 호출
            }

            // 2. 추가 랜덤 아이템 드롭 시도
            DropRandomItem();

            // 3. 상자 오브젝트 파괴
            Destroy(gameObject);
        }
    }

    // 랜덤 아이템을 바닥에 생성하는 함수
    void DropRandomItem()
    {
        float randomRoll = Random.Range(0f, 100f);
        float cumulativeChance = 0f;

        foreach (var drop in possibleItems)
        {
            cumulativeChance += drop.dropChance;
            if (randomRoll <= cumulativeChance)
            {
                if (drop.itemPrefab != null)
                {
                    Instantiate(drop.itemPrefab, transform.position, Quaternion.identity);
                    Debug.Log("상자에서 " + drop.itemPrefab.name + " 아이템이 나왔습니다!");
                }
                return; // 아이템 하나만 드롭
            }
        }

        Debug.Log("상자에서 추가 아이템은 나오지 않았습니다.");
    }
}