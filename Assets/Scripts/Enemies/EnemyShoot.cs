using UnityEngine;
using MercsOfMayhem.Weapons; // Namespace da sua bala

namespace MercsOfMayhem.Enemies
{
    // Este script será a "arma" do inimigo
    public class EnemyShoot : MonoBehaviour
    {
        [Header("Configuração da Arma")]
        [SerializeField] private GameObject bulletPrefab; // O prefab da bala do inimigo
        [SerializeField] private Transform firePoint;    // O "ponto de tiro" (um Empty Object)
        [SerializeField] private float fireRate = 1f;      // 1 tiro por segundo

        private float nextFireTime = 0f; // Controla o cooldown do tiro

        /// <summary>
        /// Esta é a função pública que o "Cérebro" (Enemy.cs) vai chamar.
        /// </summary>
        /// <param name="targetPosition">A posição do alvo (Player)</param>
        public void TryToShoot(Vector2 targetPosition)
        {
            // 1. Verifica o Cooldown
            if (Time.time < nextFireTime)
            {
                // Ainda não é hora de atirar, saia da função
                return; 
            }

            // 2. Se pode atirar, reinicia o cooldown
            nextFireTime = Time.time + fireRate;

            // 3. Calcula a direção do tiro
            // (Não precisamos mais virar o 'firePoint',
            // apenas calculamos para onde a bala deve ir)
            Vector2 direction = (targetPosition - (Vector2)firePoint.position).normalized;

            // 4. Instancia a bala
            if (bulletPrefab != null && firePoint != null)
            {
                // Cria a bala virada para a direção correta
                // (Usamos 'Quaternion.LookRotation' para "apontar" a bala)
                // (O 'transform.forward' é para 3D, então usamos 'transform.right' para 2D)
                // (Esta é uma forma avançada de rotacionar. Uma forma mais simples é Quaternion.identity se a bala se orientar sozinha)
                
                // Vamos usar o método simples primeiro,
                // já que o seu 'Projectile' usa SetDirection.
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

                var projectile = bullet.GetComponent<Projectile>();
                if (projectile != null)
                {
                    // Diz à bala para onde ir
                    projectile.SetDirection(direction); 
                    projectile.SetOwner(this.gameObject); // O "owner" é o Inimigo

                    // Ignora colisão entre a bala e o corpo do inimigo
                    Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
                    Collider2D ownerCollider = GetComponent<Collider2D>(); // Pega o CapsuleCollider do Inimigo
                    if (bulletCollider != null && ownerCollider != null)
                    {
                        Physics2D.IgnoreCollision(bulletCollider, ownerCollider, true);
                    }
                }
            }
        }
    }
}