using UnityEngine;
using System.Collections;
using MercsOfMayhem.Weapons;

namespace MercsOfMayhem.Enemies
{
    /// <summary>
    /// Controlador principal do Boss com sistema de fases, movimento e ataques variados
    /// INSTRUÇÕES: Simplesmente adicione este componente ao GameObject do Boss e ele se configurará automaticamente!
    /// </summary>
    [RequireComponent(typeof(BossHealth))]
    public class BossController : MonoBehaviour
    {
        #region Enums
        public enum BossPhase
        {
            Phase1,  // 100% - 66% HP: Ataques lentos, movimento básico
            Phase2,  // 66% - 33% HP: Ataques mais rápidos, movimento mais agressivo
            Phase3   // 33% - 0% HP: Ataques muito rápidos, padrões especiais
        }

        public enum AttackPattern
        {
            SingleShot,      // Tiro único direto no player
            TripleShot,      // 3 tiros em leque
            RapidFire,       // Rajada rápida
            CircularPattern  // Tiros em círculo (danmaku style)
        }
        #endregion

        #region Serialized Fields
        [Header("Referencias")]
        [SerializeField] private BossHealth bossHealth;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform shootingPosition;
        [SerializeField] private GameObject bulletPrefab;

        [Header("Detecção do Player")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private float detectionRange = 20f;

        [Header("Movimento")]
        [SerializeField] private bool canMove = true;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private Vector2 movementBoundsMin = new Vector2(-5f, 0f);
        [SerializeField] private Vector2 movementBoundsMax = new Vector2(5f, 5f);
        [SerializeField] private float moveInterval = 2f; // Tempo entre movimentos

        [Header("Fase 1 (100% - 66% HP)")]
        [SerializeField] private float phase1FireRate = 2.5f;
        [SerializeField] private AttackPattern phase1Pattern = AttackPattern.SingleShot;

        [Header("Fase 2 (66% - 33% HP)")]
        [SerializeField] private float phase2FireRate = 1.5f;
        [SerializeField] private AttackPattern phase2Pattern = AttackPattern.TripleShot;
        [SerializeField] private float phase2MoveSpeed = 4.5f;

        [Header("Fase 3 (33% - 0% HP)")]
        [SerializeField] private float phase3FireRate = 0.8f;
        [SerializeField] private AttackPattern phase3Pattern = AttackPattern.RapidFire;
        [SerializeField] private float phase3MoveSpeed = 6f;
        [SerializeField] private bool useCircularPattern = true;
        [SerializeField] private float circularPatternInterval = 5f;

        [Header("Configurações de Tiro")]
        [SerializeField] private float tripleShotAngle = 15f; // Ângulo entre tiros do TripleShot
        [SerializeField] private int rapidFireCount = 5;
        [SerializeField] private float rapidFireDelay = 0.15f;
        [SerializeField] private int circularBulletCount = 12;

        [Header("Animações")]
        [SerializeField] private string fireAnimationTrigger = "Fire";
        [SerializeField] private string moveAnimationBool = "IsMoving"; // Opcional: só usa se existir no Animator
        #endregion

        #region Private Fields
        private Transform playerTransform;
        private BossPhase currentPhase = BossPhase.Phase1;
        private float nextFireTime;
        private float nextMoveTime;
        private float nextCircularPatternTime;
        private Vector2 targetPosition;
        private bool isMoving;
        private bool isDead;
        private Rigidbody2D rb;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            AutoConfigureComponents();
        }

        /// <summary>
        /// Configura automaticamente todos os componentes necessários
        /// </summary>
        private void AutoConfigureComponents()
        {
            // Busca BossHealth
            if (bossHealth == null)
            {
                bossHealth = GetComponent<BossHealth>();
                if (bossHealth == null)
                {
                    Debug.LogError("BossController: BossHealth não encontrado! Adicionando automaticamente...");
                    bossHealth = gameObject.AddComponent<BossHealth>();
                }
            }

            // Busca Animator
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null)
                    Debug.LogWarning("BossController: Animator não encontrado. Animações não funcionarão.");
            }

            // Busca SpriteRenderer
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                    Debug.LogWarning("BossController: SpriteRenderer não encontrado. Efeitos visuais limitados.");
            }

            // Busca ou cria Rigidbody2D
            rb = GetComponent<Rigidbody2D>();
            if (rb == null && canMove)
            {
                Debug.LogWarning("BossController: Rigidbody2D não encontrado. Adicionando automaticamente...");
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
            }

            // Busca shooting position
            if (shootingPosition == null)
            {
                // Tenta encontrar pelo nome
                Transform found = transform.Find("ShootingPositionBoss");
                if (found == null)
                    found = transform.Find("ShootingPosition");
                
                if (found != null)
                {
                    shootingPosition = found;
                }
                else
                {
                    // Cria automaticamente se não existir
                    Debug.LogWarning("BossController: ShootingPosition não encontrado. Criando automaticamente...");
                    GameObject shootPosObj = new GameObject("ShootingPositionBoss");
                    shootPosObj.transform.SetParent(transform);
                    shootPosObj.transform.localPosition = new Vector3(-1f, 0f, 0f); // Posição padrão (à esquerda do sprite)
                    shootingPosition = shootPosObj.transform;
                }
            }

            // Busca bullet prefab se não configurado
            if (bulletPrefab == null)
            {
                // Tenta encontrar na cena ou recursos
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.name.Contains("Bullet") && obj.GetComponent<Projectile>() != null)
                    {
                        bulletPrefab = obj;
                        Debug.Log($"BossController: Bullet prefab encontrado automaticamente: {obj.name}");
                        break;
                    }
                }
                
                if (bulletPrefab == null)
                    Debug.LogError("BossController: Bullet Prefab não encontrado! O boss não poderá atirar. Configure manualmente no Inspector.");
            }

            Debug.Log("✅ BossController configurado automaticamente!");
        }

        /// <summary>
        /// [EDITOR] Método que pode ser chamado clicando com botão direito no componente
        /// </summary>
        [ContextMenu("🔧 Reconfigurar Boss Automaticamente")]
        private void ReconfigureBoss()
        {
            AutoConfigureComponents();
            Debug.Log("🔄 Boss reconfigurado com sucesso!");
        }

        private void Start()
        {
            FindPlayer();
            
            // Inscreve-se no evento de mudança de vida do boss
            BossHealth.OnBossHealthChanged += OnHealthChanged;
            BossHealth.OnBossDie += OnBossDeath;

            // Inicializa timers
            nextFireTime = Time.time + phase1FireRate;
            nextMoveTime = Time.time + moveInterval;
            nextCircularPatternTime = Time.time + circularPatternInterval;

            // Define fase inicial
            UpdatePhase();
        }

        private void OnDestroy()
        {
            // Desinscreve dos eventos
            BossHealth.OnBossHealthChanged -= OnHealthChanged;
            BossHealth.OnBossDie -= OnBossDeath;
        }

        private void Update()
        {
            if (isDead || playerTransform == null) return;

            // Atualiza fase baseado na vida
            UpdatePhase();

            // Movimentação
            if (canMove && Time.time >= nextMoveTime)
            {
                ChooseNewPosition();
                nextMoveTime = Time.time + moveInterval;
            }

            if (isMoving)
            {
                MoveTowardsTarget();
            }

            // Sistema de ataque
            if (Time.time >= nextFireTime)
            {
                PerformAttack();
                nextFireTime = Time.time + GetCurrentFireRate();
            }

            // Padrão circular especial na Fase 3
            if (currentPhase == BossPhase.Phase3 && useCircularPattern)
            {
                if (Time.time >= nextCircularPatternTime)
                {
                    StartCoroutine(CircularAttackPattern());
                    nextCircularPatternTime = Time.time + circularPatternInterval;
                }
            }

            // Atualiza direção do sprite
            UpdateFacing();
        }
        #endregion

        #region Player Detection
        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("🎯 Boss encontrou o player!");
            }
            else
            {
                Debug.LogWarning("BossController: Player não encontrado! Verifique a tag.");
            }
        }

        private bool IsPlayerInRange()
        {
            if (playerTransform == null) return false;
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            return distance <= detectionRange;
        }
        #endregion

        #region Phase Management
        private void UpdatePhase()
        {
            if (bossHealth == null) return;

            float healthPercent = (float)bossHealth.CurrentHealth / bossHealth.MaxHealth;
            BossPhase newPhase;

            if (healthPercent > 0.66f)
                newPhase = BossPhase.Phase1;
            else if (healthPercent > 0.33f)
                newPhase = BossPhase.Phase2;
            else
                newPhase = BossPhase.Phase3;

            if (newPhase != currentPhase)
            {
                currentPhase = newPhase;
                OnPhaseChanged();
            }
        }

        private void OnPhaseChanged()
        {
            Debug.Log($"🔥 Boss entrou na {currentPhase}!");

            // Ajusta velocidade de movimento baseado na fase
            switch (currentPhase)
            {
                case BossPhase.Phase1:
                    moveSpeed = GetComponent<Rigidbody2D>() ? phase2MoveSpeed : moveSpeed;
                    break;
                case BossPhase.Phase2:
                    moveSpeed = phase2MoveSpeed;
                    break;
                case BossPhase.Phase3:
                    moveSpeed = phase3MoveSpeed;
                    break;
            }

            // Efeito visual (pode adicionar partículas, flash, etc)
            StartCoroutine(PhaseChangeEffect());
        }

        private IEnumerator PhaseChangeEffect()
        {
            // Flash rápido para indicar mudança de fase
            if (spriteRenderer != null)
            {
                Color originalColor = spriteRenderer.color;
                
                for (int i = 0; i < 3; i++)
                {
                    spriteRenderer.color = Color.red;
                    yield return new WaitForSeconds(0.1f);
                    spriteRenderer.color = originalColor;
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        private float GetCurrentFireRate()
        {
            return currentPhase switch
            {
                BossPhase.Phase1 => phase1FireRate,
                BossPhase.Phase2 => phase2FireRate,
                BossPhase.Phase3 => phase3FireRate,
                _ => phase1FireRate
            };
        }

        private AttackPattern GetCurrentAttackPattern()
        {
            return currentPhase switch
            {
                BossPhase.Phase1 => phase1Pattern,
                BossPhase.Phase2 => phase2Pattern,
                BossPhase.Phase3 => phase3Pattern,
                _ => phase1Pattern
            };
        }
        #endregion

        #region Movement
        private void ChooseNewPosition()
        {
            // Escolhe posição aleatória dentro dos limites
            targetPosition = new Vector2(
                Random.Range(movementBoundsMin.x, movementBoundsMax.x),
                Random.Range(movementBoundsMin.y, movementBoundsMax.y)
            );

            isMoving = true;

            // Define animação de movimento (se o parâmetro existir no Animator)
            if (animator != null && HasParameter(animator, moveAnimationBool))
                animator.SetBool(moveAnimationBool, true);
        }

        private void MoveTowardsTarget()
        {
            float step = moveSpeed * Time.deltaTime;
            Vector2 newPosition = Vector2.MoveTowards(transform.position, targetPosition, step);

            if (rb != null)
                rb.MovePosition(newPosition);
            else
                transform.position = newPosition;

            // Chegou ao destino?
            if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                if (animator != null && HasParameter(animator, moveAnimationBool))
                    animator.SetBool(moveAnimationBool, false);
            }
        }

        /// <summary>
        /// Verifica se o Animator tem um parâmetro específico
        /// </summary>
        private bool HasParameter(Animator anim, string paramName)
        {
            if (anim == null || string.IsNullOrEmpty(paramName)) return false;
            
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == paramName)
                    return true;
            }
            return false;
        }

        private void UpdateFacing()
        {
            if (playerTransform == null || spriteRenderer == null) return;

            // Vira para o player
            // CORRIGIDO: Se player está à DIREITA (x > boss.x), flipX = true (vira pra direita)
            // Se player está à ESQUERDA (x < boss.x), flipX = false (mantém olhando pra esquerda)
            bool shouldFlip = playerTransform.position.x > transform.position.x;
            spriteRenderer.flipX = shouldFlip;
        }
        #endregion

        #region Attack System
        private void PerformAttack()
        {
            if (!IsPlayerInRange() || playerTransform == null) return;

            // Toca animação de ataque
            if (animator != null)
                animator.SetTrigger(fireAnimationTrigger);

            // Executa padrão de ataque baseado na fase
            AttackPattern pattern = GetCurrentAttackPattern();

            switch (pattern)
            {
                case AttackPattern.SingleShot:
                    FireSingleShot();
                    break;
                case AttackPattern.TripleShot:
                    FireTripleShot();
                    break;
                case AttackPattern.RapidFire:
                    StartCoroutine(FireRapidShots());
                    break;
                case AttackPattern.CircularPattern:
                    StartCoroutine(CircularAttackPattern());
                    break;
            }
        }

        private void FireSingleShot()
        {
            if (bulletPrefab == null || shootingPosition == null || playerTransform == null) return;

            Vector2 direction = (playerTransform.position - shootingPosition.position).normalized;
            CreateBullet(shootingPosition.position, direction);
        }

        private void FireTripleShot()
        {
            if (bulletPrefab == null || shootingPosition == null || playerTransform == null) return;

            Vector2 directionToPlayer = (playerTransform.position - shootingPosition.position).normalized;

            // Tiro central
            CreateBullet(shootingPosition.position, directionToPlayer);

            // Tiro superior
            Vector2 upDirection = Quaternion.Euler(0, 0, tripleShotAngle) * directionToPlayer;
            CreateBullet(shootingPosition.position, upDirection);

            // Tiro inferior
            Vector2 downDirection = Quaternion.Euler(0, 0, -tripleShotAngle) * directionToPlayer;
            CreateBullet(shootingPosition.position, downDirection);
        }

        private IEnumerator FireRapidShots()
        {
            for (int i = 0; i < rapidFireCount; i++)
            {
                FireSingleShot();
                yield return new WaitForSeconds(rapidFireDelay);
            }
        }

        private IEnumerator CircularAttackPattern()
        {
            float angleStep = 360f / circularBulletCount;
            
            for (int i = 0; i < circularBulletCount; i++)
            {
                float angle = i * angleStep;
                Vector2 direction = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );

                CreateBullet(shootingPosition.position, direction);
            }

            yield return null;
        }

        private void CreateBullet(Vector2 position, Vector2 direction)
        {
            if (bulletPrefab == null) return;

            GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);

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
        }
        #endregion

        #region Event Handlers
        private void OnHealthChanged(int newHealth)
        {
            // Aqui pode adicionar efeitos visuais quando toma dano
            Debug.Log($"Boss HP: {newHealth}/{bossHealth.MaxHealth}");
        }

        private void OnBossDeath()
        {
            isDead = true;
            isMoving = false;
            
            // Para todas as corrotinas
            StopAllCoroutines();

            Debug.Log("💀 Boss foi derrotado!");
        }
        #endregion

        #region Gizmos (Debug Visual)
        private void OnDrawGizmosSelected()
        {
            // Desenha o range de detecção
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Desenha os limites de movimento
            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3(
                (movementBoundsMin.x + movementBoundsMax.x) / 2f,
                (movementBoundsMin.y + movementBoundsMax.y) / 2f,
                0
            );
            Vector3 size = new Vector3(
                movementBoundsMax.x - movementBoundsMin.x,
                movementBoundsMax.y - movementBoundsMin.y,
                0
            );
            Gizmos.DrawWireCube(center, size);

            // Desenha posição alvo (se estiver movendo)
            if (isMoving)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(targetPosition, 0.3f);
                Gizmos.DrawLine(transform.position, targetPosition);
            }
        }
        #endregion
    }
}

