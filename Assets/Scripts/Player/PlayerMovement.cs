using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 4.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.3f;

    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private PlayerMovementState movementState;

    private float horizontalInput;
    private bool isFacingRight = true;
    private bool isGrounded;
    private float nextFireTime;
    private bool isKnockedBack = false;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private void Awake()
    {
        if (rigidBody == null) rigidBody = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (movementState == null) movementState = GetComponent<PlayerMovementState>();
    }

    private void Update()
    {
        if (isKnockedBack) return; // enquanto sofre knockback, ignora input

        HandleInput();
        FlipCharacterX();
        CheckGrounded();
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        if (isKnockedBack) return; // evita interferir durante o knockback
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
        rigidBody.linearVelocity = new Vector2(horizontalInput * moveSpeed, rigidBody.linearVelocity.y);
    }

    private void Jump()
    {
        rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);
        movementState.SetMoveState(PlayerMovementState.MoveState.Jump);
    }

    private void FlipCharacterX()
    {
        if (horizontalInput > 0 && !isFacingRight)
        {
            spriteRenderer.flipX = false;
            isFacingRight = true;
        }
        else if (horizontalInput < 0 && isFacingRight)
        {
            spriteRenderer.flipX = true;
            isFacingRight = false;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;

        var projectile = bullet.GetComponent<MercsOfMayhem.Weapons.Projectile>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.SetOwner(gameObject);
        }
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void UpdateAnimationState()
    {
        if (!isGrounded)
        {
            if (rigidBody.linearVelocity.y > 0.1f)
                movementState.SetMoveState(PlayerMovementState.MoveState.Jump);
            else if (rigidBody.linearVelocity.y < -0.1f)
                movementState.SetMoveState(PlayerMovementState.MoveState.Fall);
        }
        else
        {
            if (Mathf.Abs(rigidBody.linearVelocity.x) > 0.1f)
                movementState.SetMoveState(PlayerMovementState.MoveState.Run);
            else
                movementState.SetMoveState(PlayerMovementState.MoveState.Idle);
        }
    }

    public void KnockbackPlayer(Vector2 knockbackForce, int direction)
    {
        if (isKnockedBack) return; // previne múltiplos knockbacks

        isKnockedBack = true;
        movementState.SetMoveState(PlayerMovementState.MoveState.Fall);

        // limpa velocidade anterior
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0f;

        // aplica força no sentido inverso
        knockbackForce.x *= direction;
        rigidBody.AddForce(knockbackForce, ForceMode2D.Impulse);

        // libera controle após curto tempo
        StartCoroutine(RecoverFromKnockback(0.4f));
    }

    private System.Collections.IEnumerator RecoverFromKnockback(float delay)
    {
        yield return new WaitForSeconds(delay);
        isKnockedBack = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public bool IsGrounded => isGrounded;
}
