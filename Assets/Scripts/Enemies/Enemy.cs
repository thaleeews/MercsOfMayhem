using UnityEngine;
using MercsOfMayhem.Enemies;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int damageOnContact = 10;

    [SerializeField] private Vector2 knockbackToSelf = new Vector2(6f, 10f);
    [SerializeField] private Vector2 knockbackToPlayer = new Vector2(3f, 5f);
    [SerializeField] private float knockbackDelayToSelf = 1.5f;

    protected int currentHealth;
    protected bool isDead = false;

    public void Die() => Destroy(gameObject);

    public void HitPlayer(Transform playerTransform)
    {
        int direction = GetDirection(playerTransform);
        FindObjectOfType<PlayerMovement>().KnockbackPlayer(knockbackToPlayer, direction);
        FindObjectOfType<PlayerHealth>().DamagePlayer(damageOnContact);
        GetComponent<EnemyMovement>().KnockbackEnemy(knockbackToSelf, -direction, knockbackDelayToSelf);
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    private int GetDirection(Transform playerTransform)
        => transform.position.x > playerTransform.position.x ? -1 : 1;
}
