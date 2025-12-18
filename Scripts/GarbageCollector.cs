using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GarbageCollector : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Deja vacío para eliminar todos los objetos excepto el Player")]
    public string[] tagsAEliminar;
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // No eliminar al jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        // Si hay tags específicos definidos, solo eliminar esos
        if (tagsAEliminar.Length > 0)
        {
            foreach (string tag in tagsAEliminar)
            {
                if (collision.gameObject.CompareTag(tag))
                {
                    Debug.Log($"GarbageCollector: Eliminando {collision.gameObject.name} con tag {tag}");
                    Destroy(collision.gameObject);
                    return;
                }
            }
        }
        else
        {
            // Si no hay tags definidos, eliminar todo excepto el player
            Debug.Log($"GarbageCollector: Eliminando {collision.gameObject.name}");
            Destroy(collision.gameObject);
        }
    }
}
