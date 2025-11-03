using UnityEngine;

public class HealthPack : MonoBehaviour
{
    // 회복 가능한 체력 값들을 배열로 정의
    private int[] healAmounts = { 10, 20, 30 };

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트의 태그가 "Player"인지 확인
        if (other.CompareTag("Player"))
        {
            // 플레이어 오브젝트에서 PlayerHealth 스크립트를 가져옴
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // PlayerHealth 스크립트가 있다면
            if (playerHealth != null)
            {
                // 10, 20, 30 중에서 무작위로 회복량 선택
                int healAmount = healAmounts[Random.Range(0, healAmounts.Length)];

                // 플레이어의 체력 회복 함수 호출
                playerHealth.RestoreHealth(healAmount);

                // 아이템 오브젝트 파괴
                Destroy(gameObject);
            }
        }
    }
}