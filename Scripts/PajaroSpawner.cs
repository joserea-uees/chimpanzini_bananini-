using UnityEngine;

public class PajaroSpawner : MonoBehaviour
{
    [Header("Configuración del Spawn")]
    public GameObject pajaroPrefab;           // Asigna el prefab del pájaro en el Inspector
    public Transform[] puntosSpawn;           // Varios puntos donde pueden aparecer (izquierda/derecha)
    public float intervaloMin = 3f;           // Tiempo mínimo entre spawns
    public float intervaloMax = 6f;           // Tiempo máximo entre spawns

    [Header("Altura de Vuelo")]
    public float alturaMin = 2f;
    public float alturaMax = 5f;

    private bool spawnerActivado = false;

    void OnEnable()
    {
        ScoreManager.OnReached10Points += ActivarSpawner;
    }

    void OnDisable()
    {
        ScoreManager.OnReached10Points -= ActivarSpawner;
    }

    void ActivarSpawner()
    {
        if (!spawnerActivado)
        {
            spawnerActivado = true;
            StartCoroutine(SpawnRoutine());
            Debug.Log("¡Spawner de pájaros activado! El jugador alcanzó 10 puntos.");
        }
    }

    System.Collections.IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Espera un tiempo aleatorio antes de spawnear el siguiente pájaro
            float espera = Random.Range(intervaloMin, intervaloMax);
            yield return new WaitForSeconds(espera);

            if (pajaroPrefab == null || puntosSpawn.Length == 0) continue;

            // Elige un punto de spawn aleatorio
            Transform puntoElegido = puntosSpawn[Random.Range(0, puntosSpawn.Length)];

            // Decide si vuela a la derecha o izquierda según el lado
            bool vuelaDerecha = puntoElegido.position.x < 0; // Si spawnea a la izquierda → vuela a derecha

            // Altura aleatoria
            float y = Random.Range(alturaMin, alturaMax);

            Vector3 posicionSpawn = new Vector3(puntoElegido.position.x, y, 0);

            // Instancia el pájaro
            GameObject pajaro = Instantiate(pajaroPrefab, posicionSpawn, Quaternion.identity);

            // Configura la dirección de vuelo
            Pajaro scriptPajaro = pajaro.GetComponent<Pajaro>();
            if (scriptPajaro != null)
            {
                scriptPajaro.volarADerecha = vuelaDerecha;
            }
        }
    }
}