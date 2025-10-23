using UnityEngine;
using System;

public class PlayerMovementState : MonoBehaviour
{
    public enum MoveState { Idle, Run, Jump, Fall }

    public MoveState CurrentMoveState { get; private set; }

    [SerializeField] private Animator animator;

    private const string idleAnim = "Idle";
    private const string runAnim = "Run";
    private const string jumpAnim = "Jump";
    private const string fallAnim = "Fall";

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

        OnPlayerMoveStateChanged?.Invoke(newState);
        CurrentMoveState = newState;
    }
}
