using UnityEngine;

namespace MercsOfMayhem.Characters
{
    /// <summary>
    /// Main controller for the player character.
    /// Handles movement, shooting, and interactions.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 12f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckRadius = 0.2f;

        [Header("Shooting")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.3f;

        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private bool isGrounded;
        private float horizontalInput;
        private float nextFireTime;
        private bool isFacingRight = true;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            HandleInput();
            CheckGroundStatus();
            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void HandleInput()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                Jump();
            }

            if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }

        private void HandleMovement()
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

            // Flip sprite based on movement direction
            if (horizontalInput > 0 && spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
                isFacingRight = true;
            }
            else if (horizontalInput < 0 && spriteRenderer != null)
            {
                spriteRenderer.flipX = true;
                isFacingRight = false;
            }
        }

        private void UpdateAnimator()
        {
            if (animator == null) return;

            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VelocityY", rb.velocity.y);
        }

        private void Jump()
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        private void CheckGroundStatus()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        private void Shoot()
        {
            if (bulletPrefab == null || firePoint == null) return;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Vector2 shootDirection = isFacingRight ? Vector2.right : Vector2.left;

            var projectile = bullet.GetComponent<MercsOfMayhem.Weapons.Projectile>();
            if (projectile != null)
            {
                projectile.SetDirection(shootDirection);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}
