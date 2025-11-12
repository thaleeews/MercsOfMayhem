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

    [Header("Enemy Stats")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int damageOnContact = 10;

    [SerializeField] private Vector2 knockbackToSelf = new Vector2(6f, 10f);
    [SerializeField] private Vector2 knockbackToPlayer = new Vector2(3f, 5f);
    [SerializeField] private float knockbackDelayToSelf = 1.5f;
    [SerializeField] private float deathAnimationTime = 1.2f;
    
    [Header("Hit Effect")]
    [SerializeField] private float flashDuration = 0.1f; // Duração de cada piscada
    [SerializeField] private int flashCount = 3; // Número de piscadas
    
    [Header("Death Effect")]
    [SerializeField] private float deathFadeDuration = 0.8f; // Duração do fade out ao morrer
    [SerializeField] private bool addDeathRotation = true; // Adiciona rotação na morte
    [SerializeField] private float deathRotationSpeed = 360f; // Velocidade de rotação na morte

    protected int currentHealth;
    // --- MUDANÇA: 'isDead' é substituído pelo 'currentState'
    // protected bool isDead = false; 

    private Animator anim;
    private Collider2D col; // O colisor do "corpo" (Capsule)
    private EnemyMovement movementScript; // As "pernas"
    private Rigidbody2D rb;
    private EnemyShoot enemyShoot;
    private SpriteRenderer spriteRenderer;
    
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
        spriteRenderer = GetComponent<SpriteRenderer>();

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
        
		// 2. DECIDIR A ANIMAÇÃO (por ângulo, evitando mirar 90° para cima por pequena diferença de altura)
		Vector2 toPlayer = playerTarget.position - transform.position;
		// Usamos Mathf.Abs no X para considerar apenas o ângulo relativo ao eixo horizontal
		float angleDeg = Mathf.Atan2(toPlayer.y, Mathf.Abs(toPlayer.x)) * Mathf.Rad2Deg;

		// Limiares configuráveis (valores seguros para evitar snap para 90°)
		const float upAimAngleThreshold = 60f;   // acima disso, animação para cima
		const float downAimAngleThreshold = -30f; // abaixo disso, animação para baixo

		bool aimUp = angleDeg >= upAimAngleThreshold;
		bool aimDown = angleDeg <= downAimAngleThreshold;

		anim.SetBool("ShootNormal", !aimUp && !aimDown);
		anim.SetBool("ShootUp", aimUp);
		anim.SetBool("ShootDown", aimDown);

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
        // Para qualquer efeito de flash que esteja rodando
        StopCoroutine(nameof(FlashWhite));

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        if (movementScript != null)
        {
            movementScript.enabled = false; // Desliga as "pernas"
        }

		// IMPORTANTE: Mantemos o collider ligado para permitir que o inimigo caia e colida com o chão
        // Mas desativamos o trigger para evitar colisões indesejadas
        if (col != null)
        {
            col.isTrigger = false; // Garante que não é trigger para colidir com o chão
        }
        
		// IMPORTANTE: Mantemos o Rigidbody dinâmico para que a gravidade faça o inimigo cair
        // Não zeramos a velocidade Y para permitir que caia naturalmente
        if (rb != null)
        {
            // Mantém a velocidade X para momentum, mas permite queda livre
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y);
			rb.bodyType = RigidbodyType2D.Dynamic;
			// Garantimos gravidade mínima para que caia
			rb.gravityScale = Mathf.Max(rb.gravityScale, 1f);
            // Desativa constraints para permitir rotação e movimento livre
            rb.constraints = RigidbodyConstraints2D.None;
        }

        // Inicia os efeitos visuais de morte
        StartCoroutine(nameof(DeathEffect));
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
        var playerMovement = FindFirstObjectByType<PlayerMovement>();
        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerMovement != null) playerMovement.KnockbackPlayer(knockbackToPlayer, direction);
        if (playerHealth != null) playerHealth.DamagePlayer(damageOnContact);
        
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
        
        Debug.Log($"🎯 {gameObject.name} tomou {damage} de dano! Vida: {currentHealth}/{maxHealth}");
        
        // Efeito visual de dano (piscar branco)
        if (spriteRenderer != null)
        {
            StopCoroutine(nameof(FlashWhite)); // Para o efeito anterior se ainda estiver rodando
            StartCoroutine(nameof(FlashWhite));
        }
        
        if (currentHealth <= 0)
        {
            Debug.Log($"💀 {gameObject.name} MORREU!");
            // --- MUDANÇA: Em vez de chamar Die(), trocamos o estado ---
            ChangeState(State.Dead);
        }
    }

    // Corrotina que faz o sprite piscar branco quando toma dano
    private IEnumerator FlashWhite()
    {
        Color originalColor = spriteRenderer.color;
        
        for (int i = 0; i < flashCount; i++)
        {
            // Fica branco
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
            
            // Volta para a cor original
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
        
        // Garante que volta para a cor original no final
        spriteRenderer.color = originalColor;
    }
    
    // Corrotina que faz o efeito visual de morte (fade out + rotação)
    private IEnumerator DeathEffect()
    {
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;
        Quaternion startRotation = transform.rotation;
        
        // Espera um pouco antes de começar o fade (para ver a animação de morte)
        float delayBeforeFade = Mathf.Max(0, deathAnimationTime - deathFadeDuration);
        if (delayBeforeFade > 0)
        {
            yield return new WaitForSeconds(delayBeforeFade);
        }
        
        // Fade out gradual + rotação
        while (elapsed < deathFadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / deathFadeDuration;
            
            // Fade out (alpha vai de 1 para 0)
            if (spriteRenderer != null)
            {
                Color newColor = startColor;
                newColor.a = Mathf.Lerp(1f, 0f, progress);
                spriteRenderer.color = newColor;
            }
            
            // Rotação na morte (opcional)
            if (addDeathRotation && rb != null)
            {
                float rotationAmount = deathRotationSpeed * Time.deltaTime;
                transform.Rotate(0, 0, rotationAmount);
            }
            
            yield return null;
        }
        
        // Destrói o inimigo no final
        DestroyEnemyObject();
    }

    // --- MUDANÇA: Adicionamos a lógica de "Virar para o Player" ---
    

    private int GetDirection(Transform playerTransform)
        => transform.position.x > playerTransform.position.x ? -1 : 1;
    
    // Métodos públicos para a HealthBar acessar
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}