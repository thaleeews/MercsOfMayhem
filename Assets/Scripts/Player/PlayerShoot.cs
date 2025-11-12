using UnityEngine;
using MercsOfMayhem.Weapons; // Use o namespace da sua bala

// Podemos adicionar o 'using MercsOfMayhem.Weapons;' depois, se precisarmos.

// [RequireComponent] garante que os outros scripts estarão lá
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerShoot : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement; // Vamos precisar dele
    [SerializeField] private GameObject bulletPrefab;

    [Header("Configurações de Tiro")]
    [SerializeField] private float fireRate = 0.3f;
    private float nextFireTime;

    [Header("Pontos de Tiro (Fire Points)")]
    // Ainda não vamos usar, mas vamos deixar pronto
    [SerializeField] private Transform firePointNormal;
    [SerializeField] private Transform firePointUp;
    [SerializeField] private Transform firePointDown;
    [SerializeField] private Transform firePointJump;
    [SerializeField] private Transform firePointJumpUp;
    [SerializeField] private Transform firePointJumpDown;

    // --- Nomes dos Parâmetros do Animator ---
    // Vamos definir os nomes que VAMOS USAR
    private const string SHOOT_NORMAL_BOOL = "ShootNormal";
    private const string SHOOT_UP_BOOL = "ShootUp";
    private const string SHOOT_DOWN_BOOL = "ShootDown";
    private const string JUMP_SHOOT_NORMAL_BOOL = "JumpShootNormal";
    private const string JUMP_SHOOT_UP_BOOL = "JumpShootUp";
    private const string JUMP_SHOOT_DOWN_BOOL = "JumpShootDown";

    // Parâmetros de Movimento (que vamos LER do Animator)
    private const string IS_JUMPING_BOOL = "IsJumping";
    private const string IS_FALLING_BOOL = "IsFalling";


    private void Awake()
    {
        // Esta função pega as referências quando o jogo começa
        if (animator == null) animator = GetComponent<Animator>();
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        // --- 1. LER INPUTS ---
        bool isShooting = Input.GetMouseButton(0); // Botão esquerdo do mouse
        bool isLookingUp = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool isLookingDown = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

        // --- 2. LER ESTADO DE MOVIMENTO (do Animator e do PlayerMovement) ---
        bool isJumping = animator.GetBool(IS_JUMPING_BOOL);
        bool isFalling = animator.GetBool(IS_FALLING_BOOL);
        bool isRunning = animator.GetBool("IsRunning");
        bool isInAir = isJumping || isFalling;
        bool isMoving = Mathf.Abs(playerMovement.GetComponent<Rigidbody2D>().linearVelocity.x) > 0.1f;
        bool isGrounded = playerMovement.IsGrounded;

        // --- 3. ATUALIZAR O ANIMATOR (O CÉREBRO) ---
        // IMPORTANTE: As animações de tiro devem ser combinadas com as animações de movimento
        // O Animator precisa ter transições que permitam Run+Shoot, Jump+Shoot, etc.
        
        // Se está no ar (pulando ou caindo), usa animações de tiro no ar
        if (isInAir)
        {
            animator.SetBool(SHOOT_NORMAL_BOOL, false);
            animator.SetBool(SHOOT_UP_BOOL, false);
            animator.SetBool(SHOOT_DOWN_BOOL, false);
            animator.SetBool(JUMP_SHOOT_NORMAL_BOOL, isShooting && !isLookingUp && !isLookingDown);
            animator.SetBool(JUMP_SHOOT_UP_BOOL, isShooting && isLookingUp);
            animator.SetBool(JUMP_SHOOT_DOWN_BOOL, isShooting && isLookingDown);
        }
        else
        {
            // Se está no chão, usa animações de tiro no chão
            // IMPORTANTE: Mantém IsRunning ativo se estiver se movendo, mesmo quando atira
            animator.SetBool(JUMP_SHOOT_NORMAL_BOOL, false);
            animator.SetBool(JUMP_SHOOT_UP_BOOL, false);
            animator.SetBool(JUMP_SHOOT_DOWN_BOOL, false);
            animator.SetBool(SHOOT_NORMAL_BOOL, isShooting && !isLookingUp && !isLookingDown);
            animator.SetBool(SHOOT_UP_BOOL, isShooting && isLookingUp);
            animator.SetBool(SHOOT_DOWN_BOOL, isShooting && isLookingDown);
            
            // Garante que IsRunning seja mantido se estiver se movendo, mesmo quando atira
            if (isMoving && isGrounded)
            {
                animator.SetBool("IsRunning", true);
            }
        }

        // --- 4. LÓGICA DE DISPARO DA BALA (Ainda não implementada) ---
        // (Vamos adicionar isso no próximo passo)
        if (isShooting && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            
            // "FireBullet" é a função que vamos criar no próximo passo
            FireBullet(isInAir, isLookingUp, isLookingDown);
        }
    }

    // ... (cole esta função inteira abaixo do Update()) ...

    // Esta é a sua antiga função Shoot(), agora mais inteligente
    void FireBullet(bool isInAir, bool isLookingUp, bool isLookingDown)
    {
        // 1. Pergunta ao PlayerMovement para onde estamos virados
        bool isFacingRight = playerMovement.IsFacingRight;

        // 2. Define os padrões (tiro normal, no chão, horizontal)
        Transform spawnPoint = firePointNormal;
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        Quaternion rotation = Quaternion.identity; // Rotação 0

        // 3. Decide qual ponto, direção e rotação usar
        if (isInAir)
        {
            if (isLookingUp)
            {
                spawnPoint = firePointJumpUp;
                direction = Vector2.up;
                rotation = Quaternion.Euler(0, 0, 90); // Rotação 90 graus
            }
            else if (isLookingDown)
            {
                spawnPoint = firePointJumpDown;
                direction = Vector2.down;
                rotation = Quaternion.Euler(0, 0, -90); // Rotação -90 graus
            }
            else // Pulando, horizontal
            {
                spawnPoint = firePointJump;
                // direction e rotation já estão corretos
            }
        }
        else // No Chão
        {
            if (isLookingUp)
            {
                spawnPoint = firePointUp;
                direction = Vector2.up;
                rotation = Quaternion.Euler(0, 0, 90);
            }
            else if (isLookingDown)
            {
                spawnPoint = firePointDown;
                direction = Vector2.down;
                rotation = Quaternion.Euler(0, 0, -90);
            }
            else // Chão, horizontal
            {
                spawnPoint = firePointNormal;
                // direction e rotation já estão corretos
            }
        }
        
        // 4. Verifica se os prefabs/pontos existem
        if (bulletPrefab == null || spawnPoint == null)
        {
            Debug.LogError("PlayerShoot: Prefab da Bala ou Fire Point não configurado!");
            return;
        }

        // 5. Cria a bala (usando a posição e rotação calculadas)
        GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, rotation);
        
        // 6. Configura a bala (lógica do seu script antigo)
        var projectile = bullet.GetComponent<MercsOfMayhem.Weapons.Projectile>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.SetOwner(this.gameObject);
            
            // Ignora colisão com o Player
            Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
            Collider2D ownerCollider = GetComponent<Collider2D>();
            if (bulletCollider != null && ownerCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, ownerCollider, true);
            }
        }
    }
}