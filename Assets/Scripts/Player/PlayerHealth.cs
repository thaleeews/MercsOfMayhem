using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private int health = 10;
    public int currentHealth {get; private set;}
    public int maxHealth {get; private set;}

    public static Action<int> OnPlayerHealthChanged;
    public static Action OnPlayerDie;

    private const string flashRedAnim = "FlashRed";
    
    void Awake()
    {
        currentHealth = health;
        maxHealth = health;
    }

    public void DamagePlayer(int damage)
    {
        currentHealth -= damage;
        OnPlayerHealthChanged?.Invoke(currentHealth);
        animator.SetTrigger(flashRedAnim);
        if(currentHealth <= 0)
        {
            OnPlayerDie?.Invoke();
            Destroy(gameObject);
        }
    }

    private void RestoreHealth(int healthRecovered)
    {
        currentHealth += healthRecovered;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnPlayerHealthChanged?.Invoke(currentHealth);
    }
}
