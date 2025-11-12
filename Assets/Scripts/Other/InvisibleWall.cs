using UnityEngine;

/// <summary>
/// Parede invisível para prevenir o jogador de cair do mapa
/// Adicione este script a um GameObject vazio e configure o BoxCollider2D
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class InvisibleWall : MonoBehaviour
{
    [Header("Wall Settings")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);
    
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Garante que não é trigger (deve bloquear fisicamente)
        boxCollider.isTrigger = false;
        
        // Opcional: desabilita o SpriteRenderer se houver
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = gizmoColor;
            
            // Desenha o box collider
            Vector3 center = transform.position + (Vector3)col.offset;
            Vector3 size = col.size;
            size.x *= transform.lossyScale.x;
            size.y *= transform.lossyScale.y;
            
            Gizmos.DrawCube(center, size);
            
            // Desenha o contorno
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = Color.red;
            
            Vector3 center = transform.position + (Vector3)col.offset;
            Vector3 size = col.size;
            size.x *= transform.lossyScale.x;
            size.y *= transform.lossyScale.y;
            
            Gizmos.DrawWireCube(center, size);
        }
    }
}

