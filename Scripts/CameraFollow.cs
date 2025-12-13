using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Asigna el transform del jugador aquí
    public float smoothSpeed = 0.125f; // Opcional, para suavizado
    public Vector3 offset = new Vector3(0, 0, -10);

    void LateUpdate()
    {
        Vector3 desiredPosition = player.position + offset;
        desiredPosition.x = Mathf.Max(desiredPosition.x, 0); // Opcional: evita ir atrás del inicio
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        // O sin suavizado: transform.position = desiredPosition;
    }
}