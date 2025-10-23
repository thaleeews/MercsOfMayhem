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
            transform.Translate(direction * speed * Time.deltaTime);
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
            // evita colidir com quem atirou
            if (collision.gameObject == owner)
                return;

            // verifica se acertou inimigo
            if (((1 << collision.gameObject.layer) & hitLayers) != 0)
            {
                var enemy = collision.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }

                Destroy(gameObject);
            }
        }
    }
}
