using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
	public static MusicManager Instance { get; private set; }

	private AudioSource audioSource;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		audioSource = GetComponent<AudioSource>();
		audioSource.loop = true;

		// Evita reiniciar caso já esteja tocando ao trocar de cena/tela
		if (!audioSource.isPlaying && audioSource.clip != null)
		{
			audioSource.Play();
		}
	}

	/// <summary>
	/// Toca uma trilha. Se já for a mesma, não reinicia por padrão.
	/// </summary>
	public void PlayMusic(AudioClip clip, bool restartIfSame = false, float volume = 1f)
	{
		if (clip == null) return;

		if (!restartIfSame && audioSource.clip == clip && audioSource.isPlaying)
		{
			return;
		}

		audioSource.clip = clip;
		audioSource.volume = Mathf.Clamp01(volume);
		audioSource.Play();
	}

	public void StopMusic()
	{
		if (audioSource.isPlaying)
		{
			audioSource.Stop();
		}
	}

	/// <summary>
	/// Verifica se está tocando um clip específico
	/// </summary>
	public bool IsPlayingClip(AudioClip clip)
	{
		return audioSource.isPlaying && audioSource.clip == clip;
	}
}

