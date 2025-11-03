using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public int damage = 10;

    // Trigger가 다른 Collider(플레이어)와 충돌했을 때 호출
    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 상대가 PlayerHealth 스크립트를 가지고 있는지 확인
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // 상대의 TakeDamage 함수를 호출하여 피해를 줌
            playerHealth.TakeDamage(damage);

            // 한 번의 공격에 여러 번 피해를 주지 않도록 즉시 비활성화
            gameObject.SetActive(false);
        }
    }
}