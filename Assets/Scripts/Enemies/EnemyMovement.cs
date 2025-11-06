using UnityEngine;

namespace MercsOfMayhem.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    // --- MUDANÇA: Não precisamos mais do EnemyMovementState ---
    // [RequireComponent(typeof(EnemyMovementState))] 
    [RequireComponent(typeof(Animator))] // --- MUDANÇA: Exigimos o Animator ---
    public class EnemyMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D rigidBody;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        // --- MUDANÇA: Trocamos o 'stateController' pelo 'animator' ---
        // private EnemyMovementState stateController;
        [SerializeField] private Animator animator; // <-- Precisamos do Animator

        [Header("Movement Settings")]
        [SerializeField] private float speed = 2f;
        [SerializeField] private int startDirection = 1;
        [SerializeField] private bool paraNaBeirada = true;

        [Header("Physics")]
        [SerializeField] private float idleThreshold = 0.15f; 

        // --- Nomes dos Parâmetros do Animator ---
        private const string IS_RUNNING_BOOL = "IsRunning"; // <-- (Vamos criar este parâmetro)

        private int currentDirection;
        private float halfWidth;
        private float halfHeight;
        private bool isGrounded;
        private float movementDelay;
        
        private bool allowPatrol = true;

        private void Awake()
        {
            if (rigidBody == null) rigidBody = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            
            // --- MUDANÇA: Pegamos o Animator ---
            if (animator == null) animator = GetComponent<Animator>(); 
            // stateController = GetComponent<EnemyMovementState>(); // (Não precisamos mais)
        }

        private void Start()
        {
            halfWidth = spriteRenderer.bounds.extents.x;
            halfHeight = spriteRenderer.bounds.extents.y;
            currentDirection = startDirection;
            spriteRenderer.flipX = (startDirection != 1);
        }

        private void FixedUpdate()
        {
            if (movementDelay > 0f)
            {
                movementDelay -= Time.fixedDeltaTime;
                // --- MUDANÇA: Usamos SetBool ---
                animator.SetBool(IS_RUNNING_BOOL, false); 
                return;
            }

            if (!isGrounded)
            {
                // --- MUDANÇA: Usamos SetBool ---
                animator.SetBool(IS_RUNNING_BOOL, false); 
                return;
            }

            if (allowPatrol)
            {
                Vector2 movement = new Vector2(currentDirection * speed, rigidBody.linearVelocity.y);
                rigidBody.linearVelocity = movement;

                // --- MUDANÇA: Usamos SetBool ---
                if (Mathf.Abs(rigidBody.linearVelocity.x) > idleThreshold)
                    animator.SetBool(IS_RUNNING_BOOL, true); // "Estou a correr"
                else
                    animator.SetBool(IS_RUNNING_BOOL, false); // "Estou parado (Idle)"

                SetDirection();
            }
            else
            {
                rigidBody.linearVelocity = new Vector2(0, rigidBody.linearVelocity.y);
                // --- MUDANÇA: Usamos SetBool ---
                animator.SetBool(IS_RUNNING_BOOL, false); // "Estou parado (Idle)"
            }
        }

        // ... (OnCollisionStay2D, OnCollisionExit2D, KnockbackEnemy, SetDirection) ...
        // ... (NÃO MUDAM) ...
         private void OnCollisionStay2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Ground"))
                isGrounded = true;
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Ground"))
                isGrounded = false;
        }

        public void KnockbackEnemy(Vector2 knockbackForce, int direction, float delay)
        {
            movementDelay = delay;
            knockbackForce.x *= direction;

            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.angularVelocity = 0f;
            rigidBody.AddForce(knockbackForce, ForceMode2D.Impulse);

            // --- MUDANÇA: Usamos SetBool ---
            animator.SetBool(IS_RUNNING_BOOL, false);
        }

        private void SetDirection()
        {
            if (!isGrounded) return;

            Vector2 rightPos = (Vector2)transform.position + Vector2.right * halfWidth;
            Vector2 leftPos = (Vector2)transform.position - Vector2.right * halfWidth;

            // Colisão à direita
            if (rigidBody.linearVelocity.x > 0)
            {
                if (Physics2D.Raycast(transform.position, Vector2.right, halfWidth + 0.5f, LayerMask.GetMask("Ground")) ||
                    (paraNaBeirada && !Physics2D.Raycast(rightPos, Vector2.down, halfHeight + 0.1f, LayerMask.GetMask("Ground"))))
                {
                    currentDirection *= -1;
                    spriteRenderer.flipX = true;
                }
            }
            // Colisão à esquerda
            else if (rigidBody.linearVelocity.x < 0)
            {
                if (Physics2D.Raycast(transform.position, Vector2.left, halfWidth + 0.5f, LayerMask.GetMask("Ground")) ||
                    (paraNaBeirada && !Physics2D.Raycast(leftPos, Vector2.down, halfHeight + 0.1f, LayerMask.GetMask("Ground"))))
                {
                    currentDirection *= -1;
                    spriteRenderer.flipX = false;
                }
            }
        }

        // --- Funções de Comando (do Cérebro) ---
        public void SetPatrol(bool canPatrol)
        {
            this.allowPatrol = canPatrol;
        }

        public void ForceFaceDirection(int direction)
        {
            if (currentDirection == direction) return;
            currentDirection = direction;
            spriteRenderer.flipX = (currentDirection != 1);
        }
    }
}