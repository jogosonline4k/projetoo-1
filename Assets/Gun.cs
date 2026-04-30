using UnityEngine;
using TMPro;
using System.Collections;

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
    private float baseZ;
    private Vector3 initialScale;
    private bool isInitialized = false;

    [Header("Configurações de Áudio")]
    public AudioSource audioSource;
    public AudioClip somTiro;
    [Range(0f, 1f)] public float volumeTiro = 0.8f;
    public AudioClip somRecarga;
    [Range(0f, 1f)] public float volumeRecarga = 0.5f;
    public AudioClip somGiro;
    [Range(0f, 1f)] public float volumeGiro = 0.3f;

    private bool isReloading = false;

    [Header("Posição da Arma")]
    public Vector2 idleOffset = new Vector2(0, 0);
    public Vector2 walkOffset = new Vector2(0.05f, 0);
    public Vector2 runOffset = new Vector2(0.1f, -0.05f);
    
    [Header("Ajustes de Lado")]
    public float rightSideX = 0.2f;
    public float leftSideX = -0.2f;

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        baseZ = transform.localPosition.z;
        initialScale = transform.localScale;

        currentAmmo = maxAmmo;
        UpdateAmmoUI();

        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }

        renderers = GetComponentsInChildren<SpriteRenderer>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null) 
        {
            audioSource.playOnAwake = false;
            audioSource.volume = volumeGiro;
        }

        StartCoroutine(DelayInitialization());
    }

    IEnumerator DelayInitialization()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        isInitialized = true;
    }

    void Update()
    {
        if (Character != null && Character.isGameOver) return;

        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F)) ToggleGunVisibility();
        if (!gunVisible) return;

        if (!spinning) HandleAiming();

        if (currentAmmo <= 0 && !spinning && !isReloading) Reload();

        if (Input.GetMouseButtonDown(0) && !spinning && currentAmmo > 0 && !isReloading)
        {
            Shoot();
            ApplyRecoil();
            currentAmmo--;
            UpdateAmmoUI();
            shotCount++;

            if (audioSource != null && somTiro != null)
                audioSource.PlayOneShot(somTiro, volumeTiro);

            if (shotCount >= 20) StartSpin();
        }

        if (Input.GetKeyDown(KeyCode.R) && !spinning && !isReloading && currentAmmo < maxAmmo) Reload();

        if (spinning)
        {
            spinElapsed += Time.deltaTime;
            float t = spinElapsed / spinDuration;
            float zRotation = Mathf.Lerp(0, 360f * spinTurns, t);
            transform.localRotation = Quaternion.Euler(0, 0, zRotation);
            if (spinElapsed >= spinDuration) EndSpin();
        }
        else
        {
            UpdateWeaponPosition();
        }

        if (trail != null && !spinning) trail.emitting = false;
    }

    void HandleAiming()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 localScale = initialScale;
        if (mousePos.x < transform.position.x)
        {
            localScale.y = -initialScale.y;
        }
        else
        {
            localScale.y = initialScale.y;
        }
        transform.localScale = localScale;
    }

    void UpdateWeaponPosition()
    {
        Animator charAnimator = (Character != null) ? Character.GetComponent<Animator>() : null;

        bool isRunning = charAnimator != null && charAnimator.HasParameter("IsRunning") && charAnimator.GetBool("IsRunning");
        bool isWalking = charAnimator != null && charAnimator.HasParameter("IsWalking") && charAnimator.GetBool("IsWalking");

        Vector3 targetOffset;
        if (isRunning) targetOffset = runOffset;
        else if (isWalking) targetOffset = walkOffset;
        else targetOffset = idleOffset;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float sideX = (mousePos.x < Character.transform.position.x) ? leftSideX : rightSideX;
        
        Vector3 finalOffset = new Vector3(targetOffset.x + sideX, targetOffset.y, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition + finalOffset, Time.deltaTime * recoilSpeed);
    }

    void Shoot()
    {
        if (animator != null) animator.SetTrigger("Shoot");
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = firePoint.right * bulletSpeed;
        Destroy(bullet, 3f);
    }

    void ApplyRecoil()
    {
        transform.position -= firePoint.right * recoilDistance;
    }

    void StartSpin()
    {
        spinning = true;
        spinElapsed = 0f;
        shotCount = 0;
        if (audioSource != null && somGiro != null)
        {
            audioSource.volume = volumeGiro;
            audioSource.clip = somGiro;
            audioSource.Play();
        }
        if (trail != null) { trail.Clear(); trail.emitting = true; }
    }

    void EndSpin()
    {
        spinning = false;
        HandleAiming();
        if (audioSource != null && audioSource.clip == somGiro) audioSource.Stop();
        if (trail != null) { trail.emitting = false; trail.Clear(); }
    }

    void OnEnable() 
    { 
        if (isInitialized && Time.timeScale > 0) 
        { 
            if (audioSource != null && somRecarga != null) 
                audioSource.PlayOneShot(somRecarga, volumeRecarga); 
        } 
    }

    void OnDisable() { isReloading = false; spinning = false; if (audioSource != null) audioSource.Stop(); StopAllCoroutines(); }
    void Reload() { if (!spinning && !isReloading) { isReloading = true; StartCoroutine(RotinaRecarga()); } }
    IEnumerator RotinaRecarga() { StartSpin(); yield return new WaitForSeconds(spinDuration); if (audioSource != null && somRecarga != null) audioSource.PlayOneShot(somRecarga, volumeRecarga); currentAmmo = maxAmmo; UpdateAmmoUI(); isReloading = false; }
    public void UpdateAmmoUI() { if (ammoText != null) ammoText.text = currentAmmo + " / " + maxAmmo; }
    void ToggleGunVisibility() { gunVisible = !gunVisible; foreach (SpriteRenderer r in renderers) r.enabled = gunVisible; if (trail != null) { trail.emitting = false; trail.Clear(); } if (ammoText != null) ammoText.enabled = gunVisible; }
}