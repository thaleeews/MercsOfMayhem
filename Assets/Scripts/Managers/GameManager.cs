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
            return;
        }
        
        // Se eu sou a primeira instância, me torno o "Instance"
        // e me marco para Não Ser Destruído ao carregar novas cenas.
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Registra o evento de carregamento de cena
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Se já estamos em uma cena quando o GameManager é criado, salva ela também
        SaveCurrentSceneIfNeeded();
    }

    private void OnEnable()
    {
        // Garante que o evento está registrado
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SaveCurrentSceneIfNeeded();
    }

    private void SaveCurrentSceneIfNeeded()
    {
        // Salva a cena atual, exceto se for menu ou telas de vitória/derrota
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "MainMenu" && sceneName != "Vitoria" && sceneName != "Derrota" && sceneName != "Instrucoes")
        {
            sceneToReload = sceneName;
            Debug.Log($"✅ GameManager: Cena '{sceneName}' salva para reload");
        }
        else if (sceneName == "Derrota")
        {
            // Garante que o DefeatScreenMenu existe quando a cena de Derrota é carregada
            DefeatScreenMenu.EnsureInstance();
        }
        else if (sceneName == "Vitoria")
        {
            // Garante que o VictoryScreenMenu existe quando a cena de Vitória é carregada
            // (se você quiser fazer o mesmo para vitória)
        }
    }

    /// <summary>
    /// Garante que o GameManager existe. Cria um se não existir.
    /// </summary>
    public static void EnsureInstance()
    {
        if (Instance == null)
        {
            // Procura se já existe um GameManager na cena que ainda não foi inicializado
            GameManager existing = FindFirstObjectByType<GameManager>();
            if (existing != null)
            {
                Debug.Log("✅ GameManager encontrado na cena mas não inicializado ainda");
                Instance = existing;
                return;
            }
            
            // Cria um novo GameManager
            GameObject go = new GameObject("GameManager");
            GameManager newManager = go.AddComponent<GameManager>();
            Debug.Log("✅ GameManager criado automaticamente");
            
            // Aguarda um frame para garantir que o Awake foi chamado
            // Mas como estamos em um método estático, não podemos usar corrotina
            // O Awake será chamado automaticamente pelo Unity
        }
        else
        {
            Debug.Log("✅ GameManager já existe");
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