using UnityEngine;

public class BananaPickup : MonoBehaviour
{
    [Header("Sonido")]
    public AudioClip sonidoBanana; // Arrastra tu sonido aquí
    [Range(0f, 2f)]
    public float volumenSonido = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Añade AudioSource si no lo tiene
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // ¡FIX: Habilita el AudioSource explícitamente!
        audioSource.enabled = true;
        
        audioSource.playOnAwake = false;
        audioSource.volume = volumenSonido;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Solo si es el jugador
        if (other.CompareTag("Player"))
        {
            // Reproduce el sonido (con chequeo extra por si acaso)
            if (audioSource != null && sonidoBanana != null)
            {
                audioSource.PlayOneShot(sonidoBanana);
            }

            // Destruye la banana (delay para que suene completo)
            Destroy(gameObject, 0.2f);
        }
    }
}