using UnityEngine;

public class CollisionDebugger : MonoBehaviour
{
    // 이 스크립트는 어떤 콜라이더에 닿기만 하면 그 즉시 로그를 출력합니다.
    void OnTriggerEnter(Collider other)
    {
        // 로그가 눈에 잘 띄도록 녹색으로 강조하고, 어떤 오브젝트에 닿았는지, 그 오브젝트의 레이어는 무엇인지 출력합니다.
        Debug.Log("<color=lime>COLLISION EVENT FIRED!</color> Touched object: "
                  + other.gameObject.name
                  + ", Layer: "
                  + LayerMask.LayerToName(other.gameObject.layer), this.gameObject);
    }
}