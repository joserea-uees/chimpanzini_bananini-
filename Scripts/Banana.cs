using UnityEngine;
using System.Collections;

public class Banana : MonoBehaviour
{
    [Header("Animación")]
    public Sprite[] animationSprites;  
    public float frameTime = 0.5f; 

    [Header("Puntos")]
    public int points = 10;

    [Header("Sonido")]
    public AudioClip sonidoRecoger;
    [Range(0f, 2f)]
    public float volumenSonido = 1f;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool yaRecogida = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Configurar AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.volume = volumenSonido;
        
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
        if (other.CompareTag("Player") && !yaRecogida)
        {
            yaRecogida = true;
            
            // Agregar puntos
            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.AddPoints(points);
            }
            
            // Reproducir sonido
            if (audioSource != null && sonidoRecoger != null)
            {
                audioSource.PlayOneShot(sonidoRecoger, volumenSonido);
            }
            
            // Ocultar visualmente pero mantener el objeto para que se escuche el sonido
            spriteRenderer.enabled = false;
            GetComponent<Collider2D>().enabled = false;
            
            // Destruir después de que termine el sonido
            float tiempoDestruccion = sonidoRecoger != null ? sonidoRecoger.length : 0.2f;
            Destroy(gameObject, tiempoDestruccion);
        }
    }
}