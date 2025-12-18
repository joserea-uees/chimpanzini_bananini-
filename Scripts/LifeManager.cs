using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LifeManager : MonoBehaviour
{
    public static LifeManager instance;
    public PlayerController playerController;

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
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            StartCoroutine(playerController.MuerteFinal()); 
        }

        return false;
    }

}
