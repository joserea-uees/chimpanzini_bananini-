using UnityEngine;
using System.Collections;

using UnityEngine.SceneManagement;

public class CambiarNivel : MonoBehaviour
{
    [Header("Configuración del Cambio de Nivel")]
    public string escenaSiguiente = "Nivel2"; 

    [Header("Opcional: Efectos al tocar")]
    public AudioSource sonidoTubo; 
    public ParticleSystem particulas; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (sonidoTubo != null) sonidoTubo.Play();
            if (particulas != null) particulas.Play();

            StartCoroutine(CargarEscenaConRetraso(0.3f));
        }
    }

    private IEnumerator CargarEscenaConRetraso(float segundos)
    {
        yield return new WaitForSeconds(segundos);

        SceneManager.LoadScene(escenaSiguiente);
    }
}