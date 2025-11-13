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
            Debug.Log($"caiu aqui");

            Invoke(nameof(GoToDefeatScreen), deathAnimationTime);
        }
    }

    private void RestoreHealth(int healthRecovered)
    {
    }


    private void GoToDefeatScreen()
    {
        // Garante que o tempo está normal antes de carregar a cena
        Time.timeScale = 1f;
        
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.sceneToReload = currentSceneName;
        }
        SceneManager.LoadScene("Derrota");
    }
}