using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour // Mude "GerenciadorDeCenas" para "GameManager"
{
    // --- Lógica do Singleton ---
    public static GameManager Instance { get; private set; }
    
    // --- Nossas "Memórias" do Jogo ---
    public string sceneToReload; // Guarda a cena onde o jogador morreu

    private void Awake()
    {
        // Lógica do Singleton:
        // Se já existe uma instância e não sou eu, destrua-me.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // Se eu sou a primeira instância, me torno o "Instance"
            // e me marco para Não Ser Destruído ao carregar novas cenas.
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // --- Funções que os Botões podem chamar ---

    public void ReloadLastScene()
    {
        // Verifica se a memória não está vazia
        if (!string.IsNullOrEmpty(sceneToReload))
        {
            SceneManager.LoadScene(sceneToReload);
        }
        else
        {
            // Plano B: Se não há cena salva, volte para a Fase 1
            SceneManager.LoadScene("Fase 1"); // Mude para o nome da sua 1ª fase
        }
    }

    public void GoToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // A sua função de Sair do Jogo (estava no GerenciadorDeCenas)
    public void SairDoJogo()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}