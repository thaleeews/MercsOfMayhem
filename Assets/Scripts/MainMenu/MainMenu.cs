using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Music")]
    [SerializeField] private AudioClip menuMusicClip;
    [SerializeField] private float menuMusicVolume = 1f;

    private void Start()
    {
        // Tenta carregar automaticamente se não estiver configurado
        if (menuMusicClip == null)
        {
            menuMusicClip = MusicLoader.LoadMusic("Menu");
        }

        // Garante que o MusicManager existe
        EnsureMusicManager();
        
        // Se já está tocando a música do menu, não reinicia
        // Se está tocando música diferente (ex: gameplay), troca para música do menu
        if (menuMusicClip != null && MusicManager.Instance != null)
        {
            // Se já está tocando a mesma música do menu, não reinicia
            if (!MusicManager.Instance.IsPlayingClip(menuMusicClip))
            {
                // Se não está tocando ou é música diferente, toca a música do menu
                MusicManager.Instance.PlayMusic(menuMusicClip, restartIfSame: false, volume: menuMusicVolume);
            }
        }
        else if (menuMusicClip == null)
        {
            Debug.LogWarning("MainMenu: AudioClip não configurado! Configure no Inspector ou adicione 'Menu.mp3' em Assets/Resources/Musics/");
        }
    }

    private void EnsureMusicManager()
    {
        if (MusicManager.Instance == null)
        {
            var go = new GameObject("MusicManager");
            go.AddComponent<MusicManager>(); // adiciona também um AudioSource por causa do RequireComponent
            var src = go.GetComponent<AudioSource>();
            if (src != null)
            {
                src.playOnAwake = false;
                src.loop = true;
            }
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Fase 1");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
