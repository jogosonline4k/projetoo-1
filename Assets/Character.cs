using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour
{
    public int maxHP = 3;
    [Range(0, 3)]
    public int currentHP;

    public bool isGameOver = false;
    public bool isInvincible = false;

    [Header("Visual & Sound")]
    public ParticleSystem bloodParticle;
    public AudioClip hurtSound;
    public float flashDuration = 0.15f;

    private SpriteRenderer[] playerSprites;
    private Color[] originalColors;

    void Start()
    {
        currentHP = maxHP;
        playerSprites = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[playerSprites.Length];

        for (int i = 0; i < playerSprites.Length; i++)
        {
            originalColors[i] = playerSprites[i].color;
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (isGameOver || isInvincible) return;

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        if (hurtSound != null)
        {
            AudioSource.PlayClipAtPoint(hurtSound, transform.position);
        }

        if (bloodParticle != null)
        {
            Instantiate(bloodParticle, transform.position, Quaternion.identity);
        }

        StartCoroutine(DamageFlash());
    }

    private IEnumerator DamageFlash()
    {
        foreach (var s in playerSprites) s.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        for (int i = 0; i < playerSprites.Length; i++)
        {
            playerSprites[i].color = originalColors[i];
        }
    }

    public void Heal(int amount = 1)
    {
        if (isGameOver) return;

        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
    }
}