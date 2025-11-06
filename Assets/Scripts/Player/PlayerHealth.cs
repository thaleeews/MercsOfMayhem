using UnityEngine;
using System;
using UnityEngine.SceneManagement; 

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private int health = 100; 
    [SerializeField] private float deathAnimationTime = 1.5f; 

    public int currentHealth {get; private set;}
    public int maxHealth {get; private set;}

    public static Action<int> OnPlayerHealthChanged;
    public static Action OnPlayerDie;

    private const string flashRedAnim = "FlashRed";
    private const string dieTrigger = "Die"; 
    
    private bool isDead = false;

    void Awake()
    {
        currentHealth = health;
        maxHealth = health;
    }

    public void DamagePlayer(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        OnPlayerHealthChanged?.Invoke(currentHealth);
        
        Debug.Log($"Player tomou {damage} de dano. Vida restante: {currentHealth}");

        animator.SetTrigger(flashRedAnim);

        if(currentHealth <= 0)
        {
            isDead = true; 
            OnPlayerDie?.Invoke();
            
            animator.SetTrigger(dieTrigger);

            GetComponent<PlayerMovement>().enabled = false; 
            GetComponent<PlayerShoot>().enabled = false; 
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Invoke(nameof(RestartLevel), deathAnimationTime);
        }
    }

    private void RestartLevel()
    {
        // Pega o nome da cena atual (ex: "Fase2") e a carrega novamente
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    private void RestoreHealth(int healthRecovered)
    {
        // Possivel func para recuperar vida
    }
}