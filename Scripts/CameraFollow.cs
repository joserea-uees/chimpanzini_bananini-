using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    
    [Tooltip("Cuánto se adelanta la cámara al jugador (positivo = mira más a la derecha cuando va a la derecha)")]
    public float lookAhead = 3f; // Puedes ajustar este valor (0 = centrado perfecto)
    
    [Tooltip("Suavizado del movimiento (0 = instantáneo, mayor = más suave)")]
    public float smoothSpeed = 0.125f;

    private float alturaFijaY;
    private float currentLookAhead = 0f;
    private float lastPlayerDirection = 0f;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("¡No has asignado al Player en el script de la cámara!");
            return;
        }

        alturaFijaY = transform.position.y;
        
        // Posicionar cámara inicialmente centrada
        transform.position = new Vector3(player.position.x, alturaFijaY, transform.position.z);
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Detectar dirección del jugador por su velocidad horizontal
        float playerVelocityX = player.GetComponent<Rigidbody2D>().linearVelocity.x;
        float targetDirection = Mathf.Sign(playerVelocityX);

        // Solo aplicar look ahead si se está moviendo
        if (Mathf.Abs(playerVelocityX) > 0.1f)
        {
            lastPlayerDirection = targetDirection;
        }

        // Suavizar el look ahead
        float targetLookAhead = lookAhead * lastPlayerDirection;
        currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, smoothSpeed * 2);

        // Posición deseada: jugador + adelanto horizontal + altura fija
        float desiredX = player.position.x + currentLookAhead;
        
        Vector3 desiredPosition = new Vector3(desiredX, alturaFijaY, transform.position.z);

        // Aplicar suavizado
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}