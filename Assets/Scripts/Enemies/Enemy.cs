using UnityEngine;
using MercsOfMayhem.Enemies;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int damageOnContact = 10;

    [SerializeField] private Vector2 knockbackToSelf = new Vector2(6f, 10f);
    [SerializeField] private Vector2 knockbackToPlayer = new Vector2(3f, 5f);
    [SerializeField] private float knockbackDelayToSelf = 1.5f;

    // --- NOVO: Adicione um campo para o tempo da animação ---
    [SerializeField] private float deathAnimationTime = 1.2f; // Ajuste para a duração da sua animação "Die"

    protected int currentHealth;
    protected bool isDead = false;

    // --- NOVO: Referências para os componentes ---
    private Animator anim;
    private Collider2D col;
    private EnemyMovement movementScript;
    private Rigidbody2D rb;

    // --- NOVO: Método Start() para inicializar ---
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        
        // Pega os componentes e armazena nas variáveis
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        movementScript = GetComponent<EnemyMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    // --- MUDANÇA: Este método agora gerencia o processo de morte ---
    public void Die()
    {
        isDead = true; // Define como morto imediatamente

        // 1. Toca a animação de morte
        // (Certifique-se de que você tem um parâmetro Trigger no seu Animator chamado "Die")
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // 2. Desativa o movimento
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        // 3. Desativa o collider (para não ser atingido ou bater no player)
        if (col != null)
        {
            col.enabled = false;
        }

        // 4. (Opcional) Para o Rigidbody
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true; // Para de ser afetado pela física
        }

        // 5. Agenda a destruição do objeto para depois da animação
        Invoke(nameof(DestroyEnemyObject), deathAnimationTime);
    }

    // --- NOVO: Este método é chamado pelo Invoke() ---
    private void DestroyEnemyObject()
    {
        Destroy(gameObject);
    }

    public void HitPlayer(Transform playerTransform)
    {
        // --- MUDANÇA: Não faz nada se já estiver morto ---
        if (isDead) return;

        int direction = GetDirection(playerTransform);
        FindObjectOfType<PlayerMovement>().KnockbackPlayer(knockbackToPlayer, direction);
        FindObjectOfType<PlayerHealth>().DamagePlayer(damageOnContact);
        
        // Usa a referência 'movementScript' que pegamos no Start()
        if (movementScript != null)
        {
            movementScript.KnockbackEnemy(knockbackToSelf, -direction, knockbackDelayToSelf);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            // Chama o novo método Die() que cuida da animação
            Die(); 
        }
    }

    private int GetDirection(Transform playerTransform)
        => transform.position.x > playerTransform.position.x ? -1 : 1;
}