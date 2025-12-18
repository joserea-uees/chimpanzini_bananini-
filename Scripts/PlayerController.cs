using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public float dañoAtaque = 20f; // Daño que hace a los enemigos
    public float rangoAtaque = 2f; // Rango del ataque
    public LayerMask enemigoLayer; // Layer de los enemigos

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
    public bool muerteDefinitiva = false;
    public GameObject gameOverPanel;

    public Rigidbody2D rb;
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
            
            // En el frame medio del ataque, hacer daño
            if (attackIndex == spritesAtaque.Length / 2)
            {
                DetectarYGolpearEnemigos();
            }
            
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

    void DetectarYGolpearEnemigos()
    {
        // Calcular posición del ataque según hacia dónde mira el jugador
        Vector2 posicionAtaque = transform.position;
        if (!spriteRenderer.flipX)
        {
            posicionAtaque += Vector2.right * (rangoAtaque * 0.5f);
        }
        else
        {
            posicionAtaque += Vector2.left * (rangoAtaque * 0.5f);
        }

        // Detectar todos los enemigos en el rango
        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(posicionAtaque, rangoAtaque, enemigoLayer);

        foreach (Collider2D enemigo in enemigosGolpeados)
        {
            EnemiController enemigoScript = enemigo.GetComponent<EnemiController>();
            if (enemigoScript != null)
            {
                enemigoScript.RecibirDaño(dañoAtaque);
                Debug.Log($"Player golpea a {enemigo.name} causando {dañoAtaque} de daño!");
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

    IEnumerator ReiniciarDespuesDeDaño(bool muerte)
    {
        yield return new WaitForSeconds(spritesDaño.Length * velocidadAnimacionDaño);

        if (muerte)
        {
            StartCoroutine(MuerteFinal());
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
            yield return new WaitForSeconds(spritesDaño.Length * velocidadAnimacionDaño);
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

    void MostrarVentanaGameOver()
    {
        if (gameOverPanel != null)
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
            gameOverPanel.SetActive(true);
            StartCoroutine(SubirPanelConFade());
        }
    }

    IEnumerator SubirPanelConFade()
    {
        gameOverPanel.SetActive(true);

        RectTransform rt = gameOverPanel.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, -800);

        Image fondoImage = gameOverPanel.GetComponent<Image>();
        if (fondoImage != null)
        {
            Color c = fondoImage.color;
            c.a = 0f;
            fondoImage.color = c;

            foreach (Graphic g in gameOverPanel.GetComponentsInChildren<Graphic>())
            {
                c = g.color;
                c.a = 0f;
                g.color = c;
            }
        }

        yield return null;

        float duracion = 1.5f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / duracion;
            t = 1f - Mathf.Pow(1f - t, 3f);

            rt.anchoredPosition = new Vector2(0, Mathf.Lerp(-800, 0, t));

            if (fondoImage != null)
            {
                Color c = fondoImage.color;
                c.a = t;
                fondoImage.color = c;

                foreach (Graphic g in gameOverPanel.GetComponentsInChildren<Graphic>())
                {
                    c = g.color;
                    c.a = t;
                    g.color = c;
                }
            }

            yield return null;
        }

        rt.anchoredPosition = Vector2.zero;
        if (fondoImage != null)
        {
            Color c = fondoImage.color;
            c.a = 1f;
            fondoImage.color = c;

            foreach (Graphic g in gameOverPanel.GetComponentsInChildren<Graphic>())
            {
                c = g.color;
                c.a = 1f;
                g.color = c;
            }
        }
    }

    void IgnorarSuelo(bool ignorar)
    {
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Ground"),
            ignorar
        );
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

    private void OnDestroy()
    {
        if (mainAnimCoroutine != null)
        {
            StopCoroutine(mainAnimCoroutine);
        }
    }

    // Visualizar rango de ataque en el editor
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Dibujar el rango de ataque
        Gizmos.color = Color.red;
        Vector2 posicionAtaque = transform.position;
        
        if (spriteRenderer != null)
        {
            if (!spriteRenderer.flipX)
            {
                posicionAtaque += Vector2.right * (rangoAtaque * 0.5f);
            }
            else
            {
                posicionAtaque += Vector2.left * (rangoAtaque * 0.5f);
            }
        }
        
        Gizmos.DrawWireSphere(posicionAtaque, rangoAtaque);
    }
}