using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    public Character Character;
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float minY = -2f, maxY = 2f;

    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.4f;

    public ParticleSystem dashParticles;

    [Header("Configurações de Áudio")]
    public AudioSource audioSource;
    public AudioClip somPassos;
    public AudioClip somDash;
    [Range(0f, 1f)] public float volumeCaminhada = 0.4f;
    [Range(0f, 1f)] public float volumeCorrida = 0.7f;
    [Range(0f, 1f)] public float volumeDash = 0.8f;
    public float pitchCaminhada = 1.0f;
    public float pitchCorrida = 1.3f;

    Rigidbody2D rb;
    Animator animator;
    Vector2 input;
    float currentSpeed;

    bool isDashing;
    float dashTime;
    float dashCooldownTimer;
    Vector2 dashDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (audioSource != null)
        {
            audioSource.clip = somPassos;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (Character != null && Character.isGameOver)
        {
            PararSomDePassos();
            return;
        }

        dashCooldownTimer -= Time.deltaTime;

        if (!isDashing)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            bool shift = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = shift ? runSpeed : walkSpeed;
            input = input.normalized * currentSpeed;

            bool moving = input.sqrMagnitude > 0.01f;
            animator.SetBool("IsRunning", shift && moving);
            animator.SetBool("IsWalking", !shift && moving);

            if (moving)
            {
                GerenciarSomDePassos(shift);
            }
            else
            {
                PararSomDePassos();
            }

            if (input.x > 0)
                transform.localScale = new Vector3(3, 3, 1);
            else if (input.x < 0)
                transform.localScale = new Vector3(-3, 3, 1);

            if (Input.GetKeyDown(KeyCode.Q) && moving && dashCooldownTimer <= 0)
            {
                ExecutarDash();
            }
        }
        else
        {
            dashTime -= Time.deltaTime;
            if (dashTime <= 0)
            {
                isDashing = false;
                Character.isInvincible = false;

                if (dashParticles != null)
                    dashParticles.Stop();
            }
        }
    }

    void GerenciarSomDePassos(bool correndo)
    {
        if (audioSource == null || somPassos == null) return;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        audioSource.volume = correndo ? volumeCorrida : volumeCaminhada;
        audioSource.pitch = correndo ? pitchCorrida : pitchCaminhada;
    }

    void PararSomDePassos()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void ExecutarDash()
    {
        PararSomDePassos(); 
        
        if (audioSource != null && somDash != null)
        {
            audioSource.PlayOneShot(somDash, volumeDash);
        }

        dashCooldownTimer = dashCooldown;
        dashTime = dashDuration;
        isDashing = true;
        dashDir = input.normalized;

        if ((dashDir.y > 0 && rb.position.y >= maxY) ||
            (dashDir.y < 0 && rb.position.y <= minY))
            dashDir.y = 0;

        Character.isInvincible = true;

        if (dashParticles != null)
        {
            var vol = dashParticles.velocityOverLifetime;
            vol.enabled = true;
            vol.x = new ParticleSystem.MinMaxCurve(transform.localScale.x > 0 ? -5f : 5f);
            dashParticles.Play();
        }
    }

    void FixedUpdate()
    {
        Vector2 velocity = isDashing ? dashDir * dashSpeed : input;
        Vector2 newPos = rb.position + velocity * Time.fixedDeltaTime;
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
        rb.MovePosition(newPos);
    }
}