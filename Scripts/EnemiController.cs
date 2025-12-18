using UnityEngine;
using System.Collections;

public class EnemiController : MonoBehaviour
{
    public enum EstadoEnemigo { Idle, Persiguiendo, Atacando, RecibiendoDaño, Muerto };
    
    [Header("Configuración de Vida")]
    public float vidaMaxima = 100f;
    public float vidaActual;
    public float dañoPorGolpe = 20f;
    
    [Header("Configuración de Recepción de Daño")]
    public bool puedeSerInterrumpido = true;
    public float tiempoRecuperacionDaño = 0.3f;
    public float fuerzaRetroceso = 5f;
    public bool esInmuneDuranteAtaque = false;

    [Header("Configuración de Estados")]
    public EstadoEnemigo estadoActual = EstadoEnemigo.Idle;

    [Header("Configuración de Persecución")]
    public float rangoDeteccion = 8f;
    public float velocidadPersecucion = 3f;
    public float velocidadPatrulla = 1.5f;
    public float distanciaMinima = 1f;
    public float distanciaParaAtacar = 1.5f;
    public float tiempoEntreAtaques = 2f;

    [Header("Configuración de Ataque")]
    public Sprite[] spritesAtaque;
    public float velocidadAnimacionAtaque = 0.1f;
    public int dañoAlPlayer = 1;
    public float knockbackFuerza = 5f;
    public float stunDuracion = 0.5f;

    [Header("Animaciones")]
    public Sprite[] spritesCaminar;
    public Sprite[] spritesIdle;
    public Sprite[] spritesDaño;
    public Sprite spriteNormal;
    public float velocidadAnimacionCaminar = 0.1f;
    public float velocidadAnimacionIdle = 0.2f;

    [Header("IA - Patrulla")]
    public bool patrullaHabilitada = true;
    public Transform[] puntosPatrulla;
    public float tiempoEsperaEnPunto = 2f;
    private int puntoPatrullaActual = 0;
    private float tiempoEsperaRestante = 0f;
    private bool patrullaIda = true;

    [Header("Recompensas")]
    public int puntosPorMuerte = 100;
    public GameObject[] lootDrops;
    public float dropChance = 0.3f;

    // Componentes
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;

    // Variables de control
    private bool puedeAtacar = true;
    private float tiempoUltimoAtaque = 0f;
    private int indiceAnimacion = 0;
    private float tiempoAnimacion = 0f;
    private bool estaStuneado = false;
    private float tiempoStunRestante = 0f;
    private Color colorOriginal;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        vidaActual = vidaMaxima;
        colorOriginal = spriteRenderer.color;

        // Buscar jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        
        // Estado inicial
        estadoActual = EstadoEnemigo.Idle;
    }

    void Update()
    {
        // Actualizar stun
        if (estaStuneado)
        {
            tiempoStunRestante -= Time.deltaTime;
            if (tiempoStunRestante <= 0)
            {
                estaStuneado = false;
                spriteRenderer.color = colorOriginal;
            }
            return;
        }

        // Máquina de estados
        switch (estadoActual)
        {
            case EstadoEnemigo.Idle:
                EstadoIdle();
                break;
            case EstadoEnemigo.Persiguiendo:
                EstadoPersiguiendo();
                break;
            case EstadoEnemigo.Atacando:
                // La animación de ataque se maneja en corrutina
                break;
            case EstadoEnemigo.RecibiendoDaño:
                // Se maneja en corrutina
                break;
            case EstadoEnemigo.Muerto:
                // No hacer nada
                break;
        }

        // Actualizar animaciones según estado
        ActualizarAnimacion();
    }

    void EstadoIdle()
    {
        // Si hay jugador cerca, empezar a perseguir
        if (player != null && Vector2.Distance(transform.position, player.position) <= rangoDeteccion)
        {
            estadoActual = EstadoEnemigo.Persiguiendo;
            return;
        }

        // Patrulla si está habilitada
        if (patrullaHabilitada && puntosPatrulla.Length > 0)
        {
            if (tiempoEsperaRestante > 0)
            {
                tiempoEsperaRestante -= Time.deltaTime;
                return;
            }

            Transform puntoObjetivo = puntosPatrulla[puntoPatrullaActual];
            float distancia = Vector2.Distance(transform.position, puntoObjetivo.position);

            if (distancia > 0.1f)
            {
                // Moverse hacia el punto
                Vector2 direccion = (puntoObjetivo.position - transform.position).normalized;
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    puntoObjetivo.position,
                    velocidadPatrulla * Time.deltaTime
                );

                // Voltear sprite
                spriteRenderer.flipX = direccion.x < 0;
            }
            else
            {
                // Llegó al punto, esperar
                tiempoEsperaRestante = tiempoEsperaEnPunto;
                
                // Avanzar al siguiente punto
                if (patrullaIda)
                {
                    puntoPatrullaActual++;
                    if (puntoPatrullaActual >= puntosPatrulla.Length - 1)
                    {
                        patrullaIda = false;
                    }
                }
                else
                {
                    puntoPatrullaActual--;
                    if (puntoPatrullaActual <= 0)
                    {
                        patrullaIda = true;
                    }
                }
            }
        }
    }

    void EstadoPersiguiendo()
    {
        if (player == null) return;

        float distancia = Vector2.Distance(transform.position, player.position);

        // Si el jugador se aleja demasiado, volver a idle
        if (distancia > rangoDeteccion * 1.5f)
        {
            estadoActual = EstadoEnemigo.Idle;
            return;
        }

        // Si está en rango para atacar, atacar
        if (distancia <= distanciaParaAtacar && puedeAtacar && !estaStuneado)
        {
            estadoActual = EstadoEnemigo.Atacando;
            StartCoroutine(RealizarAtaque());
            return;
        }

        // Perseguir al jugador
        if (distancia > distanciaMinima)
        {
            Vector2 direccion = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                velocidadPersecucion * Time.deltaTime
            );

            // Voltear sprite hacia el jugador
            spriteRenderer.flipX = direccion.x < 0;
        }
    }

    IEnumerator RealizarAtaque()
    {
        puedeAtacar = false;
        tiempoUltimoAtaque = Time.time;

        // Animación de ataque
        if (spritesAtaque.Length > 0)
        {
            for (int i = 0; i < spritesAtaque.Length; i++)
            {
                spriteRenderer.sprite = spritesAtaque[i];
                
                // Aplicar daño en frame específico (ej: frame 2)
                if (i == 2 && player != null)
                {
                    float distanciaActual = Vector2.Distance(transform.position, player.position);
                    if (distanciaActual <= distanciaParaAtacar * 1.2f)
                    {
                        AplicarDañoAlPlayer();
                    }
                }
                
                yield return new WaitForSeconds(velocidadAnimacionAtaque);
            }
        }

        // Volver a estado normal
        estadoActual = EstadoEnemigo.Persiguiendo;
        
        // Cooldown antes de poder atacar de nuevo
        yield return new WaitForSeconds(tiempoEntreAtaques);
        puedeAtacar = true;
    }

    void AplicarDañoAlPlayer()
    {
        if (LifeManager.instance != null)
        {
            // Aplicar daño
            for (int i = 0; i < dañoAlPlayer; i++)
            {
                LifeManager.instance.PlayerDied();
            }
            
            // Obtener el PlayerController y llamar al método de daño
            PlayerController playerCtrl = player.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.RecibirDañoDeEnemigo(dañoAlPlayer);
            }
            else
            {
                Debug.LogWarning("No se encontró PlayerController en el player");
            }
            
            // Aplicar knockback al player
            if (playerCtrl != null && playerCtrl.rb != null)
            {
                Vector2 knockbackDir = (player.position - transform.position).normalized;
                playerCtrl.rb.AddForce(knockbackDir * knockbackFuerza, ForceMode2D.Impulse);
            }
            
            Debug.Log($"Enemigo ataca! Daño: {dañoAlPlayer} corazón(es)");
        }
    }

    public void RecibirDaño(float cantidad)
    {
        if (estadoActual == EstadoEnemigo.Muerto) return;
        
        // No recibir daño si está atacando y es inmune
        if (estadoActual == EstadoEnemigo.Atacando && esInmuneDuranteAtaque)
            return;
            
        // Interrumpir ataque si puede ser interrumpido
        if (estadoActual == EstadoEnemigo.Atacando && puedeSerInterrumpido)
        {
            StopAllCoroutines();
            estadoActual = EstadoEnemigo.RecibiendoDaño;
        }
    
        vidaActual -= cantidad;
        Debug.Log($"Enemigo recibe {cantidad} de daño. Vida: {vidaActual}/{vidaMaxima}");
        
        // Aplicar stun si el daño es significativo
        if (!estaStuneado && cantidad > vidaMaxima * 0.1f)
        {
            AplicarStun(stunDuracion);
        }
        
        // Aplicar retroceso (knockback)
        if (rb != null && player != null)
        {
            Vector2 knockbackDir = (transform.position - player.position).normalized;
            rb.AddForce(knockbackDir * fuerzaRetroceso, ForceMode2D.Impulse);
        }
        
        // Efecto visual de daño
        StartCoroutine(EfectoDañoVisual());
        
        // Cambiar a estado de daño
        if (estadoActual != EstadoEnemigo.Muerto)
        {
            estadoActual = EstadoEnemigo.RecibiendoDaño;
            StartCoroutine(EstadoRecibiendoDaño());
        }
        
        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    IEnumerator EstadoRecibiendoDaño()
    {
        yield return new WaitForSeconds(tiempoRecuperacionDaño);
        
        // Solo volver a perseguir si no está muerto
        if (estadoActual != EstadoEnemigo.Muerto)
        {
            if (player != null && Vector2.Distance(transform.position, player.position) <= rangoDeteccion)
            {
                estadoActual = EstadoEnemigo.Persiguiendo;
            }
            else
            {
                estadoActual = EstadoEnemigo.Idle;
            }
        }
    }

    IEnumerator EfectoDañoVisual()
    {
        // Parpadeo rojo
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.05f);
            spriteRenderer.color = colorOriginal;
            yield return new WaitForSeconds(0.05f);
        }
    }

    void AplicarStun(float duracion)
    {
        estaStuneado = true;
        tiempoStunRestante = duracion;
        spriteRenderer.color = new Color(0.5f, 0.5f, 1f, 0.8f);
        
        // Cancelar ataque si estaba atacando
        StopAllCoroutines();
    }

    void Morir()
    {
        estadoActual = EstadoEnemigo.Muerto;
        Debug.Log("Enemigo eliminado!");
        
        // Otorgar puntos
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddPoints(puntosPorMuerte);
            Debug.Log($"+{puntosPorMuerte} puntos!");
        }
        
        // Posible loot drop
        if (Random.value < dropChance && lootDrops.Length > 0)
        {
            GameObject loot = lootDrops[Random.Range(0, lootDrops.Length)];
            Instantiate(loot, transform.position, Quaternion.identity);
        }
        
        // Animación de muerte
        StartCoroutine(AnimacionMuerte());
    }

    IEnumerator AnimacionMuerte()
    {
        // Efecto visual de muerte
        for (int i = 0; i < 5; i++)
        {
            spriteRenderer.color = i % 2 == 0 ? Color.red : colorOriginal;
            transform.localScale *= 0.9f;
            yield return new WaitForSeconds(0.1f);
        }
        
        // Destruir enemigo
        Destroy(gameObject);
    }

    void ActualizarAnimacion()
    {
        if (estadoActual == EstadoEnemigo.Atacando || 
            estadoActual == EstadoEnemigo.RecibiendoDaño || 
            estadoActual == EstadoEnemigo.Muerto)
            return;
            
        tiempoAnimacion += Time.deltaTime;
        
        switch (estadoActual)
        {
            case EstadoEnemigo.Idle:
                if (spritesIdle != null && spritesIdle.Length > 0 && tiempoAnimacion >= velocidadAnimacionIdle)
                {
                    spriteRenderer.sprite = spritesIdle[indiceAnimacion];
                    indiceAnimacion = (indiceAnimacion + 1) % spritesIdle.Length;
                    tiempoAnimacion = 0f;
                }
                else if (spriteNormal != null && (spritesIdle == null || spritesIdle.Length == 0))
                {
                    spriteRenderer.sprite = spriteNormal;
                }
                break;
                
            case EstadoEnemigo.Persiguiendo:
                if (spritesCaminar != null && spritesCaminar.Length > 0 && tiempoAnimacion >= velocidadAnimacionCaminar)
                {
                    spriteRenderer.sprite = spritesCaminar[indiceAnimacion];
                    indiceAnimacion = (indiceAnimacion + 1) % spritesCaminar.Length;
                    tiempoAnimacion = 0f;
                }
                else if (spriteNormal != null && (spritesCaminar == null || spritesCaminar.Length == 0))
                {
                    spriteRenderer.sprite = spriteNormal;
                }
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        
        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaParaAtacar);
        
        // Distancia mínima
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanciaMinima);
        
        // Ruta de patrulla
        if (puntosPatrulla != null && puntosPatrulla.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < puntosPatrulla.Length; i++)
            {
                if (puntosPatrulla[i] != null)
                {
                    Gizmos.DrawSphere(puntosPatrulla[i].position, 0.2f);
                    if (i < puntosPatrulla.Length - 1 && puntosPatrulla[i + 1] != null)
                    {
                        Gizmos.DrawLine(puntosPatrulla[i].position, puntosPatrulla[i + 1].position);
                    }
                }
            }
        }
    }
}