using UnityEngine;
using System.Collections;

public class Pez : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidadNado = 3f;
    public bool nadarADerecha = true;  // True = va hacia la derecha, False = hacia la izquierda

    [Header("Ondulación de Natación")]
    public float amplitudOndulacion = 0.3f;
    public float frecuenciaOndulacion = 3f;

    [Header("Animación")]
    public Sprite[] spritesNatacion;
    public float velocidadAnimacion = 0.1f;

    [Header("Tiempo de Vida")]
    public float tiempoVida = 12f;

    [Header("Daño al Jugador")]
    public int dañoAlJugador = 1;
    public float cooldownDaño = 0.8f;

    private SpriteRenderer spriteRenderer;
    private int indiceFrame = 0;
    private Coroutine coroutineAnim;
    private Coroutine coroutineMovimiento;
    private Vector3 posicionInicial;
    private float xInicial;
    private bool puedeDañar = true;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // === LÍNEA ELIMINADA INTENCIONALMENTE ===
        // Ya NO volteamos el sprite según la dirección
        // spriteRenderer.flipX = !nadarADerecha;

        // Guardamos posición inicial para la ondulación
        posicionInicial = transform.position;
        xInicial = posicionInicial.x;

        coroutineAnim = StartCoroutine(AnimarNatacion());
        coroutineMovimiento = StartCoroutine(NadarYDesaparecer());
    }

    IEnumerator AnimarNatacion()
    {
        while (true)
        {
            if (spritesNatacion.Length > 0)
            {
                spriteRenderer.sprite = spritesNatacion[indiceFrame];
                indiceFrame = (indiceFrame + 1) % spritesNatacion.Length;
            }
            yield return new WaitForSeconds(velocidadAnimacion);
        }
    }

    IEnumerator NadarYDesaparecer()
    {
        float tiempoTranscurrido = 0f;
        float direccionX = nadarADerecha ? 1f : -1f;

        while (tiempoTranscurrido < tiempoVida)
        {
            tiempoTranscurrido += Time.deltaTime;

            Vector3 nuevaPos = transform.position;
            nuevaPos.x += direccionX * velocidadNado * Time.deltaTime;

            // Ondulación vertical (como onda al nadar)
            if (amplitudOndulacion > 0f)
            {
                float ondulacion = Mathf.Sin((nuevaPos.x - xInicial) * frecuenciaOndulacion) * amplitudOndulacion;
                nuevaPos.y = posicionInicial.y + ondulacion;
            }

            transform.position = nuevaPos;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && puedeDañar)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.RecibirDañoDeEnemigo(dañoAlJugador);
                Destroy(gameObject);
                puedeDañar = false;
                StartCoroutine(ReestablecerDaño());
            }
        }
    }

    IEnumerator ReestablecerDaño()
    {
        yield return new WaitForSeconds(cooldownDaño);
        puedeDañar = true;
    }

    void OnDestroy()
    {
        if (coroutineAnim != null) StopCoroutine(coroutineAnim);
        if (coroutineMovimiento != null) StopCoroutine(coroutineMovimiento);
    }
}