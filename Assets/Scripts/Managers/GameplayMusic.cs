using UnityEngine;

/// <summary>
/// Script para tocar a música de gameplay nas fases do jogo (Fase 1, Fase 2, etc.)
/// Adicione este script a um GameObject nas cenas de gameplay.
/// </summary>
public class GameplayMusic : MonoBehaviour
{
    [Header("Gameplay Music")]
    [SerializeField] private AudioClip gameplayMusicClip;
    [SerializeField] private float gameplayMusicVolume = 1f;

    private void Start()
    {
        // Garante que o GameManager existe (importante para salvar a cena)
        GameManager.EnsureInstance();
        
        // Tenta carregar automaticamente se não estiver configurado
        if (gameplayMusicClip == null)
        {
            gameplayMusicClip = MusicLoader.LoadMusic("Gameplay");
        }

        // Garante que o MusicManager existe
        EnsureMusicManager();
        
        // Para a música do menu antes de tocar a música de gameplay
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopMusic();
            
            // Toca a música de gameplay
            if (gameplayMusicClip != null)
            {
                MusicManager.Instance.PlayMusic(gameplayMusicClip, restartIfSame: true, volume: gameplayMusicVolume);
            }
            else
            {
                Debug.LogWarning("GameplayMusic: AudioClip não configurado! Configure no Inspector ou adicione 'Gameplay.mp3' em Assets/Resources/Musics/");
            }
        }
        
        // Desativa qualquer AudioSource que possa estar tocando automaticamente nesta cena
        DisableAutoPlayAudioSources();
    }

    private void EnsureMusicManager()
    {
        if (MusicManager.Instance == null)
        {
            var go = new GameObject("MusicManager");
            go.AddComponent<MusicManager>();
            var src = go.GetComponent<AudioSource>();
            if (src != null)
            {
                src.playOnAwake = false;
                src.loop = true;
            }
        }
    }

    private void DisableAutoPlayAudioSources()
    {
        // Encontra todos os AudioSources na cena e desativa o playOnAwake
        // para evitar que toquem automaticamente e interfiram com o MusicManager
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var audioSource in allAudioSources)
        {
            // Não desativa o AudioSource do MusicManager
            if (audioSource.GetComponent<MusicManager>() == null)
            {
                audioSource.playOnAwake = false;
                // Se estiver tocando e não for o MusicManager, para
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }
    }
}

