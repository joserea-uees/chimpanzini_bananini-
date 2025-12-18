using UnityEngine;
using UnityEngine.UI;  
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public int score = 0;
    public TMP_Text scoreText;  
    public TMP_Text gameOverScoreText;
    
    [SerializeField] private string scoreTextTag = "ScoreText"; // Etiqueta para el texto de puntaje

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // Suscribirse al evento
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Buscar el texto de puntaje en la nueva escena
        FindAndAssignScoreText();
        
        // Actualizar el texto con el puntaje actual
        UpdateScoreDisplay();
    }

    void FindAndAssignScoreText()
    {
        // Buscar por tag
        GameObject scoreTextObj = GameObject.FindGameObjectWithTag(scoreTextTag);
        if (scoreTextObj != null)
        {
            scoreText = scoreTextObj.GetComponent<TMP_Text>();
            Debug.Log($"ScoreText encontrado por tag: {scoreTextObj.name}");
        }
        
        // Si no se encontró por tag, buscar por nombre
        if (scoreText == null)
        {
            scoreTextObj = GameObject.Find("ScoreText");
            if (scoreTextObj != null)
            {
                scoreText = scoreTextObj.GetComponent<TMP_Text>();
                Debug.Log($"ScoreText encontrado por nombre: {scoreTextObj.name}");
            }
        }
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "<voffset=10><size=130%><sprite=0></size></voffset>" + score;
        }
        else
        {
            Debug.LogWarning("ScoreText no encontrado en la escena actual");
        }
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreDisplay();
    }

    public void SetScoreText(TMP_Text text)
    {
        scoreText = text;
        UpdateScoreDisplay();
    }

    public void AddPoints(int points)
    {
        score += points;
        UpdateScoreDisplay();
    }

    public void UpdateGameOverScore(TMP_Text gameOverText = null)
    {
        if (gameOverText != null)
        {
            gameOverScoreText = gameOverText;
        }
        
        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = score.ToString();
        }
    }

    void OnDestroy()
    {
        // Desuscribirse del evento cuando se destruya
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}