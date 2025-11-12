using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

/// <summary>
/// Script Editor para configurar automaticamente o Animator Controller do Player
/// Adiciona transições para combinar movimento com tiro (Run+Shoot, Jump+Shoot, etc.)
/// </summary>
public class AnimatorSetup : EditorWindow
{
    // Estrutura auxiliar para condições
    private struct ConditionData
    {
        public AnimatorConditionMode mode;
        public string parameter;
        public float threshold;
    }
    [MenuItem("Tools/Setup Player Animator")]
    public static void SetupPlayerAnimator()
    {
        // Carrega o Animator Controller
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
            "Assets/Animations/Player/PlayerAnim.controller");

        if (controller == null)
        {
            Debug.LogError("Não foi possível encontrar o Animator Controller em Assets/Animations/Player/PlayerAnim.controller");
            return;
        }

        // Pega a primeira layer (Base Layer)
        AnimatorControllerLayer layer = controller.layers[0];
        AnimatorStateMachine stateMachine = layer.stateMachine;

        // Encontra os estados existentes
        AnimatorState runState = FindState(stateMachine, "Player_Run");
        AnimatorState jumpState = FindState(stateMachine, "Player_Jump");
        AnimatorState fallState = FindState(stateMachine, "Player_Fall");
        AnimatorState idleState = FindState(stateMachine, "Player_Idle");
        
        // Encontra ou cria estados de tiro
        AnimatorState shootNormalState = FindOrCreateState(stateMachine, "Player_ShootNormal", "Shoot");
        AnimatorState shootUpState = FindOrCreateState(stateMachine, "Player_ShootUp", "ShootUp");
        AnimatorState shootDownState = FindOrCreateState(stateMachine, "Player_ShootDown", "ShootDown");
        AnimatorState jumpShootNormalState = FindOrCreateState(stateMachine, "Player_JumpShootNormal", "JumpShootNormal");
        AnimatorState jumpShootUpState = FindOrCreateState(stateMachine, "Player_JumpShootUp", "JumpShootUp");
        AnimatorState jumpShootDownState = FindOrCreateState(stateMachine, "Player_JumpShootDown", "JumpShootDown");

        // Remove transições antigas que podem estar conflitando
        RemoveTransitionsFromState(runState);
        RemoveTransitionsFromState(jumpState);
        RemoveTransitionsFromState(fallState);
        RemoveTransitionsFromState(idleState);

        // Adiciona transições de Run para Shoot (quando está correndo E atirando)
        if (runState != null)
        {
            // Run -> ShootNormal (quando IsRunning=true E ShootNormal=true)
            AddTransition(runState, shootNormalState, 
                new ConditionData[] {
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "IsRunning", threshold = 0 },
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "ShootNormal", threshold = 0 }
                });

            // Run -> ShootUp
            AddTransition(runState, shootUpState,
                new ConditionData[] {
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "IsRunning", threshold = 0 },
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "ShootUp", threshold = 0 }
                });

            // Run -> ShootDown
            AddTransition(runState, shootDownState,
                new ConditionData[] {
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "IsRunning", threshold = 0 },
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "ShootDown", threshold = 0 }
                });

            // Volta para Run quando para de atirar
            AddTransition(shootNormalState, runState,
                new ConditionData[] {
                    new ConditionData { mode = AnimatorConditionMode.IfNot, parameter = "ShootNormal", threshold = 0 },
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "IsRunning", threshold = 0 }
                });
        }

        // Adiciona transições de Jump/Fall para JumpShoot
        if (jumpState != null)
        {
            // Jump -> JumpShootNormal
            AddTransition(jumpState, jumpShootNormalState,
                new ConditionData[] {
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "IsJumping", threshold = 0 },
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "JumpShootNormal", threshold = 0 }
                });

            // Jump -> JumpShootUp
            AddTransition(jumpState, jumpShootUpState,
                new ConditionData[] {
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "IsJumping", threshold = 0 },
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "JumpShootUp", threshold = 0 }
                });

            // Jump -> JumpShootDown
            AddTransition(jumpState, jumpShootDownState,
                new ConditionData[] {
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "IsJumping", threshold = 0 },
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "JumpShootDown", threshold = 0 }
                });
        }

        if (fallState != null)
        {
            // Fall -> JumpShootNormal
            AddTransition(fallState, jumpShootNormalState,
                new ConditionData[] {
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "IsFalling", threshold = 0 },
                    new ConditionData { mode = AnimatorConditionMode.If, parameter = "JumpShootNormal", threshold = 0 }
                });
        }

        // Adiciona transições de volta quando para de atirar
        AddTransition(shootNormalState, idleState,
            new ConditionData[] {
                new ConditionData { mode = AnimatorConditionMode.IfNot, parameter = "ShootNormal", threshold = 0 },
                new ConditionData { mode = AnimatorConditionMode.IfNot, parameter = "IsRunning", threshold = 0 }
            });

        AddTransition(jumpShootNormalState, jumpState,
            new ConditionData[] {
                new ConditionData { mode = AnimatorConditionMode.IfNot, parameter = "JumpShootNormal", threshold = 0 },
                new ConditionData { mode = AnimatorConditionMode.If, parameter = "IsJumping", threshold = 0 }
            });

        AddTransition(jumpShootNormalState, fallState,
            new ConditionData[] {
                new ConditionData { mode = AnimatorConditionMode.IfNot, parameter = "JumpShootNormal", threshold = 0 },
                new ConditionData { mode = AnimatorConditionMode.If, parameter = "IsFalling", threshold = 0 }
            });

        // Salva as mudanças
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Animator Controller configurado com sucesso! Transições de Run+Shoot e Jump+Shoot adicionadas.");
    }

    private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
    {
        foreach (var state in stateMachine.states)
        {
            if (state.state.name == stateName)
            {
                return state.state;
            }
        }
        return null;
    }

    private static AnimatorState FindOrCreateState(AnimatorStateMachine stateMachine, string stateName, string clipName)
    {
        AnimatorState state = FindState(stateMachine, stateName);
        if (state == null)
        {
            // Tenta encontrar a animação
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                $"Assets/Animations/Player/{clipName}.anim");

            state = stateMachine.AddState(stateName);
            if (clip != null)
            {
                state.motion = clip;
            }
        }
        return state;
    }

    private static void RemoveTransitionsFromState(AnimatorState state)
    {
        if (state == null) return;

        // Remove todas as transições que vão para estados de tiro
        var transitionsToRemove = new System.Collections.Generic.List<AnimatorStateTransition>();
        foreach (var transition in state.transitions)
        {
            if (transition.destinationState != null &&
                (transition.destinationState.name.Contains("Shoot") || 
                 transition.destinationState.name.Contains("JumpShoot")))
            {
                transitionsToRemove.Add(transition);
            }
        }

        foreach (var transition in transitionsToRemove)
        {
            state.RemoveTransition(transition);
        }
    }

    private static void AddTransition(AnimatorState from, AnimatorState to, ConditionData[] conditions)
    {
        if (from == null || to == null) return;

        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0;
        transition.offset = 0;
        transition.interruptionSource = TransitionInterruptionSource.None;

        foreach (var condition in conditions)
        {
            // AddCondition usa: mode, threshold (float), parameterName (string)
            transition.AddCondition(condition.mode, condition.threshold, condition.parameter);
        }
    }
}

