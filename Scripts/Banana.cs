using UnityEngine;
using System.Collections;

public class Banana : MonoBehaviour
{
    [Header("Animación")]
    public Sprite[] animationSprites;  
    public float frameTime = 0.5f; 

    [Header("Puntos")]
    public int points = 10;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (animationSprites.Length > 0)
        {
            StartCoroutine(AnimateBanana());
        }
    }

    IEnumerator AnimateBanana()
    {
        int currentFrame = 0;
        int direction = 1;  

        while (true)
        {
            spriteRenderer.sprite = animationSprites[currentFrame];
            
            yield return new WaitForSeconds(frameTime);
            
            int nextFrame = currentFrame + direction;
            
            if (nextFrame >= animationSprites.Length)
            {
                direction = -1;
                nextFrame = animationSprites.Length - 2;  
            }
            else if (nextFrame < 0)
            {
                direction = 1;
                nextFrame = 1;  
            }
            
            currentFrame = nextFrame;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ScoreManager.instance.AddPoints(points);
            Destroy(gameObject);  
        }
    }
}