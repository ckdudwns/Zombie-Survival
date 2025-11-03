// IInteractable.cs
using UnityEngine;

public interface IInteractable
{
    // 이 오브젝트와 상호작용할 때 호출될 함수
    // 'player'는 상호작용한 플레이어 오브젝트입니다.
    void Interact(GameObject player);

    // (선택 사항) UI에 "E키 눌러 코인 줍기" 같은 텍스트를 표시할 때 사용
    // string GetInteractMessage(); 
}