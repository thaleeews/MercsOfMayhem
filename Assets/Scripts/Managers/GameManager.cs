using UnityEngine;
using UnityEngine.SceneManagement; // Importante: Adicione esta linha!

public class GerenciadorDeCenas : MonoBehaviour
{
    public void CarregarCena(string nomeDaCena)
    {
        // A música é gerenciada pelos scripts específicos de cada cena:
        // - MainMenu.cs para o menu principal
        // - MenuMusicPersist.cs para Instruções
        // - GameplayMusic.cs para as fases do jogo
        SceneManager.LoadScene(nomeDaCena);
    }
    public void SairDoJogo()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
}