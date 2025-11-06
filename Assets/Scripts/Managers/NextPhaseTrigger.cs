using UnityEngine;
using UnityEngine.SceneManagement; // 1. PRECISAMOS disto para carregar cenas!

public class NextPhaseTrigger : MonoBehaviour
{
    // O nome da cena que queremos carregar.
    // [SerializeField] permite que você mude este nome no Inspector.
    [SerializeField] private string sceneToLoad = "Fase 2";

    // A tag do objeto que pode ativar este gatilho
    [SerializeField] private string playerTag = "Player";

    // Esta função é chamada automaticamente pelo Unity
    // quando algo entra no 'Trigger'
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 2. Verifica se o objeto que entrou tem a tag "Player"
        if (other.CompareTag(playerTag))
        {
            // 3. Se for o player, carrega a cena!
            Debug.Log($"Indo para a cena: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}