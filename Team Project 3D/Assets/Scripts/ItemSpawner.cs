using UnityEngine;
using System.Collections;

[System.Serializable]
public class SpawnSetting
{
    [Header("그룹 설정")]
    public string tagName = "ItemSpawnPoint";
    public int maxSpawnLimit = 2000;

    [Range(0, 100)]
    public float spawnChance = 100f;

    [Header("위치/회전 보정")]
    [Tooltip("회전 각도 (예: 총이 누워있다면 X에 90)")]
    public Vector3 spawnRotation;

    [Tooltip("바닥 감지 후 추가로 적용할 위치 보정값 (Y값을 올려서 땅에 박히는 것 방지)")]
    public Vector3 positionOffset; // ★ 바닥 기준 추가 보정

    [Header("아이템 프리팹")]
    public GameObject[] itemPrefabs;
}

public class ItemSpawner : MonoBehaviour
{
    [Header("바닥 감지 설정")]
    [Tooltip("바닥으로 인식할 레이어를 선택하세요 (예: Default, Terrain 등)")]
    public LayerMask groundLayer; // ★ 중요: 인스펙터에서 바닥 레이어를 꼭 체크하세요!

    [Tooltip("스폰 포인트에서 아래로 탐색할 최대 거리")]
    public float rayDistance = 10f;

    public SpawnSetting[] spawnSettings;

    // 속도 조절
    private float maxTimePerFrame = 0.015f;

    IEnumerator Start()
    {
        if (spawnSettings == null || spawnSettings.Length == 0) yield break;

        Debug.Log("⚡ 아이템 생성 시작 (Raycast 바닥 감지 적용)");

        float frameStartTime = Time.realtimeSinceStartup;

        foreach (var setting in spawnSettings)
        {
            GameObject[] allPoints = GameObject.FindGameObjectsWithTag(setting.tagName);

            if (allPoints.Length == 0) continue;
            if (setting.itemPrefabs == null || setting.itemPrefabs.Length == 0) continue;

            ShuffleArray(allPoints);

            int countToUse = Mathf.Min(allPoints.Length, setting.maxSpawnLimit);
            int currentPointIndex = 0;

            Quaternion finalRotation = Quaternion.Euler(setting.spawnRotation);

            // [1단계] 확정 생성
            for (int i = 0; i < setting.itemPrefabs.Length; i++)
            {
                if (currentPointIndex >= countToUse) break;

                // ★ 바닥 좌표 계산 함수 사용
                Vector3 finalPosition = GetGroundPosition(allPoints[currentPointIndex].transform.position, setting.positionOffset);

                Instantiate(setting.itemPrefabs[i], finalPosition, finalRotation);
                currentPointIndex++;

                if (Time.realtimeSinceStartup - frameStartTime > maxTimePerFrame)
                {
                    yield return null;
                    frameStartTime = Time.realtimeSinceStartup;
                }
            }

            // [2단계] 확률 생성
            for (int i = currentPointIndex; i < countToUse; i++)
            {
                if (Random.Range(0f, 100f) <= setting.spawnChance)
                {
                    int randomIndex = Random.Range(0, setting.itemPrefabs.Length);

                    // ★ 바닥 좌표 계산 함수 사용
                    Vector3 finalPosition = GetGroundPosition(allPoints[i].transform.position, setting.positionOffset);

                    Instantiate(setting.itemPrefabs[randomIndex], finalPosition, finalRotation);

                    if (Time.realtimeSinceStartup - frameStartTime > maxTimePerFrame)
                    {
                        yield return null;
                        frameStartTime = Time.realtimeSinceStartup;
                    }
                }
            }
        }

        Debug.Log("✅ 생성 완료!");
    }

    // ★ Raycast를 이용해 가장 가까운 바닥을 찾는 함수
    Vector3 GetGroundPosition(Vector3 sourcePos, Vector3 offset)
    {
        RaycastHit hit;

        // 스폰 포인트 위치(sourcePos)에서 아래 방향(Vector3.down)으로 레이저 발사
        // rayDistance 만큼 검사하며, groundLayer에 해당하는 물체만 감지함
        if (Physics.Raycast(sourcePos, Vector3.down, out hit, rayDistance, groundLayer))
        {
            // 바닥에 닿았다면 닿은 지점(hit.point)에 오프셋을 더해서 반환
            return hit.point + offset;
        }

        // 바닥을 못 찾았다면 (공중이나 낭떠러지 등) 원래 위치 반환
        return sourcePos + offset;
    }

    void ShuffleArray(GameObject[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            GameObject temp = array[i];
            int randomIndex = Random.Range(i, array.Length);
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}