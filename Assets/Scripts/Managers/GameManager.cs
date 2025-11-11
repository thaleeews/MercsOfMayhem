using UnityEngine;
using UnityEngine.SceneManagement; // Importante: Adicione esta linha!

public class GerenciadorDeCenas : MonoBehaviour
{
    public void CarregarCena(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }
    public void SairDoJogo()
    {
        Application.Quit();
    }
    
}