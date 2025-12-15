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

    [Header("Saltos en modo natación (solo Nivel3)")]
    public int saltosExtrasPermitidos = 999; 

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool estaEnSuelo = true;
    private int saltosRestantes; 

    private int indiceAnimacion = 0;
    private int indiceAnimacionSalto = 0;

    private Coroutine coroutineCorrer;
    private Coroutine coroutineSaltar;

    private bool esModoNatacion => SceneManager.GetActiveScene().name == "Nivel3";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        ReiniciarSaltos();
        coroutineCorrer = StartCoroutine(RutinaAnimacionCorrer());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            if (saltosRestantes > 0)
            {
                Saltar();
                saltosRestantes--;
            }
        }
    }

        void FixedUpdate()
    {
        float movimientoHorizontal = 0f;
        if (Input.GetKey(KeyCode.A)) movimientoHorizontal = -1f;
        if (Input.GetKey(KeyCode.D)) movimientoHorizontal = 1f;

        rb.linearVelocity = new Vector2(movimientoHorizontal * velocidadCorrer, rb.linearVelocity.y);

        if (movimientoHorizontal > 0) spriteRenderer.flipX = false;
        else if (movimientoHorizontal < 0) spriteRenderer.flipX = true;

        if (Input.GetKey(KeyCode.S) && !estaEnSuelo)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - 20f * Time.fixedDeltaTime);
        }
    }

    void Saltar()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
        rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);

        if (estaEnSuelo)
        {
            estaEnSuelo = false;
            indiceAnimacionSalto = 0;
            if (coroutineCorrer != null) StopCoroutine(coroutineCorrer);
            coroutineSaltar = StartCoroutine(RutinaAnimacionSalto());
        }
    }

    IEnumerator RutinaAnimacionCorrer()
    {
        while (true)
        {
            if (spritesCorrer.Length > 0)
            {
                spriteRenderer.sprite = spritesCorrer[indiceAnimacion];
                indiceAnimacion = (indiceAnimacion + 1) % spritesCorrer.Length;
            }
            yield return new WaitForSeconds(velocidadAnimacionCorrer);
        }
    }

    IEnumerator RutinaAnimacionSalto()
    {
        while (!estaEnSuelo) 
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
            yield return new WaitForSeconds(velocidadAnimacionSalto);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            estaEnSuelo = true;
            ReiniciarSaltos();

            if (coroutineSaltar != null) StopCoroutine(coroutineSaltar);
            coroutineCorrer = StartCoroutine(RutinaAnimacionCorrer());
        }

        if (collision.gameObject.CompareTag("Obstaculo") || collision.gameObject.CompareTag("DeathZone"))
        {
            ReiniciarNivel();
        }
    }

    private void ReiniciarSaltos()
    {
        if (esModoNatacion)
        {
            saltosRestantes = saltosExtrasPermitidos;
        }
        else
        {
            saltosRestantes = 1;
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