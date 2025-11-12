using UnityEngine;
using System.Collections; // Adicionado para a Coroutine

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 4.5f;

    // --- LÓGICA DE TIRO REMOVIDA DESTA SEÇÃO ---

    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private PlayerMovementState movementState;

    private float horizontalInput;
    private bool isFacingRight = true;
    private bool isGrounded;
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
        if (isKnockedBack) return; 

        HandleInput();
        FlipCharacterX();
        CheckGrounded();
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        if (isKnockedBack) return; 
        HandleMovement();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // --- LÓGICA DE TIRO REMOVIDA DESTA FUNÇÃO ---
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

    // --- FUNÇÃO SHOOT() REMOVIDA DAQUI ---

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void UpdateAnimationState()
    {
        // IMPORTANTE: Não atualiza o estado se estiver atirando, para permitir combinação de animações
        // O PlayerShoot vai gerenciar as animações de tiro, mas mantém os estados de movimento
        
        if (!isGrounded)
        {
            if (rigidBody.linearVelocity.y > 0.1f)
                movementState.SetMoveState(PlayerMovementState.MoveState.Jump);
            else if (rigidBody.linearVelocity.y < -0.1f)
                movementState.SetMoveState(PlayerMovementState.MoveState.Fall);
        }
        else
        {
            // Sempre atualiza o estado de movimento, mesmo quando atirando
            // Isso permite que Run+Shoot funcione corretamente
            if (Mathf.Abs(rigidBody.linearVelocity.x) > 0.1f)
                movementState.SetMoveState(PlayerMovementState.MoveState.Run);
            else
                movementState.SetMoveState(PlayerMovementState.MoveState.Idle);
        }
    }

    public void KnockbackPlayer(Vector2 knockbackForce, int direction)
    {
        if (isKnockedBack) return; 

        isKnockedBack = true;
        movementState.SetMoveState(PlayerMovementState.MoveState.Fall);

        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0f;

        knockbackForce.x *= direction;
        rigidBody.AddForce(knockbackForce, ForceMode2D.Impulse);

        StartCoroutine(RecoverFromKnockback(0.4f));
    }

    private IEnumerator RecoverFromKnockback(float delay)
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

    // --- ADICIONADO PARA O SCRIPT DE TIRO SABER A DIREÇÃO ---
    public bool IsFacingRight => isFacingRight;
}