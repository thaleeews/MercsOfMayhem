using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD persistente do jogador que exibe a vida e persiste entre cenas.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance { get; private set; }

    [Header("Referências da Barra de Vida")]
    [SerializeField] private Image healthFillImage; // Imagem que será preenchida
    [SerializeField] private RectTransform healthFillRectTransform; // RectTransform do Fill
    [SerializeField] private GameObject healthBarPanel; // Painel da barra de vida (para esconder/mostrar)
    [SerializeField] private Canvas canvas; // Canvas da HUD
    
    [Header("Configurações da Barra")]
    [SerializeField] private float barWidth = 150f; // Largura total da barra (reduzido)
    [SerializeField] private float barHeight = 15f; // Altura da barra (reduzido)
    
    [Header("Posição")]
    [SerializeField] private Vector2 healthBarPosition = new Vector2(10, -10); // Posição no canto superior esquerdo (em pixels)
    
    private int maxHealth = 100;
    private int currentHealth = 100;
    
    private void Awake()
    {
        // Singleton pattern - garante que só existe uma HUD
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Busca o Canvas se não foi configurado
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }
        
        // Busca o RectTransform do Fill se não foi configurado
        if (healthFillRectTransform == null && healthFillImage != null)
        {
            healthFillRectTransform = healthFillImage.GetComponent<RectTransform>();
        }
    }
    
    private void OnEnable()
    {
        // Se inscreve no evento de mudança de vida do jogador
        PlayerHealth.OnPlayerHealthChanged += UpdateHealth;
    }
    
    private void OnDisable()
    {
        // Remove a inscrição quando desabilitado
        PlayerHealth.OnPlayerHealthChanged -= UpdateHealth;
    }
    
    private void Start()
    {
        // Garante que o Canvas está ativo
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);
        }
        
        // Configura o Canvas para usar a câmera da Cinemachine
        SetupCanvasCamera();
        
        // Tenta encontrar o PlayerHealth na cena atual para inicializar
        FindAndUpdatePlayerHealth();
        
        // Configura a posição inicial da barra
        SetupHealthBarPosition();
        
        // Debug: verifica se tudo está configurado
        Debug.Log($"PlayerHUD Start - Canvas: {(canvas != null ? canvas.name : "NULL")}, " +
                  $"HealthBarPanel: {(healthBarPanel != null ? healthBarPanel.name : "NULL")}, " +
                  $"FillImage: {(healthFillImage != null ? healthFillImage.name : "NULL")}");
    }
    
    /// <summary>
    /// Configura o Canvas para usar a câmera da Cinemachine
    /// </summary>
    private void SetupCanvasCamera()
    {
        if (canvas == null) return;
        
        Camera targetCamera = null;
        
        // Tenta encontrar a câmera da Cinemachine usando reflection (sem depender do namespace)
        // Busca por CinemachineVirtualCamera usando o nome do tipo
        System.Type cinemachineType = System.Type.GetType("Cinemachine.CinemachineVirtualCamera, Cinemachine");
        if (cinemachineType != null)
        {
            UnityEngine.Object[] virtualCameras = FindObjectsByType(cinemachineType, FindObjectsSortMode.None);
            if (virtualCameras != null && virtualCameras.Length > 0)
            {
                // Tenta acessar a propriedade VirtualCameraGameObject via reflection
                System.Reflection.PropertyInfo prop = cinemachineType.GetProperty("VirtualCameraGameObject");
                if (prop != null)
                {
                    GameObject vcamObj = prop.GetValue(virtualCameras[0]) as GameObject;
                    if (vcamObj != null)
                    {
                        targetCamera = vcamObj.GetComponent<Camera>();
                    }
                }
                
                // Se não conseguir, tenta pegar a câmera do primeiro objeto encontrado
                if (targetCamera == null && virtualCameras[0] != null)
                {
                    targetCamera = (virtualCameras[0] as MonoBehaviour)?.GetComponent<Camera>();
                }
            }
        }
        
        // Fallback: usa a Main Camera
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        
        // Configura o Canvas
        if (targetCamera != null)
        {
            canvas.worldCamera = targetCamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = 1f; // Distância do plano da câmera
            Debug.Log($"PlayerHUD: Canvas configurado para usar a câmera: {targetCamera.name}, RenderMode: ScreenSpaceCamera");
        }
        else
        {
            // Se não encontrar câmera, usa Overlay (mais confiável para UI)
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Debug.LogWarning("PlayerHUD: Nenhuma câmera encontrada, usando ScreenSpaceOverlay");
        }
        
        // Garante que o Canvas está renderizando
        canvas.enabled = true;
        Debug.Log($"PlayerHUD: Canvas ativo: {canvas.enabled}, RenderMode: {canvas.renderMode}");
        
        // Configura o Canvas Scaler para não escalar muito
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            // Usa Scale With Screen Size com valores que não escalam demais
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); // Resolução de referência alta
            scaler.matchWidthOrHeight = 0f; // Prioriza largura (0 = largura, 1 = altura, 0.5 = ambos)
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            
            Debug.Log($"Canvas Scaler configurado - ReferenceResolution: {scaler.referenceResolution}, Match: {scaler.matchWidthOrHeight}");
        }
        else
        {
            // Se não tiver Canvas Scaler, adiciona um
            scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0f;
            Debug.Log("Canvas Scaler adicionado automaticamente");
        }
    }
    
    /// <summary>
    /// Configura a posição e tamanho da barra de vida
    /// </summary>
    private void SetupHealthBarPosition()
    {
        if (healthBarPanel != null)
        {
            // Garante que o painel está ativo
            healthBarPanel.SetActive(true);
            
            RectTransform panelRect = healthBarPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // Configura os anchors para o canto superior esquerdo
                panelRect.anchorMin = new Vector2(0, 1); // Top-Left
                panelRect.anchorMax = new Vector2(0, 1); // Top-Left
                panelRect.pivot = new Vector2(0, 1); // Pivot no canto superior esquerdo
                
                // Define a posição (offset do canto superior esquerdo)
                panelRect.anchoredPosition = healthBarPosition;
                
                // Define o tamanho do painel (menor)
                panelRect.sizeDelta = new Vector2(barWidth + 10, barHeight + 5); // Largura + padding menor, altura + padding menor
                
                Debug.Log($"HealthBarPanel configurado - Posição: {panelRect.anchoredPosition}, Tamanho: {panelRect.sizeDelta}");
            }
            else
            {
                Debug.LogError("PlayerHUD: HealthBarPanel não tem RectTransform!");
            }
        }
        else
        {
            Debug.LogError("PlayerHUD: HealthBarPanel é NULL! Configure no Inspector.");
        }
        
        // Configura o tamanho do Fill
        if (healthFillRectTransform != null)
        {
            healthFillRectTransform.sizeDelta = new Vector2(barWidth, barHeight);
            Debug.Log($"HealthFill configurado - Tamanho: {healthFillRectTransform.sizeDelta}");
        }
        else if (healthFillImage != null)
        {
            Debug.LogWarning("PlayerHUD: HealthFillRectTransform é NULL, tentando buscar do Image...");
            healthFillRectTransform = healthFillImage.GetComponent<RectTransform>();
            if (healthFillRectTransform != null)
            {
                healthFillRectTransform.sizeDelta = new Vector2(barWidth, barHeight);
            }
        }
    }
    
    /// <summary>
    /// Busca o PlayerHealth na cena e atualiza a HUD
    /// </summary>
    private void FindAndUpdatePlayerHealth()
    {
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            maxHealth = playerHealth.maxHealth;
            currentHealth = playerHealth.currentHealth;
            UpdateHealthBar();
        }
    }
    
    /// <summary>
    /// Atualiza a vida quando o evento é disparado
    /// </summary>
    private void UpdateHealth(int newHealth)
    {
        currentHealth = newHealth;
        UpdateHealthBar();
    }
    
    /// <summary>
    /// Atualiza a visualização da barra de vida
    /// </summary>
    private void UpdateHealthBar()
    {
        if (maxHealth > 0)
        {
            float fillAmount = (float)currentHealth / maxHealth;
            
            // Atualiza a cor baseado na vida (verde -> amarelo -> vermelho)
            if (healthFillImage != null)
            {
                if (fillAmount > 0.6f)
                    healthFillImage.color = Color.green;
                else if (fillAmount > 0.3f)
                    healthFillImage.color = Color.yellow;
                else
                    healthFillImage.color = Color.red;
            }
            
            // Controla o tamanho via RectTransform
            if (healthFillRectTransform != null)
            {
                float currentWidth = barWidth * fillAmount;
                healthFillRectTransform.sizeDelta = new Vector2(currentWidth, barHeight);
            }
        }
    }
    
    /// <summary>
    /// Método público para atualizar a vida máxima (útil quando o jogador é criado)
    /// </summary>
    public void SetMaxHealth(int max)
    {
        maxHealth = max;
        UpdateHealthBar();
    }
    
    /// <summary>
    /// Método público para atualizar a vida atual
    /// </summary>
    public void SetCurrentHealth(int current)
    {
        currentHealth = current;
        UpdateHealthBar();
    }
}

