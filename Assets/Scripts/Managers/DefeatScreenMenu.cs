using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Menu de Derrota - Gerencia botões da tela de Game Over
/// </summary>
public class DefeatScreenMenu : MonoBehaviour
{
    private void Awake()
    {
        // Garante que o tempo está rodando normalmente ANTES de tudo
        Time.timeScale = 1f;
        
        // Garante que há um EventSystem na cena (necessário para botões funcionarem)
        if (EventSystem.current == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✅ EventSystem criado automaticamente na cena de Derrota");
        }
    }

    /// <summary>
    /// Garante que existe um DefeatScreenMenu na cena. Cria um se não existir.
    /// </summary>
    public static void EnsureInstance()
    {
        // Verifica se já existe um DefeatScreenMenu na cena
        DefeatScreenMenu existing = FindFirstObjectByType<DefeatScreenMenu>();
        if (existing == null)
        {
            // Cria um novo GameObject com o DefeatScreenMenu
            GameObject go = new GameObject("DefeatScreenMenu");
            go.AddComponent<DefeatScreenMenu>();
            Debug.Log("✅ DefeatScreenMenu criado automaticamente na cena de Derrota");
        }
    }

    private void Start()
    {
        // Garante que o tempo está rodando normalmente
        Time.timeScale = 1f;
        
        // Garante que o GameManager existe
        GameManager.EnsureInstance();
        
        // Desativa a música do menu se estiver tocando
        var menuMusic = FindFirstObjectByType<MenuMusicPersist>();
        if (menuMusic != null)
        {
            menuMusic.gameObject.SetActive(false);
        }
        
        Debug.Log($"💀 Tela de Derrota carregada! GameManager: {(GameManager.Instance != null ? "OK" : "NULL")}, Cena salva: '{(GameManager.Instance != null ? GameManager.Instance.sceneToReload : "N/A")}'");
        
        // Conecta os botões automaticamente
        ConnectButtonsToThis();
    }

    private void ConnectButtonsToThis()
    {
        UnityEngine.UI.Button[] buttons = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);
        Debug.Log($"🔍 Encontrados {buttons.Length} botões na cena");
        
        foreach (var button in buttons)
        {
            // Remove todos os listeners antigos e adiciona os novos
            button.onClick.RemoveAllListeners();
            
            // Identifica o botão pelo nome ou texto
            string buttonName = button.name.ToLower();
            UnityEngine.UI.Text buttonText = button.GetComponentInChildren<UnityEngine.UI.Text>();
            string textContent = buttonText != null ? buttonText.text.ToLower() : "";
            
            Debug.Log($"🔍 Botão: '{button.name}', Texto: '{textContent}'");
            
            // Conecta baseado no nome ou texto
            if (buttonName.Contains("retry") || buttonName.Contains("tentar") || buttonName.Contains("novamente") || 
                textContent.Contains("tentar") || textContent.Contains("novamente") || textContent.Contains("jogar"))
            {
                button.onClick.AddListener(RetryLevel);
                Debug.Log($"✅ '{button.name}' -> RetryLevel()");
            }
            else if (buttonName.Contains("menu") || buttonName.Contains("main") || 
                     textContent.Contains("menu") || textContent.Contains("principal"))
            {
                button.onClick.AddListener(GoToMainMenu);
                Debug.Log($"✅ '{button.name}' -> GoToMainMenu()");
            }
            else if (buttonName.Contains("quit") || buttonName.Contains("sair") || 
                     textContent.Contains("sair") || textContent.Contains("quit"))
            {
                button.onClick.AddListener(QuitGame);
                Debug.Log($"✅ '{button.name}' -> QuitGame()");
            }
            else
            {
                // Se não conseguir identificar, conecta todos os métodos (fallback)
                Debug.LogWarning($"⚠️ Botão '{button.name}' não identificado! Conectando todos os métodos...");
                button.onClick.AddListener(RetryLevel);
                button.onClick.AddListener(GoToMainMenu);
                button.onClick.AddListener(QuitGame);
            }
        }
    }

    /// <summary>
    /// Reinicia a fase atual (usa a cena salva no GameManager)
    /// Conecte este método ao botão "Tentar Novamente" / "Retry"
    /// </summary>
    public void RetryLevel()
    {
        Debug.Log("🔄 RetryLevel() CHAMADO!");
        Time.timeScale = 1f; // Garante que o tempo está normal
        
        // Garante que o GameManager existe
        GameManager.EnsureInstance();
        
        // Verifica se existe GameManager com a cena salva
        if (GameManager.Instance != null)
        {
            string sceneToLoad = GameManager.Instance.sceneToReload;
            Debug.Log($"🔄 GameManager encontrado! Cena salva: '{sceneToLoad}'");
            
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.Log($"🔄 Carregando cena: '{sceneToLoad}'");
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("⚠️ Nenhuma cena salva! Carregando Fase 1...");
                SceneManager.LoadScene("Fase 1");
            }
        }
        else
        {
            // Fallback: tenta recarregar a Fase 1
            Debug.LogError("❌ GameManager não encontrado mesmo após EnsureInstance! Carregando Fase 1...");
            SceneManager.LoadScene("Fase 1");
        }
    }

    /// <summary>
    /// Volta ao menu principal
    /// Conecte este método ao botão "Menu Principal" / "Main Menu"
    /// </summary>
    public void GoToMainMenu()
    {
        Debug.Log("🏠 GoToMainMenu() CHAMADO!");
        Time.timeScale = 1f; // Garante que o tempo está normal
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Sai do jogo
    /// Conecte este método ao botão "Sair" / "Quit"
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("👋 QuitGame() CHAMADO!");
        Time.timeScale = 1f; // Garante que o tempo está normal
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
