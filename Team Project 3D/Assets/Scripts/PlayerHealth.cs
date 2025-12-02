using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필요
using System.Collections;          // 코루틴(IEnumerator)을 위해 필요

// 이 스크립트를 추가하면 AudioSource가 자동으로 같이 추가됩니다.
[RequireComponent(typeof(AudioSource))]
public class PlayerHealth : MonoBehaviour
{
    [Header("체력 설정")]
    [Tooltip("플레이어의 최대 체력입니다.")]
    public int maxHealth = 100;

    [Header("사운드 설정")]
    public AudioClip hitSound; // 피격 시 재생할 소리

    [Header("씬 및 페이드 설정")]
    public string gameOverSceneName = "GameOver"; // 이동할 게임오버 씬 이름
    public float fadeDuration = 2.0f; // 화면이 어두워지는 시간 (2초)

    [Tooltip("화면을 가릴 검은색 패널의 CanvasGroup을 연결하세요.")]
    public CanvasGroup fadeScreen;    // 페이드 효과용 UI 패널

    private int currentHealth;
<<<<<<< Updated upstream
    private bool isDead = false; // 플레이어가 죽었는지 확인하는 변수
=======
    private bool isDead = false;
    private AudioSource audioSource;

    public HealthUI healthUI; // 체력바 UI 연결
>>>>>>> Stashed changes

    void Start()
    {
        currentHealth = maxHealth;
<<<<<<< Updated upstream
=======
        audioSource = GetComponent<AudioSource>();

        // UI 초기화
        if (healthUI != null)
            healthUI.SetHealth(currentHealth, maxHealth);

        // 페이드 화면이 연결되어 있다면, 시작할 때 투명하게 만들어서 게임이 보이게 함
        if (fadeScreen != null)
        {
            fadeScreen.alpha = 0f;
            fadeScreen.blocksRaycasts = false;
        }

>>>>>>> Stashed changes
        Debug.Log("게임 시작! 현재 체력: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        // 이미 죽었다면 추가 피해 없음
        if (isDead) return;

        currentHealth -= damage;
<<<<<<< Updated upstream
=======

        // 피격 소리 재생
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // 체력 UI 업데이트
        if (healthUI != null)
            healthUI.SetHealth(currentHealth, maxHealth);

>>>>>>> Stashed changes
        Debug.Log("플레이어가 " + damage + "의 피해를 입었습니다! 현재 체력: " + currentHealth);

        // 체력이 0 이하가 되면 사망 처리
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void RestoreHealth(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
<<<<<<< Updated upstream
        Debug.Log(amount + "만큼 체력 회복! 현재 체력: " + currentHealth);
=======

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        Debug.Log(amount + "만큼 체력 회복! 현재 체력: " + currentHealth);

        if (healthUI != null)
            healthUI.SetHealth(currentHealth, maxHealth);
>>>>>>> Stashed changes
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("플레이어가 쓰러졌습니다. 페이드 아웃 후 게임 오버!");

        // 1. 플레이어 조작 비활성화 (움직이거나 총 쏘지 못하게)
        if (GetComponent<Player>() != null) GetComponent<Player>().enabled = false;
        if (GetComponent<PlayerShooting>() != null) GetComponent<PlayerShooting>().enabled = false;

        // 2. 페이드 아웃 및 씬 이동 코루틴 시작
        StartCoroutine(FadeOutAndLoadScene());
    }

    // 화면을 서서히 어둡게 하고 씬을 이동하는 코루틴
    IEnumerator FadeOutAndLoadScene()
    {
        // 페이드 스크린이 연결 안 되어 있으면 경고 후 즉시 이동
        if (fadeScreen == null)
        {
            Debug.LogWarning("Fade Screen(Canvas Group)이 연결되지 않았습니다! 즉시 씬을 이동합니다.");
            SceneManager.LoadScene(gameOverSceneName);
            yield break;
        }

        // 페이드 시작
        float timer = 0f;
        fadeScreen.blocksRaycasts = true; // 페이드 중 마우스 클릭 등 차단

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // 알파값을 0(투명)에서 1(불투명 검정)로 부드럽게 변경
            fadeScreen.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null; // 다음 프레임까지 대기
        }

        // 확실하게 완전 검은색으로 설정
        fadeScreen.alpha = 1f;

        // 씬 넘어가기 전 상태 정리
        Time.timeScale = 1f;            // 시간 정상화
        Cursor.lockState = CursorLockMode.None; // 마우스 커서 잠금 해제
        Cursor.visible = true;          // 마우스 커서 보이기

        // 게임오버 씬 로드
        SceneManager.LoadScene(gameOverSceneName);
    }
<<<<<<< Updated upstream
=======

    public bool IsDead()
    {
        return isDead;
    }
>>>>>>> Stashed changes
}