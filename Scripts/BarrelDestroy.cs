using UnityEngine;
using System.Collections;

public class BarrelDestroy : MonoBehaviour
{
    [Header("Configuración de Destrucción")]
    public Sprite[] spritesDestruccion;
    public float velocidadAnimacion = 0.1f;
    
    private SpriteRenderer spriteRenderer;
    private bool estaDestruyendose = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si el objeto que colisiona es el player
        if (collision.gameObject.CompareTag("Player") && !estaDestruyendose)
        {
            StartCoroutine(AnimacionDestruccion());
        }
    }

    IEnumerator AnimacionDestruccion()
    {
        estaDestruyendose = true;

        // Desactivar el collider para que no haya más colisiones
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Reproducir la animación de destrucción
        if (spritesDestruccion.Length > 0)
        {
            for (int i = 0; i < spritesDestruccion.Length; i++)
            {
                spriteRenderer.sprite = spritesDestruccion[i];
                yield return new WaitForSeconds(velocidadAnimacion);
            }
        }

        // Destruir el objeto después de la animación
        Destroy(gameObject);
    }
}
