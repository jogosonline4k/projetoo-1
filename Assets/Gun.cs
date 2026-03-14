using UnityEngine;
using TMPro;

public class Gun : MonoBehaviour
{
    public Character Character;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public int maxAmmo = 16;
    public int currentAmmo;
    public float recoilAngle = 15f;
    public float recoilDistance = 0.1f;
    public float recoilSpeed = 10f;
    public float spinDuration = 0.166f;
    public int spinTurns = 3;
    public TrailRenderer trail;
    public TMP_Text ammoText;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private int shotCount = 0;
    private bool spinning = false;
    private float spinElapsed = 0f;
    private bool gunVisible = true;
    public Animator animator;
    private SpriteRenderer[] renderers;
    public float runTiltAngle = -45f;
    private float baseZ;

    [Header("Posição da Arma")]
    public Vector2 idleOffset = new Vector2(0, 0);
    public Vector2 walkOffset = new Vector2(0.05f, 0);
    public Vector2 runOffset = new Vector2(0.1f, -0.05f);

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        baseZ = transform.localPosition.z;
        currentAmmo = maxAmmo;
        UpdateAmmoUI();

        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }

        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (Character != null && Character.isGameOver) return;

        if (Input.GetKeyDown(KeyCode.F)) ToggleGunVisibility();
        if (!gunVisible) return;

        if (currentAmmo <= 0 && !spinning)
        {
            Reload();
        }

        if (Input.GetMouseButtonDown(0) && !spinning && currentAmmo > 0)
        {
            Shoot();
            ApplyRecoil();
            currentAmmo--;
            UpdateAmmoUI();
            shotCount++;
            if (shotCount >= 20)
            {
                StartSpin();
            }
        }

        if (Input.GetKey(KeyCode.R) && !spinning)
        {
            Reload();
        }

        if (spinning)
        {
            spinElapsed += Time.deltaTime;
            float t = spinElapsed / spinDuration;
            float zRotation = Mathf.Lerp(0, 360f * spinTurns, t);
            transform.localRotation = originalRotation * Quaternion.Euler(0, 0, zRotation);
            if (spinElapsed >= spinDuration)
            {
                EndSpin();
            }
        }
        else
        {
            Animator charAnimator = null;
            if (Character != null)
            {
                charAnimator = Character.GetComponent<Animator>();
            }

            bool isRunning = false;
            bool isWalking = false;

            if (charAnimator != null)
            {
                if (charAnimator.HasParameter("IsRunning")) isRunning = charAnimator.GetBool("IsRunning");
                if (charAnimator.HasParameter("IsWalking")) isWalking = charAnimator.GetBool("IsWalking");
            }

            Vector3 targetOffset;
            Quaternion targetRotation = originalRotation;

            if (isRunning)
            {
                targetOffset = new Vector3(runOffset.x, runOffset.y, baseZ);
                targetRotation *= Quaternion.Euler(0, 0, runTiltAngle);
            }
            else if (isWalking)
            {
                targetOffset = new Vector3(walkOffset.x, walkOffset.y, baseZ);
            }
            else
            {
                targetOffset = new Vector3(idleOffset.x, idleOffset.y, baseZ);
            }

            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * recoilSpeed);
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition + targetOffset, Time.deltaTime * recoilSpeed);
        }

        if (trail != null && !spinning)
        {
            trail.emitting = false;
        }
    }

    void Shoot()
    {
        if (animator != null) animator.SetTrigger("Shoot");

        // Instancia o clone da bala, garantindo que o prefab nunca seja destruído
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = new Vector2(Mathf.Sign(transform.parent.localScale.x), 0);
            rb.velocity = direction * bulletSpeed;
        }

        // Destrói apenas o clone depois de 3 segundos
        Destroy(bullet, 3f);

        Vector3 fixedPos = transform.localPosition;
        fixedPos.z = baseZ;
        transform.localPosition = fixedPos;
    }

    void ApplyRecoil()
    {
        transform.localRotation = Quaternion.Euler(0, 0, recoilAngle);
        Vector3 pos = transform.localPosition - new Vector3(0, 0, recoilDistance);
        pos.z = baseZ;
        transform.localPosition = pos;
    }

    void StartSpin()
    {
        spinning = true;
        spinElapsed = 0f;
        shotCount = 0;

        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }
    }

    void EndSpin()
    {
        spinning = false;
        transform.localRotation = originalRotation;

        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }
    }

    void Reload()
    {
        if (!spinning)
        {
            StartSpin();
            currentAmmo = maxAmmo;
            UpdateAmmoUI();
        }
    }

    public void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + maxAmmo;
        }
    }

    void ToggleGunVisibility()
    {
        gunVisible = !gunVisible;

        foreach (SpriteRenderer r in renderers)
        {
            r.enabled = gunVisible;
        }

        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }

        if (ammoText != null) ammoText.enabled = gunVisible;
    }
}

public static class AnimatorExtensions
{
    public static bool HasParameter(this Animator animator, string paramName)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}
