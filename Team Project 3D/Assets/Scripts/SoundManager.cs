using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("배경음악 오디오 소스")]
    public AudioSource bgmSource;

    [Header("볼륨 설정")]
    [Range(0f, 1f)] public float masterVolume = 0.3f; // 기본값 0.3 (조절 가능)

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 변경 시에도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }

        if (bgmSource == null) bgmSource = GetComponent<AudioSource>();

        bgmSource.loop = true;
        bgmSource.volume = masterVolume; // 초기 볼륨 적용
    }

    private void Update()
    {
        // [편의 기능] 게임 도중에 슬라이더를 움직이면 즉시 볼륨에 반영됨
        if (bgmSource != null)
        {
            bgmSource.volume = masterVolume;
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        // 이미 같은 음악이 나오고 있다면 다시 틀지 않음
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = masterVolume; // 설정된 볼륨으로 재생
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }
}