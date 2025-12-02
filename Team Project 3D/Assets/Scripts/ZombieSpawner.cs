using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // [중요] NavMesh 기능을 사용하기 위해 추가

public class ZombieSpawner : MonoBehaviour
{
    [Header("Basic Settings")]
    public GameObject[] zombiePrefabs;
    public Transform player;
    public Camera playerCamera;
    // public LayerMask groundLayer; // Raycast용 레이어는 더 이상 필요 없습니다.

    [Header("Spawn Progression")]
    public int maxZombiesNearPlayer = 10;
    public float spawnInterval = 3f;

    [Header("Spawn Range")]
    public Vector2 mapSize = new Vector2(200, 200);
    public float minSpawnDist = 15f;
    public float maxSpawnDist = 40f;

    [Header("Current Status")]
    public int currentZombieCount = 0;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
        if (playerCamera == null)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
        }
        SpawnInitialZombies(50);

        StartCoroutine(SpawnRoutine());
    }

    void SpawnInitialZombies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-mapSize.x / 2, mapSize.x / 2),
                0,
                Random.Range(-mapSize.y / 2, mapSize.y / 2)
            );

            SpawnZombieAt(randomPos, true);
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (currentZombieCount < maxZombiesNearPlayer)
            {
                TrySpawnNearPlayer();
            }
        }
    }

    void TrySpawnNearPlayer()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minSpawnDist, maxSpawnDist);
        Vector3 spawnPos = player.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        if (IsValidSpawnPosition(spawnPos, out Vector3 validPos))
        {
            SpawnZombieAt(validPos, false);
        }
    }

    // [수정됨] Raycast 대신 NavMesh 위인지 확인하는 함수
    bool IsValidSpawnPosition(Vector3 targetPos, out Vector3 validPos)
    {
        validPos = Vector3.zero;

        // NavMesh.SamplePosition(검사할위치, 결과저장변수, 검색반경, 영역마스크)
        // targetPos 주변 5.0f 반경 내에서 가장 가까운 NavMesh 위치를 찾습니다.
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 5.0f, NavMesh.AllAreas))
        {
            validPos = hit.position; // NavMesh 위의 정확한 위치로 보정됨

            // 화면 밖인지 체크
            Vector3 viewportPoint = playerCamera.WorldToViewportPoint(validPos);
            bool isOnScreen = (viewportPoint.z > 0 &&
                               viewportPoint.x > 0 && viewportPoint.x < 1 &&
                               viewportPoint.y > 0 && viewportPoint.y < 1);

            if (!isOnScreen)
            {
                return true;
            }
        }
        return false;
    }

    private GameObject GetRandomZombie()
    {
        if (zombiePrefabs.Length == 0)
        {
            Debug.Log("null array");
            return null;
        }
        int randomIndex = Random.Range(0, zombiePrefabs.Length);
        return zombiePrefabs[randomIndex];
    }

    // [수정됨] 스폰 로직도 NavMesh 기준으로 변경
    void SpawnZombieAt(Vector3 pos, bool forceSpawn)
    {
        GameObject zombieToSpawn = GetRandomZombie();
        if (zombieToSpawn == null)
            return;

        Debug.Log("Spawn Call");

        NavMeshHit hit;
        // 강제 스폰(초기 배치)일 때도 NavMesh 위에 안착시킵니다.
        // 검색 범위를 10f로 넉넉하게 주어 높낮이 차이가 있어도 바닥을 찾게 합니다.
        if (NavMesh.SamplePosition(pos, out hit, 10.0f, NavMesh.AllAreas))
        {
            Instantiate(zombieToSpawn, hit.position, Quaternion.identity);
            currentZombieCount++;
        }
        // forceSpawn이 아닐 때는 이미 IsValidSpawnPosition에서 위치를 검증했으므로 그냥 생성해도 되지만,
        // 안전을 위해 위 로직으로 통일하거나, 정확한 위치를 넘겨받았다면 바로 생성해도 됩니다.
    }

    public void UpgradeDifficulty(int addMaxCount, float reduceInterval)
    {
        maxZombiesNearPlayer += addMaxCount;
        spawnInterval = Mathf.Max(0.5f, spawnInterval - reduceInterval);
        Debug.Log($"Max Zombie: {maxZombiesNearPlayer}, Spawn Interval: {spawnInterval}");
    }

    public void ZombieDied()
    {
        currentZombieCount--;
    }
}