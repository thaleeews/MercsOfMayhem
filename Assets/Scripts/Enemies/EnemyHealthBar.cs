using UnityEngine;
using UnityEngine.UI;

namespace MercsOfMayhem.Enemies
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private Enemy enemy;
        [SerializeField] private Image fillImage;
        [SerializeField] private GameObject healthBarCanvas; // <-- Este ainda usamos para o timer
        
        // --- NOVO: Referência para os objetos visuais ---
        [SerializeField] private GameObject backgroundObject; // Arraste o 'Background' aqui
        [SerializeField] private GameObject fillObject;     // Arraste o 'Fill' aqui
        
        [Header("Configurações")]
        [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0); 
        [SerializeField] private bool hideWhenFull = true; 
        [SerializeField] private float hideDelay = 2f; 
        
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
            
            // --- MUDANÇA: Esconde os filhos, não o pai ---
            if (hideWhenFull)
            {
                // healthBarCanvas?.SetActive(false); // <-- Linha antiga
                SetVisualsActive(false); // <-- Nova função
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
                    
                    // --- MUDANÇA: Mostra os filhos ---
                    if (hideWhenFull)
                    {
                        // healthBarCanvas.SetActive(true); // <-- Linha antiga
                        SetVisualsActive(true); // <-- Nova função
                        hideTimer = hideDelay;
                    }
                }
            }
            
            // Timer para esconder a barra quando estiver cheia
            // (Verificamos 'fillObject.activeSelf' para saber se está visível)
            if (hideWhenFull && fillObject != null && fillObject.activeSelf)
            {
                if (currentHealth >= maxHealth)
                {
                    hideTimer -= Time.deltaTime;
                    if (hideTimer <= 0)
                    {
                        // healthBarCanvas.SetActive(false); // <-- Linha antiga
                        SetVisualsActive(false); // <-- Nova função
                    }
                }
            }
        }
        
        // --- NOVA FUNÇÃO ---
        // Liga ou desliga as partes visuais da barra
        private void SetVisualsActive(bool isActive)
        {
            if (backgroundObject != null)
                backgroundObject.SetActive(isActive);
                
            if (fillObject != null)
                fillObject.SetActive(isActive);
        }
        
        private void UpdateHealthBar()
        {
            // (Esta função continua igual)
            if (fillImage != null && maxHealth > 0)
            {
                float fillAmount = (float)currentHealth / maxHealth;
                fillImage.fillAmount = fillAmount;
                
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