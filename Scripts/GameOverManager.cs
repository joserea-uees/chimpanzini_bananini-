using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null) {
            player.muerteDefinitiva = false;
            player.rb.gravityScale = 1f;
            player.rb.linearVelocity = Vector2.zero;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Ground"), false);
        }

        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.ResetScore();
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
        
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.ResetScore();
        }
        
        CleanUpPreviousGame();
        SceneManager.LoadScene("Inicio");
    }

    void CleanUpPreviousGame()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 1f;
                rb.linearVelocity = Vector2.zero;
            }
            
            Destroy(player);
        }
        
        LifeManager lifeManager = FindObjectOfType<LifeManager>();
        if (lifeManager != null)
        {
            Destroy(lifeManager.gameObject);
        }
        
        Time.timeScale = 1f;
        AudioListener.pause = false;
        
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Ground"),
            false
        );
    }
}