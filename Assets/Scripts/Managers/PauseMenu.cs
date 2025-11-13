using UnityEngine;
using UnityEngine.SceneManagement;

// Este script é "mortal". Ele vive no PlayerHUDCanvas
// e morre quando a cena muda.
public class PauseMenu : MonoBehaviour
{
    // Arraste aqui o seu Panel (o que tem os botões)
    [SerializeField] private GameObject pauseMenuPanel;

    private bool isPaused = false;

    void Start()
    {
        // Garante que o menu começa escondido e o tempo normal
        isPaused = false;
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        Time.timeScale = 1f; // Tempo normal
    }

    void Update()
    {
        // "Ouve" a tecla Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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

    // --- Funções Públicas para os Botões ---

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false); // Esconde o menu
        Time.timeScale = 1f; // Volta o tempo ao normal
    }

    public void RestartLevel()
    {
        // IMPORTANTE: Volta o tempo ao normal ANTES de recarregar
        Time.timeScale = 1f; 
        
        // Pega o nome da cena ATUAL e a recarrega
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void LoadMenu()
    {
        // Volta o tempo ao normal ANTES de sair
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Mude para o nome da sua cena de menu
    }
    private void PauseGame()
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true); // Mostra o menu de pause
        Time.timeScale = 0f; // CONGELA o tempo do jogo
    }
}