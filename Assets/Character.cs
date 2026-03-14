using UnityEngine;

public class Character : MonoBehaviour
{
    public int maxHP = 3;
    [Range(0, 3)]
    public int currentHP;

    public bool isGameOver = false;
    public bool isInvincible = false;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage = 1)
    {
        if (isGameOver) return;
        if (isInvincible) return;

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
    }

    public void Heal(int amount = 1)
    {
        if (isGameOver) return;

        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
    }
}
