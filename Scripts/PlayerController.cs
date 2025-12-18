using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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
    
    [Header("Configuración de Combate")]
    public float rangoAtaque = 1.2f;
    public float dañoAtaque = 25f;
    public string tagEnemigo = "Enemy"; // Usar tag en lugar de layer
    public Transform puntoAtaque;

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

    [Header("Game Over UI")]
    public TMP_Text gameOverScoreText;

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

        if (Input.GetKeyDown(KeyCode.E) && !isAttacking && spritesAtaque.Length > 0 && !recibiendoDaño)
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
            
            // Detectar golpe en frame específico (mitad de la animación)
            if (attackIndex == spritesAtaque.Length / 2)
            {
                DetectarGolpeEnemigos();
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

    void DetectarGolpeEnemigos()
    {
        Vector2 attackPos;
        if (puntoAtaque != null)
        {
            attackPos = puntoAtaque.position;
        }
        else
        {
            attackPos = (Vector2)transform.position + (spriteRenderer.flipX ? Vector2.left * 0.5f : Vector2.right * 0.5f);
        }
        
        // Detectar todos los colliders y filtrar por tag
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPos, rangoAtaque);
        
        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag(tagEnemigo))
            {
                EnemiController enemy = collider.GetComponent<EnemiController>();
                if (enemy != null)
                {
                    enemy.RecibirDaño(dañoAtaque);
                    Debug.Log($"¡Golpe a enemigo! Daño: {dañoAtaque}");
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

    private bool recibiendoDaño = false;

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

    // Método público para que el enemigo pueda causar daño al jugador
    public void RecibirDañoDeEnemigo(int dañoAlPlayer = 1)
    {
        if (invulnerable || muerteDefinitiva || recibiendoDaño) return;

        Debug.Log($"Player recibe {dañoAlPlayer} de daño del enemigo");

        // Usar el LifeManager para quitar vidas
        bool esUltimaVida = false;
        if (LifeManager.instance != null)
        {
            for (int i = 0; i < dañoAlPlayer; i++)
            {
                esUltimaVida = LifeManager.instance.PlayerDied();
            }
        }

        // Activar animación de daño
        IniciarDaño();
        
        // Aplicar invulnerabilidad
        StartCoroutine(Invulnerabilidad());

        // Aplicar knockback ligero
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 8f);

        if (esUltimaVida)
        {
            StartCoroutine(MuerteFinal());
        }
        else
        {
            StartCoroutine(ReiniciarDespuesDeDaño(false));
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
            // Verificar si es la última vida
            bool esUltimaVida = false;
            if (LifeManager.instance != null)
            {
                esUltimaVida = LifeManager.instance.PlayerDied();
            }
            
            IniciarDaño();
            StartCoroutine(Invulnerabilidad());
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 8f);
            
            if (esUltimaVida)
            {
                StartCoroutine(MuerteFinal());
            }
            else
            {
                StartCoroutine(ReiniciarDespuesDeDaño(false));
            }
        }

        // Detectar colisión con enemigo (para daño por contacto)
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemiController enemy = collision.gameObject.GetComponent<EnemiController>();
            if (enemy != null && !invulnerable && !recibiendoDaño)
            {
                RecibirDañoDeEnemigo(enemy.dañoAlPlayer);
            }
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
            StartCoroutine(MuerteFinal());
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
        
        // Solo esperar si hay sprites de daño
        if (spritesDaño != null && spritesDaño.Length > 0)
        {
            yield return new WaitForSeconds(
                spritesDaño.Length * velocidadAnimacionDaño
            );
        }
        
        // Configurar física para la caída (COPIADO DEL ORIGINAL)
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 5f; // Esta línea está en el original
        rb.AddForce(Vector2.down * fuerzaCaidaMuerte, ForceMode2D.Impulse);

        // Ignorar colisiones con el suelo (COPIADO DEL ORIGINAL)
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
        // Actualizar score en el panel
        if (ScoreManager.instance != null && gameOverScoreText != null)
        {
            gameOverScoreText.text = ScoreManager.instance.score.ToString();
        }
        
        Time.timeScale = 0f;
        AudioListener.pause = true;

        gameOverPanel.SetActive(true);

        StartCoroutine(SubirPanelConFade());
    }

    IEnumerator SubirPanelConFade()
    {
        // 1. Activamos el panel
        gameOverPanel.SetActive(true);

        // 2. Inmediatamente lo hacemos invisible y lo ponemos abajo
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

        // 3. Esperamos 1 frame para que Unity lo renderice correctamente
        yield return null;

        // 4. Ahora animamos suave
        float duracion = 1.5f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / duracion;
            t = 1f - Mathf.Pow(1f - t, 3f);  // Ease out suave

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

        // Final exacto
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

    private void OnDestroy()
    {
        if (mainAnimCoroutine != null)
        {
            StopCoroutine(mainAnimCoroutine);
        }
    }
}