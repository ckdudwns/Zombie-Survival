using UnityEngine;

// IInteractable 인터페이스를 상속받도록 추가합니다.
public class Trap : MonoBehaviour, IInteractable
{
    [Header("함정 설정")]
    [Tooltip("이 반경 안에 있는 모든 적에게 영향을 줍니다.")]
    public float alertRadius = 20f;
    [Tooltip("효과가 지속될 시간(초)입니다.")]
    public float effectDuration = 10f;

    [Header("사운드 설정")]
    [Tooltip("함정을 발동시켰을 때 재생할 사운드 클립입니다.")]
    public AudioClip trapSound;

    // --- OnTriggerEnter는 삭제하고 아래 함수를 추가 ---
    // IInteractable 인터페이스의 필수 구현 함수
    public void Interact(GameObject player)
    {
        // (플레이어가 함정을 줍는 것이 아니라 '발동'시키는 것이므로
        // player 매개변수를 사용할 필요는 없지만, 인터페이스 규격상 받아야 합니다.)

        ActivateTrap();
        Destroy(gameObject); // 함정 아이템은 발동 즉시 사라짐
    }

    void ActivateTrap()
    {
        // 1. 사운드 재생
        if (trapSound != null)
        {
            AudioSource.PlayClipAtPoint(trapSound, transform.position);
        }

        // 2. 주변의 모든 적 찾기
        Collider[] colliders = Physics.OverlapSphere(transform.position, alertRadius);

        // 3. 찾은 적들에게 강화 효과 적용
        foreach (Collider col in colliders)
        {
            Enemy enemy = col.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                // 적의 강화 함수를 호출
                enemy.ActivateFrenzyMode(effectDuration);
            }
        }

        Debug.Log("함정 발동! 주변 적들이 광분합니다.");
    }

    // Scene 뷰에서 함정의 범위를 시각적으로 표시 (변경 없음)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, alertRadius);
    }
}