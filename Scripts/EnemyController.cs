using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public float vidaMaxima = 100f;
    public float vidaActual;
    public float dañoPorGolpe = 20f;

    [Header("Configuración de Persecución")]
    public float rangoDeteccion = 8f;
    public float velocidadPersecucion = 3f;
    public float distanciaMinima = 1f;

    [Header("Configuración de Ataque")]
    public Sprite[] spritesAtaque;
    public float velocidadAnimacionAtaque = 0.1f;
    public int dañoAlPlayer = 1;
    public float rangoAtaque = 1.5f;
    public float tiempoEntreAtaques = 2f;

    [Header("Configuración Visual")]
    public Sprite spriteNormal;

    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Collider2D enemyCollider;

    private bool estaAtacando = false;
    private bool puedeAtacar = true;
    private bool estaPersiguiendo = false;

    private Coroutine ataqueCoroutine;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
        vidaActual = vidaMaxima;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanciaAlPlayer = Vector2.Distance(transform.position, player.position);

        if (distanciaAlPlayer <= rangoDeteccion)
        {
            estaPersiguiendo = true;

            if (!estaAtacando)
            {
                if (distanciaAlPlayer <= rangoAtaque && puedeAtacar && ataqueCoroutine == null)
                {
                    ataqueCoroutine = StartCoroutine(Atacar());
                }
                else if (distanciaAlPlayer > distanciaMinima)
                {
                    PerseguirPlayer();
                }
                else
                {
                    VolverASpriteNormal();
                }
            }
        }
        else
        {
            estaPersiguiendo = false;
            VolverASpriteNormal();
        }
    }

    void PerseguirPlayer()
    {
        Vector2 direccion = (player.position - transform.position).normalized;

        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            velocidadPersecucion * Time.deltaTime
        );

        if (direccion.x < 0)
            spriteRenderer.flipX = true;
        else if (direccion.x > 0)
            spriteRenderer.flipX = false;
    }

    IEnumerator Atacar()
    {
        estaAtacando = true;
        puedeAtacar = false;

        if (spritesAtaque.Length > 0)
        {
            for (int i = 0; i < spritesAtaque.Length; i++)
            {
                spriteRenderer.sprite = spritesAtaque[i];
                yield return new WaitForSeconds(velocidadAnimacionAtaque);
            }
        }

        if (Vector2.Distance(transform.position, player.position) <= rangoAtaque)
        {
            HacerDañoAlPlayer();
        }

        VolverASpriteNormal();

        estaAtacando = false;

        yield return new WaitForSeconds(tiempoEntreAtaques);
        puedeAtacar = true;
        ataqueCoroutine = null;
    }

    void HacerDañoAlPlayer()
    {
        if (LifeManager.instance != null)
        {
            for (int i = 0; i < dañoAlPlayer; i++)
            {
                LifeManager.instance.PlayerDied();
            }
        }
    }

    public void RecibirDaño(float cantidad)
    {
        vidaActual -= cantidad;

        if (vidaActual <= 0)
        {
            Morir();
        }
        else
        {
            StartCoroutine(EfectoDaño());
        }
    }

    IEnumerator EfectoDaño()
    {
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = original;
    }

    void Morir()
    {
        Destroy(gameObject);
    }

    void VolverASpriteNormal()
    {
        if (!estaAtacando && spriteNormal != null)
        {
            spriteRenderer.sprite = spriteNormal;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !estaAtacando)
        {
            RecibirDaño(dañoPorGolpe);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanciaMinima);
    }
}
