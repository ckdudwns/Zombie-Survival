using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Basic Settings")]
    public GameObject[] zombiePrefabs;
    public Transform player;
    public Camera playerCamera;
    public LayerMask groundLayer;

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


    bool IsValidSpawnPosition(Vector3 targetPos, out Vector3 validPos)
    {
        validPos = Vector3.zero;

        RaycastHit hit;
        if (Physics.Raycast(targetPos + Vector3.up * 5f, Vector3.down, out hit, 10f, groundLayer))
        {
            validPos = hit.point;

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
    void SpawnZombieAt(Vector3 pos, bool forceSpawn)
    {
        GameObject zombieToSpawn = GetRandomZombie();
        if (zombieToSpawn == null)
            return;

        Debug.Log("Spawn Call");

        if (forceSpawn)
        {
            RaycastHit hit;
            if (Physics.Raycast(pos + Vector3.up * 5f, Vector3.down, out hit, 10f, groundLayer))
            {
                Instantiate(zombieToSpawn, hit.point, Quaternion.identity);
                currentZombieCount++;
            }
        }
        else
        {
            Instantiate(zombieToSpawn, pos, Quaternion.identity);
            currentZombieCount++;
        }
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
