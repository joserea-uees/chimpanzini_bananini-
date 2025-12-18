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
        Debug.Log("Intentando cargar MenuPrincipal");
        SceneManager.LoadScene("Inicio");
    }
}