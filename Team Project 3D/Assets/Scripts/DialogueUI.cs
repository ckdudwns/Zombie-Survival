using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI instance;

    [Header("UI Components")]
    public GameObject panel;
    public TMP_Text text;
    public AudioSource audioSource; // 일반 대사 스피커 (스킵 시 끊김)

    private string[] lines;
    private AudioClip[] sounds;
    private float[] durations;
    private bool[] keeps; // [추가] 스킵해도 유지할지 여부

    private int index;
    private System.Action onFinished;
    private Coroutine autoStopCoroutine;

    [Header("State")]
    public bool isDialogueOpen = false;
    public bool isFinished = false;

    private void Awake()
    {
        instance = this;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    // [변경] keepSounds(유지 여부 배열) 파라미터 추가
    public void ShowDialogue(string[] dialogueLines, AudioClip[] dialogueSounds, float[] soundDurations, bool[] keepSounds, System.Action finishedCallback = null)
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        lines = dialogueLines;
        sounds = dialogueSounds;
        durations = soundDurations;
        keeps = keepSounds; // 데이터 저장

        index = 0;
        onFinished = finishedCallback;

        panel.SetActive(true);
        isDialogueOpen = true;
        isFinished = false;

        audioSource.Stop();
        StopAutoStopCoroutine();

        UpdateDialogue();

        Player.isPaused = true;
        Time.timeScale = 0f;
    }

    // (호환용 오버로딩)
    public void ShowDialogue(string[] dialogueLines, System.Action finishedCallback = null)
    {
        ShowDialogue(dialogueLines, null, null, null, finishedCallback);
    }

    private void Update()
    {
        if (!panel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // [핵심] 일반 소리만 끕니다. (따로 생성한 효과음은 건드리지 않음)
            if (audioSource.isPlaying) audioSource.Stop();
            StopAutoStopCoroutine();

            index++;

            if (index < lines.Length)
            {
                UpdateDialogue();
                return;
            }

            StartCoroutine(CloseDialogue());
        }
    }

    private void UpdateDialogue()
    {
        text.text = lines[index];

        // 소리 데이터가 있는지 확인
        if (sounds != null && index < sounds.Length && sounds[index] != null)
        {
            // 재생 시간 (0이면 클립 전체 길이)
            float duration = (durations != null && index < durations.Length && durations[index] > 0)
                             ? durations[index]
                             : sounds[index].length;

            // 유지 여부 확인
            bool isKeep = (keeps != null && index < keeps.Length && keeps[index]);

            if (isKeep)
            {
                // [유지 모드] 임시 스피커를 만들어서 재생 (스페이스 눌러도 영향 안 받음)
                StartCoroutine(PlayPersistentSound(sounds[index], duration));
            }
            else
            {
                // [일반 모드] 기존 스피커 사용 (스페이스 누르면 꺼짐)
                audioSource.PlayOneShot(sounds[index]);

                // 시간 설정이 있다면 자동 정지 예약
                if (durations != null && index < durations.Length && durations[index] > 0)
                {
                    autoStopCoroutine = StartCoroutine(StopMainSoundDelay(duration));
                }
            }
        }
    }

    // 끊기지 않는 소리를 재생하는 코루틴
    IEnumerator PlayPersistentSound(AudioClip clip, float duration)
    {
        // 1. 임시 게임오브젝트 생성
        GameObject tempGO = new GameObject("TempAudio_" + clip.name);
        tempGO.transform.SetParent(this.transform); // 정리하기 쉽게 자식으로

        // 2. 오디오 소스 추가 및 설정
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.playOnAwake = false;
        tempSource.spatialBlend = 0f; // 2D 사운드
        tempSource.volume = audioSource.volume; // 볼륨 맞춤

        // 3. 재생
        tempSource.Play();

        // 4. 시간만큼 대기 (Time.timeScale이 0일 수 있으니 Realtime 사용)
        yield return new WaitForSecondsRealtime(duration);

        // 5. 삭제 (소리가 서서히 줄어들게 하려면 여기에 페이드 아웃 추가 가능)
        Destroy(tempGO);
    }

    IEnumerator StopMainSoundDelay(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        if (audioSource.isPlaying) audioSource.Stop();
    }

    void StopAutoStopCoroutine()
    {
        if (autoStopCoroutine != null)
        {
            StopCoroutine(autoStopCoroutine);
            autoStopCoroutine = null;
        }
    }

    private IEnumerator CloseDialogue()
    {
        if (audioSource.isPlaying) audioSource.Stop();
        StopAutoStopCoroutine();

        yield return null;
        while (Input.GetKey(KeyCode.Space))
            yield return null;

        panel.SetActive(false);
        isDialogueOpen = false;
        isFinished = true;

        Player.isPaused = false;
        Time.timeScale = 1f;

        yield return null;
        onFinished?.Invoke();
    }
}