using UnityEngine;

/// <summary>
/// Helper para garantir que as animações de movimento e tiro sejam combinadas corretamente.
/// Este script garante que IsRunning seja mantido quando o jogador está correndo e atirando.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimationHelper : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        // Garante que IsRunning seja mantido se estiver se movendo horizontalmente
        // Isso permite que Run+Shoot funcione corretamente
        if (playerMovement != null && rb != null)
        {
            bool isGrounded = playerMovement.IsGrounded;
            bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
            bool isInAir = !isGrounded;

            // Se está no chão e se movendo, mantém IsRunning ativo
            // mesmo quando atirando (permite combinação Run+Shoot)
            if (isGrounded && isMoving && !isInAir)
            {
                animator.SetBool("IsRunning", true);
            }
            // Se está no ar, mantém os estados de pulo/queda
            else if (isInAir)
            {
                if (rb.linearVelocity.y > 0.1f)
                {
                    animator.SetBool("IsJumping", true);
                    animator.SetBool("IsFalling", false);
                }
                else if (rb.linearVelocity.y < -0.1f)
                {
                    animator.SetBool("IsJumping", false);
                    animator.SetBool("IsFalling", true);
                }
            }
        }
    }
}

