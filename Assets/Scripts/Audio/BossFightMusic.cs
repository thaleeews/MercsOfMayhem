using UnityEngine;

/// <summary>
/// Script para tocar a música de Boss Fight quando o boss aparecer.
/// Adicione este script ao GameObject do Boss ou chame StartBossFightMusic() quando o boss aparecer.
/// </summary>
public class BossFightMusic : MonoBehaviour
{
    [Header("Boss Fight Music")]
    [SerializeField] private AudioClip bossFightMusicClip;
    [SerializeField] private float bossFightMusicVolume = 1f;
    [SerializeField] private bool playOnStart = false;

    private void Start()
    {
        // Tenta carregar automaticamente se não estiver configurado
        if (bossFightMusicClip == null)
        {
            bossFightMusicClip = MusicLoader.LoadMusic("Boss Fight");
        }

        if (playOnStart)
        {
            StartBossFightMusic();
        }
    }

    /// <summary>
    /// Inicia a música de Boss Fight. Chame este método quando o boss aparecer.
    /// </summary>
    public void StartBossFightMusic()
    {
        if (MusicManager.Instance == null)
        {
            EnsureMusicManager();
        }

        if (MusicManager.Instance != null)
        {
            // Para a música atual (gameplay) antes de tocar a música do boss
            MusicManager.Instance.StopMusic();

            if (bossFightMusicClip != null)
            {
                MusicManager.Instance.PlayMusic(bossFightMusicClip, restartIfSame: true, volume: bossFightMusicVolume);
            }
            else
            {
                Debug.LogWarning("BossFightMusic: AudioClip não configurado! Configure no Inspector ou adicione 'Boss Fight.mp3' em Assets/Resources/Musics/");
            }
        }
    }

    /// <summary>
    /// Para a música de Boss Fight e volta para a música de gameplay
    /// </summary>
    public void StopBossFightMusic(AudioClip gameplayMusicClip, float gameplayVolume = 1f)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopMusic();
            if (gameplayMusicClip != null)
            {
                MusicManager.Instance.PlayMusic(gameplayMusicClip, restartIfSame: false, volume: gameplayVolume);
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

