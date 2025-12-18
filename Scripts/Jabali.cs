using UnityEngine;
using System.Collections;

public class Jabali : MonoBehaviour
{
    [Header("Configuración de Carrera")]
    public float velocidadCarrera = 4f;  
    public bool correrADerecha = true;

    [Header("Trote Irregular (Opcional)")]
    public float amplitudTrote = 0.15f;   
    public float frecuenciaTrote = 4f;    

    [Header("Animación de Patas/Correr")]
    public Sprite[] spritesCorrer;
    public float velocidadAnimacion = 0.08f; 

    [Header("Tiempo Total")]
    public float tiempoVida = 10f;

    private SpriteRenderer spriteRenderer;
    private int indiceFrame = 0;
    private Coroutine coroutineAnim;
    private Coroutine coroutineCarrera;
    private Vector3 posicionInicial;
    private float xInicial;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        spriteRenderer.flipX = !correrADerecha;
        posicionInicial = transform.position;
        xInicial = posicionInicial.x;

        coroutineAnim = StartCoroutine(AnimarCorrer());
        coroutineCarrera = StartCoroutine(CorrerYDesaparecer());
    }

    IEnumerator AnimarCorrer()
    {
        while (true)
        {
            if (spritesCorrer.Length > 0)
            {
                spriteRenderer.sprite = spritesCorrer[indiceFrame];
                indiceFrame = (indiceFrame + 1) % spritesCorrer.Length;
            }
            yield return new WaitForSeconds(velocidadAnimacion);
        }
    }

    IEnumerator CorrerYDesaparecer()
    {
        float tiempoTranscurrido = 0f;
        float direccionX = correrADerecha ? 1f : -1f;

        while (tiempoTranscurrido < tiempoVida)
        {
            tiempoTranscurrido += Time.deltaTime;

            Vector3 nuevaPos = transform.position;
            nuevaPos.x += direccionX * velocidadCarrera * Time.deltaTime;

            
            if (amplitudTrote > 0f)
            {
                float trote = Mathf.Sin((nuevaPos.x - xInicial) * frecuenciaTrote) * amplitudTrote;
                nuevaPos.y = posicionInicial.y + trote;
            }

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
        if (coroutineAnim != null) StopCoroutine(coroutineAnim);
        if (coroutineCarrera != null) StopCoroutine(coroutineCarrera);
    }
}