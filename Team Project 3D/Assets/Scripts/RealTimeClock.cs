using UnityEngine;
using TMPro; // TextMeshPro를 사용할 경우 필수 (권장)
using UnityEngine.UI; // 일반 Text를 사용할 경우
using System;

public class RealTimeClock : MonoBehaviour
{
    [Header("UI 연결")]
    // TextMeshProUGUI를 쓰신다면 이걸 사용하세요 (권장)
    public TextMeshProUGUI timeTextTMP;

    // 혹시 옛날 방식인 일반 Text 컴포넌트를 쓰신다면 이걸 사용하세요
    public Text timeTextLegacy;

    [Header("설정")]
    public bool use24HourFormat = true; // 체크하면 14:00, 해제하면 오후 2:00

    private void Update()
    {
        // 현재 시스템 시간 가져오기
        DateTime now = DateTime.Now;
        string timeString = "";

        // 포맷 설정 (원하는 스타일로 변경 가능)
        if (use24HourFormat)
        {
            // 예: 14:30
            timeString = now.ToString("HH:mm");
        }
        else
        {
            // 예: 오후 2:30 (한글 OS 기준) / PM 2:30 (영문 OS 기준)
            timeString = now.ToString("tt h:mm");
        }

        // UI에 텍스트 적용
        if (timeTextTMP != null)
            timeTextTMP.text = timeString;

        if (timeTextLegacy != null)
            timeTextLegacy.text = timeString;
    }
}