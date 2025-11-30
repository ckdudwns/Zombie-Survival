using UnityEngine;

public class SmartphoneController : MonoBehaviour
{
    [Header("화면 오브젝트 연결")]
    public GameObject homeScreen;       // 홈 화면
    public GameObject messageAppScreen; // 메시지 목록
    public GameObject chatRoomScreen;   // 채팅방
    public GameObject mapAppScreen;     // [추가됨] 지도 앱 화면

    // --- [1. 앱 진입/이동 함수들] ---

    // 메시지 앱 열기
    public void OpenMessageApp()
    {
        CloseAllScreens();
        messageAppScreen.SetActive(true);
    }

    // 채팅방 들어가기
    public void EnterChatRoom()
    {
        CloseAllScreens();
        chatRoomScreen.SetActive(true);
    }

    // [추가됨] 지도 앱 열기 (지도 아이콘 클릭 시 실행)
    public void OpenMapApp()
    {
        CloseAllScreens();
        mapAppScreen.SetActive(true);
    }

    // --- [2. 네비게이션 버튼 기능 (홈키, 백키)] ---

    // 🏠 홈 버튼 기능 (무조건 홈으로)
    public void OnHomeButton()
    {
        CloseAllScreens();
        homeScreen.SetActive(true);
    }

    // 🔙 뒤로가기 버튼 기능 (상황에 따라 다르게)
    public void OnBackButton()
    {
        // 1. 만약 '채팅방'이 켜져 있다면 -> '메시지 목록'으로
        if (chatRoomScreen.activeSelf)
        {
            CloseAllScreens();
            messageAppScreen.SetActive(true);
        }
        // 2. [추가됨] 만약 '지도 앱'이 켜져 있다면 -> '홈'으로
        else if (mapAppScreen.activeSelf)
        {
            OnHomeButton();
        }
        // 3. 만약 '메시지 목록'이 켜져 있다면 -> '홈'으로
        else if (messageAppScreen.activeSelf)
        {
            OnHomeButton(); // 홈으로 이동
        }
        // 4. 만약 이미 '홈'이라면? -> (선택사항) 폰을 끄거나 아무것도 안 함
        else if (homeScreen.activeSelf)
        {
            Debug.Log("이미 홈 화면입니다.");
            // 폰을 아예 끄고 싶다면 여기에 Player 스크립트의 폰 끄기 함수 호출
        }
    }

    // --- [편의 기능] ---
    // 모든 화면을 일단 다 끄는 함수 (중복 켜짐 방지)
    void CloseAllScreens()
    {
        homeScreen.SetActive(false);
        messageAppScreen.SetActive(false);
        chatRoomScreen.SetActive(false);

        // [추가됨] 지도 화면도 끄기 (연결되어 있을 때만)
        if (mapAppScreen != null)
            mapAppScreen.SetActive(false);
    }
}