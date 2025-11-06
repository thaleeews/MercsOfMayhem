using UnityEngine;
using MercsOfMayhem.Enemies;
using System.Collections; // Necessário para o Invoke

public class Enemy : MonoBehaviour
{
    // --- NOVO: Estados da nossa IA ---
    public enum State
    {
        Patrolling, // Patrulhando (usando EnemyMovement)
        Attacking,  // Atacando (parado, atirando no player)
        Dead        // Morto (animação de morte)
    }

    [Header("IA (Inteligência Artificial)")]
    [SerializeField] private string playerTag = "Player"; // Qual tag seu Player usa?
    private State currentState;     // O estado atual
    private Transform playerTarget; // O "alvo" (o player)
    [SerializeField] private float verticalAimTolerance = 0.5f;

    [Header("Enemy Stats")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int damageOnContact = 10;

    [SerializeField] private Vector2 knockbackToSelf = new Vector2(6f, 10f);
    [SerializeField] private Vector2 knockbackToPlayer = new Vector2(3f, 5f);
    [SerializeField] private float knockbackDelayToSelf = 1.5f;
    [SerializeField] private float deathAnimationTime = 1.2f;

    protected int currentHealth;
    // --- MUDANÇA: 'isDead' é substituído pelo 'currentState'
    // protected bool isDead = false; 

    private Animator anim;
    private Collider2D col; // O colisor do "corpo" (Capsule)
    private EnemyMovement movementScript; // As "pernas"
    private Rigidbody2D rb;
    private EnemyShoot enemyShoot;
    
    // --- NOVO: Referência para o script de tiro que VAMOS CRIAR ---
    // private EnemyShoot enemyShoot; 

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>(); // Assume que este é o CapsuleCollider
        movementScript = GetComponent<EnemyMovement>();
        rb = GetComponent<Rigidbody2D>();
        enemyShoot = GetComponent<EnemyShoot>(); 

        // --- MUDANÇA: Começamos no estado de Patrulha ---
        ChangeState(State.Patrolling);
    }

    // --- NOVO: A Máquina de Estados principal ---
    private void Update()
    {
        // Se o inimigo não estiver morto, ele roda a lógica de IA
        if (currentState == State.Dead) return;
        
        // O que o inimigo faz a cada frame depende do seu estado
        switch (currentState)
        {
            case State.Patrolling:
                // O script EnemyMovement está no comando.
                // A única coisa que fazemos aqui é esperar o trigger da "visão".
                break;
                
            case State.Attacking:
                // Se estamos atacando, primeiro checamos se o player
                // saiu do nosso range (se o alvo for nulo)
                if (playerTarget == null)
                {
                    ChangeState(State.Patrolling); // Volte a patrulhar
                    return;
                }
                
                // Se o player ainda está aqui, executa a lógica de ataque
                HandleAttacking();
                break;
        }
    }

    // --- NOVO: O "Campo de Visão" (O Círculo Trigger) ---
    // (Para isso funcionar, adicione um CircleCollider2D no inimigo
    // com 'Is Trigger' marcado e um Raio grande)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Se já estamos mortos ou já estamos atacando, ignora
        if (currentState == State.Dead || currentState == State.Attacking) return;

        // O Player entrou no "range"?
        if (other.CompareTag(playerTag) && !other.isTrigger) // Ignora outros triggers
        {
            Debug.Log("Player ENTROU no range!");
            playerTarget = other.transform; // Salva quem é o player
            ChangeState(State.Attacking);   // Muda o estado para ATACANDO
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Se estamos mortos ou nem tínhamos um alvo, ignora
        if (currentState == State.Dead || playerTarget == null) return;
        
        // O Player saiu do "range"?
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player SAIU no range!");
            playerTarget = null;            // Esquece o player
            ChangeState(State.Patrolling);  // Muda o estado para PATRULHANDO
        }
    }

    // --- NOVO: Função que gerencia a troca de estados ---
    private void ChangeState(State newState)
    {
        if (currentState == newState) return; 
        
        currentState = newState;
        
        // Lógica de "setup" quando entramos em um novo estado
        switch (currentState)
        {
            case State.Patrolling:
                Debug.Log("Inimigo: Entrando no estado 'Patrolling'");
                movementScript.enabled = true;  
                movementScript.SetPatrol(true); 
                
                // --- Animação de Patrulha ---
                if (anim != null)
                {
                    anim.SetBool("IsRunning", true); 
                    anim.SetBool("ShootNormal", false);
                    anim.SetBool("ShootUp", false);
                    anim.SetBool("ShootDown", false);
                }
                break;
                
            case State.Attacking:
                Debug.Log("Inimigo: Entrando no estado 'Attacking'");
                movementScript.enabled = true;   
                movementScript.SetPatrol(false); 
                
                // --- Animação de Ataque ---
                // (O HandleAttacking vai decidir QUAL bool de tiro ligar)
                if (anim != null)
                {
                    anim.SetBool("IsRunning", false);
                }
                break;
                
            case State.Dead:
                Debug.Log("Inimigo: Entrando no estado 'Dead'");
                // Limpa todos os bools ao morrer
                if (anim != null)
                {
                    anim.SetBool("IsRunning", false); 
                    anim.SetBool("ShootNormal", false);
                    anim.SetBool("ShootUp", false);
                    anim.SetBool("ShootDown", false);
                }
                HandleDeathLogic();
                break;
        }
    }

    // --- NOVO: Lógica de ataque (o que fazemos no estado 'Attacking') ---
    private void HandleAttacking()
    {
        // 1. VIRAR PARA O PLAYER (Isso já funciona)
        int directionToPlayer = GetDirection(playerTarget);
        movementScript.ForceFaceDirection(directionToPlayer);
        
        // 2. DECIDIR A ANIMAÇÃO (Com base na altura do Player)
        float yDifference = playerTarget.position.y - transform.position.y;

        if (yDifference > verticalAimTolerance)
        {
            // Player está ACIMA
            anim.SetBool("ShootNormal", false);
            anim.SetBool("ShootUp", true);
            anim.SetBool("ShootDown", false);
        }
        else if (yDifference < -verticalAimTolerance)
        {
            // Player está ABAIXO
            anim.SetBool("ShootNormal", false);
            anim.SetBool("ShootUp", false);
            anim.SetBool("ShootDown", true);
        }
        else
        {
            // Player está NA MESMA ALTURA (em frente)
            anim.SetBool("ShootNormal", true);
            anim.SetBool("ShootUp", false);
            anim.SetBool("ShootDown", false);
        }

        // 3. ATIRAR (Isso já funciona)
        // O Cérebro dá a ordem para a Arma "Tentar Atirar"
        if (enemyShoot != null && playerTarget != null)
        {
            enemyShoot.TryToShoot(playerTarget.position);
        }
        
        // (O Debug.Log de teste continua o mesmo)
        if (Time.frameCount % 60 == 0) 
            Debug.Log("Estou ATACANDO (e virado para) o player: " + playerTarget.name);
    }

    // --- MUDANÇA: Renomeamos 'Die()' para 'HandleDeathLogic()' ---
    // Esta é a sua função Die() antiga, mas agora é privada.
    private void HandleDeathLogic()
    {
        // (isDead = true;) // Não precisamos mais, o estado é 'Dead'

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        if (movementScript != null)
        {
            movementScript.enabled = false; // Desliga as "pernas"
        }

        if (col != null)
        {
            col.enabled = false; // Desliga o collider principal
        }
        
        // Desliga o Rigidbody
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true; 
        }

        Invoke(nameof(DestroyEnemyObject), deathAnimationTime);
    }

    private void DestroyEnemyObject()
    {
        Destroy(gameObject);
    }

    public void HitPlayer(Transform playerTransform)
    {
        // --- MUDANÇA: Só damos dano de contato se estivermos patrulhando ---
        if (currentState != State.Patrolling) return;
        
        // (Seu código de HitPlayer continua o mesmo)
        int direction = GetDirection(playerTransform);
        FindObjectOfType<PlayerMovement>().KnockbackPlayer(knockbackToPlayer, direction);
        FindObjectOfType<PlayerHealth>().DamagePlayer(damageOnContact);
        
        if (movementScript != null)
        {
            movementScript.KnockbackEnemy(knockbackToSelf, -direction, knockbackDelayToSelf);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        // --- MUDANÇA: Usamos 'currentState' para checar ---
        if (currentState == State.Dead) return; // Já estamos mortos
        
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            // --- MUDANÇA: Em vez de chamar Die(), trocamos o estado ---
            ChangeState(State.Dead);
        }
    }

    

    // --- MUDANÇA: Adicionamos a lógica de "Virar para o Player" ---
    

    private int GetDirection(Transform playerTransform)
        => transform.position.x > playerTransform.position.x ? -1 : 1;
}