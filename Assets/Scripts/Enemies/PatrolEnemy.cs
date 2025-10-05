using UnityEngine;

namespace MercsOfMayhem.Enemies
{
    /// <summary>
    /// Simple enemy that patrols left and right
    /// </summary>
    public class PatrolEnemy : EnemyBase
    {
        [Header("Patrol Settings")]
        [SerializeField] private float patrolDistance = 5f;
        [SerializeField] private bool startMovingRight = true;

        private Vector3 startPosition;
        private bool movingRight;
        private SpriteRenderer spriteRenderer;
        private Animator animator;

        protected override void Awake()
        {
            base.Awake();
            startPosition = transform.position;
            movingRight = startMovingRight;
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        protected override void UpdateBehavior()
        {
            Patrol();
        }

        private void Patrol()
        {
            // Move in current direction
            float direction = movingRight ? 1f : -1f;
            transform.Translate(Vector2.right * direction * moveSpeed * Time.deltaTime);

            // Flip sprite based on direction
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !movingRight;
            }

            // Check if reached patrol distance
            float distanceFromStart = transform.position.x - startPosition.x;

            if (movingRight && distanceFromStart >= patrolDistance)
            {
                movingRight = false;
            }
            else if (!movingRight && distanceFromStart <= -patrolDistance)
            {
                movingRight = true;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 drawPosition = Application.isPlaying ? startPosition : transform.position;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                drawPosition + Vector3.left * patrolDistance,
                drawPosition + Vector3.right * patrolDistance
            );

            Gizmos.DrawWireSphere(drawPosition + Vector3.left * patrolDistance, 0.3f);
            Gizmos.DrawWireSphere(drawPosition + Vector3.right * patrolDistance, 0.3f);
        }
    }
}
