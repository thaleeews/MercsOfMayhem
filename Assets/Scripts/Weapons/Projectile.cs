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


        void Awake()
        {
            Destroy(gameObject, 3f);
        }
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

            if (collision.isTrigger)
            {
            return; // Pare a função aqui e não faça nada.
            }
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

            else if (collision.CompareTag("Player"))
            {
                var playerHealth = collision.GetComponent<PlayerHealth>(); 
                if (playerHealth != null)
                {
                    playerHealth.DamagePlayer(damage); 
                    // Se der erro, o nome da sua função pode ser 'TakeDamage'
                    // playerHealth.TakeDamage(damage); 
                }
                Destroy(gameObject); // Destrói a bala
        }
        }
    }
}
