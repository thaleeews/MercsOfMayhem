using UnityEngine;

/// <summary>
/// Script simples para garantir que a música do menu continue tocando
/// quando entrar na cena de Instruções. Adicione este script a qualquer GameObject
/// na cena de Instruções.
/// </summary>
public class MenuMusicPersist : MonoBehaviour
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
        
        // Se o MusicManager já está tocando a música do menu, não faz nada (não reseta)
        // Se não está tocando ou é uma música diferente, toca a música do menu
        if (MusicManager.Instance != null)
        {
            // Se já está tocando a mesma música do menu, não reinicia
            if (menuMusicClip != null && MusicManager.Instance.IsPlayingClip(menuMusicClip))
            {
                // Apenas ajusta o volume se necessário
                return;
            }
            
            // Se não está tocando ou é música diferente, toca a música do menu
            if (menuMusicClip != null)
            {
                MusicManager.Instance.PlayMusic(menuMusicClip, restartIfSame: false, volume: menuMusicVolume);
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
                if (audioSource.isPlaying && audioSource.clip == menuMusicClip)
                {
                    audioSource.Stop();
                }
            }
        }
    }
}

