using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovementState : MonoBehaviour
{
    public enum MoveState { Idle, Run, Jump, Fall, DoubleJump, WallJump }

    public MoveState CurrentMoveState { get; private set; }

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private MercsOfMayhem.Characters.PlayerController playerController;

    public static Action<MoveState> OnPlayerMoveStateChanged;

    private const string idleAnim = "Idle";
    private const string runAnim = "Run";
    private const string jumpAnim = "Jump";
    private const string fallAnim = "Fall";

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (playerController == null)
            playerController = GetComponent<MercsOfMayhem.Characters.PlayerController>();
    }

    private void Update()
    {
        UpdateMoveState();
    }

    private void UpdateMoveState()
    {
        float velY = rb.linearVelocity.y;
        float velX = Mathf.Abs(rb.linearVelocity.x);
        bool grounded = IsGrounded();

        if (grounded)
        {
            if (velX < 0.1f)
                SetMoveState(MoveState.Idle);
            else
                SetMoveState(MoveState.Run);
        }
        else
        {
            if (velY > 0.1f)
                SetMoveState(MoveState.Jump);
            else if (velY < -0.1f)
                SetMoveState(MoveState.Fall);
        }
    }

    private bool IsGrounded()
    {
        // Se o PlayerController já tiver lógica de chão, podemos usá-la
        if (playerController != null)
        {
            var groundCheckField = typeof(MercsOfMayhem.Characters.PlayerController)
                .GetField("isGrounded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (groundCheckField != null)
                return (bool)groundCheckField.GetValue(playerController);
        }

        // Caso contrário, consideramos o player no ar
        return false;
    }

    public void SetMoveState(MoveState moveState)
    {
        if (CurrentMoveState == moveState) return;

        switch (moveState)
        {
            case MoveState.Idle:
                animator.Play(idleAnim);
                break;
            case MoveState.Run:
                animator.Play(runAnim);
                break;
            case MoveState.Jump:
                animator.Play(jumpAnim);
                break;
            case MoveState.Fall:
                animator.Play(fallAnim);
                break;
            default:
                animator.Play(idleAnim);
                break;
        }

        OnPlayerMoveStateChanged?.Invoke(moveState);
        CurrentMoveState = moveState;
    }
}
