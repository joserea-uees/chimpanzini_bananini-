using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LifeManager : MonoBehaviour
{
    public static LifeManager instance;

    public int maxLives = 3;
    public int currentLives;

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

    public void UpdateLivesUI()
    {
        string hearts = "";

        for (int i = 0; i < currentLives; i++)
        {
            hearts += "<sprite=0> ";
        }

        livesText.text = hearts;
    }

    public bool PlayerDied()
    {
        currentLives--;

        if (currentLives <= 0)
        {
            // Muerte definitiva
            UpdateLivesUI();
            return true;
        }
        else
        {
            UpdateLivesUI();
            return false;
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
