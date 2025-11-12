using UnityEngine;
using UnityEngine.UI;

namespace MercsOfMayhem.Enemies
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private Enemy enemy;
        [SerializeField] private Image fillImage;
        [SerializeField] private GameObject healthBarCanvas;
        
        [Header("Configurações")]
        [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0); // Offset acima do inimigo
        [SerializeField] private bool hideWhenFull = true; // Esconde quando está com vida cheia
        [SerializeField] private float hideDelay = 2f; // Tempo para esconder após ficar cheia
        
        private int maxHealth;
        private int currentHealth;
        private float hideTimer;
        
        private void Start()
        {
            if (enemy == null)
                enemy = GetComponentInParent<Enemy>();
                
            if (enemy != null)
            {
                maxHealth = enemy.GetMaxHealth();
                currentHealth = maxHealth;
            }
            
            if (hideWhenFull)
            {
                healthBarCanvas?.SetActive(false);
            }
            
            UpdateHealthBar();
        }
        
        private void Update()
        {
            // Mantém a barra sempre acima do inimigo
            if (enemy != null)
            {
                transform.position = enemy.transform.position + offset;
            }
            
            // Atualiza a barra
            if (enemy != null)
            {
                int newHealth = enemy.GetCurrentHealth();
                if (newHealth != currentHealth)
                {
                    currentHealth = newHealth;
                    UpdateHealthBar();
                    
                    // Mostra a barra quando toma dano
                    if (hideWhenFull && healthBarCanvas != null)
                    {
                        healthBarCanvas.SetActive(true);
                        hideTimer = hideDelay;
                    }
                }
            }
            
            // Timer para esconder a barra quando estiver cheia
            if (hideWhenFull && healthBarCanvas != null && healthBarCanvas.activeSelf)
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
        
        private void UpdateHealthBar()
        {
            if (fillImage != null && maxHealth > 0)
            {
                float fillAmount = (float)currentHealth / maxHealth;
                fillImage.fillAmount = fillAmount;
                
                // Muda a cor baseado na vida (verde -> amarelo -> vermelho)
                if (fillAmount > 0.6f)
                    fillImage.color = Color.green;
                else if (fillAmount > 0.3f)
                    fillImage.color = Color.yellow;
                else
                    fillImage.color = Color.red;
            }
        }
    }
}

