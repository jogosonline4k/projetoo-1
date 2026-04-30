using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public int damage = 1;

    [Header("Audio")]
    public AudioClip hitNormalSound;
    [Range(0f, 1f)] public float normalVolume = 0.5f;
    
    public AudioClip hitTankSound;
    [Range(0f, 1f)] public float tankVolume = 0.8f;

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyZombie zombie = other.GetComponentInParent<EnemyZombie>();
        EnemyZombieTank tank = null;

        if (zombie == null)
            tank = other.GetComponentInParent<EnemyZombieTank>();

        if (zombie != null)
        {
            PlaySound(hitNormalSound, normalVolume);
            Vector2 dir = ((Vector2)zombie.transform.position - (Vector2)transform.position).normalized;
            zombie.TakeDamage(damage, dir);
            Destroy(gameObject);
            return;
        }

        if (tank != null)
        {
            PlaySound(hitTankSound, tankVolume);
            Vector2 dir = ((Vector2)tank.transform.position - (Vector2)transform.position).normalized;
            tank.TakeDamage(damage, dir);
            Destroy(gameObject);
            return;
        }
    }

    void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        }
    }
}