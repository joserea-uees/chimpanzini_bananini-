using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LifeManager : MonoBehaviour
{
    public static LifeManager instance;

    public int maxLives = 3;
    private int currentLives;

    public TMP_Text livesText;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        currentLives = maxLives;
        UpdateLivesUI();
    }

    void UpdateLivesUI()
    {
        string hearts = "";

        for (int i = 0; i < currentLives; i++)
        {
            hearts += "<sprite=0> ";
        }

        livesText.text = hearts;
    }

    public void PlayerDied()
    {
        currentLives--;

        if (currentLives <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            UpdateLivesUI();
        }
    }

    public bool AddLife()
    {
        // Solo agregar vida si no está al máximo
        if (currentLives < maxLives)
        {
            currentLives++;
            UpdateLivesUI();
            Debug.Log($"¡Vida extra recogida! Vidas actuales: {currentLives}/{maxLives}");
            return true;
        }
        else
        {
            Debug.Log($"Vida al máximo ({maxLives}). No se puede recoger.");
            return false;
        }
    }
}
