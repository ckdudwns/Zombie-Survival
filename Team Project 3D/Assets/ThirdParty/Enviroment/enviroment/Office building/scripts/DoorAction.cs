using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAction : MonoBehaviour
{
    public float interactDistance = 3f; // 상호작용 거리

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;

            // ⭐ 거리 제한 추가 (3미터)
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, interactDistance))
            {
                Debug.Log("감지된 오브젝트: " + hit.collider.name + " / 태그: " + hit.collider.tag);

                // 문 열기
                if (hit.transform.tag == "door")
                {
                    Debug.Log("문 열기 시도!");
                    Door doorScript = hit.transform.gameObject.GetComponent<Door>();
                    if (doorScript != null)
                    {
                        doorScript.ActionDoor();
                    }
                    else
                    {
                        Debug.LogError("Door 스크립트를 찾을 수 없습니다!");
                    }
                }

                // 엘리베이터 버튼들
                if (hit.collider.gameObject.name == "Button floor 1")
                {
                    hit.transform.gameObject.GetComponent<pass_on_parent>().MyParent.GetComponent<evelator_controll>().AddTaskEve("Button floor 1");
                }
                if (hit.collider.gameObject.name == "Button floor 2")
                {
                    hit.transform.gameObject.GetComponent<pass_on_parent>().MyParent.GetComponent<evelator_controll>().AddTaskEve("Button floor 2");
                }
                if (hit.collider.gameObject.name == "Button floor 3")
                {
                    hit.transform.gameObject.GetComponent<pass_on_parent>().MyParent.GetComponent<evelator_controll>().AddTaskEve("Button floor 3");
                }
                if (hit.collider.gameObject.name == "Button floor 4")
                {
                    hit.transform.gameObject.GetComponent<pass_on_parent>().MyParent.GetComponent<evelator_controll>().AddTaskEve("Button floor 4");
                }
                if (hit.collider.gameObject.name == "Button floor 5")
                {
                    hit.transform.gameObject.GetComponent<pass_on_parent>().MyParent.GetComponent<evelator_controll>().AddTaskEve("Button floor 5");
                }
                if (hit.collider.gameObject.name == "Button floor 6")
                {
                    hit.transform.gameObject.GetComponent<pass_on_parent>().MyParent.GetComponent<evelator_controll>().AddTaskEve("Button floor 6");
                }
            }
            else
            {
                Debug.Log("아무것도 감지되지 않음 (거리: " + interactDistance + "m)");
            }
        }
    }

    // Scene 뷰에서 Raycast 시각화 (디버깅용)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * interactDistance);
    }
}