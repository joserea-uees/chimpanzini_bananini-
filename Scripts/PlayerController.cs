using UnityEngine;
using System.Collections;               
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float fuerzaSalto = 15f;
    public float velocidadCorrer = 10f;

    [Header("Configuración de Animación")]
    public Sprite[] spritesCorrer;    
    public Sprite[] spritesSaltar;       
    public float velocidadAnimacionCorrer = 0.08f;
    public float velocidadAnimacionSalto = 0.12f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool estaEnSuelo = true;
    private int indiceAnimacion = 0;
    private int indiceAnimacionSalto = 0;

    private Coroutine coroutineCorrer;
    private Coroutine coroutineSaltar;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        estaEnSuelo = true;
        indiceAnimacion = 0;

        coroutineCorrer = StartCoroutine(RutinaAnimacionCorrer());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (estaEnSuelo)
            {
                Saltar();
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(velocidadCorrer, rb.linearVelocity.y);
    }

    void Saltar()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
        rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
        estaEnSuelo = false;
        indiceAnimacionSalto = 0;

        if (coroutineCorrer != null) StopCoroutine(coroutineCorrer);
        coroutineSaltar = StartCoroutine(RutinaAnimacionSalto());
    }

    IEnumerator RutinaAnimacionCorrer()
    {
        while (true)
        {
            if (spritesCorrer.Length > 0)
            {
                spriteRenderer.sprite = spritesCorrer[indiceAnimacion];
                indiceAnimacion++;
                if (indiceAnimacion >= spritesCorrer.Length)
                {
                    indiceAnimacion = 0;
                }
            }
            yield return new WaitForSeconds(velocidadAnimacionCorrer);
        }
    }

    IEnumerator RutinaAnimacionSalto()
    {
        while (true)
        {
            if (!estaEnSuelo)
            {
                if (spritesSaltar.Length > 0)
                {
                    spriteRenderer.sprite = spritesSaltar[indiceAnimacionSalto];
                    indiceAnimacionSalto++;
                    if (indiceAnimacionSalto >= spritesSaltar.Length)
                    {
                        indiceAnimacionSalto = spritesSaltar.Length - 1;
                    }
                }
            }

            yield return new WaitForSeconds(velocidadAnimacionSalto);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            estaEnSuelo = true;

            if (coroutineSaltar != null) StopCoroutine(coroutineSaltar);
            coroutineCorrer = StartCoroutine(RutinaAnimacionCorrer());
        }

        if (collision.gameObject.CompareTag("Obstaculo") || collision.gameObject.CompareTag("DeathZone"))
        {
            ReiniciarNivel();
        }
    }

    void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (coroutineCorrer != null) StopCoroutine(coroutineCorrer);
        if (coroutineSaltar != null) StopCoroutine(coroutineSaltar);
    }
}