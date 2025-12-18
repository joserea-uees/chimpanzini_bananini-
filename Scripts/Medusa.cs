using UnityEngine;
using System.Collections;

public class Medusa : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidadFlotacion = 2.5f;
    public bool flotarADerecha = true;

    [Header("Ondulación de Flotación")]
    public float amplitudOndulacion = 0.4f;
    public float frecuenciaOndulacion = 2.5f;

    [Header("Animación")]
    public Sprite[] spritesFlotacion;
    public float velocidadAnimacion = 0.12f;

    [Header("Tiempo de Vida")]
    public float tiempoVida = 15f;

    [Header("Daño al Jugador")]
    public int dañoAlJugador = 1;
    public float cooldownDaño = 1f;

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
        posicionInicial = transform.position;
        xInicial = posicionInicial.x;

        coroutineAnim = StartCoroutine(AnimarFlotacion());
        coroutineMovimiento = StartCoroutine(FlotarYDesaparecer());
    }

    IEnumerator AnimarFlotacion()
    {
        while (true)
        {
            if (spritesFlotacion.Length > 0)
            {
                spriteRenderer.sprite = spritesFlotacion[indiceFrame];
                indiceFrame = (indiceFrame + 1) % spritesFlotacion.Length;
            }
            yield return new WaitForSeconds(velocidadAnimacion);
        }
    }

    IEnumerator FlotarYDesaparecer()
    {
        float tiempoTranscurrido = 0f;
        float direccionX = flotarADerecha ? 1f : -1f;

        while (tiempoTranscurrido < tiempoVida)
        {
            tiempoTranscurrido += Time.deltaTime;

            Vector3 nuevaPos = transform.position;
            nuevaPos.x += direccionX * velocidadFlotacion * Time.deltaTime;

            if (amplitudOndulacion > 0f)
            {
                float ondulacion = Mathf.Sin((nuevaPos.x - xInicial) * frecuenciaOndulacion + Time.time * 2f) * amplitudOndulacion;
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