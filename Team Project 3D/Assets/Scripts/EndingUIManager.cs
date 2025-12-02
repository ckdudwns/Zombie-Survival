using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingUIManager : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu"; // 메인 메뉴 씬 이름

    // [추가된 부분] 씬이 시작될 때 마우스 커서 설정을 초기화합니다.
    private void Start()
    {
        // 커서 잠금 해제 (화면 밖으로 나갈 수 있게)
        Cursor.lockState = CursorLockMode.None;

        // 커서 보이게 설정
        Cursor.visible = true;
    }

    public void OnClickGoToMain()
    {
        Debug.Log("메인 메뉴로 이동합니다.");

        // 시간 조작 복구
        Time.timeScale = 1f;

        // 메인 메뉴 씬 로드
        SceneManager.LoadScene(mainMenuSceneName);
    }
}