using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ItemPair
{
    public string pairName = "Par nuevo";
    public GameObject itemA;
    public GameObject itemB; 
    [Range(0f, 3f)]
    public float weight = 1f;
}

public class Generador : MonoBehaviour
{
    [Header("Ajustes del Generador")]

    [Tooltip("Prefabs que pueden aparecer solos. Déjalo vacío si solo quieres pares.")]
    public GameObject[] singleItemPrefabs;

    [Tooltip("Define aquí los pares exactos que quieres que salgan juntos.")]
    public List<ItemPair> doubleItemPairs = new List<ItemPair>();

    // Probabilidad de generar dos ítems en lugar de uno
    [Range(0f, 1f)]
    public float doubleSpawnChance = 0.3f;

    // Separación horizontal/lateral entre los dos ítems
    public float doubleSpawnOffset = 0.5f;
    public Vector2 doubleSpawnDirection = Vector2.right;

    // ¡NUEVO! Cuánto más arriba aparece el Item B respecto al Item A
    [Header("Posición vertical del par")]
    [Tooltip("Distancia adicional hacia arriba que tendrá el Item B respecto al Item A.")]
    public float itemBHeightOffset = 2f;

    // Intervalo de tiempo entre generaciones
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 3f;

    private float nextSpawnTime;
    private float totalPairWeight = 0f;

    void Start()
    {
        if ((singleItemPrefabs == null || singleItemPrefabs.Length == 0) &&
            (doubleItemPairs == null || doubleItemPairs.Count == 0))
        {
            Debug.LogError("Generador: No hay prefabs individuales ni pares definidos.");
            enabled = false;
            return;
        }

        RecalculatePairWeights();
        ScheduleNextSpawn();
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnItems();
            ScheduleNextSpawn();
        }
    }

    private void SpawnItems()
    {
        bool spawnDouble = Random.value < doubleSpawnChance;

        if (spawnDouble && doubleItemPairs.Count > 0)
        {
            ItemPair selectedPair = ChooseWeightedPair();

            // Posición base (centro del generador)
            Vector3 basePosition = transform.position;

            // Offset lateral (izquierda/derecha según la dirección configurada)
            Vector3 lateralOffset = doubleSpawnDirection.normalized * doubleSpawnOffset * 0.5f;

            // Offset vertical adicional para Item B
            Vector3 upOffset = Vector3.up * itemBHeightOffset;

            // Item A: a la izquierda/abajo del centro + posición base
            if (selectedPair.itemA != null)
                Instantiate(selectedPair.itemA, basePosition - lateralOffset, transform.rotation);

            // Item B: a la derecha/arriba del centro + posición base + altura extra
            if (selectedPair.itemB != null)
                Instantiate(selectedPair.itemB, basePosition + lateralOffset + upOffset, transform.rotation);
        }
        else
        {
            // Ítem individual (en el centro exacto)
            if (singleItemPrefabs != null && singleItemPrefabs.Length > 0)
            {
                GameObject prefab = singleItemPrefabs[Random.Range(0, singleItemPrefabs.Length)];
                Instantiate(prefab, transform.position, transform.rotation);
            }
        }
    }

    private ItemPair ChooseWeightedPair()
    {
        float randomValue = Random.Range(0f, totalPairWeight);
        float accumulated = 0f;

        foreach (ItemPair pair in doubleItemPairs)
        {
            accumulated += pair.weight;
            if (randomValue <= accumulated)
                return pair;
        }

        return doubleItemPairs[doubleItemPairs.Count - 1];
    }

    private void RecalculatePairWeights()
    {
        totalPairWeight = 0f;
        foreach (ItemPair pair in doubleItemPairs)
        {
            totalPairWeight += Mathf.Max(0f, pair.weight);
        }

        if (totalPairWeight <= 0f && doubleItemPairs.Count > 0)
        {
            totalPairWeight = doubleItemPairs.Count;
            foreach (ItemPair pair in doubleItemPairs) pair.weight = 1f;
        }
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        RecalculatePairWeights();
    }
#endif
}