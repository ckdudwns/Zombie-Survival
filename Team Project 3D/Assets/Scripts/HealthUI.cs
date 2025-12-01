using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthUI : MonoBehaviour
{
    [Header("UI 요소")]
    public Slider healthSlider;
    public Image fillImage; // 체력바 색 변경
    public Image damageFlashImage;  // 전체 화면 빨간 플래시 이미지

    [Header("애니메이션 설정")]
    public float smoothSpeed = 20f; // 체력바 Lerp 속도
    public float flashDuration = 0.07f; // 체력바 깜빡임 속도
    public float flashAlpha = 0.35f; // 화면 플래시 강도
    public float shakeAmount = 8f; // 체력바 흔들림 강도
    public float shakeDuration = 0.15f; // 흔들림 시간

    [Header("자연 회복 옵션")]
    public bool autoRegen = false;
    public int regenAmount = 1;
    public float regenInterval = 1.0f;

    private float targetValue;
    private Vector2 originalPos;
    private Coroutine flashRoutine;

    void Start()
    {
        if (healthSlider == null) return;

        originalPos = healthSlider.transform.localPosition;

        if (autoRegen)
            StartCoroutine(AutoRegenRoutine());
    }

    public void SetHealth(int current, int max)
    {
        // 슬라이더 갱신 준비
        healthSlider.maxValue = max;
        targetValue = current;
        healthSlider.value = current;

        // 부드러운 체력바 이동
        StopCoroutine(nameof(SmoothHealthBar));
        StartCoroutine(nameof(SmoothHealthBar));

        // 색상 업데이트
        UpdateColor();

        // 데미지 받았으면 깜빡임 + 화면 플래시 + 흔들림
        if (current < max)
        {
            // 체력바 깜빡임
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashDamageEffect());

            // 화면 플래시
            StartCoroutine(ScreenFlash());

            // 체력바 흔들림
            StartCoroutine(ShakeHealthBar());
        }
    }

    private IEnumerator SmoothHealthBar()
    {
        while (Mathf.Abs(healthSlider.value - targetValue) > 0.01f)
        {
            healthSlider.value = Mathf.Lerp(
                healthSlider.value,
                targetValue,
                smoothSpeed * Time.unscaledDeltaTime
            );
            yield return null;
        }

        healthSlider.value = targetValue;
    }

    private void UpdateColor()
    {
        float percent = healthSlider.value / healthSlider.maxValue;

        if (percent > 0.6f)
            fillImage.color = Color.green;
        else if (percent > 0.3f)
            fillImage.color = Color.yellow;
        else
            fillImage.color = Color.red;
    }

    private IEnumerator FlashDamageEffect()
    {
        Color original = fillImage.color;

        fillImage.color = Color.white;
        yield return new WaitForSecondsRealtime(flashDuration);

        UpdateColor(); // 원래 색 복귀
    }

    private IEnumerator ScreenFlash()
    {
        damageFlashImage.gameObject.SetActive(true);

        // 알파 상승
        damageFlashImage.color = new Color(1, 0, 0, flashAlpha);
        yield return new WaitForSecondsRealtime(0.05f);

        // 알파 서서히 감소
        for (float t = flashAlpha; t > 0; t -= Time.unscaledDeltaTime * 2)
        {
            damageFlashImage.color = new Color(1, 0, 0, t);
            yield return null;
        }

        damageFlashImage.gameObject.SetActive(false);
    }

    private IEnumerator ShakeHealthBar()
    {
        float timer = 0;
        while (timer < shakeDuration)
        {
            Vector2 offset = Random.insideUnitCircle * shakeAmount;
            healthSlider.transform.localPosition = originalPos + offset;

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        healthSlider.transform.localPosition = originalPos;
    }

    private IEnumerator AutoRegenRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(regenInterval);

            PlayerHealth ph = FindObjectOfType<PlayerHealth>();
            if (ph != null && !ph.IsDead())
                ph.RestoreHealth(regenAmount);
        }
    }
}
