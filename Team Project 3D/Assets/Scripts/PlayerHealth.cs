using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("체력 설정")]
    [Tooltip("플레이어의 최대 체력입니다.")]
    public int maxHealth = 100;

    private int currentHealth;
    private bool isDead = false; // 플레이어가 죽었는지 확인하는 변수

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log("게임 시작! 현재 체력: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        // 이미 죽었다면 피해를 받지 않음
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("플레이어가 " + damage + "의 피해를 입었습니다! 현재 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void RestoreHealth(int amount)
    {
        // 죽은 상태에서는 체력 회복 불가
        if (isDead) return;

        currentHealth += amount;
        Debug.Log(amount + "만큼 체력 회복! 현재 체력: " + currentHealth);
    }

    void Die()
    {
        // 중복 실행 방지
        if (isDead) return;
        isDead = true;

        Debug.Log("플레이어가 쓰러졌습니다. 게임 오버!");

        // --- 여기가 추가된 부분입니다 ---

        // 1. 게임 시간을 멈춥니다.
        Time.timeScale = 0f;

        // 2. 마우스 커서를 보이게 하고 잠금을 해제합니다.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. (선택 사항) 플레이어의 움직임과 사격 스크립트를 비활성화합니다.
        // 이렇게 하면 게임이 멈춘 상태에서 다른 스크립트들이 작동하는 것을 방지할 수 있습니다.
        GetComponent<Player>().enabled = false;
        GetComponent<PlayerShooting>().enabled = false;

        // --- 여기까지 ---

        // Destroy(gameObject); // 플레이어 오브젝트를 파괴하는 대신, 게임을 멈추도록 변경합니다.
    }
}