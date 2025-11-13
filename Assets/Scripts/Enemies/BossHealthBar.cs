using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida do Boss. Exibida na posição do GameObject BossLife.
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private Transform bossLifePosition; // GameObject BossLife (posição da barra)
    [SerializeField] private Image fillImage; // Imagem que será preenchida
    [SerializeField] private RectTransform fillRectTransform; // RectTransform do Fill (para controlar tamanho)
    [SerializeField] private GameObject healthBarCanvas; // Canvas da barra de vida
    
    [Header("Tamanho da Barra")]
    [SerializeField] private float barWidth = 200f; // Largura total da barra
    [SerializeField] private float barHeight = 20f; // Altura da barra
    
    [Header("Configurações")]
    [SerializeField] private bool alwaysVisible = true; // Se false, esconde quando está cheia
    [SerializeField] private float hideDelay = 2f; // Tempo para esconder após ficar cheia (se alwaysVisible = false)
    
    private int maxHealth;
    private int currentHealth;
    private float hideTimer;
    
    private void Start()
    {
        // Busca o BossHealth automaticamente se não foi configurado
        if (bossHealth == null)
        {
            bossHealth = FindFirstObjectByType<BossHealth>();
            if (bossHealth == null)
            {
                Debug.LogError("BossHealthBar: BossHealth não encontrado! Adicione o componente BossHealth ao Boss.");
                return;
            }
        }
        
        // Busca o BossLife automaticamente se não foi configurado
        if (bossLifePosition == null)
        {
            // Tenta encontrar na hierarquia do boss
            Transform found = bossHealth.transform.Find("BossLife");
            if (found != null)
            {
                bossLifePosition = found;
            }
            else
            {
                Debug.LogError("BossHealthBar: GameObject 'BossLife' não encontrado na hierarquia do Boss!");
                return;
            }
        }
        
        // Busca o RectTransform do Fill se não foi configurado
        if (fillRectTransform == null && fillImage != null)
        {
            fillRectTransform = fillImage.GetComponent<RectTransform>();
        }
        
        // Inicializa valores
        if (bossHealth != null)
        {
            maxHealth = bossHealth.MaxHealth;
            currentHealth = maxHealth;
        }
        
        // Esconde a barra se não deve estar sempre visível
        if (!alwaysVisible && healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }
        
        UpdateHealthBar();
    }
    
    private void Update()
    {
        // Mantém a barra na posição do BossLife
        if (bossLifePosition != null)
        {
            transform.position = bossLifePosition.position;
        }
        
        // Atualiza a barra quando a vida muda
        if (bossHealth != null)
        {
            int newHealth = bossHealth.CurrentHealth;
            if (newHealth != currentHealth)
            {
                currentHealth = newHealth;
                UpdateHealthBar();
                
                // Mostra a barra quando toma dano (se não estiver sempre visível)
                if (!alwaysVisible && healthBarCanvas != null)
                {
                    healthBarCanvas.SetActive(true);
                    hideTimer = hideDelay;
                }
            }
        }
        
        // Timer para esconder a barra quando estiver cheia (se não estiver sempre visível)
        if (!alwaysVisible && healthBarCanvas != null && healthBarCanvas.activeSelf)
        {
            if (currentHealth >= maxHealth)
            {
                hideTimer -= Time.deltaTime;
                if (hideTimer <= 0)
                {
                    healthBarCanvas.SetActive(false);
                }
            }
        }
    }
    
    /// <summary>
    /// Atualiza a visualização da barra de vida
    /// </summary>
    private void UpdateHealthBar()
    {
        if (maxHealth > 0)
        {
            float fillAmount = (float)currentHealth / maxHealth;
            
            // Método 1: Usa fillAmount se o Image Type for Filled
            if (fillImage != null)
            {
                // Tenta usar fillAmount (funciona se Image Type = Filled)
                try
                {
                    fillImage.fillAmount = fillAmount;
                }
                catch
                {
                    // Se não funcionar, usa o método alternativo abaixo
                }
                
                // Muda a cor baseado na vida (verde -> amarelo -> vermelho)
                if (fillAmount > 0.6f)
                    fillImage.color = Color.green;
                else if (fillAmount > 0.3f)
                    fillImage.color = Color.yellow;
                else
                    fillImage.color = Color.red;
            }
            
            // Método 2: Controla o tamanho via RectTransform (funciona sempre)
            if (fillRectTransform != null)
            {
                // Calcula a largura baseada na vida
                float currentWidth = barWidth * fillAmount;
                
                // Atualiza o tamanho do RectTransform
                fillRectTransform.sizeDelta = new Vector2(currentWidth, barHeight);
            }
        }
    }
}

