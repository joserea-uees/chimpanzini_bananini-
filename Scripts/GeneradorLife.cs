using UnityEngine;

public class GeneradorLife : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject lifeExtraPrefab;
    public float intervaloGeneracion = 15f; // Tiempo entre generaciones en segundos

    private float tiempoProximaGeneracion;

    void Start()
    {
        // Programar la primera generación
        tiempoProximaGeneracion = Time.time + intervaloGeneracion;
    }

    void Update()
    {
        // Verificar si es momento de generar
        if (Time.time >= tiempoProximaGeneracion)
        {
            GenerarVidaExtra();
            // Programar la siguiente generación
            tiempoProximaGeneracion = Time.time + intervaloGeneracion;
        }
    }

    void GenerarVidaExtra()
    {
        if (lifeExtraPrefab != null)
        {
            Instantiate(lifeExtraPrefab, transform.position, Quaternion.identity);
            Debug.Log($"Vida extra generada en {transform.position}");
        }
        else
        {
            Debug.LogError("GeneradorLife: No hay prefab de vida extra asignado!");
        }
    }
}
