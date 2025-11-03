using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSpeed = 50f;

    void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.forward * bulletSpeed;
        }
        Destroy(gameObject, 15f);
    }

    // 무언가와 충돌하면 데미지 없이 그냥 사라지기만 함
    void OnTriggerEnter(Collider other)
    {
        // 데미지 로직을 모두 삭제합니다.
        Destroy(gameObject);
    }
}