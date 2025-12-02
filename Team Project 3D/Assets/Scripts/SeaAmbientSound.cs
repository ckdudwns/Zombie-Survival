using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SeaAmbientSound : MonoBehaviour
{
    [Header("오디오 설정")]
    public AudioClip waveSound; // 파도 소리 파일
    [Range(0f, 1f)] public float volume = 0.5f; // 소리 크기

    [Header("거리 설정")]
    public float minDistance = 5.0f;  // 이 거리 안에서는 최대 볼륨으로 들림
    public float maxDistance = 50.0f; // 이 거리 밖에서는 소리가 안 들림

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 오디오 소스 설정 (3D 사운드)
        audioSource.clip = waveSound;
        audioSource.loop = true; // 계속 재생 (반복)
        audioSource.volume = volume;
        audioSource.spatialBlend = 1.0f; // 완전한 3D 사운드로 설정
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic; // 거리에 따라 자연스럽게 줄어듦
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.playOnAwake = true; // 시작하자마자 재생

        audioSource.Play();
    }

    // 에디터에서 소리 범위를 눈으로 확인하기 위한 기능
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minDistance); // 최소 거리 (진하게)

        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, maxDistance); // 최대 거리 (연하게)
    }
}