using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("함정 설정")]
    [Tooltip("이 반경 안에 있는 모든 적에게 영향을 줍니다.")]
    public float alertRadius = 20f;
    [Tooltip("효과가 지속될 시간(초)입니다.")]
    public float effectDuration = 10f;

    [Header("사운드 설정")]
    [Tooltip("함정을 발동시켰을 때 재생할 사운드 클립입니다.")]
    public AudioClip trapSound;

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 아이템을 먹었을 때
        if (other.CompareTag("Player"))
        {
            ActivateTrap();
            Destroy(gameObject);
        }
    }

    void ActivateTrap()
    {
        Debug.Log("함정 발동!");
        // 1. 사운드 재생
        if (trapSound != null)
        {
            // 3D 공간의 아이템 위치에서 사운드를 한 번 재생하고 사라지게 함
            AudioSource.PlayClipAtPoint(trapSound, transform.position);
        }

        // 2. 주변의 모든 적 찾기
        // alertRadius 반경 안의 모든 콜라이더를 찾아냄
        Collider[] colliders = Physics.OverlapSphere(transform.position, alertRadius);

        // 3. 찾은 적들에게 강화 효과 적용
        foreach (Collider col in colliders)
        {
            // 찾은 콜라이더에서 EnemyAI 스크립트를 가져옴
            Enemy enemy = col.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                // 적의 강화 함수를 호출
                enemy.ActivateFrenzyMode(effectDuration);
            }
        }
    }

    // Scene 뷰에서 함정의 범위를 시각적으로 표시
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, alertRadius);
    }
}