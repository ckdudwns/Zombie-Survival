using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ZombieSoundController : MonoBehaviour
{
    [Header("오디오 클립")]
    [Tooltip("좀비 울음소리 파일들을 여기에 여러 개 넣으세요")]
    public AudioClip[] zombieClips;

    [Header("울음소리 간격 (초)")]
    public float minInterval = 3.0f; // 최소 대기 시간
    public float maxInterval = 7.0f; // 최대 대기 시간

    [Header("3D 사운드 거리 설정")]
    public float maxDistance = 20.0f; // 이 거리 밖에서는 안 들림

    private AudioSource audioSource;
    private Coroutine soundCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 3D 사운드 자동 설정 (중요!)
        audioSource.spatialBlend = 1.0f; // 3D 사운드로 설정
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic; // 거리에 따라 자연스럽게 작아짐
        audioSource.minDistance = 1.0f;
        audioSource.maxDistance = maxDistance;
        audioSource.playOnAwake = false;
        audioSource.loop = false; // 루프 끄기 (코루틴으로 제어함)

        // 소리 재생 시작
        soundCoroutine = StartCoroutine(PlayZombieSoundsLoop());
    }

    // 좀비가 비활성화되면(죽으면) 소리 루프도 중지
    void OnDisable()
    {
        if (soundCoroutine != null) StopCoroutine(soundCoroutine);
    }

    IEnumerator PlayZombieSoundsLoop()
    {
        while (true)
        {
            // 1. 랜덤한 시간만큼 대기 (불규칙하게 울어야 무서움)
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            // 2. 소리 재생 (주신 로직 활용)
            PlayRandomZombieSound();
        }
    }

    // 작성해주신 로직 적용
    public void PlayRandomZombieSound()
    {
        if (zombieClips.Length == 0) return;

        // 1. 클립 랜덤 선택
        int randomIndex = Random.Range(0, zombieClips.Length);
        audioSource.clip = zombieClips[randomIndex];

        // 2. 피치(음정) 랜덤 조절 (0.8 ~ 1.2)
        // 낮은 피치는 덩치 큰 괴물, 높은 피치는 날카로운 괴물 느낌
        audioSource.pitch = Random.Range(0.8f, 1.2f);

        // 3. 볼륨 랜덤 조절 (0.8 ~ 1.0)
        audioSource.volume = Random.Range(0.8f, 1.0f);

        audioSource.Play();
    }
}