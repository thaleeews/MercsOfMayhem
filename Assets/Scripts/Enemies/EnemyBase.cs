using UnityEngine;
using MercsOfMayhem.Characters;

namespace MercsOfMayhem.Enemies
{
    /// <summary>
    /// Base class for all enemy types.
    /// Extend this for specific enemy behaviors.
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] protected int maxHealth = 100;
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected int damageOnContact = 10;

        protected int currentHealth;
        protected bool isDead = false;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
        }

        protected virtual void Update()
        {
            if (!isDead)
            {
                UpdateBehavior();
            }
        }

        /// <summary>
        /// Override this to define enemy-specific behavior
        /// </summary>
        protected abstract void UpdateBehavior();

        public virtual void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            OnTakeDamage();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void OnTakeDamage()
        {
            // Override for damage feedback (flash, sound, etc.)
        }

        protected virtual void Die()
        {
            isDead = true;
            OnDeath();
            Destroy(gameObject, 0.5f);
        }

        protected virtual void OnDeath()
        {
            // Override for death effects (explosion, loot, etc.)
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // Deal damage to player
                var player = collision.gameObject.GetComponent<PlayerController>();
                if (player != null)
                {
                    // TODO: Implement player damage system
                }
            }
        }
    }
}
