using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Zombie Prefabs")]
    public GameObject normalZombiePrefab;
    public GameObject tankZombiePrefab;

    [Header("Spawn Chances (0 to 100)")]
    [Range(0, 100)] public float normalChance = 80f;
    [Range(0, 100)] public float tankChance = 20f;  

    [Header("General Settings")]
    public float spawnInterval = 2f;
    public int maxZombies = 10;
    [HideInInspector] public int currentZombies = 0;
    private float timer = 0f;

    [Header("Spawn Area")]
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
        Vector2 spawnPos = Vector2.zero;
        bool validPos = false;

        for (int i = 0; i < 15; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            spawnPos = (Vector2)transform.position + randomOffset;

            if (playerTransform == null) break;

            float distanceToPlayer = Vector2.Distance(spawnPos, playerTransform.position);
            if (distanceToPlayer >= minDistanceFromPlayer)
            {
                validPos = true;
                break;
            }
        }

        if (!validPos) return;

        GameObject chosenPrefab = null;
        float totalWeight = normalChance + tankChance;
        float randomValue = Random.Range(0, totalWeight);

        if (randomValue <= normalChance)
        {
            chosenPrefab = normalZombiePrefab;
        }
        else
        {
            chosenPrefab = tankZombiePrefab;
        }

        if (chosenPrefab == null) return;

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
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, spawnRadius);

        if (playerTransform != null)
        {
            Gizmos.color = new Color(0f, 0f, 1f, 0.25f);
            Gizmos.DrawSphere(playerTransform.position, minDistanceFromPlayer);
        }
    }
}