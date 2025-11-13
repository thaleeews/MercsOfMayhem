using UnityEngine;
using UnityEngine.SceneManagement; // Precisamos disto!

public class PauseManager : MonoBehaviour
{
    // Não precisamos de [SerializeField] se o vamos encontrar automaticamente
    [SerializeField] private GameObject pauseMenuPanel;

    private bool isPaused = false;

    // --- NOVO: LÓGICA DE SINGLETON E "AUTO-FIND" ---

    public static PauseManager Instance; // Torna-o um Singleton
    
    private void Awake()
    {
        // Lógica de Singleton (para garantir que só existe um)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // "Ouve" por novas cenas a serem carregadas
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    // Chamado sempre que uma nova cena é carregada
void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Se a nova cena NÃO é o MainMenu (ex: é a Fase 1, Fase 2...)
        if (scene.name != "MainMenu") 
        {
            // Tenta encontrar o novo painel de pause na nova cena
            var panelObject = GameObject.Find("PauseMenuPanel"); 
            
            if (panelObject != null)
            {
                pauseMenuPanel = panelObject;
                Debug.Log("PauseManager encontrou o novo 'PauseMenuPanel'!");
                
                // Garante que ele começa escondido E
                // que o estado 'isPaused' está limpo
                pauseMenuPanel.SetActive(false); 
                isPaused = false;
                Time.timeScale = 1f;
            }
            else
            {
                Debug.LogWarning("PauseManager: Não foi possível encontrar 'PauseMenuPanel' na cena " + scene.name);
                pauseMenuPanel = null; // Garante que a referência está limpa
            }
        }
        else // --- ADIÇÃO IMPORTANTE ---
        {
            // Se a nova cena É o "MainMenu"
            
            // 1. Esquece a referência ao painel de pause (que foi destruído)
            pauseMenuPanel = null;
            
            // 2. Garante que o estado de pause está limpo
            isPaused = false;
            Time.timeScale = 1f; 
        }
    }
    
    // --- FIM DA NOVA LÓGICA ---

    void Start()
    {
        // A lógica de Start() agora é mais simples
        // Apenas garantimos que o tempo está normal
        isPaused = false;
        Time.timeScale = 1f; 
    }

    void Update()
    {
        // "Ouve" a tecla Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Se não tivermos um painel para mostrar, não faz nada
            if (pauseMenuPanel == null) return; 
            
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true); 
        Time.timeScale = 0f; 
    }

    // --- Funções Públicas para os Botões ---

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false); 
        Time.timeScale = 1f; 
    }

    public void RestartLevel()
    {
        // --- MUDANÇA IMPORTANTE ---
        // Dizemos ao script que não estamos mais pausados
        // ANTES de recarregar a cena.
        isPaused = false; 
        Time.timeScale = 1f; 

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void LoadMenu()
    {
        isPaused = false; // Mesmo se voltarmos ao menu
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); 
    }
}