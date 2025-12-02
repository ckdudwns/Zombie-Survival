using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수

public class MainMenuManager : MonoBehaviour
{
    [Header("이동할 게임 씬 이름")]
    // 주의: 실제 게임 플레이 씬의 이름을 여기에 정확히 적어야 합니다.
    public string gameSceneName = "GameScene";

    // 시작 버튼을 눌렀을 때 실행될 함수
    public void OnClickStart()
    {
        Debug.Log("게임 시작!");
        SceneManager.LoadScene(gameSceneName);
    }

    // 종료 버튼을 눌렀을 때 실행될 함수
    public void OnClickExit()
    {
        Debug.Log("게임 종료");

        // 에디터에서 실행 중일 때는 플레이 모드를 끄고, 실제 빌드에서는 프로그램을 종료함
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}