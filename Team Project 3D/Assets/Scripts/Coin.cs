using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("코인 설정")]
    [Tooltip("획득할 코인의 최소값")]
    public int minCoinValue = 50;
    [Tooltip("획득할 코인의 최대값")]
    public int maxCoinValue = 200;

    // 회전 기능은 그대로 유지
    [Header("회전 설정")]
    public float rotationSpeed = 50f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player playerScript = other.GetComponent<Player>();
            if (playerScript != null)
            {
                // --- 여기가 핵심 수정 부분입니다 ---
                // minCoinValue와 maxCoinValue 사이의 랜덤한 정수 값을 계산
                int amount = Random.Range(minCoinValue, maxCoinValue + 1);

                // 계산된 랜덤 값을 플레이어에게 전달
                playerScript.AddCoins(amount);
            }

            Destroy(gameObject);
        }
    }
}