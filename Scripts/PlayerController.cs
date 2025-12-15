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
    public Sprite[] spritesIdle;      
    public Sprite[] spritesSaltar;       
    public float velocidadAnimacionCorrer = 0.03f;  
    public float velocidadAnimacionIdle = 0.09f;     
    public float velocidadAnimacionSalto = 0.08f;

    [Header("Saltos en modo natación (solo Nivel3)")]
    public int saltosExtrasPermitidos = 999; 

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private int groundContacts = 0;
    private bool estaEnSuelo => groundContacts > 0;
    private int saltosRestantes; 

    private int runIndex = 0;
    private int idleIndex = 0;        
    private int jumpIndex = 0;
    private Coroutine mainAnimCoroutine;

    // Buffers de input para mejor respuesta
    private float moveInput;
    private bool isMoving;
    private bool downPressed;

    private bool esModoNatacion => SceneManager.GetActiveScene().name == "Nivel3";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        runIndex = 0;
        idleIndex = 0;  
        jumpIndex = 0;
        ReiniciarSaltos();
        mainAnimCoroutine = StartCoroutine(MainAnimationLoop());
    }

    void Update()
    {
        // Input horizontal y flip inmediato
        moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        isMoving = Mathf.Abs(moveInput) > 0.1f;

        // Flip sprite basado en dirección (se mantiene al parar)
        if (moveInput > 0) spriteRenderer.flipX = false;
        else if (moveInput < 0) spriteRenderer.flipX = true;

        // Input down
        downPressed = Input.GetKey(KeyCode.S);

        // Input salto
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && saltosRestantes > 0)
        {
            Saltar();
            saltosRestantes--;
        }
    }

    void FixedUpdate()
    {
        // Aplicar movimiento suave
        rb.linearVelocity = new Vector2(moveInput * velocidadCorrer, rb.linearVelocity.y);

        // Caída rápida con S (solo en aire)
        if (downPressed && !estaEnSuelo)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - 20f * Time.fixedDeltaTime);
        }
    }

    void Saltar()
    {
        // Reset velocidad Y y aplicar impulso (mantiene X)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);

        // Reset animación de salto (para saltos múltiples)
        jumpIndex = 0;
    }

    // ¡BUCLE PRINCIPAL DE ANIMACIÓN! Maneja TODO
    IEnumerator MainAnimationLoop()
    {
        while (true)
        {
            if (!estaEnSuelo)
            {
                // Animación de salto/caída (cicla hasta último frame)
                UpdateJumpFrame();
                yield return new WaitForSeconds(velocidadAnimacionSalto);
            }
            else
            {
                if (isMoving)
                {
                    // CORRER solo cuando te mueves (¡AHORA MÁS RÁPIDA!)
                    UpdateRunFrame();
                    yield return new WaitForSeconds(velocidadAnimacionCorrer);
                }
                else
                {
                    // IDLE/PARADO con animación completa o fallback!
                    UpdateIdleFrame();
                    yield return new WaitForSeconds(velocidadAnimacionIdle);
                }
            }
        }
    }

    void UpdateRunFrame()
    {
        if (spritesCorrer.Length > 0)
        {
            spriteRenderer.sprite = spritesCorrer[runIndex];
            runIndex = (runIndex + 1) % spritesCorrer.Length;
        }
    }

    // Función para animar IDLE
    void UpdateIdleFrame()
    {
        if (spritesIdle.Length > 0)
        {
            // Usa spritesIdle si los tienes
            spriteRenderer.sprite = spritesIdle[idleIndex];
            idleIndex = (idleIndex + 1) % spritesIdle.Length;
        }
        else
        {
            // FALLBACK: Primer sprite de correr como pose parada
            if (spritesCorrer.Length > 0)
            {
                spriteRenderer.sprite = spritesCorrer[0];
            }
        }
    }

    void UpdateJumpFrame()
    {
        if (spritesSaltar.Length > 0)
        {
            spriteRenderer.sprite = spritesSaltar[jumpIndex];
            jumpIndex++;
            if (jumpIndex >= spritesSaltar.Length)
            {
                jumpIndex = spritesSaltar.Length - 1;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            groundContacts++;
            ReiniciarSaltos();
            runIndex = 0;
            idleIndex = 0;  
        }

        if (collision.gameObject.CompareTag("Obstaculo") || collision.gameObject.CompareTag("DeathZone"))
        {
            ReiniciarNivel();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            groundContacts--;
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
        if (mainAnimCoroutine != null)
        {
            StopCoroutine(mainAnimCoroutine);
        }
    }
}