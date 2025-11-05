using UnityEngine;
using System;

public class PlayerMovementState : MonoBehaviour
{
    public enum MoveState { Idle, Run, Jump, Fall }
    public MoveState CurrentMoveState { get; private set; }

    [SerializeField] private Animator animator;

    // --- MUDANÇA: Nomes dos Parâmetros (Bools) ---
    // Estes são os nomes que vamos criar no Animator.
    // (Note que os nomes são de "estados", como "Está Correndo?")
    private const string IS_RUNNING_BOOL = "IsRunning";
    private const string IS_JUMPING_BOOL = "IsJumping";
    private const string IS_FALLING_BOOL = "IsFalling";

    public static Action<MoveState> OnPlayerMoveStateChanged;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void SetMoveState(MoveState newState)
    {
        if (CurrentMoveState == newState) return;

        switch (newState)
        {
            case MoveState.Idle:
                animator.SetBool(IS_RUNNING_BOOL, false);
                animator.SetBool(IS_JUMPING_BOOL, false);
                animator.SetBool(IS_FALLING_BOOL, false);
                break;
                
            case MoveState.Run:
                animator.SetBool(IS_RUNNING_BOOL, true);
                animator.SetBool(IS_JUMPING_BOOL, false);
                animator.SetBool(IS_FALLING_BOOL, false);
                break;
                
            case MoveState.Jump:
                animator.SetBool(IS_JUMPING_BOOL, true);
                animator.SetBool(IS_RUNNING_BOOL, false);
                animator.SetBool(IS_FALLING_BOOL, false);
                break;
                
            case MoveState.Fall:
                animator.SetBool(IS_FALLING_BOOL, true);
                animator.SetBool(IS_RUNNING_BOOL, false);
                animator.SetBool(IS_JUMPING_BOOL, false);
                break;
                
            default:
                animator.SetBool(IS_RUNNING_BOOL, false);
                animator.SetBool(IS_JUMPING_BOOL, false);
                animator.SetBool(IS_FALLING_BOOL, false);
                break;
        }

        OnPlayerMoveStateChanged?.Invoke(newState);
        CurrentMoveState = newState;
    }
}