using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper para carregar músicas automaticamente da pasta Assets/Sounds/Musics
/// </summary>
public static class MusicLoader
{
    /// <summary>
    /// Carrega uma música da pasta Assets/Sounds/Musics pelo nome (sem extensão)
    /// </summary>
    public static AudioClip LoadMusic(string musicName)
    {
        // Tenta carregar via Resources primeiro (substitui espaços por underscores)
        string resourcesName = musicName.Replace(" ", "_");
        AudioClip clip = Resources.Load<AudioClip>($"Musics/{resourcesName}");
        
        if (clip != null)
        {
            return clip;
        }

        // Se não encontrou via Resources, tenta carregar diretamente (só funciona no Editor)
        #if UNITY_EDITOR
        // Tenta com o nome original (com espaços)
        string path = $"Assets/Sounds/Musics/{musicName}.mp3";
        clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip != null)
        {
            return clip;
        }
        
        // Tenta sem espaços
        string pathNoSpaces = $"Assets/Sounds/Musics/{musicName.Replace(" ", "")}.mp3";
        clip = AssetDatabase.LoadAssetAtPath<AudioClip>(pathNoSpaces);
        if (clip != null)
        {
            return clip;
        }
        #endif

        Debug.LogWarning($"MusicLoader: Não foi possível carregar '{musicName}'. Verifique se o arquivo existe em Assets/Sounds/Musics/");
        return null;
    }
}

