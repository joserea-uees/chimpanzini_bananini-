using UnityEngine;
using System.Collections;

public class EnemiController : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public float vidaMaxima = 100f;
    public float vidaActual;
    public float dañoPorGolpe = 20f; // Porcentaje de daño que recibe del player

    [Header("Configuración de Persecución")]
    public float rangoDeteccion = 8f; // Distancia para empezar a perseguir
    public float velocidadPersecucion = 3f; // Velocidad al perseguir
    public float distanciaMinima = 1f; // Distancia mínima al player

    [Header("Configuración de Ataque")]
    public Sprite[] spritesAtaque;
    public float velocidadAnimacionAtaque = 0.1f;
    public int dañoAlPlayer = 1; // Corazones que quita al player
    public float rangoAtaque = 1.5f; // Distancia para atacar
    public float tiempoEntreAtaques = 2f; // Tiempo entre ataques

    [Header("Configuración de Animación de Movimiento")]
    public Sprite[] spritesCorrer;
    public float velocidadAnimacionCorrer = 0.1f;

    [Header("Configuración Visual")]
    public Sprite spriteNormal; // Sprite cuando no está atacando

    private SpriteRenderer spriteRenderer;
    private Transform player;
    private bool estaAtacando = false;
    private bool puedeAtacar = true;
    private Collider2D enemyCollider;
    private bool estaPersiguiendo = false;
    private int indiceAnimacionCorrer = 0;
    private float tiempoUltimoFrame = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
        vidaActual = vidaMaxima;

        // Buscar al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player != null)
        {
            float distanciaAlPlayer = Vector2.Distance(transform.position, player.position);
            
            // Verificar si el player está en rango de detección
            if (distanciaAlPlayer <= rangoDeteccion)
            {
                estaPersiguiendo = true;

                if (!estaAtacando)
                {
                    // Si está en rango de ataque, atacar
                    if (distanciaAlPlayer <= rangoAtaque && puedeAtacar)
                    {
                        StartCoroutine(Atacar());
                    }
                    // Si no está en rango de ataque, perseguir
                    else if (distanciaAlPlayer > distanciaMinima)
                    {
                        PerseguirPlayer();
                        AnimarMovimiento();
                    }
                    else
                    {
                        // Si está muy cerca pero no atacando, usar sprite normal
                        if (spriteNormal != null)
                        {
                            spriteRenderer.sprite = spriteNormal;
                        }
                    }
                }
            }
            else
            {
                estaPersiguiendo = false;
                // Volver al sprite normal cuando no está persiguiendo
                if (!estaAtacando && spriteNormal != null)
                {
                    spriteRenderer.sprite = spriteNormal;
                }
            }
        }
    }

    void PerseguirPlayer()
    {
        // Mover hacia el player
        Vector2 direccion = (player.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(
            transform.position, 
            player.position, 
            velocidadPersecucion * Time.deltaTime
        );

        // Voltear el sprite según la dirección (opcional)
        if (direccion.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (direccion.x > 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    void AnimarMovimiento()
    {
        // Animar solo si hay sprites de correr
        if (spritesCorrer != null && spritesCorrer.Length > 0)
        {
            tiempoUltimoFrame += Time.deltaTime;
            
            if (tiempoUltimoFrame >= velocidadAnimacionCorrer)
            {
                spriteRenderer.sprite = spritesCorrer[indiceAnimacionCorrer];
                indiceAnimacionCorrer = (indiceAnimacionCorrer + 1) % spritesCorrer.Length;
                tiempoUltimoFrame = 0f;
            }
        }
    }

    IEnumerator Atacar()
    {
        estaAtacando = true;
        puedeAtacar = false;

        // Reproducir animación de ataque
        if (spritesAtaque.Length > 0)
        {
            for (int i = 0; i < spritesAtaque.Length; i++)
            {
                spriteRenderer.sprite = spritesAtaque[i];
                yield return new WaitForSeconds(velocidadAnimacionAtaque);
            }
        }

        // Hacer daño al player al final de la animación
        if (Vector2.Distance(transform.position, player.position) <= rangoAtaque)
        {
            HacerDañoAlPlayer();
        }

        // Volver al sprite normal
        if (spriteNormal != null)
        {
            spriteRenderer.sprite = spriteNormal;
        }

        estaAtacando = false;

        // Esperar antes de poder atacar de nuevo
        yield return new WaitForSeconds(tiempoEntreAtaques);
        puedeAtacar = true;
    }

    void HacerDañoAlPlayer()
    {
        // Usar el LifeManager para quitar corazones
        if (LifeManager.instance != null)
        {
            for (int i = 0; i < dañoAlPlayer; i++)
            {
                LifeManager.instance.PlayerDied();
            }
            Debug.Log($"Enemigo ataca al Player! Quita {dañoAlPlayer} corazón(es)");
        }
    }

    public void RecibirDaño(float cantidad)
    {
        vidaActual -= cantidad;
        Debug.Log($"Enemigo recibe {cantidad} de daño. Vida restante: {vidaActual}/{vidaMaxima}");

        if (vidaActual <= 0)
        {
            Morir();
        }
        else
        {
            // Efecto visual de daño (opcional)
            StartCoroutine(EfectoDaño());
        }
    }

    IEnumerator EfectoDaño()
    {
        // Parpadeo para indicar que recibió daño
        Color colorOriginal = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = colorOriginal;
    }

    void Morir()
    {
        Debug.Log("Enemigo eliminado!");
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si el player choca con el enemigo, recibe daño
        if (collision.gameObject.CompareTag("Player") && !estaAtacando)
        {
            RecibirDañoDelPlayer();
        }
    }

    void RecibirDañoDelPlayer()
    {
        RecibirDaño(dañoPorGolpe);
    }

    // Visualizar los rangos en el editor
    private void OnDrawGizmosSelected()
    {
        // Rango de detección (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        
        // Rango de ataque (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
        
        // Distancia mínima (verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanciaMinima);
    }
}
