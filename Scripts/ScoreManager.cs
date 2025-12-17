using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public int score = 0;
    public TMP_Text scoreText;

    // Evento que se dispara cuando se alcanzan 10 puntos por primera vez
    public static event System.Action OnReached10Points;

    private bool hasReached10Points = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Opcional: si quieres que persista entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPoints(int points)
    {
        score += points;

        if (scoreText != null)
        {
            scoreText.text = "<voffset=10><size=130%><sprite=0></size></voffset> " + score;
        }

        // Solo notificamos una vez cuando llegamos a 10 o más
        if (!hasReached10Points && score >= 10)
        {
            hasReached10Points = true;
            OnReached10Points?.Invoke(); // Activa el generador
        }
    }
}