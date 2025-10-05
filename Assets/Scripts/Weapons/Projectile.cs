using UnityEngine;

namespace MercsOfMayhem.Weapons
{
    /// <summary>
    /// Projectile that moves in a direction and damages enemies
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private int damage = 25;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private LayerMask hitLayers;

        private Rigidbody2D rb;
        private Vector2 direction;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            Destroy(gameObject, lifetime);
        }

        public void SetDirection(Vector2 dir)
        {
            direction = dir.normalized;
            rb.velocity = direction * speed;

            // Rotate sprite to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Check if we hit an enemy
            if (((1 << collision.gameObject.layer) & hitLayers) != 0)
            {
                var enemy = collision.GetComponent<MercsOfMayhem.Enemies.EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
    }
}
