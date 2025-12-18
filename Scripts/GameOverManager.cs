using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Resetear estados manualmente antes de reload
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null) {
            player.muerteDefinitiva = false;
            player.rb.gravityScale = 1f;  // Valor original, ajusta si es diferente
            player.rb.linearVelocity = Vector2.zero;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Ground"), false);
            Debug.Log("Reseteando jugador antes de reload");
        }

        if (LifeManager.instance != null)
        {
            LifeManager.instance.currentLives = LifeManager.instance.maxLives;
            LifeManager.instance.UpdateLivesUI();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        CleanUpPreviousGame();
        Debug.Log("Intentando cargar MenuPrincipal");
        SceneManager.LoadScene("Inicio");
    }

     void CleanUpPreviousGame()
    {
        Debug.Log("Limpiando restos del juego anterior...");
        
        // Buscar y destruir cualquier Player que haya quedado
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            // Antes de destruir, resetear físicas si es necesario
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 1f; // Valor normal de gravedad
                rb.linearVelocity = Vector2.zero;
            }
            
            Debug.Log($"Destruyendo Player: {player.name}");
            Destroy(player);
        }
        
        // Mantener ScoreManager para que el puntaje persista entre niveles.
        // Si quieres reiniciar el puntaje al volver al menú, usa:
        // if (ScoreManager.instance != null) ScoreManager.instance.ResetScore();
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            Debug.Log("Preservando ScoreManager para mantener puntaje entre niveles");
        }
        
        // Buscar y destruir LifeManager si existe
        LifeManager lifeManager = FindObjectOfType<LifeManager>();
        if (lifeManager != null)
        {
            Debug.Log("Destruyendo LifeManager");
            Destroy(lifeManager.gameObject);
        }
        
        // Asegurar que el tiempo esté normalizado
        Time.timeScale = 1f;
        AudioListener.pause = false;
        
        // Asegurar que las colisiones entre Player y Ground estén activas
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Ground"),
            false
        );
        
        Debug.Log("Limpieza completada");
    }
}