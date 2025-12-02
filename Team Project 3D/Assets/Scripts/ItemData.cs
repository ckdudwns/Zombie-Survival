using UnityEngine;

// "Assets > Create > Inventory/Item" 메뉴를 통해 생성할 수 있습니다.
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite icon = null; // 인벤토리 UI에 표시될 아이콘
    [TextArea] public string description = "Item Description"; // (팁: TextArea를 쓰면 줄바꿈이 가능해집니다)

    [Header("아이템 설정")]
    public bool isUsable = true;

    // [★추가됨] 아이템 사용 시 재생할 사운드
    public AudioClip useSound;

    /// <summary>
    /// 이 아이템을 인벤토리에서 사용했을 때 호출될 함수입니다.
    /// </summary>
    /// <param name="player">아이템을 사용한 플레이어</param>
    public virtual void Use(Player player)
    {
        // 기본적으로는 아무것도 하지 않습니다.
        Debug.Log(itemName + " 아이템 사용 (기본 동작)");
    }
}