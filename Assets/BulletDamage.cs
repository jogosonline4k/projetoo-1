using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Tenta pegar EnemyZombie
        EnemyZombie zombie = other.GetComponentInParent<EnemyZombie>();

        // Se for nulo, tenta pegar EnemyZombieTank
        EnemyZombieTank tank = null;
        if (zombie == null)
            tank = other.GetComponentInParent<EnemyZombieTank>();

        // Se acertou ZOMBIE NORMAL
        if (zombie != null)
        {
            Vector2 dir = ((Vector2)zombie.transform.position - (Vector2)transform.position).normalized;
            zombie.TakeDamage(damage, dir);
            Destroy(gameObject);
            return;
        }

        // Se acertou ZOMBIE TANK
        if (tank != null)
        {
            Vector2 dir = ((Vector2)tank.transform.position - (Vector2)transform.position).normalized;
            tank.TakeDamage(damage, dir);
            Destroy(gameObject);
            return;
        }
    }
}
