using UnityEngine;

namespace MercsOfMayhem.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(EnemyMovementState))]
    public class EnemyMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D rigidBody;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private EnemyMovementState stateController;

        [Header("Movement Settings")]
        [SerializeField] private float speed = 2f;
        [SerializeField] private int startDirection = 1;
        [SerializeField] private bool paraNaBeirada = true;

        [Header("Physics")]
        [SerializeField] private float idleThreshold = 0.15f; // velocidade mínima para Run

        private int currentDirection;
        private float halfWidth;
        private float halfHeight;
        private bool isGrounded;
        private float movementDelay;

        private void Awake()
        {
            if (rigidBody == null) rigidBody = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            stateController = GetComponent<EnemyMovementState>();
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
            // Se em knockback ou delay, fica Idle
            if (movementDelay > 0f)
            {
                movementDelay -= Time.fixedDeltaTime;
                stateController.SetMoveState(EnemyMovementState.MoveState.Idle);
                return;
            }

            if (!isGrounded)
            {
                stateController.SetMoveState(EnemyMovementState.MoveState.Idle);
                return;
            }

            // Movimento horizontal
            Vector2 movement = new Vector2(currentDirection * speed, rigidBody.linearVelocity.y);
            rigidBody.linearVelocity = movement;

            // Atualiza animação com base na velocidade
            if (Mathf.Abs(rigidBody.linearVelocity.x) > idleThreshold)
                stateController.SetMoveState(EnemyMovementState.MoveState.Run);
            else
                stateController.SetMoveState(EnemyMovementState.MoveState.Idle);

            // Define direção com base em colisão/bordas
            SetDirection();
        }

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

            stateController.SetMoveState(EnemyMovementState.MoveState.Idle);
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
    }
}
