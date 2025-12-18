using UnityEngine;
using UnityEngine.UI;  
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public int score = 0;
    public TMP_Text scoreText;  

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
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
    }
}