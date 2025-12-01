using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    public TMP_Text ammoText; // TextMeshPro 사용

    // 총 이름 추가
    public void SetAmmo(string gunName, int current, int max, int reserve)
    {
        // 예: Rifle : 10 / 30 | 80
        if (ammoText != null)
        {
            ammoText.text = $"{gunName}\n{current} / {max}   |   {reserve}";
        }
    }
}