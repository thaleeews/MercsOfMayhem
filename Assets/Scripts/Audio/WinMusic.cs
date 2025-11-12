using UnityEngine;

/// <summary>
/// Script para tocar a música de vitória quando o jogador vencer.
/// Adicione este script a um GameObject ou chame PlayWinMusic() quando o jogador vencer.
/// </summary>
public class WinMusic : MonoBehaviour
{
    [Header("Win Music")]
    [SerializeField] private AudioClip winMusicClip;
    [SerializeField] private float winMusicVolume = 1f;
    [SerializeField] private bool playOnStart = false;

    private void Start()
    {
        // Tenta carregar automaticamente se não estiver configurado
        if (winMusicClip == null)
        {
            winMusicClip = MusicLoader.LoadMusic("Win");
        }

        if (playOnStart)
        {
            PlayWinMusic();
        }
    }

    /// <summary>
    /// Toca a música de vitória. Chame este método quando o jogador vencer.
    /// </summary>
    public void PlayWinMusic()
    {
        if (MusicManager.Instance == null)
        {
            EnsureMusicManager();
        }

        if (MusicManager.Instance != null)
        {
            // Para a música atual antes de tocar a música de vitória
            MusicManager.Instance.StopMusic();

            if (winMusicClip != null)
            {
                MusicManager.Instance.PlayMusic(winMusicClip, restartIfSame: true, volume: winMusicVolume);
            }
            else
            {
                Debug.LogWarning("WinMusic: AudioClip não configurado! Configure no Inspector ou adicione 'Win.mp3' em Assets/Resources/Musics/");
            }
        }
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
}

