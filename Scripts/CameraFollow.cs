using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    
    [Tooltip("El suavizado del movimiento (0 es instantáneo, 1 es muy lento)")]
    public float smoothSpeed = 0.125f;

    private Vector3 offset;
    private float alturaFijaY;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("¡No has asignado al Player en el script de la cámara!");
            return;
        }

        alturaFijaY = transform.position.y;

        offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        if (player == null) return;

        float targetX = player.position.x + offset.x;
        
        Vector3 desiredPosition = new Vector3(targetX, alturaFijaY, transform.position.z);

        
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }
}