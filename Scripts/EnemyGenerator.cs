using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [Header("Configuración del Boss")]
    public GameObject bossPrefab; // El prefab del enemigo boss
    public Transform puntoGeneracion; // Punto donde aparecerá el boss (opcional)
    
    [Header("Condición de Aparición")]
    public int puntajeRequerido = 30; // Puntaje necesario para generar el boss
    
    private bool bossGenerado = false; // Para asegurar que solo se genere una vez

    void Update()
    {
        // Verificar si ya se alcanzó el puntaje requerido y el boss no ha sido generado
        if (!bossGenerado && ScoreManager.instance != null)
        {
            if (ScoreManager.instance.score >= puntajeRequerido)
            {
                GenerarBoss();
            }
        }
    }

    void GenerarBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("EnemyGenerator: No hay prefab de boss asignado!");
            return;
        }

        // Determinar la posición de generación
        Vector3 posicionGeneracion;
        if (puntoGeneracion != null)
        {
            posicionGeneracion = puntoGeneracion.position;
        }
        else
        {
            // Si no hay punto específico, usar la posición del generador
            posicionGeneracion = transform.position;
        }

        // Generar el boss
        GameObject boss = Instantiate(bossPrefab, posicionGeneracion, Quaternion.identity);
        bossGenerado = true;

        Debug.Log($"¡Boss generado en {posicionGeneracion} al alcanzar {puntajeRequerido} puntos!");
    }

    // Método opcional para resetear el generador (útil si quieres generar otro boss después)
    public void ResetearGenerador()
    {
        bossGenerado = false;
    }

    // Visualizar el punto de generación en el editor
    private void OnDrawGizmosSelected()
    {
        Vector3 posicion = (puntoGeneracion != null) ? puntoGeneracion.position : transform.position;
        
        // Dibujar un círculo en el punto de generación
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(posicion, 1f);
        
        // Dibujar una cruz para marcar el punto
        Gizmos.DrawLine(posicion + Vector3.up, posicion + Vector3.down);
        Gizmos.DrawLine(posicion + Vector3.left, posicion + Vector3.right);
    }
}