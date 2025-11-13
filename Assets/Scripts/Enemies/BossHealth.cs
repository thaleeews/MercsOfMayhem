using UnityEngine;
using System;
using System.Collections;
using MercsOfMayhem.Enemies;
using UnityEngine.SceneManagement; // <-- ADICIONE ESTA LINHA

/// <summary>
/// Script de vida do Boss. Gerencia HP, dano e morte.
/// </summary>
public class BossHealth : MonoBehaviour
{
    [Header("Vida do Boss")]
    [SerializeField] private int maxHealth = 500; // Vida máxima do boss
    [SerializeField] private float flashDuration = 0.1f; // Duração de cada piscada ao tomar dano
    [SerializeField] private int flashCount = 3; // Número de piscadas
    
    [Header("Efeitos de Morte")]
    [SerializeField] private float deathAnimationTime = 2f; // Tempo da animação de morte
    [SerializeField] private float deathFadeDuration = 1f; // Duração do fade out
    [SerializeField] private bool addDeathRotation = true; // Adiciona rotação na morte
    [SerializeField] private float deathRotationSpeed = 180f; // Velocidade de rotação
    [SerializeField] private float delayAfterDeath = 3f;
    
    [Header("Componentes")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    
    public static Action<int> OnBossHealthChanged;
    public static Action OnBossDie;
    
    private bool isDead = false;
    private const string dieTrigger = "Die"; // Nome do trigger de morte (se tiver animação)
    
    private void Awake()
    {
        CurrentHealth = maxHealth;
        
        // Busca componentes automaticamente se não foram configurados
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    /// <summary>
    /// Causa dano ao boss
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(0, CurrentHealth); // Garante que não fica negativo
        
        OnBossHealthChanged?.Invoke(CurrentHealth);
        
        Debug.Log($"🎯 Boss tomou {damage} de dano! Vida restante: {CurrentHealth}/{maxHealth}");
        
        // Efeito visual de dano (piscar branco)
        if (spriteRenderer != null)
        {
            StopCoroutine(nameof(FlashWhite));
            StartCoroutine(nameof(FlashWhite));
        }
        
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Mata o boss
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("💀 BOSS MORREU!");
        
        OnBossDie?.Invoke();
        
        // Executa animação de morte se tiver
        if (animator != null)
        {
            animator.SetTrigger(dieTrigger);
        }
        
        // Desabilita scripts do boss
        var bossShoot = GetComponent<BossShoot>();
        if (bossShoot != null) bossShoot.enabled = false;
        
        // Inicia efeito visual de morte
        StartCoroutine(nameof(DeathEffect));
    }
    
    /// <summary>
    /// Corrotina que faz o sprite piscar branco quando toma dano
    /// </summary>
    private IEnumerator FlashWhite()
    {
        if (spriteRenderer == null) yield break;
        
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
    
    /// <summary>
    /// Corrotina que faz o efeito visual de morte (fade out + rotação)
    /// </summary>
    private IEnumerator DeathEffect()
    {
        if (spriteRenderer == null) yield break;
        
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
            if (addDeathRotation)
            {
                float rotationAmount = deathRotationSpeed * Time.deltaTime;
                transform.Rotate(0, 0, rotationAmount);
            }
            
            yield return null;
        }
        
        // Desativa o boss (ou destrói, dependendo do que você quiser)
        gameObject.SetActive(false);
        SceneManager.LoadScene("Vitoria");
        // Ou: Destroy(gameObject);
    }
}

