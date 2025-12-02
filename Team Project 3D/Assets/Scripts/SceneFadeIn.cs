using UnityEngine;
using UnityEngine.UI; // 혹은 TMPro
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    // 엔딩 씬의 Canvas 안에 꽉 찬 검은색 Image를 만드세요.
    public Image blackPanel;
    public float fadeDuration = 3.0f; // 밝아지는 데 걸리는 시간

    void Start()
    {
        if (blackPanel != null)
        {
            // 처음에 완전 검은색으로 시작
            blackPanel.color = new Color(0, 0, 0, 1);
            blackPanel.gameObject.SetActive(true);
            StartCoroutine(DoFadeIn());
        }
    }

    IEnumerator DoFadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration); // 1(검정) -> 0(투명)
            blackPanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        blackPanel.gameObject.SetActive(false);
    }
}