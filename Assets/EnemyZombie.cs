using UnityEngine;
using System.Collections;

public class EnemyZombie : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 3f;
    public float patrolTime = 2f;
    public float waitTime = 2f;
    public float visionRange = 10f;
    public float yMin = -2f;
    public float yMax = 2f;

    [Header("Combat")]
    public int maxHP = 3;
    public float knockbackForce = 3f;
    public float attackCooldown = 1f;

    [Header("VFX")]
    public ParticleSystem bloodExplosion;

    [Header("Blood Decals On Death")]
    public GameObject[] bloodPrefabs;      // <-- SPRITES DE SANGUE
    public float bloodSpawnRadius = 0.6f;  // <-- RAIO DO SPAWN
    public float bloodYMin = -2f;          // <-- LIMITE Y
    public float bloodYMax = 2f;           // <-- LIMITE Y
    public int bloodAmount = 3;            // <-- QUANTIDADE

    [Header("Blood Scale")]
    public Vector2 bloodScaleMin = new Vector2(0.8f, 0.8f);
    public Vector2 bloodScaleMax = new Vector2(1.3f, 1.3f);

    [Header("Blood Rotation")]
    public bool lockBloodRotation = false; // <-- opção para travar rotação do sangue

    int currentHP;
    float nextAttackTime = 0f;

    Rigidbody2D rb;
    SpriteRenderer[] sprites;
    Color[] originalColors;

    Vector2 patrolDir;
    float patrolTimer;
    float waitTimer;

    Character playerCharacter;
    Transform playerTransform;

    [HideInInspector] public ZombieSpawner spawner;

    Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        sprites = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[sprites.Length];

        for (int i = 0; i < sprites.Length; i++)
            originalColors[i] = sprites[i].color;

        currentHP = maxHP;

        Character ch = FindObjectOfType<Character>();
        if (ch != null)
        {
            playerCharacter = ch;
            playerTransform = ch.transform;
        }

        patrolTimer = patrolTime;
        waitTimer = waitTime;
        patrolDir = NewPatrolDirection();
    }

    void Update()
    {
        if (playerTransform == null)
        {
            Character ch = FindObjectOfType<Character>();
            if (ch != null)
            {
                playerCharacter = ch;
                playerTransform = ch.transform;
            }
            else return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist <= visionRange)
            ChasePlayer();
        else
            Patrol();
    }

    // ========== PATROL ==========
    void Patrol()
    {
        bool moving = false;

        patrolTimer -= Time.deltaTime;

        if (patrolTimer > 0f)
        {
            moving = true;

            Vector2 pos = rb.position;
            pos += patrolDir * moveSpeed * Time.deltaTime;

            pos.y = Mathf.Clamp(pos.y, yMin, yMax);

            rb.MovePosition(pos);

            if (rb.position.y <= yMin + 0.05f || rb.position.y >= yMax - 0.05f)
                patrolDir = NewPatrolDirection();
        }
        else
        {
            waitTimer -= Time.deltaTime;
            rb.velocity = Vector2.zero;

            if (waitTimer <= 0f)
            {
                patrolTimer = patrolTime;
                waitTimer = waitTime;
                patrolDir = NewPatrolDirection();
            }
        }

        animator.SetBool("isMoving", moving);
    }

    Vector2 NewPatrolDirection()
    {
        Vector2 dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        if (dir.magnitude < 0.1f)
            dir = Vector2.right;

        return dir.normalized;
    }

    // ========== CHASE ==========
    void ChasePlayer()
    {
        Vector2 dir = ((Vector2)playerTransform.position - rb.position).normalized;
        Vector2 pos = rb.position + dir * chaseSpeed * Time.deltaTime;

        pos.y = Mathf.Clamp(pos.y, yMin, yMax);

        rb.MovePosition(pos);

        animator.SetBool("isMoving", true);
    }

    // ========== DAMAGE ==========
    public void TakeDamage(int amount, Vector2 knockDir)
    {
        currentHP -= amount;

        if (currentHP <= 0)
        {
            if (spawner != null)
                spawner.currentZombies--;

            if (bloodExplosion != null)
                Instantiate(bloodExplosion, transform.position, Quaternion.identity);

            SpawnBloodOnGround();

            Destroy(gameObject);
            return;
        }

        rb.AddForce(knockDir.normalized * knockbackForce, ForceMode2D.Impulse);

        StopAllCoroutines();
        StartCoroutine(DamageFlash());
    }

    // ========== BLOOD DECALS ==========
    void SpawnBloodOnGround()
    {
        if (bloodPrefabs.Length == 0) return;

        for (int i = 0; i < bloodAmount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * bloodSpawnRadius;

            Vector2 spawnPos = new Vector2(
                transform.position.x + offset.x,
                Mathf.Clamp(transform.position.y + offset.y, bloodYMin, bloodYMax)
            );

            GameObject prefab = bloodPrefabs[Random.Range(0, bloodPrefabs.Length)];
            GameObject decal = Instantiate(prefab, spawnPos, Quaternion.identity);

            // rotação
            if (!lockBloodRotation)
                decal.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            else
                decal.transform.rotation = Quaternion.identity;

            // escala X e Y aleatória
            float scaleX = Random.Range(bloodScaleMin.x, bloodScaleMax.x);
            float scaleY = Random.Range(bloodScaleMin.y, bloodScaleMax.y);
            decal.transform.localScale = new Vector3(scaleX, scaleY, 1);
        }
    }

    IEnumerator DamageFlash()
    {
        foreach (var s in sprites)
            s.color = Color.red;

        yield return new WaitForSeconds(0.12f);

        for (int i = 0; i < sprites.Length; i++)
            sprites[i].color = originalColors[i];
    }

    // ========== ATTACK ==========
    void OnTriggerStay2D(Collider2D other)
    {
        if (playerCharacter == null) return;

        Character ch = other.GetComponent<Character>();

        if (ch != null && Time.time >= nextAttackTime)
        {
            ch.TakeDamage(1);
            nextAttackTime = Time.time + attackCooldown;
        }
    }
}
