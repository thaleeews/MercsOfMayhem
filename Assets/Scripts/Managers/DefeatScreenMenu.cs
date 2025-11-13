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

    private void Start()
    {
        // Garante que o tempo está rodando normalmente
        Time.timeScale = 1f;
        
        // Desativa a música do menu se estiver tocando
        var menuMusic = FindFirstObjectByType<MenuMusicPersist>();
        if (menuMusic != null)
        {
            menuMusic.gameObject.SetActive(false);
        }
        
        Debug.Log("💀 Tela de Derrota carregada!");
    }

    /// <summary>
    /// Reinicia a fase atual (usa a cena salva no GameManager)
    /// Conecte este método ao botão "Tentar Novamente" / "Retry"
    /// </summary>
    public void RetryLevel()
    {
        Time.timeScale = 1f; // Garante que o tempo está normal
        Debug.Log("🔄 Tentando novamente...");
        
        // Verifica se existe GameManager com a cena salva
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReloadLastScene();
        }
        else
        {
            // Fallback: tenta recarregar a Fase 1
            Debug.LogWarning("GameManager não encontrado! Carregando Fase 1...");
            SceneManager.LoadScene("Fase 1");
        }
    }

    /// <summary>
    /// Volta ao menu principal
    /// Conecte este método ao botão "Menu Principal" / "Main Menu"
    /// </summary>
    public void GoToMainMenu()
    {
        Debug.Log("🏠 Voltando ao menu principal...");
        Time.timeScale = 1f; // Garante que o tempo está normal
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Sai do jogo
    /// Conecte este método ao botão "Sair" / "Quit"
    /// </summary>
    public void QuitGame()
    {
        Time.timeScale = 1f; // Garante que o tempo está normal
        Debug.Log("👋 Saindo do jogo...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
