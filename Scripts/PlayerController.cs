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
    public Sprite[] spritesDaño;
    public float velocidadAnimacionCorrer = 0.03f;  
    public float velocidadAnimacionIdle = 0.09f;     
    public float velocidadAnimacionSalto = 0.08f;
    public float velocidadAnimacionDaño = 0.08f;

    [Header("Configuración de Ataque")]
    public Sprite[] spritesAtaque;  
    public float velocidadAnimacionAtaque = 0.05f;
    public bool bloquearMovimientoDuranteAtaque = true;  

    private bool recibiendoDaño = false;

    [Header("Saltos en modo natación (solo Nivel3)")]
    public int saltosExtrasPermitidos = 999; 

    [Header("Invulnerabilidad")]
    [SerializeField] private float tiempoInvulnerable = 1f;
    [SerializeField] private float intervaloParpadeo = 0.1f;
    private bool invulnerable = false;
    private Coroutine parpadeoCoroutine;
    [Header("Muerte Definitiva")]
    public float fuerzaCaidaMuerte = 10f;

    private bool muerteDefinitiva = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private int groundContacts = 0;
    private bool estaEnSuelo => groundContacts > 0;
    private int saltosRestantes; 

    private int runIndex = 0;
    private int idleIndex = 0;        
    private int jumpIndex = 0;
    private int attackIndex = 0;  
    private int dañoIndex = 0;
    private bool isAttacking = false;  
    public GameObject gameOverPanel;


    private Coroutine mainAnimCoroutine;

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
        attackIndex = 0;  
        ReiniciarSaltos();
        mainAnimCoroutine = StartCoroutine(MainAnimationLoop());
    }

    void Update()
    {
        moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        isMoving = Mathf.Abs(moveInput) > 0.1f;

        if (moveInput > 0) spriteRenderer.flipX = false;
        else if (moveInput < 0) spriteRenderer.flipX = true;

        downPressed = Input.GetKey(KeyCode.S);

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && saltosRestantes > 0)
        {
            Saltar();
            saltosRestantes--;
        }

        if (Input.GetKeyDown(KeyCode.E) && !isAttacking && spritesAtaque.Length > 0)
        {
            isAttacking = true;
            attackIndex = 0;
            Debug.Log("¡Ataque activado!"); 
        }
    }

    void FixedUpdate()
    {
        float finalMoveInput = moveInput;
        if (isAttacking && bloquearMovimientoDuranteAtaque)
        {
            finalMoveInput = 0f;
        }

        rb.linearVelocity = new Vector2(finalMoveInput * velocidadCorrer, rb.linearVelocity.y);

        if (downPressed && !estaEnSuelo)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - 20f * Time.fixedDeltaTime);
        }
    }

    void Saltar()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);

        jumpIndex = 0;
    }

    IEnumerator MainAnimationLoop()
    {
        while (true)
        {
            if (recibiendoDaño)
            {
                UpdateDañoFrame();
                yield return new WaitForSeconds(velocidadAnimacionDaño);
                continue;
            }
            if (isAttacking)
            {
                UpdateAttackFrame();
                yield return new WaitForSeconds(velocidadAnimacionAtaque);
                continue;  
            }

            if (!estaEnSuelo)
            {
                UpdateJumpFrame();
                yield return new WaitForSeconds(velocidadAnimacionSalto);
            }
            else
            {
                if (isMoving)
                {
                    UpdateRunFrame();
                    yield return new WaitForSeconds(velocidadAnimacionCorrer);
                }
                else
                {
                    UpdateIdleFrame();
                    yield return new WaitForSeconds(velocidadAnimacionIdle);
                }
            }
        }
    }

    void UpdateAttackFrame()
    {
        if (spritesAtaque.Length > 0)
        {
            spriteRenderer.sprite = spritesAtaque[attackIndex];
            attackIndex++;
            if (attackIndex >= spritesAtaque.Length)
            {
                isAttacking = false;
                attackIndex = 0;
                Debug.Log("¡Ataque terminado!"); 
            }
        }
        else
        {
            isAttacking = false;
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

    void UpdateIdleFrame()
    {
        if (spritesIdle.Length > 0)
        {
            spriteRenderer.sprite = spritesIdle[idleIndex];
            idleIndex = (idleIndex + 1) % spritesIdle.Length;
        }
        else
        {
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

    void UpdateDañoFrame()
    {
        if (spritesDaño.Length > 0)
        {
            spriteRenderer.sprite = spritesDaño[dañoIndex];
            dañoIndex++;

            if (dañoIndex >= spritesDaño.Length)
            {
                dañoIndex = 0;
                recibiendoDaño = false;
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
            attackIndex = 0;  
        }

        if (collision.gameObject.CompareTag("Obstaculo") || collision.gameObject.CompareTag("DeathZone"))
        {
            Morir();
        }

    }   

    IEnumerator Invulnerabilidad()
    {
        invulnerable = true;

        parpadeoCoroutine = StartCoroutine(Parpadeo());

        yield return new WaitForSeconds(tiempoInvulnerable);

        invulnerable = false;

        if (parpadeoCoroutine != null)
            StopCoroutine(parpadeoCoroutine);

        spriteRenderer.enabled = true;
    }

    IEnumerator Parpadeo()
    {
        while (true)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(intervaloParpadeo);
        }
    }

    public void Morir()
    {
        if (invulnerable || muerteDefinitiva) return;

        bool esUltimaVida = LifeManager.instance.PlayerDied();
        IniciarDaño();
        StartCoroutine(Invulnerabilidad());
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 8f);
        if (esUltimaVida)
        {
            StartCoroutine(ReiniciarDespuesDeDaño(true));
        }
        else
        {
            StartCoroutine(ReiniciarDespuesDeDaño(false));
        }
    }

    IEnumerator ReiniciarDespuesDeDaño(bool muerteDefinitiva)
    {
        yield return new WaitForSeconds(
            spritesDaño.Length * velocidadAnimacionDaño
        );

        if (muerteDefinitiva)
        {
            StartCoroutine(MuerteFinal());
        }
    }


    IEnumerator AnimacionDaño()
    {
        recibiendoDaño = true;

        for (int i = 0; i < spritesDaño.Length; i++)
        {
            spriteRenderer.sprite = spritesDaño[i];
            yield return new WaitForSeconds(velocidadAnimacionDaño);
        }

        recibiendoDaño = false;
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

    void IniciarDaño()
    {
        recibiendoDaño = true;
        dañoIndex = 0;
    }

    public IEnumerator MuerteFinal()
    {
        if (muerteDefinitiva) yield break;
        muerteDefinitiva = true;
        if (spritesDaño != null && spritesDaño.Length > 0)
        {
            yield return new WaitForSeconds(
                spritesDaño.Length * velocidadAnimacionDaño
            );
        }
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 5f;
        rb.AddForce(Vector2.down * fuerzaCaidaMuerte, ForceMode2D.Impulse);

        IgnorarSuelo(true);

        Camera cam = Camera.main;

        while (true)
        {
            Vector3 viewPos = cam.WorldToViewportPoint(transform.position);

            if (viewPos.y < -0.2f)
            {
                MostrarVentanaGameOver();
                yield break;
            }

            yield return null;
        }
    }


    void LateUpdate()
    {
        if (!muerteDefinitiva) return;

        if (!EstaEnPantalla())
        {
            MostrarVentanaGameOver();
            muerteDefinitiva = false;
        }
    }

    bool EstaEnPantalla()
    {
    Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
    return viewPos.y > -0.2f;
    }

    void MostrarVentanaGameOver()
    {
        Time.timeScale = 0f;  
        AudioListener.pause = true;
        gameOverPanel.SetActive(true);
    }

    void IgnorarSuelo(bool ignorar)
    {
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Ground"),
            ignorar
        );
    }


    private void OnDestroy()
    {
        if (mainAnimCoroutine != null)
        {
            StopCoroutine(mainAnimCoroutine);
        }
    }
}