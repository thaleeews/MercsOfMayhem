using UnityEngine;
using System;

namespace MercsOfMayhem.Enemies
{
    public class EnemyMovementState : MonoBehaviour
    {
        public enum MoveState { Idle, Run }
        public MoveState CurrentMoveState { get; private set; }

        [Header("References")]
        [SerializeField] private Animator animator;

        private const string idleAnim = "Idle";
        private const string runAnim = "Run";

        public static Action<MoveState> OnEnemyMoveStateChanged;

        public void SetMoveState(MoveState moveState)
        {
            if (CurrentMoveState == moveState)
                return;

            switch (moveState)
            {
                case MoveState.Idle:
                    animator.Play(idleAnim);
                    break;
                case MoveState.Run:
                    animator.Play(runAnim);
                    break;
                default:
                    animator.Play(idleAnim);
                    break;
            }

            OnEnemyMoveStateChanged?.Invoke(moveState);
            CurrentMoveState = moveState;
        }
    }
}
