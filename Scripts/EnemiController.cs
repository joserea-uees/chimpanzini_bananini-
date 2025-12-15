using UnityEngine;

public class EnemiController : MonoBehaviour
{
    [Header("Configuración de Persecución")]
    public Transform player;
    public float velocidadMovimiento = 5f;
    public float distanciaDetras = 3f;
    public float suavidadMovimiento = 2f;

    private Vector3 posicionObjetivo;

    void Start()
    {
        // Buscar automáticamente al player si no está asignado
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (player != null)
        {
            SeguirPlayer();
        }
    }

    void SeguirPlayer()
    {
        // Calcular la posición objetivo (detrás del player)
        posicionObjetivo = new Vector3(
            player.position.x - distanciaDetras,  // Mantener distancia en X
            player.position.y,                      // Seguir la altura del player
            transform.position.z                    // Mantener la profundidad Z
        );

        // Mover suavemente hacia la posición objetivo
        transform.position = Vector3.Lerp(
            transform.position, 
            posicionObjetivo, 
            suavidadMovimiento * Time.deltaTime
        );
    }
}
