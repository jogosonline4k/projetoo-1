using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
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
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (Character != null && Character.isGameOver) return;

        dashCooldownTimer -= Time.deltaTime;

        if (!isDashing)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            bool shift = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = shift ? runSpeed : walkSpeed;
            input = input.normalized * currentSpeed;

            bool moving = input.sqrMagnitude > 0;
            animator.SetBool("IsRunning", shift && moving);
            animator.SetBool("IsWalking", !shift && moving);

            // Virar o personagem
            if (input.x > 0)
                transform.localScale = new Vector3(3, 3, 1);
            else if (input.x < 0)
                transform.localScale = new Vector3(-3, 3, 1);

            // Dash
            if (Input.GetKeyDown(KeyCode.Q) && moving && dashCooldownTimer <= 0)
            {
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
                    // Ajusta Velocity over Lifetime X dependendo da direção
                    var vol = dashParticles.velocityOverLifetime;
                    vol.enabled = true;

                    if (transform.localScale.x > 0) // virado para direita
                        vol.x = new ParticleSystem.MinMaxCurve(-5f);
                    else // virado para esquerda
                        vol.x = new ParticleSystem.MinMaxCurve(5f);

                    dashParticles.Play();
                }
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

    void FixedUpdate()
    {
        if (!isDashing)
        {
            Vector2 newPos = rb.position + input * Time.fixedDeltaTime;
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
            rb.MovePosition(newPos);
        }
        else
        {
            Vector2 newPos = rb.position + dashDir * dashSpeed * Time.fixedDeltaTime;
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
            rb.MovePosition(newPos);
        }
    }
}
