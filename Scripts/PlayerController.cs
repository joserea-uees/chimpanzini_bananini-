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
    public Sprite spriteSaltar;       
    public float velocidadAnimacion = 0.05f; 

    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool estaEnSuelo;
    private int indiceAnimacion = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        
        StartCoroutine(RutinaAnimacion());
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
    }

    IEnumerator RutinaAnimacion()
    {
        while (true) 
        {
            if (estaEnSuelo)
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
            }
            else
            {
                if (spriteSaltar != null)
                {
                    spriteRenderer.sprite = spriteSaltar;
                }
                
            
            }

            yield return new WaitForSeconds(velocidadAnimacion);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            estaEnSuelo = true;
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
}