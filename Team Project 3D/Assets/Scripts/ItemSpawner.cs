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

    [Tooltip("회전 후 위치가 삐뚤다면 여기서 X, Z 값을 조절해 중앙을 맞추세요.")]
    public Vector3 positionOffset; // ★ 새로 추가된 위치 보정값

    [Header("아이템 프리팹")]
    public GameObject[] itemPrefabs;
}

public class ItemSpawner : MonoBehaviour
{
    public SpawnSetting[] spawnSettings;

    // 속도 조절
    private float maxTimePerFrame = 0.015f;

    IEnumerator Start()
    {
        if (spawnSettings == null || spawnSettings.Length == 0) yield break;

        Debug.Log("⚡ 아이템 생성 시작 (회전/위치 보정 적용)");

        float frameStartTime = Time.realtimeSinceStartup;

        foreach (var setting in spawnSettings)
        {
            GameObject[] allPoints = GameObject.FindGameObjectsWithTag(setting.tagName);

            if (allPoints.Length == 0) continue;
            if (setting.itemPrefabs == null || setting.itemPrefabs.Length == 0) continue;

            ShuffleArray(allPoints);

            int countToUse = Mathf.Min(allPoints.Length, setting.maxSpawnLimit);
            int currentPointIndex = 0;

            // 회전값 미리 계산
            Quaternion finalRotation = Quaternion.Euler(setting.spawnRotation);

            // [1단계] 확정 생성
            for (int i = 0; i < setting.itemPrefabs.Length; i++)
            {
                if (currentPointIndex >= countToUse) break;

                // ★ 위치 계산: (스폰위치) + (기본 높이 0.5) + (사용자 설정 보정값)
                Vector3 finalPosition = allPoints[currentPointIndex].transform.position
                                      + setting.positionOffset;

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

                    // ★ 위치 계산 적용
                    Vector3 finalPosition = allPoints[i].transform.position
                                          + Vector3.up * 0.5f
                                          + setting.positionOffset;

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