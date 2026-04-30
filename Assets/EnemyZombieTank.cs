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
    public float chargeSpeed = 25f;
    public float chargeDuration = 0.5f;
    public float chargeCooldown = 7f;

    [Header("Audio")]
    public AudioSource audioSource; 
    public AudioClip stepSound;
    public AudioClip[] randomVoices = new AudioClip[4];
    [Range(0f, 1f)] public float stepVolume = 0.5f;
    [Range(0f, 1f)] public float voiceVolume = 0.7f;

    [Header("VFX")]
    public ParticleSystem bloodExplosion;

    [Header("Blood Decals On Death")]
    public GameObject[] bloodPrefabs;
    public float bloodSpawnRadius = 0.6f;
    public float bloodYMin = -2f;
    public float bloodYMax = 2f;
    public int bloodAmount = 3;
    public Vector2 bloodScaleMin = new Vector2(0.8f, 0.8f);
    public Vector2 bloodScaleMax = new Vector2(1.3f, 1.3f);
    public bool lockBloodRotation = false;

    [Header("Visual (Obrigatório)")]
    public Transform spriteChild;

    private int currentHP;
    private float nextAttackTime = 0f;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer[] sprites;
    private Color[] originalColors;
    private Vector2 patrolDir;
    private float patrolTimer;
    private float waitTimer;
    private Character playerCharacter;
    private Transform playerTransform;
    [HideInInspector] public ZombieSpawner spawner;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private Vector3 originalSpriteScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (spriteChild == null && animator != null) spriteChild = animator.transform;
        if (spriteChild != null) originalSpriteScale = spriteChild.localScale;

        sprites = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            originalColors[i] = sprites[i].color;

        currentHP = maxHP;

        Character ch = FindObjectOfType<Character>();
        if (ch != null) { playerCharacter = ch; playerTransform = ch.transform; }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.clip = stepSound;
        }

        patrolTimer = patrolTime;
        waitTimer = waitTime;
        patrolDir = NewPatrolDirection();
        chargeTimer = chargeCooldown;

        Vector3 spawnSeguro = transform.position;
        spawnSeguro.y = Mathf.Clamp(spawnSeguro.y, yMin, yMax);
        transform.position = spawnSeguro;

        StartCoroutine(RandomVoiceRoutine());
    }

    void Update()
    {
        if (playerTransform == null || isCharging) return;

        chargeTimer -= Time.deltaTime;
        if (chargeTimer <= 0)
        {
            StartCoroutine(DoCharge());
            chargeTimer = chargeCooldown;
            return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= visionRange)
            ChasePlayer();
        else
            Patrol();

        HandleMovementAudio();
    }

    void HandleMovementAudio()
    {
        if (audioSource == null || stepSound == null) return;
        if (Time.timeScale <= 0) { if (audioSource.isPlaying) audioSource.Pause(); return; }

        bool moving = rb.velocity.magnitude > 0.1f || animator.GetBool("isMoving");

        if (moving && !audioSource.isPlaying)
        {
            audioSource.volume = stepVolume;
            audioSource.UnPause(); 
            if (!audioSource.isPlaying) audioSource.Play();
        }
        else if (!moving && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    IEnumerator RandomVoiceRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(10f, 20f));
            if (audioSource != null && randomVoices.Length > 0 && Time.timeScale > 0)
            {
                AudioClip clip = randomVoices[Random.Range(0, randomVoices.Length)];
                if (clip != null) audioSource.PlayOneShot(clip, voiceVolume);
            }
        }
    }

    void UpdateSpriteDirection(float moveX)
    {
        if (spriteChild == null || Mathf.Abs(moveX) < 0.01f) return;

        float sign = (moveX > 0) ? 1f : -1f;
        spriteChild.localScale = new Vector3(Mathf.Abs(originalSpriteScale.x) * sign, originalSpriteScale.y, originalSpriteScale.z);
    }

    void Patrol()
    {
        bool moving = false;
        patrolTimer -= Time.deltaTime;

        if (patrolTimer > 0f)
        {
            moving = true;
            Vector2 movement = patrolDir * moveSpeed * Time.deltaTime;
            Vector2 targetPos = rb.position + movement;
            targetPos.y = Mathf.Clamp(targetPos.y, yMin, yMax);
            rb.MovePosition(targetPos);
            UpdateSpriteDirection(patrolDir.x);
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

    void ChasePlayer()
    {
        Vector2 dir = ((Vector2)playerTransform.position - rb.position).normalized;
        Vector2 targetPos = rb.position + dir * chaseSpeed * Time.deltaTime;
        targetPos.y = Mathf.Clamp(targetPos.y, yMin, yMax);
        rb.MovePosition(targetPos);
        UpdateSpriteDirection(dir.x);
        animator.SetBool("isMoving", true);
    }

    IEnumerator DoCharge()
    {
        isCharging = true;
        animator.SetBool("isCharging", true);
        animator.SetBool("isMoving", false);

        Vector2 chargeDirection = ((Vector2)playerTransform.position - rb.position).normalized;
        UpdateSpriteDirection(chargeDirection.x);

        yield return new WaitForSeconds(0.3f);

        float timer = 0f;
        while (timer < chargeDuration)
        {
            Vector2 targetPos = rb.position + chargeDirection * chargeSpeed * Time.deltaTime;
            targetPos.y = Mathf.Clamp(targetPos.y, yMin, yMax);
            rb.MovePosition(targetPos);
            timer += Time.deltaTime;
            yield return null;
        }

        ResetChargeState();
    }

    void ResetChargeState()
    {
        isCharging = false;
        rb.velocity = Vector2.zero;
        animator.SetBool("isCharging", false);
    }

    public void TakeDamage(int amount, Vector2 knockDir)
    {
        currentHP -= amount;

        if (bloodExplosion != null)
            Instantiate(bloodExplosion, transform.position, Quaternion.identity);

        if (isCharging) ResetChargeState();

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        rb.AddForce(knockDir.normalized * knockbackForce, ForceMode2D.Impulse);
        
        StopAllCoroutines();
        StartCoroutine(DamageFlash());
        StartCoroutine(RandomVoiceRoutine()); 
    }

    void Die()
    {
        if (spawner != null) spawner.currentZombies--;
        SpawnBloodOnGround();
        Destroy(gameObject);
    }

    IEnumerator DamageFlash()
    {
        foreach (var s in sprites) s.color = Color.red;
        yield return new WaitForSeconds(0.12f);
        for (int i = 0; i < sprites.Length; i++)
            sprites[i].color = originalColors[i];
    }

    void SpawnBloodOnGround()
    {
        if (bloodPrefabs.Length == 0) return;
        for (int i = 0; i < bloodAmount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * bloodSpawnRadius;
            Vector2 spawnPos = new Vector2(transform.position.x + offset.x, Mathf.Clamp(transform.position.y + offset.y, bloodYMin, bloodYMax));
            GameObject decal = Instantiate(bloodPrefabs[Random.Range(0, bloodPrefabs.Length)], spawnPos, Quaternion.identity);
            decal.transform.rotation = lockBloodRotation ? Quaternion.identity : Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            decal.transform.localScale = new Vector3(Random.Range(bloodScaleMin.x, bloodScaleMax.x), Random.Range(bloodScaleMin.y, bloodScaleMax.y), 1);
        }
    }

    Vector2 NewPatrolDirection()
    {
        Vector2 dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        return dir.magnitude < 0.1f ? Vector2.right : dir.normalized;
    }

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