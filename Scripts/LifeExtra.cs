using UnityEngine;

public class LifeExtra : MonoBehaviour
{
    [Header("Configuración")]
    public AudioClip sonidoRecoger;
    [Range(0f, 2f)]
    public float volumenSonido = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Intentar agregar vida
            if (LifeManager.instance != null)
            {
                bool vidaAgregada = LifeManager.instance.AddLife();
                
                // Solo destruir si se pudo agregar la vida
                if (vidaAgregada)
                {
                    // Reproducir sonido si está configurado
                    if (sonidoRecoger != null)
                    {
                        AudioSource.PlayClipAtPoint(sonidoRecoger, transform.position, volumenSonido);
                    }
                    
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogError("LifeExtra: No se encontró LifeManager.instance");
            }
        }
    }
}
