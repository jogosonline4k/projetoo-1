using UnityEngine;
using System.Collections;

public class EnemyZombieTank : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float chaseSpeed = 10f;
    public float patrolTime = 2f;
    public float waitTime = 2f;
    public float visionRange = 8f;
    public float yMin = -5.5f;
    public float yMax = 1.8f;

    [Header("Combat")]
    public int maxHP = 4;
    public float knockbackForce = 3f;
    public float attackCooldown = 1f;

    [Header("Charge Attack")]
    public float chargeSpeed = 9f;
    public float chargeDuration = 0.6f;
    public float chargeCooldown = 7f;

    [Header("VFX")]
    public ParticleSystem bloodExplosion;

    [Header("Blood Decals On Death")]
    public GameObject[] bloodPrefabs;
    public float bloodSpawnRadius = 0.6f;
    public float bloodYMin = -2f;
    public float bloodYMax = 2f;
    public int bloodAmount = 3;

    [Header("Blood Scale")]
    public Vector2 bloodScaleMin = new Vector2(0.8f, 0.8f);
    public Vector2 bloodScaleMax = new Vector2(1.3f, 1.3f);

    [Header("Blood Rotation")]
    public bool lockBloodRotation = false;

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

    // Charge control
    bool isCharging = false;
    float chargeTimer = 0f;

    // --- Facing control ---
    int facingSign = 1;               // 1 = right, -1 = left
    Vector3 originalLocalScale;       // guarda o scale original pra preservar tamanho

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

        chargeTimer = chargeCooldown;

        // Guarda scale original (usa o transform do objeto que contém este script)
        originalLocalScale = transform.localScale;

        // Inicializa facingSign a partir do scale atual (caso já esteja invertido)
        facingSign = (transform.localScale.x < 0f) ? -1 : 1;
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

        // Charge timer
        if (!isCharging)
        {
            chargeTimer -= Time.deltaTime;
            if (chargeTimer <= 0)
            {
                StartCoroutine(DoCharge());
                chargeTimer = chargeCooldown;
                return;
            }
        }

        if (isCharging) return;

        // Normal behavior
        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist <= visionRange)
            ChasePlayer();
        else
            Patrol();
    }

    // =======================
    // FLIP DO SPRITE (ROBUSTO)
    // dir: direção de movimento ou direção alvo (em world space)
    // Só altera facing quando dir.x tem magnitude significativa
    // =======================
    void UpdateSpriteDirection(Vector2 dir)
    {
        float threshold = 0.1f;

        if (dir.x > threshold)
            facingSign = 1;
        else if (dir.x < -threshold)
            facingSign = -1;

        // Aplica scale preservando o tamanho original (evita "gigante" se originalLocalScale != 1)
        transform.localScale = new Vector3(Mathf.Abs(originalLocalScale.x) * facingSign,
                                           originalLocalScale.y,
                                           originalLocalScale.z);
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
            Vector2 movement = patrolDir * moveSpeed * Time.deltaTime;
            pos += movement;

            pos.y = Mathf.Clamp(pos.y, yMin, yMax);

            rb.MovePosition(pos);

            // Flip aqui usando o vetor de movimento (se movimento horizontal for quase zero, mantém o facing anterior)
            UpdateSpriteDirection(movement);

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
        animator.SetBool("isCharging", false);
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

        // flip (usa direção para o player)
        UpdateSpriteDirection(dir);

        animator.SetBool("isMoving", true);
        animator.SetBool("isCharging", false);
    }

    // ========== CHARGE ATTACK ==========
    IEnumerator DoCharge()
    {
        isCharging = true;

        animator.SetBool("isCharging", true);
        animator.SetBool("isMoving", false);

        float timer = 0f;

        // Use facingSign para saber a direção do charge (preserva o facing atual)
        float dirX = facingSign;

        while (timer < chargeDuration)
        {
            Vector2 velocity = new Vector2(dirX * chargeSpeed, 0);
            rb.velocity = velocity;

            // mantém flip coerente mesmo durante o charge
            UpdateSpriteDirection(velocity);

            timer += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;

        animator.SetBool("isCharging", false);

        isCharging = false;
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

    IEnumerator DamageFlash()
    {
        foreach (var s in sprites)
            s.color = Color.red;

        yield return new WaitForSeconds(0.12f);

        for (int i = 0; i < sprites.Length; i++)
            sprites[i].color = originalColors[i];
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

            if (!lockBloodRotation)
                decal.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            else
                decal.transform.rotation = Quaternion.identity;

            float scaleX = Random.Range(bloodScaleMin.x, bloodScaleMax.x);
            float scaleY = Random.Range(bloodScaleMin.y, bloodScaleMax.y);
            decal.transform.localScale = new Vector3(scaleX, scaleY, 1);
        }
    }

    // ========== NORMAL ATTACK ==========
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
