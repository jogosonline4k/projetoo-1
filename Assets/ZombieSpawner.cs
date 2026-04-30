using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Zombie Prefabs (Normal + Tank)")]
    public GameObject[] zombiePrefabs;

    public float spawnInterval = 2f;
    public int maxZombies = 10;

    [HideInInspector] public int currentZombies = 0;
    private float timer = 0f;

    [Header("Spawn Settings")]
    public float spawnRadius = 5f;

    [Header("Player Safe Zone")]
    public Transform playerTransform;
    public float minDistanceFromPlayer = 3f;

void Awake()
{
    timer = 0f;
}

void Update()
{
    if (Time.timeScale <= 0) return;

    timer += Time.deltaTime;
    if (timer >= spawnInterval && currentZombies < maxZombies)
    {
        SpawnZombie();
        timer = 0f;
    }
}

    void SpawnZombie()
    {
        if (zombiePrefabs.Length == 0)
        {
            Debug.LogError("Nenhum prefab de zumbi definido no ZombieSpawner!");
            return;
        }

        Vector2 spawnPos = Vector2.zero;
        bool validPos = false;

        for (int i = 0; i < 15; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            spawnPos = (Vector2)transform.position + randomOffset;

            if (playerTransform == null)
                break;

            float distanceToPlayer = Vector2.Distance(spawnPos, playerTransform.position);

            if (distanceToPlayer >= minDistanceFromPlayer)
            {
                validPos = true;
                break;
            }
        }

        if (!validPos)
            return;

        GameObject chosenPrefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];
        GameObject z = Instantiate(chosenPrefab, spawnPos, Quaternion.identity);
        z.transform.localScale = chosenPrefab.transform.localScale;

        var ez = z.GetComponent<EnemyZombie>();
        var ezt = z.GetComponent<EnemyZombieTank>();

        if (ez != null)
        {
            ez.spawner = this;
            currentZombies++;
        }
        else if (ezt != null)
        {
            ezt.spawner = this;
            currentZombies++;
        }
        else
        {
            Debug.LogWarning("O prefab spawnado não possui EnemyZombie nem EnemyZombieTank!");
            Destroy(z);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, spawnRadius);

        Gizmos.color = new Color(0f, 0f, 1f, 0.25f);
        Gizmos.DrawSphere(playerTransform != null ? playerTransform.position : transform.position, minDistanceFromPlayer);
    }
}