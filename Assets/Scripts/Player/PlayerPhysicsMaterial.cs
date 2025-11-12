using UnityEngine;

/// <summary>
/// Script para configurar o material físico do player e prevenir que ele fique travado em bordas
/// Adicione este script ao Player GameObject
/// </summary>
public class PlayerPhysicsMaterial : MonoBehaviour
{
    [Header("Physics Material Settings")]
    [SerializeField] private PhysicsMaterial2D playerMaterial;
    
    private Rigidbody2D rb;
    private Collider2D playerCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        
        ApplyPhysicsMaterial();
    }

    private void ApplyPhysicsMaterial()
    {
        // Se não foi configurado no Inspector, cria um material com zero friction
        if (playerMaterial == null)
        {
            playerMaterial = new PhysicsMaterial2D("PlayerMaterial");
            playerMaterial.friction = 0f;
            playerMaterial.bounciness = 0f;
            
            Debug.Log("PlayerPhysicsMaterial: Material criado automaticamente com zero friction");
        }
        
        // Aplica o material ao Collider2D
        if (playerCollider != null)
        {
            playerCollider.sharedMaterial = playerMaterial;
            Debug.Log($"PlayerPhysicsMaterial: Material aplicado ao collider. Friction: {playerMaterial.friction}");
        }
        
        // Configura o Rigidbody2D para Continuous collision detection
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Debug.Log("PlayerPhysicsMaterial: Collision detection ajustado para Continuous");
        }
    }

    private void OnValidate()
    {
        // Aplica as mudanças quando alterar valores no Inspector
        if (Application.isPlaying)
        {
            ApplyPhysicsMaterial();
        }
    }
}

