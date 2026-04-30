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
    bool isCharging = false;
    float chargeTimer = 0f;
    int facingSign = 1;
    Vector3 originalLocalScale;

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
        if (ch != null) { playerCharacter = ch; playerTransform = ch.transform; }

        patrolTimer = patrolTime;
        waitTimer = waitTime;
        patrolDir = NewPatrolDirection();
        chargeTimer = chargeCooldown;
        originalLocalScale = transform.localScale;
        facingSign = (transform.localScale.x < 0f) ? -1 : 1;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.clip = stepSound;
        }

        StartCoroutine(RandomVoiceRoutine());
    }

    void Update()
    {
        if (playerTransform == null) return;

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

        if (Time.timeScale <= 0)
        {
            if (audioSource.isPlaying) audioSource.Pause();
            return;
        }

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
                if (clip != null)
                    audioSource.PlayOneShot(clip, voiceVolume);
            }
        }
    }

    void UpdateSpriteDirection(Vector2 dir)
    {
        float threshold = 0.1f;
        if (dir.x > threshold) facingSign = 1;
        else if (dir.x < -threshold) facingSign = -1;

        transform.localScale = new Vector3(Mathf.Abs(originalLocalScale.x) * facingSign, originalLocalScale.y, originalLocalScale.z);
    }

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
    }

    Vector2 NewPatrolDirection()
    {
        Vector2 dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        return dir.magnitude < 0.1f ? Vector2.right : dir.normalized;
    }

    void ChasePlayer()
    {
        Vector2 dir = ((Vector2)playerTransform.position - rb.position).normalized;
        Vector2 pos = rb.position + dir * chaseSpeed * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, yMin, yMax);
        rb.MovePosition(pos);
        UpdateSpriteDirection(dir);
        animator.SetBool("isMoving", true);
    }

    IEnumerator DoCharge()
    {
        isCharging = true;
        animator.SetBool("isCharging", true);
        animator.SetBool("isMoving", false);

        float timer = 0f;
        float dirX = facingSign;

        while (timer < chargeDuration)
        {
            rb.velocity = new Vector2(dirX * chargeSpeed, 0);
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