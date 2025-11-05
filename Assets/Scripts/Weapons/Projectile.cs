using UnityEngine;

namespace MercsOfMayhem.Weapons
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private int damage = 20;
        [SerializeField] private LayerMask hitLayers;
        private Vector2 direction;
        private GameObject owner;

        private void Update()
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }

        public void SetDirection(Vector2 dir)
        {
            direction = dir.normalized;
        }

        public void SetOwner(GameObject shooter)
        {
            owner = shooter;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 1. Evita colidir com quem atirou (como você já tinha)
            if (collision.gameObject == owner)
                return;

            // 2. Verifica se colidiu com o Inimigo
            if (collision.CompareTag("Enemy"))
            {
                var enemy = collision.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }

                // Destrói a bala ao acertar o inimigo
                Destroy(gameObject);
            }

            // 3. Verifica se colidiu com o Chão
            else if (collision.CompareTag("Ground"))
            {
                // Destrói a bala ao acertar o chão
                Destroy(gameObject);
            }
        }
    }
}
