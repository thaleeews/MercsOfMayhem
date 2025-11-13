using UnityEngine;
using MercsOfMayhem.Weapons;

namespace MercsOfMayhem.Enemies
{
    /// <summary>
    /// Script de tiro do Boss. Atira periodicamente para a esquerda.
    /// </summary>
    public class BossShoot : MonoBehaviour
    {
        [Header("Configuração da Arma")]
        [SerializeField] private GameObject bulletPrefab; // O prefab da bala (Bullet)
        [SerializeField] private Transform shootingPosition; // O ponto de tiro (ShootingPositionBoss)
        [SerializeField] private float fireRate = 2f; // Tempo entre tiros em segundos (padrão: 2 segundos)
        
        [Header("Animação")]
        [SerializeField] private Animator animator; // Animator do boss
        [SerializeField] private string fireAnimationTrigger = "Fire"; // Nome do trigger da animação Fire

        private float nextFireTime = 0f;
        private bool isShooting = false;

        private void Awake()
        {
            // Se o shootingPosition não foi configurado, tenta encontrar pelo nome
            if (shootingPosition == null)
            {
                Transform found = transform.Find("ShootingPositionBoss");
                if (found != null)
                {
                    shootingPosition = found;
                }
                else
                {
                    Debug.LogError($"BossShoot: Não foi possível encontrar 'ShootingPositionBoss' na hierarquia do boss '{gameObject.name}'!");
                }
            }

            // Se o animator não foi configurado, tenta pegar do próprio GameObject
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null)
                {
                    Debug.LogWarning($"BossShoot: Animator não encontrado no boss '{gameObject.name}'!");
                }
            }
        }

        private void Start()
        {
            // Inicia o primeiro tiro após o fireRate
            nextFireTime = Time.time + fireRate;
        }

        private void Update()
        {
            // Verifica se é hora de atirar
            if (Time.time >= nextFireTime && !isShooting)
            {
                Shoot();
            }
        }

        /// <summary>
        /// Executa o tiro: animação + instanciação da bala
        /// </summary>
        private void Shoot()
        {
            if (bulletPrefab == null)
            {
                Debug.LogError($"BossShoot: Bullet prefab não configurado no boss '{gameObject.name}'!");
                return;
            }

            if (shootingPosition == null)
            {
                Debug.LogError($"BossShoot: ShootingPosition não configurado no boss '{gameObject.name}'!");
                return;
            }

            // Executa a animação Fire
            if (animator != null)
            {
                animator.SetTrigger(fireAnimationTrigger);
            }

            // Instancia a bala
            FireBullet();

            // Atualiza o próximo tempo de tiro
            nextFireTime = Time.time + fireRate;
        }

        /// <summary>
        /// Instancia a bala e a configura para ir para a esquerda (eixo X invertido)
        /// </summary>
        private void FireBullet()
        {
            // O boss está virado para a esquerda, então atiramos para a esquerda (Vector2.left)
            Vector2 direction = Vector2.left; // Eixo X invertido = esquerda

            // Cria a bala na posição de tiro
            GameObject bullet = Instantiate(bulletPrefab, shootingPosition.position, Quaternion.identity);

            // Configura a bala
            var projectile = bullet.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetDirection(direction);
                projectile.SetOwner(this.gameObject);

                // Ignora colisão entre a bala e o boss
                Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
                Collider2D bossCollider = GetComponent<Collider2D>();
                if (bulletCollider != null && bossCollider != null)
                {
                    Physics2D.IgnoreCollision(bulletCollider, bossCollider, true);
                }
            }
            else
            {
                Debug.LogWarning($"BossShoot: O prefab da bala não tem o componente Projectile!");
            }
        }

        /// <summary>
        /// Pode ser chamado pela animação para sincronizar o tiro com a animação
        /// </summary>
        public void OnFireAnimationEvent()
        {
            // Este método pode ser chamado por um Animation Event na animação Fire
            // Se preferir, pode mover a lógica de FireBullet() para cá
        }
    }
}

