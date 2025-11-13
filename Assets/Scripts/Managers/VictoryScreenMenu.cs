using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu de Vitória - Gerencia botões da tela de vitória
/// </summary>
public class VictoryScreenMenu : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private string nextLevelScene = ""; // Nome da próxima fase (se houver)
    [SerializeField] private bool hasNextLevel = false; // Se tem próxima fase

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
        
        Debug.Log("🎉 Tela de Vitória carregada!");
    }

    /// <summary>
    /// Avança para a próxima fase
    /// Conecte este método ao botão "Próxima Fase" / "Next Level"
    /// </summary>
    public void NextLevel()
    {
        if (hasNextLevel && !string.IsNullOrEmpty(nextLevelScene))
        {
            Debug.Log($"➡️ Indo para a próxima fase: {nextLevelScene}");
            SceneManager.LoadScene(nextLevelScene);
        }
        else
        {
            Debug.LogWarning("Próxima fase não configurada! Voltando ao menu...");
            GoToMainMenu();
        }
    }

    /// <summary>
    /// Reinicia a fase atual
    /// Conecte este método ao botão "Jogar Novamente" / "Play Again"
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log("🔄 Reiniciando fase...");
        
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
        Debug.Log("👋 Saindo do jogo...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// [EDITOR] Detecta automaticamente qual é a próxima fase
    /// </summary>
    [ContextMenu("🔍 Auto-Detectar Próxima Fase")]
    private void AutoDetectNextLevel()
    {
        // Se estamos na Fase 1, próxima é Fase 2
        if (GameManager.Instance != null && GameManager.Instance.sceneToReload == "Fase 1")
        {
            nextLevelScene = "Fase 2";
            hasNextLevel = true;
            Debug.Log("✅ Próxima fase detectada: Fase 2");
        }
        // Se estamos na Fase 2, não tem próxima (ou configurar manualmente)
        else
        {
            hasNextLevel = false;
            Debug.Log("ℹ️ Última fase do jogo (ou configure manualmente)");
        }
    }
}

