using UnityEngine;
using TMPro;

public class Escopeta : MonoBehaviour
{
    public Character Character;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 5f;
    public int maxAmmo = 8;
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

    [Header("Shotgun Settings")]
    public int shotsPerFire = 5;       
    public float spreadAngle = 10f;    
    public float fireRate = 0.2f;      
    private float nextFireTime = 0f;

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

        if (currentAmmo <= 0 && !spinning) Reload();

        if (Input.GetMouseButtonDown(0) && !spinning && currentAmmo > 0 && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            ShootShotgun();
            ApplyRecoil();
            currentAmmo--;
            UpdateAmmoUI();

            shotCount++;
            if (shotCount >= 20) StartSpin();
        }

        if (Input.GetKey(KeyCode.R) && !spinning) Reload();

        HandleSpinAndAnimation();
    }

    void ShootShotgun()
    {
        if (animator != null) animator.SetTrigger("Shoot");

        for (int i = 0; i < shotsPerFire; i++)
        {
            float angleOffset = Random.Range(-spreadAngle, spreadAngle);

            // Instancia apenas o clone, nunca o prefab
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            bullet.transform.localScale = bulletPrefab.transform.localScale;

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float directionX = Mathf.Sign(transform.parent.localScale.x);
                Vector2 direction = Quaternion.Euler(0, 0, angleOffset) * new Vector2(directionX, 0);
                rb.velocity = direction.normalized * bulletSpeed;
            }

            // Destrói somente o clone depois de 3 segundos
            Destroy(bullet, 3f);
        }

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
        if (ammoText != null) ammoText.text = currentAmmo + " / " + maxAmmo;
    }

    void ToggleGunVisibility()
    {
        gunVisible = !gunVisible;

        foreach (SpriteRenderer r in renderers)
            r.enabled = gunVisible;

        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }

        if (ammoText != null) ammoText.enabled = gunVisible;
    }

    void HandleSpinAndAnimation()
    {
        if (spinning)
        {
            spinElapsed += Time.deltaTime;
            float t = spinElapsed / spinDuration;
            float zRotation = Mathf.Lerp(0, 360f * spinTurns, t);
            transform.localRotation = originalRotation * Quaternion.Euler(0, 0, zRotation);
            if (spinElapsed >= spinDuration) EndSpin();
        }
        else
        {
            Animator charAnimator = Character != null ? Character.GetComponent<Animator>() : null;
            bool isRunning = charAnimator != null && charAnimator.HasParameter("IsRunning") && charAnimator.GetBool("IsRunning");
            bool isWalking = charAnimator != null && charAnimator.HasParameter("IsWalking") && charAnimator.GetBool("IsWalking");

            Vector3 targetOffset;
            Quaternion targetRotation = originalRotation;

            if (isRunning)
            {
                targetOffset = new Vector3(runOffset.x, runOffset.y, baseZ);
                targetRotation *= Quaternion.Euler(0, 0, runTiltAngle);
            }
            else if (isWalking) targetOffset = new Vector3(walkOffset.x, walkOffset.y, baseZ);
            else targetOffset = new Vector3(idleOffset.x, idleOffset.y, baseZ);

            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * recoilSpeed);
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition + targetOffset, Time.deltaTime * recoilSpeed);
        }
    }
}
