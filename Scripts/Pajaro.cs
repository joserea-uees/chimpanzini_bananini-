using UnityEngine;
using System.Collections;

public class Pajaro : MonoBehaviour
{
    [Header("Configuración de Vuelo")]
    public float velocidadVuelo = 3f;
    public bool volarADerecha = true;

    [Header("Onda de Vuelo")]
    public float amplitudOnda = 0.5f;
    public float frecuenciaOnda = 2f;

    [Header("Animación de Alas")]
    public Sprite[] spritesAlas;
    public float velocidadAnimacionAlas = 0.1f;

    [Header("Tiempo Total")]
    public float tiempoVida = 10f;

    private SpriteRenderer spriteRenderer;
    private int indiceFrameAlas = 0;
    private Coroutine coroutineAnimAlas;
    private Coroutine coroutineVuelo;
    private Vector3 posicionInicial;
    private float xInicial;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        spriteRenderer.flipX = !volarADerecha;
        posicionInicial = transform.position;
        xInicial = posicionInicial.x;
        coroutineAnimAlas = StartCoroutine(AnimarAlas());
        coroutineVuelo = StartCoroutine(VolarYDesaparecer());
    }

    IEnumerator AnimarAlas()
    {
        while (true)
        {
            if (spritesAlas.Length > 0)
            {
                spriteRenderer.sprite = spritesAlas[indiceFrameAlas];
                indiceFrameAlas = (indiceFrameAlas + 1) % spritesAlas.Length;
            }
            yield return new WaitForSeconds(velocidadAnimacionAlas);
        }
    }

    IEnumerator VolarYDesaparecer()
    {
        float tiempoTranscurrido = 0f;
        float direccionX = volarADerecha ? 1f : -1f;

        while (tiempoTranscurrido < tiempoVida)
        {
            tiempoTranscurrido += Time.deltaTime;
            Vector3 nuevaPos = transform.position;
            nuevaPos.x += direccionX * velocidadVuelo * Time.deltaTime;
            float onda = Mathf.Sin((nuevaPos.x - xInicial) * frecuenciaOnda) * amplitudOnda;
            nuevaPos.y = posicionInicial.y + onda;
            transform.position = nuevaPos;
            yield return null;
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                
                player.Morir();
                Destroy(gameObject); 
            }
        }
    }

    void OnDestroy()
    {
        if (coroutineAnimAlas != null) StopCoroutine(coroutineAnimAlas);
        if (coroutineVuelo != null) StopCoroutine(coroutineVuelo);
    }
}